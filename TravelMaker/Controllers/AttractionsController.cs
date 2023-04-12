using System;
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

            }).Distinct().OrderByDescending(a => a.AverageScore).Skip(pageSize * (page - 1)).Take(pageSize).ToDictionary(a=>a.AttractionId,a=>a.AverageScore);

            var result = _db.Attractions.Where(a => searchAttractions.Keys.Contains(a.AttractionId)).ToList().Select(a => new
            {
                IsCollect = _db.AttractionCollections.FirstOrDefault(c => c.AttractionId == a.AttractionId && c.UserId == myUserId) == null ? false : true,
                AttractionId = a.AttractionId,
                AttractionName = a.AttractionName,
                CityDistrict = a.District.City.CittyName + " " + a.District.DistrictName,
                AverageScore = searchAttractions[a.AttractionId],
                Category = _db.CategoryAttractions.Where(c => c.AttractionId == a.AttractionId && c.CategoryId != 8 && c.CategoryId != 9).Select(c => c.Category.CategoryName).DefaultIfEmpty("餐廳").ToList(),
                ImageUrl = imgPath + _db.Images.FirstOrDefault(i => i.AttractionId == a.AttractionId).ImageName
            })
                .OrderByDescending(a=>a.AverageScore);


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
                    InitDate = Tool.CommentTime((DateTime)c.InitDate) + c.EditDate == null ? "" : " (已編輯)"
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




        /// <summary>
        ///     新增單一景點評論
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("comments/add")]
        public IHttpActionResult AddAttractionComments(AddAttractionView view)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var attraction = _db.Attractions.FirstOrDefault(a => a.AttractionId == view.AttractionId && a.OpenStatus == true);
            if (attraction != null)
            {
                if (string.IsNullOrWhiteSpace(view.Comment) || view.Comment.Length > 500)
                {
                    return BadRequest("評論不得空白或超過500字");
                }
                if (view.Score == 0)
                {
                    return BadRequest("請選擇星數");
                }

                AttractionComment comment = new AttractionComment();
                comment.AttractionId = view.AttractionId;
                comment.UserId = userId;
                comment.Comment = view.Comment;
                comment.Score = view.Score;
                comment.Status = true;
                comment.InitDate = DateTime.Now;

                _db.AttractionComments.Add(comment);
                _db.SaveChanges();

                return Ok(new { AttractionCommentId = comment.AttractionCommentId });
            }
            else
            {
                return BadRequest("無此景點");
            }
        }




        /// <summary>
        ///     編輯評論
        /// </summary>
        [HttpPut]
        [JwtAuthFilter]
        [Route("comments/edit")]
        public IHttpActionResult EditAttractionComments(EditAttractionCommentsView view)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var originComment = _db.AttractionComments.FirstOrDefault(a=>a.AttractionCommentId == view.AttractionCommentId && a.UserId==userId);

            if (originComment != null)
            {
                if (originComment.Status == true)
                {
                    if (string.IsNullOrWhiteSpace(view.Comment) || view.Comment.Length > 500)
                    {
                        return BadRequest("評論不得空白或超過500字");
                    }
                    if (view.Score == 0)
                    {
                        return BadRequest("請選擇星數");
                    }

                    originComment.Comment = view.Comment;
                    originComment.Score = view.Score;
                    originComment.EditDate = DateTime.Now;
                    _db.SaveChanges();

                    return Ok("評論已修改");
                }
                else
                {
                    return BadRequest("此評論已刪除");
                }
            }
            else
            {
                return BadRequest("無法編輯此評論");
            }
        }




        /// <summary>
        ///     刪除評論
        /// </summary>
        /// <param name="attractionCommentId">要刪除的評論Id</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("comments/{attractionCommentId}")]
        public IHttpActionResult RemoveAttractionComments([FromUri] int attractionCommentId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var originComment = _db.AttractionComments.FirstOrDefault(a=>a.AttractionCommentId == attractionCommentId && a.UserId == userId);

            if (originComment != null)
            {
                if (originComment.Status == true)
                {
                    originComment.Status = false;
                    _db.SaveChanges();

                    return Ok("評論刪除成功");
                }
                else
                {
                    return BadRequest("此評論已刪除");
                }
            }
            else
            {
                return BadRequest("無法刪除此評論");
            }
        }
    }
}
