﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TravelMaker.Models;
using TravelMaker.Security;

namespace TravelMaker.Controllers
{
    /// <summary>
    ///     房間相關
    /// </summary>
    [RoutePrefix("api/rooms")]
    public class RoomsController : ApiController
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext();

        /// <summary>
        ///     主揪新增房間
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        public IHttpActionResult RoomAdd(RoomAddView roomAdd)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //新增房間
            Room room = new Room();
            room.RoomName = roomAdd.RoomName;
            room.Status = true;
            room.RoomGuid = Guid.NewGuid().ToString().Trim() + DateTime.Now.ToString("ff");
            room.InitDate = DateTime.Now;
            _db.Rooms.Add(room);

            //新增房主身份
            RoomMember roomMember = new RoomMember();
            roomMember.UserId = userId;
            roomMember.RoomId = _db.Rooms.Where(r => r.RoomGuid == room.RoomGuid).Select(r => r.RoomId).FirstOrDefault();
            roomMember.Permission = 1;
            roomMember.InitDate = DateTime.Now;
            _db.RoomMembers.Add(roomMember);

            //新增房間景點
            RoomAttraction roomAttraction = new RoomAttraction();
            int i = 1;
            foreach (int id in roomAdd.Attractions)
            {
                roomAttraction.RoomId = roomMember.RoomId;
                roomAttraction.AttractionId = id;
                roomAttraction.UserId = userId;
                roomAttraction.AttrOrder = i;
                i++;

                _db.RoomAttractions.Add(roomAttraction);
                _db.SaveChanges();
            }

