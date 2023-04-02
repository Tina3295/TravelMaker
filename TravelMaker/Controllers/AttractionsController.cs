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
    ///     景點相關
    /// </summary>
    [RoutePrefix("api/attractions")]
    public class AttractionsController : ApiController
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext();


        /// <summary>
        ///     收藏景點
        /// </summary>
        /// <param name="attractionId">景點Id</param>
        /// <returns></returns>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{attractionId}/collect")]
        public IHttpActionResult CollectAdd([FromUri]int attractionId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();

            AttractionCollection attractionCollection = new AttractionCollection();
            attractionCollection.AttractionId = attractionId;
            attractionCollection.UserId = userId;
            _db.AttractionCollections.Add(attractionCollection);
            _db.SaveChanges();
            return Ok(new { Message = "已加入收藏" });
        }




        /// <summary>
        ///     取消收藏景點
        /// </summary>
        /// <param name="attractionId">景點Id</param>
        /// <returns></returns>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("{attractionId}/collect")]
        public IHttpActionResult CollectRemove([FromUri] int attractionId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();

            var collection = _db.AttractionCollections.Where(a => a.UserId == userId && a.AttractionId == attractionId).FirstOrDefault();
            _db.AttractionCollections.Remove(collection);
            _db.SaveChanges();
            return Ok(new { Message = "已取消收藏" });
        }
    }
}
