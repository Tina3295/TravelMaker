using System;
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
    [RoutePrefix("api/room")]
    public class RoomController : ApiController
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
            string userGuuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();

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
            roomMember.RoomId = _db.Rooms.Where(r => r.RoomGuid==room.RoomGuid).Select(r => r.RoomId).FirstOrDefault();
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
                roomAttraction.AttrOrder=i;
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
            if(hadRoom!=null)
            {
                if(hadRoom.Status==true)
                {
                    RoomContentView roomContent = new RoomContentView();
                    roomContent.RoomGuid = roomGuid;
                    roomContent.RoomName = hadRoom.RoomName;

                    int createrId = _db.RoomMembers.Where(r => r.RoomId == hadRoom.RoomId && r.Permission == 1).Select(r => r.UserId).FirstOrDefault();
                    roomContent.CreaterGuid = _db.Users.Where(u => u.UserId == createrId).Select(u => u.UserGuid).FirstOrDefault();

                    //房間用戶
                    string savePath= "https://" + Request.RequestUri.Host + "/upload/profilePicture/";
                    var userIds = _db.RoomMembers.Where(r => r.RoomId == hadRoom.RoomId).Select(r => r.UserId);

                    roomContent.Users = new List<object>();
                    foreach (var userId in userIds)
                    {
                        var user = _db.Users.Where(u => u.UserId == userId).Select(u => new
                        {
                            UserGuid = u.UserGuid,
                            UserName=u.UserName,
                            ProfilePicture = u.ProfilePicture == null ? "" : savePath + u.ProfilePicture
                        });

                        roomContent.Users.Add(user);
                    }

                    //房間景點
                    string attrPath = "https://" + Request.RequestUri.Host + "/upload/attractionImage/";

                    roomContent.AttrationsData = new List<object>();
                    var attractions = _db.RoomAttractions.Where(r => r.RoomId == hadRoom.RoomId).ToList();

                    foreach(var attraction in attractions)
                    {
                        var attTemp = _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).FirstOrDefault();

                        AttrationsData attrationsData = new AttrationsData();
                        attrationsData.AttractionId = attraction.AttractionId;
                        attrationsData.UserGuid = _db.Users.Where(u => u.UserId == attraction.UserId).Select(u => u.UserGuid).FirstOrDefault();
                        attrationsData.AttractionName = attTemp.AttractionName;
                        attrationsData.Elong = attTemp.Elong;
                        attrationsData.Nlat = attTemp.Nlat;
                        attrationsData.ImageUrl = attrPath + _db.Images.Where(i => i.AttractionId == attraction.AttractionId).Select(i => i.ImageName).FirstOrDefault();
                        attrationsData.Order = attraction.AttrOrder;

                        roomContent.AttrationsData.Add(attrationsData);
                    }


                    //投票日期!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    int myUserId = 0;
                    if (Request.Headers.Authorization != null)
                    {
                        var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                        string userGuuid = (string)userToken["UserGuid"];
                        myUserId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();
                    }

                    roomContent.VoteDates = new List<object>();
                    var voteDates = _db.VoteDates.Where(v => v.RoomId == hadRoom.RoomId).ToList();

                    foreach (var voteDate in voteDates)
                    {
                        VoteDatesData voteDatesData = new VoteDatesData();
                        voteDatesData.VoteDateId = voteDate.VoteDateId;
                        voteDatesData.Date = voteDate.Date.Year+"-"+ voteDate.Date.Month+"-"+ voteDate.Date.Day;
                        voteDatesData.Count = _db.Votes.Where(v => v.VoteDateId == voteDate.VoteDateId).Count();
                        voteDatesData.IsVoted = _db.Votes.Where(v => v.VoteDateId == voteDate.VoteDateId).Any(v => v.UserId == myUserId) ? true : false;

                        roomContent.VoteDates.Add(voteDatesData);
                    }

                    return Ok(roomContent);
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
        /// 
        /// </summary>
        /// <param name="roomGuid">房間識別碼</param>
        /// <param name="RoomName">新房間名稱</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("{roomGuid}/name")]
        public IHttpActionResult RoomName([FromUri] string roomGuid,[FromBody] string RoomName)
        {
            //房客才可以修改名字
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();
            int roomId = _db.Rooms.Where(r => r.RoomGuid == roomGuid).Select(r => r.RoomId).FirstOrDefault();
            var inRoom = _db.RoomMembers.Where(r => r.UserId == userId && r.RoomId == roomId).FirstOrDefault();

            if (inRoom != null)
            {
                var room = _db.Rooms.Where(r => r.RoomId == roomId).FirstOrDefault();
                room.RoomName = RoomName;
                _db.SaveChanges();

                return Ok("修改成功");
            }
            else
            {
                return BadRequest("非該房間成員");
            }
        }










        /// <summary>
        ///     房客編輯(儲存)房間-房間資訊(行程)
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("modify")]
        public IHttpActionResult RoomModify(RoomModifyView roomModify)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            
            //是否為該房間房客
            int roomId = _db.Rooms.Where(r => r.RoomGuid == roomModify.RoomGuid).Select(r=>r.RoomId).FirstOrDefault();
            var inRoom = _db.RoomMembers.Where(r => r.RoomId==roomId && r.User.UserGuid == userGuid).FirstOrDefault();

            if (inRoom!=null)
            {
                //原本正式景點數
                int originCount = _db.RoomAttractions.Where(r => r.RoomId==roomId).Select(r => r.AttrOrder).Max();
                int x=-1;

                //先刪掉原本的備用景點
                var originAtts = _db.RoomAttractions.Where(r => r.RoomId == roomId && r.AttrOrder == 0);
                foreach (var originAtt in originAtts)
                {
                    _db.RoomAttractions.Remove(originAtt);
                }
                _db.SaveChanges();

                //一一更改正式景點
                foreach (var newAttraction in roomModify.AttrationsData)
                {
                    if (newAttraction.Order != 0) //正式景點
                    {
                        var originAttraction = _db.RoomAttractions.Where(r => r.RoomId==roomId && r.AttrOrder == newAttraction.Order).FirstOrDefault();

                        if(originAttraction!=null) //原本有此次序-修改
                        {
                            originAttraction.AttractionId = newAttraction.AttractionId;
                            originAttraction.UserId = _db.Users.Where(u => u.UserGuid == newAttraction.UserGuid).Select(u => u.UserId).FirstOrDefault();
                        }
                        else //原本無此次序-新增
                        {
                            RoomAttraction roomAttraction = new RoomAttraction();
                            roomAttraction.RoomId = roomId;
                            roomAttraction.AttractionId = newAttraction.AttractionId;
                            roomAttraction.UserId = _db.Users.Where(u => u.UserGuid == newAttraction.UserGuid).Select(u => u.UserId).FirstOrDefault();
                            roomAttraction.AttrOrder = newAttraction.Order;
                            _db.RoomAttractions.Add(roomAttraction);
                        }
                        _db.SaveChanges();


                        //記錄正式景點數 後是否<前
                        if (newAttraction.Order>x)
                        {
                            x = newAttraction.Order;
                        }
                    }
                    else //加入備用景點
                    {
                        RoomAttraction roomAttraction = new RoomAttraction();
                        roomAttraction.RoomId = roomId;
                        roomAttraction.AttractionId = newAttraction.AttractionId;
                        roomAttraction.UserId = _db.Users.Where(u => u.UserGuid == newAttraction.UserGuid).Select(u => u.UserId).FirstOrDefault();
                        roomAttraction.AttrOrder = 0;
                        _db.RoomAttractions.Add(roomAttraction);

                        _db.SaveChanges();
                    } 
                }



                //正式景點數前>後 需刪除!
                if (originCount>x)
                {
                    for(int i=x+1;i<=originCount ;i++)
                    {
                        var deleteAtt = _db.RoomAttractions.Where(r => r.Room.RoomGuid == roomModify.RoomGuid && r.AttrOrder == i).FirstOrDefault();

                        _db.RoomAttractions.Remove(deleteAtt);
                    }
                }
                _db.SaveChanges();

                return Ok("房間景點修改成功");
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
        [Route("date")]
        public IHttpActionResult VoteDateAdd(DateView dateView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //是房客才可以新增
            var inRoom = _db.RoomMembers.Where(r => r.Room.RoomGuid==dateView.RoomGuid && r.UserId==userId).FirstOrDefault();

            if (inRoom != null)
            {
                //處理日期傳入格式為字串ex.2023-5-3
                string[] tempDate = dateView.Date.Split('-');
                DateTime date = new DateTime(Convert.ToInt32(tempDate[0]), Convert.ToInt32(tempDate[1]), Convert.ToInt32(tempDate[2]));

                //日期有無重複
                var hadDate = _db.VoteDates.Where(d => d.Room.RoomGuid == dateView.RoomGuid && d.Date == date).FirstOrDefault();
                if(hadDate==null)
                {
                    VoteDate voteDate = new VoteDate();
                    voteDate.RoomId = _db.Rooms.Where(r => r.RoomGuid == dateView.RoomGuid).Select(r => r.RoomId).FirstOrDefault();
                    voteDate.Date = date;
                    voteDate.UserId = userId;
                    _db.VoteDates.Add(voteDate);
                    _db.SaveChanges();

                    return Ok("日期新增成功");
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
        //[HttpDelete]
        //[JwtAuthFilter]
        //[Route("date")]
        //public IHttpActionResult VoteDateDelete(DateView dateView)
        //{
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
        //    string userGuid = (string)userToken["UserGuid"];
        //    int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

        //    //是房客才可以新增
        //    var inRoom = _db.RoomMembers.Where(r => r.Room.RoomGuid == dateView.RoomGuid && r.UserId == userId).FirstOrDefault();

        //    if (inRoom != null)
        //    {
        //        //處理日期傳入格式為字串ex.2023-5-3
        //        string[] tempDate = dateView.Date.Split('-');
        //        DateTime date = new DateTime(Convert.ToInt32(tempDate[0]), Convert.ToInt32(tempDate[1]), Convert.ToInt32(tempDate[2]));
        //        var hadVoteDate = _db.VoteDates.Where(r => r.Room.RoomGuid == dateView.RoomGuid && r.Date == date).FirstOrDefault();

        //        if (inRoom.Permission==1 || hadVoteDate.UserId == userId) //房主或提出該日期的房客可以刪除
        //        {
        //            //先刪投票的人!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!






        //            _db.VoteDates.Remove(hadVoteDate);
        //            _db.SaveChanges();

        //            return Ok("刪除成功");
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest("非該房間成員");
        //    }
            
        //}




    }
}
