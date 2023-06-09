﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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
        public IHttpActionResult AttractionsSearch([FromUri]SearchViewModel view)
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

            if (view.District != null)
            {
                temp = temp.Where(a => view.District.Contains(a.District.DistrictName));
            }

            if (!string.IsNullOrEmpty(view.Keyword))
            {
                temp = temp.Where(a => a.AttractionName.Contains(view.Keyword) || a.Introduction.Contains(view.Keyword) || a.Address.Contains(view.Keyword));
            }

            if (view.Type!= null)
            {
                var attractions = _db.CategoryAttractions.Where(a => view.Type.Contains( a.Category.CategoryName )).Select(a => a.AttractionId).Distinct().ToList();

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
                int averageScoreRound = (int)Math.Round(averageScore,1);

                return new
                {
                    AttractionId = a.AttractionId,
                    AverageScore = averageScoreRound
                };

            }).Distinct().OrderByDescending(a => a.AverageScore).Skip(pageSize * (view.Page - 1)).Take(pageSize).ToDictionary(a=>a.AttractionId,a=>a.AverageScore);

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

            var attraction = _db.Attractions.FirstOrDefault(a => a.AttractionId == attractionId && a.OpenStatus == true);

            if (attraction != null)
            {
                var attractionData = new
                {
                    IsCollect = _db.AttractionCollections.Where(c => c.AttractionId == attraction.AttractionId).Any(c => c.UserId == myUserId) ? true : false,
                    AttractionId = attraction.AttractionId,
                    AttractionName = attraction.AttractionName,
                    Introduction = attraction.Introduction,
                    Address=attraction.Address,
                    Tel = attraction.Tel,
                    Email = attraction.Email,
                    OfficialSite = attraction.OfficialSite,
                    Facebook = attraction.Facebook,
                    OpenTime = attraction.OpenTime,
                    ImageUrl = _db.Images.Where(i => i.AttractionId == attractionId).Select(i => imgPath + i.ImageName).ToList()
                };


                //10則高->低評論，若有登入自己評論置頂
                if (Request.Headers.Authorization != null)
                {
                    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                    string userGuid = (string)userToken["UserGuid"];
                    myUserId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;
                }

                var comments = _db.AttractionComments
                    .Where(c => c.AttractionId == attraction.AttractionId && c.Status == true)
                    .OrderByDescending(c => c.UserId == myUserId)
                    .ThenByDescending(c => c.InitDate).Take(10).ToList().Select(c => new
                    {
                        AttractionCommentId = c.AttractionCommentId,
                        IsMyComment = c.UserId == myUserId,
                        UserGuid = c.User.UserGuid,
                        UserName = c.User.UserName,
                        ProfilePicture = c.User.ProfilePicture == null ? "" : profilePath + c.User.ProfilePicture,
                        Score = c.Score,
                        Comment = c.Comment,
                        InitDate = Tool.CommentTime((DateTime)c.InitDate) + (c.EditDate == null ? "" : " (已編輯)")
                    });

                //平均星數
                double averageScore = comments.Any() ? Math.Round(_db.AttractionComments.Where(c => c.AttractionId == attraction.AttractionId && c.Status == true).Select(c => c.Score).Average(), 1) : 0;


                //更多附近景點*3
                DbGeography location = _db.Attractions.FirstOrDefault(a => a.AttractionId == attractionId).Location;

                var moreAttractions = _db.Attractions.Where(a => a.Location.Distance(location) < 1000 && a.AttractionId != attractionId).Take(3).ToList().Select(a => new
                {
                    AttractionId = a.AttractionId,
                    AttractionName = a.AttractionName,
                    City = a.District.City.CittyName + "  距離" + Math.Round((double)a.Location.Distance(location)) + "公尺",
                    ImageUrl = imgPath + _db.Images.Where(i => i.AttractionId == a.AttractionId).Select(i => i.ImageName).FirstOrDefault(),
                    IsColeect = _db.AttractionCollections.Where(c => c.AttractionId == a.AttractionId).Any(c => c.UserId == myUserId) ? true : false
                });


                var result = new
                {
                    AttractionData = attractionData,
                    CommentData = new {
                        AverageScore=averageScore,
                        Comments=comments
                    },
                    MoreAttractions= moreAttractions
                };

                return Ok(result);
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
                //如果有登入，自己的評論會在最上面
                if (Request.Headers.Authorization != null)
                {
                    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                    string userGuid = (string)userToken["UserGuid"];
                    myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
                }


                // 先依照分數高到低或低到高或時間排序
                if (view.Order == "higher")
                {
                    allComments = allComments.OrderByDescending(c => c.UserId == myUserId).ThenByDescending(c => c.Score).ThenByDescending(c => c.InitDate).ToList();
                }
                else if (view.Order == "lower")
                {
                    allComments = allComments.OrderByDescending(c => c.UserId == myUserId).ThenBy(c => c.Score).ThenByDescending(c => c.InitDate).ToList();
                }
                else
                {
                    allComments = allComments.OrderByDescending(c => c.UserId == myUserId).ThenByDescending(c => c.InitDate).ToList();
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
                    InitDate = Tool.CommentTime((DateTime)c.InitDate) + (c.EditDate == null ? "" : " (已編輯)")
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





        /// <summary>
        ///     【維護】變更景點營業狀態
        /// </summary>
        /// <param name="attractionId">景點Id</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("status/{attractionId}")]
        public IHttpActionResult ChangeStatus([FromUri] int attractionId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            var user = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            if (user != null && (user.Permission == 1 || user.Permission == 2))
            {
                var attraction = _db.Attractions.FirstOrDefault(a => a.AttractionId == attractionId);
                if (attraction != null)
                {
                    if (attraction.OpenStatus == true)
                    {
                        attraction.OpenStatus = false;
                        _db.SaveChanges();
                        return Ok(attraction.AttractionName + " 營業狀態已關閉");
                    }
                    else
                    {
                        attraction.OpenStatus = true;
                        _db.SaveChanges();
                        return Ok(attraction.AttractionName + " 營業狀態已開啟");
                    }
                }
                else
                {
                    return BadRequest("無此景點");
                }
            }
            else
            {
                return BadRequest("您沒有管理權限");
            }
        }





        /// <summary>
        ///     【維護】修改景點資訊
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("edit")]
        public async Task<IHttpActionResult> AttractionEdit()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            var user = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            if (user != null && (user.Permission == 1 || user.Permission == 2))
            {
                try
                {
                    var provider = new MultipartMemoryStreamProvider();
                    try
                    {
                        await Request.Content.ReadAsMultipartAsync(provider);
                    }
                    catch
                    {
                        return BadRequest("檔案超過限制大小");
                    }

                    //檢查非圖檔
                    var images = provider.Contents.Where(c => c.Headers.ContentDisposition.Name == "\"Image\""); ;
                    if (images.Any())
                    {
                        foreach (var image in images)
                        {
                            try
                            {
                                string fileName = image.Headers.ContentDisposition.FileName.Trim('"');

                                if (!Tool.IsImage(fileName))
                                {
                                    return BadRequest("上傳的檔案必須為圖片檔");
                                }
                            }
                            catch
                            {
                                return BadRequest("圖片上傳失敗");
                            }
                        }
                    }


                    // 取出json
                    var attData = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name == "\"AttractionData\"");
                    if (attData == null)
                    {
                        return BadRequest("景點更新內容缺失");
                    }
                    var attJson = await attData.ReadAsStringAsync();
                    var attraction = JsonConvert.DeserializeObject<AttEditView>(attJson);


                    var originAtt = _db.Attractions.FirstOrDefault(a => a.AttractionId == attraction.AttractionId);

                    //檢查是否有此景點
                    if (originAtt == null)
                    {
                        return BadRequest("無此景點Id");
                    }
                    else if (!ModelState.IsValid)
                    {
                        var errorMessages = string.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        return BadRequest(errorMessages);
                    }

                    //修改景點內容
                    originAtt.AttractionName = attraction.AttractionName;
                    originAtt.Introduction = attraction.Introduction;

                    string [] cityDistrict = attraction.District.Replace("台","臺").Split(' ');
                    string city = cityDistrict[0];
                    string district = cityDistrict[1];
                    int districtId = _db.Districts.FirstOrDefault(d => d.DistrictName == district && d.City.CittyName == city).DistrictId;

                    if(districtId==0)
                    {
                        return BadRequest("城市區域錯誤");
                    }

                    originAtt.DistrictId = districtId;
                    originAtt.Address = attraction.Address;
                    originAtt.Tel = attraction.Tel;
                    originAtt.Email = attraction.Email;

                    originAtt.Elong = attraction.Elong;
                    originAtt.Nlat = attraction.Nlat;
                    try
                    {
                        originAtt.Location = DbGeography.FromText($"POINT({attraction.Elong} {attraction.Nlat})");
                    }
                    catch
                    {
                        return BadRequest("經緯度有誤");
                    }
                    originAtt.OfficialSite = attraction.OfficialSite;
                    originAtt.Facebook = attraction.Facebook;
                    originAtt.OpenTime = attraction.OpenTime;



                    //類別
                    if (!attraction.Category.All(c => _db.Categories.Any(n => n.CategoryName == c)))
                    {
                        return BadRequest("類別名稱有誤");
                    }

                    //移除舊類別
                    var ca = _db.CategoryAttractions.Where(c => c.AttractionId == attraction.AttractionId);
                    _db.CategoryAttractions.RemoveRange(ca);

                    //加入新類別
                    _db.CategoryAttractions.AddRange(attraction.Category.Select(c => new CategoryAttraction
                    {
                        AttractionId = attraction.AttractionId,
                        CategoryId = _db.Categories.FirstOrDefault(x => x.CategoryName == c).CategoryId
                    }));



                    //刪除原本的景點照片
                    var originImages = _db.Images.Where(i => i.AttractionId == attraction.AttractionId);
                    _db.Images.RemoveRange(originImages);

                    //更新上傳的景點照片
                    _db.Images.AddRange(attraction.ImageNames.Select(n => new Image
                    {
                        AttractionId = attraction.AttractionId,
                        ImageName = n
                    }));



                    // 儲存景點照片檔案
                    string imgPath = HttpContext.Current.Server.MapPath(@"~/Upload/AttractionImage");

                    if (images.Any())
                    {
                        foreach (var image in images)
                        {
                            try
                            {
                                var imageBytes = await image.ReadAsByteArrayAsync();
                                string fileName = image.Headers.ContentDisposition.FileName.Trim('"');
                                var outputPath = Path.Combine(imgPath, fileName);
                                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                                {
                                    await output.WriteAsync(imageBytes, 0, imageBytes.Length);
                                }
                            }
                            catch
                            {
                                return BadRequest("圖片上傳失敗");
                            }
                        }
                    }
                    _db.SaveChanges();

                    return Ok("已儲存編輯內容");
                }
                catch (Exception e)
                {

                    return BadRequest(e.Message);
                }
            }
            else
            {
                return BadRequest("您沒有編輯權限");
            }
        }




        /// <summary>
        ///     【維護】新增景點
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("add")]
        public async Task<IHttpActionResult> AttractionAdd()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            var user = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            if (user != null && (user.Permission == 1 || user.Permission == 2))
            {
                try
                {
                    var provider = new MultipartMemoryStreamProvider();
                    try
                    {
                        await Request.Content.ReadAsMultipartAsync(provider);
                    }
                    catch
                    {
                        return BadRequest("檔案超過限制大小");
                    }

                    //檢查非圖檔
                    var images = provider.Contents.Where(c => c.Headers.ContentDisposition.Name == "\"Image\""); ;
                    if (images.Any())
                    {
                        foreach (var image in images)
                        {
                            try
                            {
                                string fileName = image.Headers.ContentDisposition.FileName.Trim('"');

                                if (!Tool.IsImage(fileName))
                                {
                                    return BadRequest("上傳的檔案必須為圖片檔");
                                }
                            }
                            catch
                            {
                                return BadRequest("圖片上傳失敗");
                            }
                        }
                    }


                    // 取出json
                    var attData = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name == "\"AttractionData\"");
                    if (attData == null)
                    {
                        return BadRequest("景點更新內容缺失");
                    }
                    var attJson = await attData.ReadAsStringAsync();
                    var attraction = JsonConvert.DeserializeObject<AttEditView>(attJson);

                    //檢查有無重複名字的景點
                    var hadName = _db.Attractions.FirstOrDefault(a => a.AttractionName == attraction.AttractionName);

                    //檢查是否有此景點
                    if (hadName != null)
                    {
                        return BadRequest("景點名字重複");
                    }
                    else if (!ModelState.IsValid)
                    {
                        var errorMessages = string.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        return BadRequest(errorMessages);
                    }

                    //新增景點內容
                    Attraction newAttraction = new Attraction();
                    newAttraction.AttractionName = attraction.AttractionName;
                    newAttraction.Introduction = attraction.Introduction;
                    newAttraction.OpenStatus = true;

                    string[] cityDistrict = attraction.District.Replace("台", "臺").Split(' ');
                    string city = cityDistrict[0];
                    string district = cityDistrict[1];
                    int districtId = _db.Districts.FirstOrDefault(d => d.DistrictName == district && d.City.CittyName == city).DistrictId;
                    if (districtId == 0)
                    {
                        return BadRequest("城市區域錯誤");
                    }
                    newAttraction.DistrictId = districtId;

                    newAttraction.Address = attraction.Address;
                    newAttraction.Tel = attraction.Tel;
                    newAttraction.Email = attraction.Email;

                    newAttraction.Elong = attraction.Elong;
                    newAttraction.Nlat = attraction.Nlat;
                    try
                    {
                        newAttraction.Location = DbGeography.FromText($"POINT({attraction.Elong} {attraction.Nlat})");
                    }
                    catch
                    {
                        return BadRequest("經緯度有誤");
                    }
                    newAttraction.OfficialSite = attraction.OfficialSite;
                    newAttraction.Facebook = attraction.Facebook;
                    newAttraction.OpenTime = attraction.OpenTime;
                    newAttraction.InitDate = DateTime.Now;

                    _db.Attractions.Add(newAttraction);
                    _db.SaveChanges();

                    //類別
                    if (!attraction.Category.All(c => _db.Categories.Any(n => n.CategoryName == c)))
                    {
                        return BadRequest("類別名稱有誤");
                    }

                    //加入類別
                    _db.CategoryAttractions.AddRange(attraction.Category.Select(c => new CategoryAttraction
                    {
                        AttractionId = newAttraction.AttractionId,
                        CategoryId = _db.Categories.FirstOrDefault(x => x.CategoryName == c).CategoryId
                    }));


                    //加入景點照片
                    _db.Images.AddRange(attraction.ImageNames.Select(n => new Image
                    {
                        AttractionId = newAttraction.AttractionId,
                        ImageName = n
                    }));

                    // 儲存景點照片檔案
                    string imgPath = HttpContext.Current.Server.MapPath(@"~/Upload/AttractionImage");

                    if (images.Any())
                    {
                        foreach (var image in images)
                        {
                            try
                            {
                                var imageBytes = await image.ReadAsByteArrayAsync();
                                string fileName = image.Headers.ContentDisposition.FileName.Trim('"');
                                var outputPath = Path.Combine(imgPath, fileName);
                                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                                {
                                    await output.WriteAsync(imageBytes, 0, imageBytes.Length);
                                }
                            }
                            catch
                            {
                                return BadRequest("圖片上傳失敗");
                            }
                        }
                    }
                    _db.SaveChanges();

                    return Ok("景點已新增");
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            else
            {
                return BadRequest("您沒有權限新增景點");
            }
        }
    }
}