            var result = new
            {
                Message = "成功創建",
                RoomGuid = room.RoomGuid
            };
            return Ok(result);
        }



        /// <summary>
        ///     取得單一房間所有資訊
        /// </summary>
        [HttpGet]
        [Route("{roomGuid}")]
        public IHttpActionResult RoomInfo([FromUri] string roomGuid)
        {
            var hadRoom = _db.Rooms.Where(r => r.RoomGuid == roomGuid).FirstOrDefault();
            if (hadRoom != null)
            {
                if (hadRoom.Status == true)
                {
                    string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
                    string attrPath = "https://" + Request.RequestUri.Host + "/upload/attractionImage/";
                    int myUserId = 0;
                    if (Request.Headers.Authorization != null)
                    {
                        var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                        string userGuid = (string)userToken["UserGuid"];
                        myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
                    }

                    var room = new
                    {
                        RoomGuid = roomGuid,
                        RoomName = hadRoom.RoomName,
                        CreaterGuid = _db.RoomMembers.FirstOrDefault(r => r.RoomId == hadRoom.RoomId && r.Permission == 1).User.UserGuid,

                        Users = _db.RoomMembers.Where(r => r.RoomId == hadRoom.RoomId).Select(r => new
                        {
                            UserGuid = r.User.UserGuid,
                            UserName = r.User.UserName,
                            ProfilePicture = r.User.ProfilePicture == null ? "" : profilePath + r.User.ProfilePicture
                        }),

                        VoteDates = _db.VoteDates.Where(d => d.RoomId == hadRoom.RoomId).OrderBy(d => d.Date).Select(d => new
                        {
                            VoteDateId = d.VoteDateId,
                            UserGuid = d.User.UserGuid,
                            Date = d.Date.Year + "-" + d.Date.Month + "-" + d.Date.Day,
                            Count = _db.Votes.Where(v => v.VoteDateId == d.VoteDateId).Count(),
                            IsVoted = _db.Votes.Where(v => v.VoteDateId == d.VoteDateId).Any(v => v.UserId == myUserId) ? true : false
                        }),

                        AttrationsData = _db.RoomAttractions.Where(r => r.RoomId == hadRoom.RoomId).Select(a => new
                        {
                            AttractionId = a.AttractionId,
                            UserGuid = _db.Users.FirstOrDefault(u => u.UserId == a.UserId).UserGuid,
                            AttractionName = a.Attraction.AttractionName,
                            Elong = a.Attraction.Elong,
                            Nlat = a.Attraction.Nlat,
                            ImageUrl = attrPath + _db.Images.FirstOrDefault(i => i.AttractionId == a.AttractionId).ImageName,
                            Order = a.AttrOrder
                        })
                    };

                    return Ok(room);
                }
                else
                {
                    return BadRequest("房間已解散");
                }
            }
            else
            {
                return BadRequest("房間不存在");
            }
        }




        /// <summary>
        ///     修改房間名稱
        /// </summary>
        [HttpPut]
        [JwtAuthFilter]
        [Route("rename")]
        public IHttpActionResult RoomName(RoomNameView roomNameView)
        {
            //房客才可以修改名字
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            var inRoom = _db.RoomMembers.FirstOrDefault(r => r.User.UserGuid == userGuid && r.Room.RoomGuid == roomNameView.RoomGuid);

            if (inRoom != null)
            {
                var room = _db.Rooms.FirstOrDefault(r => r.RoomGuid == roomNameView.RoomGuid);

                //先發通知
                var roomMembers = _db.RoomMembers.Where(r => r.Room.RoomGuid == roomNameView.RoomGuid && r.User.UserGuid != userGuid).ToList();

                int senderId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;
                int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "房間名稱").NotificationTypeId;

                var notifications = roomMembers.Select(roomMember => new Notification
                {
                    Status = true,
                    IsRead = false,
                    Sender = senderId,
                    Receiver = roomMember.UserId,
                    NotificationTypeId = typeId,
                    InitDate = DateTime.Now,
                    RoomGuid = roomNameView.RoomGuid,
                    OldRoomName = room.RoomName,
                    NewRoomName = roomNameView.RoomName,
                });

                _db.Notifications.AddRange(notifications);
                _db.SaveChanges();

                //再修改房間名稱
                room.RoomName = roomNameView.RoomName;
                _db.SaveChanges();

                return Ok(new { Message = "修改成功" });
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }










        /// <summary>
        ///     主揪,被揪編輯(儲存)房間-房間資訊(行程)
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("modify")]
        public IHttpActionResult RoomModify(RoomModifyView roomModify)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            //是否為該房間成員
            var inRoom = _db.RoomMembers.Where(r => r.Room.RoomGuid == roomModify.RoomGuid && r.User.UserGuid == userGuid).FirstOrDefault();

            if (inRoom != null)
            {
                //刪掉原本的景點
                var originAtts = _db.RoomAttractions.Where(r => r.Room.RoomGuid == roomModify.RoomGuid);

                if (originAtts.Any())
                {
                    foreach (var originAtt in originAtts)
                    {
                        _db.RoomAttractions.Remove(originAtt);
                    }
                    _db.SaveChanges();
                }

                //儲存更新的景點
                int roomId = _db.Rooms.FirstOrDefault(r => r.RoomGuid == roomModify.RoomGuid).RoomId;

                foreach (var newAttraction in roomModify.AttrationsData)
                {
                    RoomAttraction roomAttraction = new RoomAttraction();
                    roomAttraction.RoomId = roomId;
                    roomAttraction.AttractionId = newAttraction.AttractionId;
                    roomAttraction.UserId = _db.Users.FirstOrDefault(u => u.UserGuid == newAttraction.UserGuid).UserId;
                    roomAttraction.AttrOrder = newAttraction.Order;

                    _db.RoomAttractions.Add(roomAttraction);
                }
                _db.SaveChanges();

                //通知房間成員
                var roomMembers = _db.RoomMembers.Where(r => r.Room.RoomGuid == roomModify.RoomGuid && r.User.UserGuid != userGuid).ToList();

                int senderId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;
                int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "房間編輯").NotificationTypeId;

                var notifications = roomMembers.Select(roomMember => new Notification
                {
                    Status = true,
                    IsRead = false,
                    Sender = senderId,
                    Receiver = roomMember.UserId,
                    NotificationTypeId = typeId,
                    InitDate = DateTime.Now,
                    RoomGuid = roomModify.RoomGuid
                });

                _db.Notifications.AddRange(notifications);
                _db.SaveChanges();

                return Ok(new { Message = "房間景點修改成功" });
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }







        /// <summary>
        ///     主揪,被揪新增日期選項
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("dates")]
        public IHttpActionResult VoteDateAdd(DateView dateView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //是房間成員才可以新增
            var inRoom = _db.RoomMembers.Where(r => r.Room.RoomGuid == dateView.RoomGuid && r.UserId == userId).FirstOrDefault();

            if (inRoom != null)
            {
                //處理日期傳入格式為字串ex.2023-5-3
                string[] tempDate = dateView.Date.Split('-');
                DateTime date = new DateTime(Convert.ToInt32(tempDate[0]), Convert.ToInt32(tempDate[1]), Convert.ToInt32(tempDate[2]));

                //日期有無重複
                var hadDate = _db.VoteDates.Where(d => d.Room.RoomGuid == dateView.RoomGuid && d.Date == date).FirstOrDefault();
                if (hadDate == null)
                {
                    VoteDate voteDate = new VoteDate();
                    voteDate.RoomId = _db.Rooms.Where(r => r.RoomGuid == dateView.RoomGuid).Select(r => r.RoomId).FirstOrDefault();
                    voteDate.Date = date;
                    voteDate.UserId = userId;
                    _db.VoteDates.Add(voteDate);
                    _db.SaveChanges();

                    //通知房間成員
                    var roomMembers = _db.RoomMembers.Where(r => r.Room.RoomGuid == dateView.RoomGuid && r.User.UserGuid != userGuid).ToList();

                    int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "日期新增").NotificationTypeId;

                    var notifications = roomMembers.Select(roomMember => new Notification
                    {
                        Status = true,
                        IsRead = false,
                        Sender = userId,
                        Receiver = roomMember.UserId,
                        NotificationTypeId = typeId,
                        InitDate = DateTime.Now,
                        RoomGuid = dateView.RoomGuid,
                        AddVoteDate = $"{tempDate[0]}/{tempDate[1]}/{tempDate[2]}"
                    });

                    _db.Notifications.AddRange(notifications);
                    _db.SaveChanges();

                    return Ok(new
                    {
                        Message = "日期新增成功",
                        VoteDateId = voteDate.VoteDateId
                    });
                }
                else
                {
                    return BadRequest("投票日期已存在");
                }
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }


        /// <summary>
        ///     主揪,被揪刪除日期選項
        /// </summary>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("dates/{voteDateId}")]
        public IHttpActionResult VoteDateDelete([FromUri] int voteDateId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var voteDateDetail = _db.VoteDates.Where(v => v.VoteDateId == voteDateId).FirstOrDefault();
            var inRoom = _db.RoomMembers.Where(r => r.RoomId == voteDateDetail.RoomId && r.UserId == userId).FirstOrDefault();

            //是房間成員，且主揪或提出該日期的被揪可以刪除
            if (inRoom != null)
            {
                if (inRoom.Permission == 1 || voteDateDetail.UserId == userId)
                {
                    //先刪投票的人，再刪日期
                    var votes = _db.Votes.Where(v => v.VoteDateId == voteDateId).ToList();

                    foreach (var vote in votes)
                    {
                        _db.Votes.Remove(vote);
                    }

                    _db.VoteDates.Remove(voteDateDetail);
                    _db.SaveChanges();

                    return Ok(new { Message = "刪除成功" });
                }
                else
                {
                    return BadRequest("非房主或提出日期的房客");
                }
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }



        /// <summary>
        ///     主揪.被揪投票
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("votes/{voteDateId}")]
        public IHttpActionResult VoteAdd([FromUri] int voteDateId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var voteDateDetail = _db.VoteDates.Where(v => v.VoteDateId == voteDateId).FirstOrDefault();
            var inRoom = _db.RoomMembers.Where(r => r.RoomId == voteDateDetail.RoomId && r.UserId == userId).FirstOrDefault();

            if (inRoom != null)
            {
                Vote vote = new Vote();
                vote.UserId = userId;
                vote.VoteDateId = voteDateId;
                _db.Votes.Add(vote);
                _db.SaveChanges();

                return Ok(new { Message = "投票成功" });
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }




        /// <summary>
        ///     主揪.被揪取消投票
        /// </summary>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("votes/{voteDateId}")]
        public IHttpActionResult VoteDelete([FromUri] int voteDateId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var voteDateDetail = _db.VoteDates.Where(v => v.VoteDateId == voteDateId).FirstOrDefault();
            var inRoom = _db.RoomMembers.Where(r => r.RoomId == voteDateDetail.RoomId && r.UserId == userId).FirstOrDefault();

            if (inRoom != null)
            {
                var vote = _db.Votes.Where(v => v.VoteDateId == voteDateId && v.UserId == userId).FirstOrDefault();

                _db.Votes.Remove(vote);
                _db.SaveChanges();

                return Ok(new { Message = "已取消投票" });
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }




        /// <summary>
        ///     新增被揪
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("members")]
        public IHttpActionResult RoomMemberAdd(RoomMemberAddView memberView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            User user = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            //被揪Email是否為平台用戶
            var memberAdd = _db.Users.FirstOrDefault(u => u.Account == memberView.UserEmail);
            if (memberAdd != null)
            {
                //被揪是否已在房間內
                var inRoom = _db.RoomMembers.FirstOrDefault(r => r.Room.RoomGuid == memberView.RoomGuid && r.UserId == memberAdd.UserId);
                if (inRoom == null)
                {
                    RoomMember roomMember = new RoomMember();
                    roomMember.UserId = memberAdd.UserId;
                    roomMember.RoomId = _db.Rooms.Where(r => r.RoomGuid == memberView.RoomGuid).Select(r => r.RoomId).FirstOrDefault();
                    roomMember.Permission = 2;
                    roomMember.InitDate = DateTime.Now;

                    _db.RoomMembers.Add(roomMember);
                    _db.SaveChanges();

                    //發通知給被揪(別人加自己時)
                    if (user.Account != memberView.UserEmail)
                    {
                        int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "房間邀請").NotificationTypeId;

                        var notification = new Notification
                        {
                            Status = true,
                            IsRead = false,
                            Sender = user.UserId,
                            Receiver = roomMember.UserId,
                            NotificationTypeId = typeId,
                            InitDate = DateTime.Now,
                            RoomGuid = memberView.RoomGuid
                        };

                        _db.Notifications.Add(notification);
                        _db.SaveChanges();
                    }

                    var result = new
                    {
                        UserGuid = memberAdd.UserGuid,
                        UserName = memberAdd.UserName,
                        ProfilePicture = memberAdd.ProfilePicture == null ? "" : "https://" + Request.RequestUri.Host + "/upload/profile/" + memberAdd.ProfilePicture
                    };

                    return Ok(result);
                }
                else
                {
                    return BadRequest("被揪已在房間內");
                }
            }
            else
            {
                return BadRequest("此帳號尚未註冊為平台會員");
            }
        }



        /// <summary>
        ///     主揪刪除被揪、被揪刪除自己
        /// </summary>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("members")]
        public IHttpActionResult RoomMemberDelete(RoomMemberDelView memberView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            User user = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            //主揪或被揪自己才有權限刪除
            var inRoom = _db.RoomMembers.FirstOrDefault(r => r.User.UserGuid == userGuid && r.Room.RoomGuid == memberView.RoomGuid);

            if (inRoom != null)
            {
                var memberDel = _db.RoomMembers.Where(r => r.User.UserGuid == memberView.UserGuid && r.Room.RoomGuid == memberView.RoomGuid).FirstOrDefault();

                if (memberDel != null && (inRoom.Permission == 1 || inRoom.UserId == memberDel.UserId))
                {
                    //刪投票紀錄
                    var votes = _db.Votes.Where(v => v.VoteDate.Room.RoomGuid == memberView.RoomGuid && v.UserId == memberDel.UserId).ToList();
                    foreach (var vote in votes)
                    {
                        _db.Votes.Remove(vote);
                    }
                    //刪被揪
                    _db.RoomMembers.Remove(memberDel);
                    _db.SaveChanges();


                    //發通知給被揪(主揪刪被揪時)
                    if (userGuid != memberView.UserGuid)
                    {
                        int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "房間退出").NotificationTypeId;

                        var notification = new Notification
                        {
                            Status = true,
                            IsRead = false,
                            Sender = user.UserId,
                            Receiver = memberDel.UserId,
                            NotificationTypeId = typeId,
                            InitDate = DateTime.Now,
                            RoomGuid = memberView.RoomGuid
                        };

                        _db.Notifications.Add(notification);
                        _db.SaveChanges();
                    }

                    return Ok(new { message = "刪除成功" });
                }
                else
                {
                    return BadRequest("權限不足");
                }
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }




        /// <summary>
        ///     主揪刪除房間
        /// </summary>
        /// <param name="roomGuid">房間Guid</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("{roomGuid}")]
        public IHttpActionResult RoomName([FromUri] string roomGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            //房主才有權限刪房間
            var roomOwner = _db.RoomMembers.Where(r => r.User.UserGuid == userGuid && r.Room.RoomGuid == roomGuid && r.Permission == 1).FirstOrDefault();

            if (roomOwner != null)
            {
                var room = _db.Rooms.Where(r => r.RoomGuid == roomGuid).FirstOrDefault();
                room.Status = false;
                _db.SaveChanges();

                return Ok(new { message = "刪除成功" });
            }
            else
            {
                return BadRequest("非該房間房主");
            }
        }


        /// <summary>
        ///     主揪,被揪加景點進房間前需要get所在的房間
        /// </summary>
        [HttpGet]
        [JwtAuthFilter]
        [Route("getRooms/{attractionId}")]
        public IHttpActionResult GetRooms([FromUri] int attractionId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var result = _db.RoomMembers.Where(r => r.User.UserGuid == userGuid && r.Room.Status == true).Select(r => new
            {
                r.Room.RoomGuid,
                r.Room.RoomName,
                IsExisted = _db.RoomAttractions.FirstOrDefault(a => a.RoomId == r.RoomId && a.AttractionId == attractionId) != null ? true : false
            }).ToList();

            if (result.Any())
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("沒有任何房間");
            }
        }






        /// <summary>
        ///     主揪,被揪加景點進房間
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("addAttractions")]
        public IHttpActionResult AttractionAdd(AttractionAddView addView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var inRoom = _db.RoomMembers.Where(r => r.Room.RoomGuid == addView.RoomGuid && r.User.UserGuid == userGuid).FirstOrDefault();

            if (inRoom != null)
            {
                var hadAttraction = _db.RoomAttractions.Where(a => a.Room.RoomGuid == addView.RoomGuid && a.AttractionId == addView.AttractionId).FirstOrDefault();

                if (hadAttraction == null)
                {
                    RoomAttraction roomAttraction = new RoomAttraction();
                    roomAttraction.RoomId = _db.Rooms.Where(r => r.RoomGuid == addView.RoomGuid).Select(r => r.RoomId).FirstOrDefault();
                    roomAttraction.AttractionId = addView.AttractionId;
                    roomAttraction.UserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
                    roomAttraction.AttrOrder = 0;

                    _db.RoomAttractions.Add(roomAttraction);
                    _db.SaveChanges();

                    var result = new
                    {
                        Message = "景點新增成功",
                        AttractionId = addView.AttractionId
                    };

                    return Ok(result);
                }
                else
                {
                    return BadRequest("此景點已在房間內");
                }
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }
    }
}
