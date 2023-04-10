﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
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
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var collection = _db.AttractionCollections.Where(a => a.UserId == userId && a.AttractionId == attractionId).FirstOrDefault();
            if(collection==null)
            {
                AttractionCollection attractionCollection = new AttractionCollection();
                attractionCollection.AttractionId = attractionId;
                attractionCollection.UserId = userId;
                _db.AttractionCollections.Add(attractionCollection);
                _db.SaveChanges();
                return Ok(new { Message = "加入收藏成功" });
            }
            else
            {
                return BadRequest("此景點已加過收藏");
            }
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
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var collection = _db.AttractionCollections.Where(a => a.UserId == userId && a.AttractionId == attractionId).FirstOrDefault();
            if (collection != null)
            {
                _db.AttractionCollections.Remove(collection);
                _db.SaveChanges();
            }
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




        /// <summary>
        ///     取得單一景點資訊
        /// </summary>
        /// <param name="attractionId">景點Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{attractionId}")]
        public IHttpActionResult AttractionsInfo([FromUri]int attractionId)
        {
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string userGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            }
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/attractionImage/";
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";

            var hadAttraction = _db.Attractions.Where(a => a.AttractionId == attractionId && a.OpenStatus == true).FirstOrDefault();

            if (hadAttraction != null)
            {
                AttractionInfoView attractionInfo = new AttractionInfoView();

                //景點資訊
                var attraction = _db.Attractions.FirstOrDefault(a => a.AttractionId == attractionId);

                bool isCollect;
                if (myUserId != 0)
                {
                    isCollect = _db.AttractionCollections
                        .Any(c => c.AttractionId == attraction.AttractionId && c.UserId == myUserId);
                }
                else
                {
                    isCollect = false;
                }

                attractionInfo.AttractionData = new AttractionData
                {
                    IsCollect = isCollect,
                    AttractionId = attraction.AttractionId,
                    AttractionName = attraction.AttractionName,
                    Introduction = attraction.Introduction,
                    Address = attraction.Address,
                    Tel = attraction.Tel,
                    Email = attraction.Email,
                    OfficialSite = attraction.OfficialSite,
                    Facebook = attraction.Facebook,
                    OpenTime = attraction.OpenTime,
                    ImageUrl = _db.Images.Where(i => i.AttractionId == attractionId).Select(i => imgPath + i.ImageName).ToList()
                };



                //10則高->低評論
                var hadComment = _db.AttractionComments.Where(c => c.AttractionId == attractionId && c.Status == true).FirstOrDefault();
                CommentData comment = new CommentData();

                if (hadComment!=null)
                {
                    comment.AverageScore = (int)Math.Round(_db.AttractionComments.Where(c => c.AttractionId == attractionId && c.Status == true).Select(c => c.Score).Average());
                    comment.Comments = new List<Comments>();


                    if (myUserId != 0)//如果有登入，自己的評論要在最上面
                    {
                        //用戶自己評論數
                        int myCommentCount = _db.AttractionComments.Where(c => c.AttractionId == attractionId && c.UserId == myUserId && c.Status == true).Count();

                        if (myCommentCount != 0)
                        {
                            var myComments = _db.AttractionComments.Where(c => c.AttractionId == attractionId && c.UserId == myUserId && c.Status == true).OrderByDescending(c => c.Score).ThenByDescending(c => c.InitDate).ToList().Select(c => new Comments
                            {
                                AttractionCommentId = c.AttractionCommentId,
                                IsMyComment = true,
                                UserGuid=c.User.UserGuid,
                                UserName = c.User.UserName,
                                ProfilePicture = c.User.ProfilePicture == null ? "" : profilePath + c.User.ProfilePicture,
                                Score = c.Score,
                                Comment = c.Comment,
                                InitDate = Tool.CommentTime((DateTime)c.InitDate)
                            }).ToList();

                            comment.Comments.AddRange(myComments);
                        }


                        //其他評論數
                        var otherComments = _db.AttractionComments.Where(c => c.AttractionId == attractionId && c.UserId != myUserId && c.Status == true).OrderByDescending(c => c.Score).ThenByDescending(c => c.InitDate).Take(10 - myCommentCount).ToList().Select(c => new Comments
                        {
                            AttractionCommentId = c.AttractionCommentId,
                            IsMyComment=false,
                            UserGuid = c.User.UserGuid,
                            UserName = c.User.UserName,
                            ProfilePicture = c.User.ProfilePicture == null ? "" : profilePath + c.User.ProfilePicture,
                            Score = c.Score,
                            Comment = c.Comment,
                            InitDate = Tool.CommentTime((DateTime)c.InitDate)
                        }).ToList();
                        comment.Comments.AddRange(otherComments);
                    }
                    else //無登入
                    {
                        var allComments = _db.AttractionComments.Where(c => c.AttractionId == attractionId && c.Status == true).OrderByDescending(c => c.Score).ThenByDescending(c=>c.InitDate).Take(10).ToList().Select(c => new Comments
                        {
                            AttractionCommentId = c.AttractionCommentId,
                            IsMyComment = false,
                            UserGuid = c.User.UserGuid,
                            UserName = c.User.UserName,
                            ProfilePicture = c.User.ProfilePicture == null ? "" : profilePath + c.User.ProfilePicture,
                            Score = c.Score,
                            Comment = c.Comment,
                            InitDate = Tool.CommentTime((DateTime)c.InitDate)
                        }).ToList();
                        comment.Comments.AddRange(allComments);
                    }
                }
                else
                {
                    comment.AverageScore = 0;
                }

                attractionInfo.CommentData = comment ;


                //更多附近景點*3
                DbGeography location = _db.Attractions.Where(a => a.AttractionId == attractionId).Select(a => a.Location).FirstOrDefault();

                var moreAttractions = _db.Attractions.Where(a => a.Location.Distance(location) < 1000 && a.AttractionId != attractionId).Take(3).ToList().Select(a =>
                    {
                        if (myUserId != 0)
                        {
                            isCollect = _db.AttractionCollections.Where(c => c.AttractionId == a.AttractionId).Any(c => c.UserId == myUserId) ? true : false;
                        }
                        else
                        {
                            isCollect = false;
                        }


                        return new
                        {
                            AttractionId = a.AttractionId,
                            AttractionName = a.AttractionName,
                            City = a.District.City.CittyName+"  距離"+Math.Round((double)a.Location.Distance(location))+"公尺",
                            ImageUrl = imgPath + _db.Images.Where(i => i.AttractionId == a.AttractionId).Select(i => i.ImageName).FirstOrDefault(),
                            IsColeect = isCollect
                        };
                    });

                attractionInfo.MoreAttractions =  moreAttractions.ToList<object>();

                return Ok(attractionInfo);
            }
            else
            {
                return BadRequest("無此景點頁面");
            }
        }






        /// <summary>
        ///     取得更多景點評論
        /// </summary>
        [HttpPost]
        [Route("comments")]
        public IHttpActionResult MoreComment(MoreCommentView view)
        {
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            int pagesize = 10;
            int myUserId = 0;

            var allComments = _db.AttractionComments.Where(c => c.AttractionId == view.AttractionId && c.Status == true).ToList();

            if (allComments.Any())
            {
                // 先依照分數高到低或低到高排序
                if (view.Order == "higher")
                {
                    allComments = allComments.OrderByDescending(c => c.Score).ThenByDescending(c => c.InitDate).ToList();
                }
                else
                {
                    allComments = allComments.OrderBy(c => c.Score).ThenByDescending(c => c.InitDate).ToList();
                }

                //如果有登入，自己的評論會在最上面
                if (Request.Headers.Authorization != null)
                {
                    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                    string userGuid = (string)userToken["UserGuid"];
                    myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

                    var myComments = allComments.Where(c => c.UserId == myUserId).ToList();
                    allComments = allComments.Except(myComments).ToList();
                    allComments = myComments.Concat(allComments).ToList();
                }

                // 分頁
                var result = allComments.Skip((view.Page - 1) * pagesize).Take(pagesize).ToList().Select(c => new
                {
                    AttractionCommentId = c.AttractionCommentId,
                    IsMyComment = c.UserId == myUserId,
                    UserGuid = c.User.UserGuid,
                    UserName = c.User.UserName,
                    ProfilePicture = c.User.ProfilePicture == null ? "" : profilePath + c.User.ProfilePicture,
                    Score = c.Score,
                    Comment = c.Comment,
                    InitDate = Tool.CommentTime((DateTime)c.InitDate)
                }).ToList();

                if (result.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("無更多景點評論");
                }
            }
            else
            {
                return BadRequest("尚無評論");
            }
        }
    }
}
