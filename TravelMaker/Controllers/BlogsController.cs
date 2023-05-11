using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    ///     遊記相關
    /// </summary>
    [RoutePrefix("api/blogs")]
    public class BlogsController : ApiController
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext();


        /// <summary>
        ///     新增草稿遊記前需要取得所有行程id名字
        /// </summary>
        [HttpGet]
        [JwtAuthFilter]
        [Route("tours")]
        public IHttpActionResult GetToursBeforeBlogDraft()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var result = _db.Tours.Where(t => t.User.UserGuid == userGuid).OrderByDescending(t => t.InitDate).Select(t => new
            {
                t.TourId,
                t.TourName
            }).ToList();

            if (result.Any())
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("沒有任何創建行程");
            }
        }



        /// <summary>
        ///     新增草稿遊記
        /// </summary>
        /// <param name="tourId">行程Id</param>
        /// <returns></returns>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{tourId}")]
        public IHttpActionResult BlogAdd([FromUri] int tourId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            var hadTour = _db.Tours.FirstOrDefault(t => t.TourId == tourId);
            if (hadTour != null)
            {
                Blog blog = new Blog();
                blog.BlogGuid = Guid.NewGuid().ToString().Trim() + DateTime.Now.ToString("ff");
                blog.UserId = userId;
                blog.Title = hadTour.TourName;
                blog.Status = 0;
                blog.PageViewCounts = 0;
                blog.InitDate = DateTime.Now;

                _db.Blogs.Add(blog);
                _db.SaveChanges();

                int blogId = _db.Blogs.Where(b => b.BlogId == blog.BlogId).Select(b => b.BlogId).FirstOrDefault();

                var attractionIds = _db.TourAttractions.Where(a => a.TourId == tourId).Select(a => a.AttractionId).ToList();

                foreach (var attractionId in attractionIds)
                {
                    BlogAttraction attraction = new BlogAttraction();
                    attraction.BlogId = blogId;
                    attraction.AttractionId = attractionId;
                    attraction.InitDate = DateTime.Now;

                    _db.BlogAttractions.Add(attraction);
                }
                _db.SaveChanges();

                return Ok(new { BlogGuid = blog.BlogGuid });
            }
            else
            {
                return BadRequest("無此行程");
            }
        }





        /// <summary>
        ///     取得草稿遊記
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilter]
        [Route("draft/{blogGuid}")]
        public IHttpActionResult GetBlogDraft([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var isMyDraftBlog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.User.UserGuid == userGuid);

            if (isMyDraftBlog != null)
            {
                if (isMyDraftBlog.Status != 2)
                {
                    string blogImagePath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";

                    var blog = _db.Blogs.Where(b => b.BlogGuid == blogGuid).FirstOrDefault();

                    var result = new
                    {
                        Title = blog.Title,
                        Cover = blog.Cover == null ? "" : blogImagePath + blog.Cover,
                        Category = blog.Category == null ? new string[0] : blog.Category.Split(','),
                        Status = blog.Status,
                        BlogAttractionList = _db.BlogAttractions.Where(a => a.Blog.BlogGuid == blogGuid).Select(a => new
                        {
                            AttractionId = a.AttractionId,
                            AttractionName = _db.Attractions.FirstOrDefault(n => n.AttractionId == a.AttractionId).AttractionName,
                            Description = a.Description,
                            ImageUrl = _db.BlogImages.Where(i => i.BlogAttractionId == a.BlogAttractionId).Select(i => blogImagePath + i.ImageName)
                        }).ToList()
                    };

                    return Ok(result);
                }
                else
                {
                    return BadRequest("此遊記已刪除");
                }
            }
            else
            {
                return BadRequest("非此遊記創建者");
            }
        }



        /// <summary>
        ///     新增遊記(發布)
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("release/{blogGuid}")]
        public IHttpActionResult BlogRelease([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var isMyDraftBlog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.User.UserGuid == userGuid);
            if (isMyDraftBlog != null)
            {
                if (isMyDraftBlog.Status == 0)
                {
                    isMyDraftBlog.Status = 1;
                    isMyDraftBlog.InitDate = DateTime.Now;
                    isMyDraftBlog.EditDate = null;
                    _db.SaveChanges();

                    //發通知給粉絲
                    var fans = _db.BlogFollowers.Where(f => f.User.UserGuid == userGuid).ToList();

                    int senderId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;
                    int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "遊記新增").NotificationTypeId;

                    var notifications = fans.Select(fan => new Notification
                    {
                        Status = true,
                        IsRead = false,
                        Sender = senderId,
                        Receiver = fan.FollowingUserId,
                        NotificationTypeId = typeId,
                        InitDate = DateTime.Now,
                        BlogGuid = blogGuid
                    });

                    _db.Notifications.AddRange(notifications);
                    _db.SaveChanges();

                    return Ok("成功發佈");
                }
                else if (isMyDraftBlog.Status == 1)
                {
                    return BadRequest("此遊記已發佈");
                }
                else
                {
                    return BadRequest("此遊記已刪除");
                }
            }
            else
            {
                return BadRequest("非此遊記創建者");
            }
        }



        /// <summary>
        ///     刪除遊記
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("remove/{blogGuid}")]
        public IHttpActionResult BlogRemove([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var isMyDraftBlog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.User.UserGuid == userGuid);
            if (isMyDraftBlog != null)
            {
                if (isMyDraftBlog.Status == 0 || isMyDraftBlog.Status == 1)
                {
                    isMyDraftBlog.Status = 2;
                    _db.SaveChanges();
                    return Ok("成功刪除");
                }
                else
                {
                    return BadRequest("此遊記已刪除");
                }
            }
            else
            {
                return BadRequest("非此遊記創建者");
            }
        }





        /// <summary>
        ///     編輯遊記
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("edit")]
        public async Task<IHttpActionResult> BlogEdit()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            string imgPath = HttpContext.Current.Server.MapPath(@"~/Upload/blogImage");
            //string imgPath = "C:/Users/swps4/source/repos/TravelMaker/TravelMaker/Upload/blogImage/";

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
                var blogData = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition.Name == "\"BlogData\"");
                if (blogData == null)
                {
                    return BadRequest("遊記更新內容缺失");
                }
                var blogJson = await blogData.ReadAsStringAsync();
                var blog = JsonConvert.DeserializeObject<BlogEditView>(blogJson);


                var originBlog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blog.BlogGuid && b.User.UserGuid == userGuid);

                //檢查是否擁有此篇遊記
                if (originBlog == null)
                {
                    return BadRequest("非此遊記創建者");
                }

                //更新遊記內容
                originBlog.Title = blog.Title;
                originBlog.Category = blog.Category == null ? null : string.Join(",", blog.Category);
                originBlog.Cover = blog.Cover;
                originBlog.EditDate = DateTime.Now;

                //刪除原本的景點照片、景點
                _db.BlogImages.Where(b => b.BlogAttraction.Blog.BlogGuid == blog.BlogGuid).ToList().ForEach(image => _db.BlogImages.Remove(image));
                _db.BlogAttractions.Where(b => b.Blog.BlogGuid == blog.BlogGuid).ToList().ForEach(att => _db.BlogAttractions.Remove(att));

                _db.SaveChanges();


                //更新上傳的景點、景點照片
                foreach (var attraction in blog.BlogAttractionList)
                {
                    BlogAttraction blogAttraction = new BlogAttraction();
                    blogAttraction.BlogId = originBlog.BlogId;
                    blogAttraction.AttractionId = attraction.AttractionId;
                    blogAttraction.Description = attraction.Description;
                    blogAttraction.InitDate = DateTime.Now;

                    _db.BlogAttractions.Add(blogAttraction);
                    _db.SaveChanges();

                    foreach (string image in attraction.ImageUrl)
                    {
                        BlogImage blogImage = new BlogImage();
                        blogImage.BlogAttractionId = blogAttraction.BlogAttractionId;
                        blogImage.ImageName = image;
                        blogImage.InitDate = DateTime.Now;

                        _db.BlogImages.Add(blogImage);
                        _db.SaveChanges();
                    }
                }



                // 儲存遊記照片檔案
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

                return Ok("已儲存編輯內容");
            }
            catch (Exception)
            {
                return BadRequest("編輯內容儲存失敗");
            }
        }







        /// <summary>
        ///     取得單一遊記資訊
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{blogGuid}")]
        public IHttpActionResult GetBlogInfo([FromUri] string blogGuid)
        {
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            string blogPath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";
            int myUserId = 0;

            var blog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.Status == 1);

            if (blog != null)
            {
                if (Request.Headers.Authorization != null)
                {
                    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                    string userGuid = (string)userToken["UserGuid"];
                    myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

                    //10分鐘內沒有瀏覽過，瀏覽量+1
                    var hadbrowse = _db.BlogBrowses.Where(b => b.UserId == myUserId && b.Blog.BlogGuid == blogGuid).Max(b => b.InitDate);

                    if (hadbrowse == null || DateTime.Now.Subtract((DateTime)hadbrowse).Minutes > 10)
                    {
                        blog.PageViewCounts++;
                        _db.SaveChanges();
                    }

                    //記錄此次瀏覽
                    BlogBrowse blogBrowse = new BlogBrowse();
                    blogBrowse.BlogId = blog.BlogId;
                    blogBrowse.UserId = myUserId;
                    blogBrowse.InitDate = DateTime.Now;
                    _db.BlogBrowses.Add(blogBrowse);
                    _db.SaveChanges();
                }



                //一市多區
                var attractions = _db.Attractions.Where(a => _db.BlogAttractions.Where(b => b.Blog.BlogGuid == blogGuid).Select(b => b.AttractionId).Contains(a.AttractionId)).Select(a => new
                {
                    District = a.District.DistrictName,
                    City = a.District.City.CittyName
                }).Distinct().ToList();

                var cityDistricts = new Dictionary<string, List<string>>();
                foreach (var a in attractions)
                {
                    if (!cityDistricts.ContainsKey(a.City))
                    {
                        cityDistricts.Add(a.City, new List<string>());
                    }
                    cityDistricts[a.City].Add(a.District);
                }

                // 處理格式-> 臺北市 大安區、中正區
                var cityAndDistricts = new List<string>();
                foreach (var cityDistrict in cityDistricts)
                {
                    var city = cityDistrict.Key;
                    var districts = string.Join("、", cityDistrict.Value.Distinct());
                    cityAndDistricts.Add(city + " " + districts);
                }



                var allComments = _db.BlogComments.Where(c => c.Blog.BlogGuid == blogGuid && c.Status == true).OrderByDescending(c => c.InitDate).ToList();

                //如果有登入，自己的評論會在最上面
                if (Request.Headers.Authorization != null)
                {
                    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                    string userGuid = (string)userToken["UserGuid"];
                    myUserId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

                    var myComments = allComments.Where(c => c.UserId == myUserId).ToList();
                    allComments = allComments.Except(myComments).ToList();
                    allComments = myComments.Concat(allComments).ToList();
                }

                // 取10筆評論
                var comments = allComments.Take(10).ToList().Select(c =>
                {
                    var user = _db.Users.FirstOrDefault(u => u.UserId == c.UserId);

                    return new
                    {
                        IsMyComment = c.UserId == myUserId ? true : false,
                        BlogCommentId = c.BlogCommentId,
                        UserGuid = user.UserGuid,
                        UserName = user.UserName,
                        InitDate = Tool.CommentTime((DateTime)c.InitDate) + (c.EditDate == null ? "" : " (已編輯)"),
                        ProfilePicture = user.ProfilePicture == null ? "" : profilePath + user.ProfilePicture,
                        Comment = c.Comment,
                        Replies = _db.BlogReplies.Where(r => r.BlogCommentId == c.BlogCommentId && r.Status == true).ToList().Select(r =>
                        {
                            var userReply = _db.Users.FirstOrDefault(u => u.UserId == r.UserId);

                            return new
                            {
                                IsMyComment = r.UserId == myUserId ? true : false,
                                BlogReplyId = r.BlogReplyId,
                                UserGuid = userReply.UserGuid,
                                UserName = userReply.UserName,
                                InitDate = Tool.CommentTime((DateTime)r.InitDate) + (r.EditDate == null ? "" : " (已編輯)"),
                                ProfilePicture = userReply.ProfilePicture == null ? "" : profilePath + userReply.ProfilePicture,
                                Reply = r.Reply
                            };
                        })
                    };
                }).ToList();




                var result = new
                {
                    IsCollect = _db.BlogCollections.FirstOrDefault(c => c.Blog.BlogGuid == blogGuid && c.UserId == myUserId) == null ? false : true,
                    BlogGuid = blogGuid,
                    Cover = blog.Cover == null ? "" : blogPath + blog.Cover,
                    Title = blog.Title,
                    Category = blog.Category == null ? new string[0] : blog.Category.Split(','),
                    CityAndDistricts = string.Join("；", cityAndDistricts), //未來有其他縣市以;區隔
                    UserGuid = blog.User.UserGuid,
                    UserName = blog.User.UserName,
                    ProfilePicture = blog.User.ProfilePicture == null ? "" : profilePath + blog.User.ProfilePicture,
                    InitDate = blog.InitDate.Value.ToString("yyyy-MM-dd HH:mm") + (blog.EditDate == null ? "" : " (已編輯)"),
                    Sees = blog.PageViewCounts,
                    Likes = _db.BlogLikes.Where(l => l.BlogId == blog.BlogId).Count(),
                    CommentCounts = _db.BlogComments.Where(c => c.Blog.BlogGuid == blogGuid && c.Status == true).Count() + _db.BlogReplies.Where(r => r.BlogComment.Blog.BlogGuid == blogGuid && r.Status == true).Count(),
                    AttractionData = _db.BlogAttractions.Where(a => a.Blog.BlogGuid == blogGuid).Select(a => new
                    {
                        AttractionId = a.AttractionId,
                        AttractionName = _db.Attractions.FirstOrDefault(n => n.AttractionId == a.AttractionId).AttractionName,
                        Description = a.Description,
                        ImageUrl = _db.BlogImages.Where(i => i.BlogAttraction.BlogAttractionId == a.BlogAttractionId).Select(i => blogPath + i.ImageName)
                    }),
                    Comments = comments
                };

                return Ok(result);
            }
            else
            {
                return BadRequest("此篇遊記不存在");
            }
        }




        /// <summary>
        ///     取得更多遊記評論
        /// </summary>
        [HttpGet]
        [Route("{blogGuid}/comments/{page}")]
        public IHttpActionResult MoreBlogComments([FromUri] string blogGuid, int page)
        {
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            int pagesize = 10;
            int myUserId = 0;

            var allComments = _db.BlogComments.Where(c => c.Blog.BlogGuid == blogGuid && c.Status == true).OrderByDescending(c => c.InitDate).ToList();

            if (allComments.Any())
            {
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
                var result = allComments.Skip((page - 1) * pagesize).Take(pagesize).ToList().Select(c =>
                {
                    var user = _db.Users.FirstOrDefault(u => u.UserId == c.UserId);

                    return new
                    {
                        IsMyComment = c.UserId == myUserId ? true : false,
                        BlogCommentId = c.BlogCommentId,
                        UserGuid = user.UserGuid,
                        UserName = user.UserName,
                        InitDate = Tool.CommentTime((DateTime)c.InitDate) + (c.EditDate == null ? "" : " (已編輯)"),
                        ProfilePicture = user.ProfilePicture == null ? "" : profilePath + user.ProfilePicture,
                        Comment = c.Comment,
                        Replies = _db.BlogReplies.Where(r => r.BlogCommentId == c.BlogCommentId && r.Status == true).ToList().Select(r =>
                        {
                            var userReply = _db.Users.FirstOrDefault(u => u.UserId == r.UserId);

                            return new
                            {
                                IsMyComment = r.UserId == myUserId ? true : false,
                                BlogReplyId = r.BlogReplyId,
                                UserGuid = userReply.UserGuid,
                                UserName = userReply.UserName,
                                InitDate = Tool.CommentTime((DateTime)r.InitDate) + (r.EditDate == null ? "" : " (已編輯)"),
                                ProfilePicture = userReply.ProfilePicture == null ? "" : profilePath + userReply.ProfilePicture,
                                Reply = r.Reply
                            };
                        })
                    };
                }).ToList();

                if (result.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("無更多遊記評論");
                }
            }
            else
            {
                return BadRequest("尚無評論");
            }
        }



        /// <summary>
        ///     給參數搜尋遊記(熱門話題取得所有遊記)
        /// </summary>
        [HttpGet]
        [Route("search")]
        public IHttpActionResult BlogsSearch([FromUri] SearchViewModel view)
        {
            int pageSize = 9;
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            string blogPath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string userGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            }


            //遊記搜尋篩選
            var temp = _db.BlogAttractions.Where(b => b.Blog.Status == 1).AsQueryable();

            if (view.District != null)
            {
                var attractions = _db.Attractions.Where(a => view.District.Contains(a.District.DistrictName)).Select(a => a.AttractionId).Distinct().ToList();

                temp = temp.Where(t => attractions.Contains(t.AttractionId));
            }

            if (!string.IsNullOrEmpty(view.Keyword))
            {
                var attractions = _db.Attractions.Where(a => a.AttractionName.Contains(view.Keyword) || a.Introduction.Contains(view.Keyword) || a.Address.Contains(view.Keyword)).Select(a => a.AttractionId).Distinct().ToList();

                temp = temp.Where(t => attractions.Contains(t.AttractionId) || t.Blog.Title.Contains(view.Keyword) || t.Description.Contains(view.Keyword));
            }

            if (view.Type != null)
            {
                temp = temp.Where(t => t.Blog.Category != null);
                temp = temp.ToList().Where(t => view.Type.Any(x => t.Blog.Category.Contains(x))).AsQueryable();
            }

            //符合搜尋結果的總項目
            int totalItem = temp.Select(t => t.BlogId).Distinct().Count();
            //總頁數
            int totalPages = totalItem % pageSize == 0 ? totalItem / pageSize : totalItem / pageSize + 1;
            //該頁顯示項目
            var searchBlogs = temp.Select(t => new
            {
                BlogGuid = t.Blog.BlogGuid,
                Likes = _db.BlogLikes.Where(l => l.BlogId == t.BlogId).Count(),
            }).Distinct().OrderByDescending(t => t.Likes).Skip(pageSize * (view.Page - 1)).Take(pageSize).ToDictionary(t => t.BlogGuid, t => t.Likes);

            var result = _db.Blogs.Where(b => searchBlogs.Keys.Contains(b.BlogGuid)).ToList().Select(b => new
            {
                IsCollect = _db.BlogCollections.FirstOrDefault(c => c.Blog.BlogGuid == b.BlogGuid && c.UserId == myUserId) != null ? true : false,
                BlogGuid = b.BlogGuid,
                Title = b.Title,
                Cover = b.Cover == null ? "" : blogPath + b.Cover,
                UserGuid = b.User.UserGuid,
                UserName = b.User.UserName,
                ProfilePicture = b.User.ProfilePicture == null ? "" : profilePath + b.User.ProfilePicture,
                InitDate = b.InitDate.Value.ToString("yyyy-MM-dd HH:mm"),
                Sees = b.PageViewCounts,
                Likes = searchBlogs[b.BlogGuid],
                Comments = _db.BlogComments.Where(c => c.BlogId == b.BlogId && c.Status == true).Count() + _db.BlogReplies.Where(c => c.BlogComment.BlogId == b.BlogId && c.Status == true).Count(),
                Category = b.Category == null ? new string[0] : b.Category.Split(',')
            })
                .OrderByDescending(a => a.Likes);

            if (totalItem != 0)
            {
                return Ok(new { TotalPages = totalPages, TotalItem = totalItem, Tours = result });
            }
            else
            {
                return BadRequest("尚無符合搜尋條件的遊記");
            }
        }



        /// <summary>
        ///     新增追蹤
        /// </summary>
        /// <param name="userGuid">追蹤用戶Guid</param>
        /// <returns></returns>
        [HttpPost]
        [JwtAuthFilter]
        [Route("follow/{userGuid}")]
        public IHttpActionResult FollowerAdd([FromUri] string userGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = userToken["UserGuid"].ToString();
            int myId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;

            int followedUserId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;
            var hadFollewed = _db.BlogFollowers.FirstOrDefault(f => f.UserId == followedUserId && f.FollowingUserId == myId);

            if (hadFollewed == null)
            {
                BlogFollower blogFollower = new BlogFollower();
                blogFollower.UserId = followedUserId;
                blogFollower.FollowingUserId = myId;
                blogFollower.InitDate = DateTime.Now;
                _db.BlogFollowers.Add(blogFollower);
                _db.SaveChanges();

                //發通知給被追蹤者
                int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "社群追蹤").NotificationTypeId;

                var notification = new Notification
                {
                    Status = true,
                    IsRead = false,
                    Sender = myId,
                    Receiver = followedUserId,
                    NotificationTypeId = typeId,
                    InitDate = DateTime.Now
                };

                _db.Notifications.Add(notification);
                _db.SaveChanges();

                return Ok("追蹤成功");
            }
            else
            {
                return BadRequest("已追蹤此用戶");
            }
        }



        /// <summary>
        ///     取消追蹤
        /// </summary>
        /// <param name="userGuid">追蹤用戶Guid</param>
        /// <returns></returns>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("follow/{userGuid}")]
        public IHttpActionResult FollowerRemove([FromUri] string userGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = userToken["UserGuid"].ToString();
            int myId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;

            int followedUserId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;
            var hadFollewed = _db.BlogFollowers.FirstOrDefault(f => f.UserId == followedUserId && f.FollowingUserId == myId);

            if (hadFollewed != null)
            {
                _db.BlogFollowers.Remove(hadFollewed);
                _db.SaveChanges();

                return Ok("已取消追蹤");
            }
            else
            {
                return BadRequest("未追蹤此用戶");
            }
        }



        /// <summary>
        ///     取得單一用戶社群頁面(遊記)
        /// </summary>
        [HttpGet]
        [Route("profile/{userGuid}")]
        public IHttpActionResult BlogProfile([FromUri] string userGuid)
        {
            int pageSize = 12;
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            string blogPath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string myGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            }

            var blogger = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);
            if (blogger != null)
            {
                var result = new
                {
                    ProfilePicture = blogger.ProfilePicture == null ? "" : profilePath + blogger.ProfilePicture,
                    UserName = blogger.UserName,
                    IsFollow = _db.BlogFollowers.FirstOrDefault(f => f.UserId == blogger.UserId && f.FollowingUserId == myUserId) == null ? false : true,
                    Blogs = _db.Blogs.Where(b => b.User.UserGuid == userGuid && b.Status == 1).Count(),
                    Fans = _db.BlogFollowers.Where(f => f.User.UserGuid == userGuid).Count(),
                    Follows = _db.BlogFollowers.Where(f => f.FollowingUserId == blogger.UserId).Count(),
                    BlogData = _db.Blogs.Where(b => b.User.UserGuid == userGuid && b.Status == 1).OrderByDescending(b => b.InitDate).Take(pageSize).ToList().Select(b => new
                    {
                        BlogGuid = b.BlogGuid,
                        IsCollect = _db.BlogCollections.FirstOrDefault(c => c.BlogId == b.BlogId && c.UserId == myUserId) == null ? false : true,
                        Cover = b.Cover == null ? "" : blogPath + b.Cover,
                        Title = b.Title,
                        Profile = blogger.ProfilePicture == null ? "" : profilePath + blogger.ProfilePicture,
                        UserName = blogger.UserName,
                        InitDate = b.InitDate.Value.ToString("yyyy-MM-dd HH:mm"),
                        Sees = b.PageViewCounts,
                        Likes = _db.BlogLikes.Where(l => l.BlogId == b.BlogId).Count(),
                        Comments = _db.BlogComments.Where(c => c.BlogId == b.BlogId).Count() + _db.BlogReplies.Where(l => l.BlogComment.BlogId == b.BlogId).Count(),
                        Category = b.Category == null ? new string[0] : b.Category.Split(',')
                    })
                };

                return Ok(result);
            }
            else
            {
                return BadRequest("沒有此用戶頁面");
            }

        }


        /// <summary>
        ///     取得更多單一用戶社群頁面的遊記
        /// </summary>
        [HttpGet]
        [Route("profile/{userGuid}/{page}")]
        public IHttpActionResult BlogProfileMore([FromUri] string userGuid, int page)
        {
            int pageSize = 12;
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            string blogPath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string myGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            }

            var blogger = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);
            if (blogger != null)
            {
                var result = _db.Blogs.Where(b => b.User.UserGuid == userGuid && b.Status == 1).OrderByDescending(b => b.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(b => new
                {
                    BlogGuid = b.BlogGuid,
                    IsCollect = _db.BlogCollections.FirstOrDefault(c => c.BlogId == b.BlogId && c.UserId == myUserId) == null ? false : true,
                    Cover = b.Cover == null ? "" : blogPath + b.Cover,
                    Title = b.Title,
                    Profile = blogger.ProfilePicture == null ? "" : profilePath + blogger.ProfilePicture,
                    UserName = blogger.UserName,
                    InitDate = b.InitDate.Value.ToString("yyyy-MM-dd HH:mm"),
                    Sees = b.PageViewCounts,
                    Likes = _db.BlogLikes.Where(l => l.BlogId == b.BlogId).Count(),
                    Comments = _db.BlogComments.Where(c => c.BlogId == b.BlogId).Count() + _db.BlogReplies.Where(l => l.BlogComment.BlogId == b.BlogId).Count(),
                    Category = b.Category == null ? new string[0] : b.Category.Split(',')
                }).ToList();

                if (result.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("已無更多遊記");
                }
            }
            else
            {
                return BadRequest("沒有此用戶頁面");
            }
        }





        /// <summary>
        ///     新增留言
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("comments/add")]
        public IHttpActionResult AddBlogComments(AddBlogCommentsView view)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            User user = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            var blog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == view.BlogGuid && b.Status == 1);
            if (blog != null)
            {
                if (string.IsNullOrWhiteSpace(view.Comment) || view.Comment.Length > 500)
                {
                    return BadRequest("留言不得空白或超過500字");
                }

                BlogComment blogComment = new BlogComment();
                blogComment.BlogId = blog.BlogId;
                blogComment.UserId = user.UserId;
                blogComment.Comment = view.Comment;
                blogComment.Status = true;
                blogComment.InitDate = DateTime.Now;
                _db.BlogComments.Add(blogComment);
                _db.SaveChanges();


                //發通知給遊記作者(留言自己遊記不通知)
                if (user.UserId != blog.UserId)
                {
                    int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "遊記留言").NotificationTypeId;

                    var notification = new Notification
                    {
                        Status = true,
                        IsRead = false,
                        Sender = user.UserId,
                        Receiver = blog.UserId,
                        NotificationTypeId = typeId,
                        InitDate = DateTime.Now,
                        BlogGuid = view.BlogGuid
                    };

                    _db.Notifications.Add(notification);
                    _db.SaveChanges();
                }
                

                string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
                var result = new
                {
                    IsMyComment = true,
                    BlogCommentId = blogComment.BlogCommentId,
                    UserGuid = userGuid,
                    UserName = user.UserName,
                    InitDate = "剛剛",
                    ProfilePicture = user.ProfilePicture != null ? profilePath + user.ProfilePicture : "",
                    Comment= view.Comment
                };

                return Ok(result);
            }
            else
            {
                return BadRequest("無此遊記");
            }
        }




        /// <summary>
        ///     編輯留言
        /// </summary>
        [HttpPut]
        [JwtAuthFilter]
        [Route("comments/edit")]
        public IHttpActionResult EditBlogComments(EditBlogCommentsView view)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var originComment = _db.BlogComments.FirstOrDefault(b => b.BlogCommentId == view.BlogCommentId && b.UserId == userId);

            if (originComment != null)
            {
                if (originComment.Status == true)
                {
                    if (string.IsNullOrWhiteSpace(view.Comment) || view.Comment.Length > 500)
                    {
                        return BadRequest("留言不得空白或超過500字");
                    }
                    originComment.Comment = view.Comment;
                    originComment.EditDate = DateTime.Now;
                    _db.SaveChanges();

                    return Ok("留言已修改");
                }
                else
                {
                    return BadRequest("此留言已刪除");
                }
            }
            else
            {
                return BadRequest("無法編輯此留言");
            }
        }




        /// <summary>
        ///     刪除留言
        /// </summary>
        /// <param name="blogCommentId">要刪除的留言Id</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("comments/{blogCommentId}")]
        public IHttpActionResult RemoveBlogComments([FromUri] int blogCommentId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var originComment = _db.BlogComments.FirstOrDefault(b => b.BlogCommentId == blogCommentId && b.UserId == userId);

            if (originComment != null)
            {
                if (originComment.Status == true)
                {
                    originComment.Status = false;
                    _db.SaveChanges();

                    return Ok("留言刪除成功");
                }
                else
                {
                    return BadRequest("此留言已刪除");
                }
            }
            else
            {
                return BadRequest("無法刪除此留言");
            }
        }




        /// <summary>
        ///     新增回覆
        /// </summary>
        [HttpPost]
        [JwtAuthFilter]
        [Route("replies/add")]
        public IHttpActionResult AddBlogCommentReplies(AddBlogCommentRepliesView view)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var blogComment = _db.BlogComments.FirstOrDefault(b => b.BlogCommentId == view.BlogCommentId && b.Status == true);
            if (blogComment != null)
            {
                if (string.IsNullOrWhiteSpace(view.Reply) || view.Reply.Length > 500)
                {
                    return BadRequest("回覆不得空白或超過500字");
                }

                BlogReply blogReply = new BlogReply();
                blogReply.BlogCommentId = view.BlogCommentId;
                blogReply.UserId = userId;
                blogReply.Reply = view.Reply;
                blogReply.Status = true;
                blogReply.InitDate = DateTime.Now;

                _db.BlogReplies.Add(blogReply);
                _db.SaveChanges();


                //發通知給該則留言者(回覆自己留言不通知)
                if (userId != blogComment.UserId)
                {
                    int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "留言回覆").NotificationTypeId;

                    var notification = new Notification
                    {
                        Status = true,
                        IsRead = false,
                        Sender = userId,
                        Receiver = blogComment.UserId,
                        NotificationTypeId = typeId,
                        InitDate = DateTime.Now,
                        BlogGuid = blogComment.Blog.BlogGuid
                    };

                    _db.Notifications.Add(notification);
                    _db.SaveChanges();
                }
                
                return Ok(new { BlogReplyId = blogReply.BlogReplyId });
            }
            else
            {
                return BadRequest("無此遊記留言");
            }
        }





        /// <summary>
        ///     編輯回覆
        /// </summary>
        [HttpPut]
        [JwtAuthFilter]
        [Route("replies/edit")]
        public IHttpActionResult EditBlogCommentReplies(EditBlogCommentRepliesView view)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var originReply = _db.BlogReplies.FirstOrDefault(b => b.BlogReplyId == view.BlogReplyId && b.UserId == userId);

            if (originReply != null)
            {
                if (originReply.Status == true)
                {
                    if (string.IsNullOrWhiteSpace(view.Reply) || view.Reply.Length > 500)
                    {
                        return BadRequest("回覆不得空白或超過500字");
                    }
                    originReply.Reply = view.Reply;
                    originReply.EditDate = DateTime.Now;
                    _db.SaveChanges();

                    return Ok("回覆已修改");
                }
                else
                {
                    return BadRequest("此回覆已刪除");
                }
            }
            else
            {
                return BadRequest("無法編輯此回覆");
            }
        }



        /// <summary>
        ///     刪除回覆
        /// </summary>
        /// <param name="blogReplyId">遊記留言回覆Id</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("replies/{blogReplyId}")]
        public IHttpActionResult RemoveBlogCommentReplies([FromUri] int blogReplyId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var originReply = _db.BlogReplies.FirstOrDefault(b => b.BlogReplyId == blogReplyId && b.UserId == userId);

            if (originReply != null)
            {
                if (originReply.Status == true)
                {
                    originReply.Status = false;
                    _db.SaveChanges();

                    return Ok("回覆刪除成功");
                }
                else
                {
                    return BadRequest("此回覆已刪除");
                }
            }
            else
            {
                return BadRequest("無法刪除此回覆");
            }
        }



        /// <summary>
        ///     收藏遊記
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{blogGuid}/collect")]
        public IHttpActionResult BlogCollectAdd([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            Blog blog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.Status == 1);
            if (blog != null)
            {
                BlogCollection blogCollection = new BlogCollection();
                blogCollection.BlogId = blog.BlogId;
                blogCollection.UserId = userId;
                blogCollection.InitDate = DateTime.Now;
                _db.BlogCollections.Add(blogCollection);
                _db.SaveChanges();

                return Ok(new { Message = "收藏遊記成功" });
            }
            else
            {
                return BadRequest("此遊記不存在");
            }
        }


        /// <summary>
        ///     取消收藏遊記
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("{blogGuid}/collect")]
        public IHttpActionResult BlogCollectRemove([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var collection = _db.BlogCollections.FirstOrDefault(a => a.UserId == userId && a.Blog.BlogGuid == blogGuid);
            if (collection != null)
            {
                _db.BlogCollections.Remove(collection);
                _db.SaveChanges();
            }
            return Ok(new { Message = "已取消收藏" });
        }




        /// <summary>
        ///     按遊記愛心
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpPost]
        [JwtAuthFilter]
        [Route("{blogGuid}/like")]
        public IHttpActionResult BlogLike([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = userToken["UserGuid"].ToString();
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            Blog blog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.Status == 1);
            if (blog != null)
            {
                BlogLike blogLike = new BlogLike();
                blogLike.BlogId = blog.BlogId;
                blogLike.UserId = userId;
                _db.BlogLikes.Add(blogLike);
                _db.SaveChanges();

                //發通知給遊記作者
                int typeId = _db.NotificationTypes.FirstOrDefault(n => n.Type == "遊記喜歡").NotificationTypeId;

                var notification = new Notification
                {
                    Status = true,
                    IsRead = false,
                    Sender = userId,
                    Receiver = blog.UserId,
                    NotificationTypeId = typeId,
                    InitDate = DateTime.Now,
                    BlogGuid = blogGuid
                };

                _db.Notifications.Add(notification);
                _db.SaveChanges();

                return Ok(new { Message = "按喜歡成功" });
            }
            else
            {
                return BadRequest("此遊記不存在");
            }
        }




        /// <summary>
        ///      取消遊記愛心
        /// </summary>
        /// <param name="blogGuid">遊記Guid</param>
        /// <returns></returns>
        [HttpDelete]
        [JwtAuthFilter]
        [Route("{blogGuid}/like")]
        public IHttpActionResult BlogUnlike([FromUri] string blogGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var like = _db.BlogLikes.FirstOrDefault(a => a.UserId == userId && a.Blog.BlogGuid == blogGuid);
            if (like != null)
            {
                _db.BlogLikes.Remove(like);
                _db.SaveChanges();
            }
            return Ok(new { Message = "已取消喜歡" });
        }




        /// <summary>
        ///     顯示粉絲
        /// </summary>
        /// <param name="userGuid">用戶Guid</param>
        /// <param name="page">頁數</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userGuid}/fans/{page}")]
        public IHttpActionResult BlogFans([FromUri] string userGuid, int page)
        {
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            int pageSize = 20;
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string myGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            }

            User blogger = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            if (blogger != null)
            {
                var result = new
                {
                    UserName = blogger.UserName,
                    ProfilePicture = blogger.ProfilePicture == null ? "" : profilePath + blogger.ProfilePicture,
                    BlogCounts = _db.Blogs.Where(b => b.UserId == blogger.UserId && b.Status == 1).Count(),
                    Fans = _db.BlogFollowers.Where(f => f.UserId == blogger.UserId).Count(),
                    Followers = _db.BlogFollowers.Where(f => f.FollowingUserId == blogger.UserId).Count(),

                    FanData = _db.BlogFollowers.Where(f => f.UserId == blogger.UserId).OrderByDescending(f => f.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(f =>
                    {
                        User user = _db.Users.FirstOrDefault(u => u.UserId == f.FollowingUserId);
                        return new
                        {
                            UserGuid = user.UserGuid,
                            UserName = user.UserName,
                            ProfilePicture = user.ProfilePicture == null ? "" : profilePath + user.ProfilePicture,
                            IsFollow = _db.BlogFollowers.FirstOrDefault(x => x.UserId == user.UserId && x.FollowingUserId == myUserId) == null ? false : true
                        };
                    })
                };

                if (result.FanData.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("無更多粉絲");
                }
            }
            else
            {
                return BadRequest("沒有此用戶頁面");
            }
        }




        /// <summary>
        ///     顯示追蹤
        /// </summary>
        /// <param name="userGuid">用戶Guid</param>
        /// <param name="page">頁數</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userGuid}/follows/{page}")]
        public IHttpActionResult BlogFollows([FromUri] string userGuid, int page)
        {
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            int pageSize = 20;
            int myUserId = 0;
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string myGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            }

            User blogger = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid);

            if (blogger != null)
            {
                var result = new
                {
                    UserName = blogger.UserName,
                    ProfilePicture = blogger.ProfilePicture == null ? "" : profilePath + blogger.ProfilePicture,
                    BlogCounts = _db.Blogs.Where(b => b.UserId == blogger.UserId && b.Status == 1).Count(),
                    Fans = _db.BlogFollowers.Where(f => f.UserId == blogger.UserId).Count(),
                    Followers = _db.BlogFollowers.Where(f => f.FollowingUserId == blogger.UserId).Count(),

                    FollowData = _db.BlogFollowers.Where(f => f.FollowingUserId == blogger.UserId).OrderByDescending(f => f.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(f =>
                    {
                        User user = _db.Users.FirstOrDefault(u => u.UserId == f.UserId);
                        return new
                        {
                            UserGuid = user.UserGuid,
                            UserName = user.UserName,
                            ProfilePicture = user.ProfilePicture == null ? "" : profilePath + user.ProfilePicture,
                            IsFollow = _db.BlogFollowers.FirstOrDefault(x => x.UserId == user.UserId && x.FollowingUserId == myUserId) == null ? false : true
                        };
                    })
                };

                if (result.FollowData.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("無更多追蹤者");
                }
            }
            else
            {
                return BadRequest("沒有此用戶頁面");
            }
        }
    }
}
