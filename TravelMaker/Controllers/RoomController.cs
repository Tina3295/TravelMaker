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
        public IHttpActionResult RoomContent([FromUri] string roomGuid)
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
    }
}
