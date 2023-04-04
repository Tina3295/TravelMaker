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





        /// <summary>
        ///     給參數搜尋景點(熱門話題取得所有景點)
        /// </summary>
        [HttpGet]
        [Route("search")]
        public IHttpActionResult AttractionsSearch(string type = "", string district = "", string keyword = "", int page = 1)
        {
            int pageSize = 9;
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string userGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            }


            //景點搜尋篩選
            var temp = _db.Attractions.Where(a => a.OpenStatus == true).AsQueryable();

            if (!string.IsNullOrEmpty(district))
            {
                temp = temp.Where(a => a.District.DistrictName == district);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                temp = temp.Where(a => a.AttractionName.Contains(keyword) || a.Introduction.Contains(keyword) || a.Address.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(type))
            {
                var attractions = _db.CategoryAttractions.Where(a => a.Category.CategoryName == type).Select(a => a.AttractionId).Distinct().ToList();

                temp = temp.Where(a => attractions.Contains(a.AttractionId));
            }

            //符合搜尋結果的總項目
            int totalItem = temp.Select(a => a.AttractionId).Distinct().Count();
            //總頁數
            int totalPages = totalItem % pageSize == 0 ? totalItem / pageSize : totalItem / pageSize + 1;
            //該頁顯示項目
            var searchAttractions = temp.ToList().Select(a =>
            {
                // 景點評分計算
                var scores = _db.AttractionComments.Where(c => c.Status == true && c.AttractionId == a.AttractionId).Select(c => c.Score);
                double averageScore = scores.Any() ? scores.Average() : 0;
                int averageScoreRound = (int)Math.Round(averageScore);

                return new
                {
                    AttractionId = a.AttractionId,
                    AverageScore = averageScoreRound
                };

            }).Distinct().OrderByDescending(a => a.AverageScore).Skip(pageSize * (page - 1)).Take(pageSize);

            var result = new List<object>();

            foreach (var searchAttraction in searchAttractions)
            {
                AttractionView attraction = new AttractionView();
                attraction.AttractionId = searchAttraction.AttractionId;
                attraction.AttractionName = _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).Select(a => a.AttractionName).FirstOrDefault();

                attraction.CityDistrict = _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).Select(a => a.District.City.CittyName).FirstOrDefault()
                    + " " + _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).Select(a => a.District.DistrictName).FirstOrDefault();

                attraction.AverageScore = searchAttraction.AverageScore;

                attraction.Category = _db.CategoryAttractions.Where(c => c.AttractionId == attraction.AttractionId && c.CategoryId != 8 && c.CategoryId != 9).Select(c => c.Category.CategoryName).ToList();
                if (attraction.Category.Count == 0)
                {
                    attraction.Category.Add("餐廳");
                }

                attraction.ImageUrl = imgPath + _db.Images.Where(i => i.AttractionId == attraction.AttractionId).Select(i => i.ImageName).FirstOrDefault();


                if (myUserId != 0)
                {
                    attraction.IsCollect = _db.AttractionCollections.Where(a => a.AttractionId == attraction.AttractionId).Any(a => a.UserId == myUserId) ? true : false;
                }
                else
                {
                    attraction.IsCollect = false;
                }

                result.Add(attraction);
            }



            if (totalItem != 0)
            {
                return Ok(new { TotalPages = totalPages, TotalItem = totalItem, Attractions = result });
            }
            else
            {
                return BadRequest("尚無符合搜尋條件的景點");
            }
        }
    }
}
