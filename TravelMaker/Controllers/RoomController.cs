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



        ///// <summary>
        /////     取得單一房間所有資訊
        ///// </summary>
        //[HttpGet]
        //[Route("{roomGuid}")]
        //public IHttpActionResult RoomContent([FromUri] string roomGuid)
        //{

        //}
    }
}
