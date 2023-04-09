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
                blog.InitDate = DateTime.Now;

                _db.Blogs.Add(blog);
                _db.SaveChanges();

                int blogId = _db.Blogs.Where(b => b.BlogId == blog.BlogId).Select(b => b.BlogId).FirstOrDefault();

                var attractionIds = _db.TourAttractions.Where(a => a.TourId == tourId).Select(a => a.AttractionId).ToList();

                foreach(var attractionId in attractionIds)
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

            if(isMyDraftBlog!=null)
            {
                if(isMyDraftBlog.Status!=2)
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
                if(isMyDraftBlog.Status==0)
                {
                    isMyDraftBlog.Status = 1;
                    isMyDraftBlog.InitDate = DateTime.Now;
                    isMyDraftBlog.EditDate = null;

                    _db.SaveChanges();
                    return Ok("成功發佈");
                }
                else if(isMyDraftBlog.Status == 1)
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
                if (isMyDraftBlog.Status == 0|| isMyDraftBlog.Status == 1)
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

                            if (!IsImage(fileName))
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
                foreach(var attraction in blog.BlogAttractionList)
                {
                    BlogAttraction blogAttraction = new BlogAttraction();
                    blogAttraction.BlogId = originBlog.BlogId;
                    blogAttraction.AttractionId = attraction.AttractionId;
                    blogAttraction.Description = attraction.Description;
                    blogAttraction.InitDate = DateTime.Now;

                    _db.BlogAttractions.Add(blogAttraction);
                    _db.SaveChanges();

                    foreach(string image in attraction.ImageUrl)
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

        
        private bool IsImage(string fileName) //檢查是否為圖片檔
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp" || ext == ".svg" || ext == ".ico";
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
            if (Request.Headers.Authorization != null)
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                string userGuid = (string)userToken["UserGuid"];
                myUserId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            }


            var blog = _db.Blogs.FirstOrDefault(b => b.BlogGuid == blogGuid && b.Status == 1);

            if (blog != null)
            {
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



                var result = new
                {
                    IsCollect = _db.BlogCollections.FirstOrDefault(c => c.Blog.BlogGuid == blogGuid && c.UserId == myUserId) == null ? false : true,
                    Cover = blog.Cover == null ? "" : blogPath + blog.Cover,
                    Title = blog.Title,
                    Category = blog.Category == null ? new string[0] : blog.Category.Split(','),
                    CityAndDistricts = string.Join("；", cityAndDistricts), //未來有其他縣市以;區隔
                    UserGuid = blog.User.UserGuid,
                    UserName = blog.User.UserName,
                    ProfilePicture = blog.User.ProfilePicture == null ? "" : profilePath + blog.User.ProfilePicture,
                    InitDate = blog.InitDate.Value.ToString("yyyy-MM-dd HH:mm") + (blog.EditDate == null ? "" : " (已編輯)"),
                    Likes = _db.BlogLikes.Where(l => l.BlogId == blog.BlogId).Count(),
                    CommentCount = _db.BlogComments.Where(c => c.Blog.BlogGuid == blogGuid && c.Status == true).Count()+_db.BlogReplies.Where(r=>r.BlogComment.Blog.BlogGuid==blogGuid&&r.Status==true).Count(),
                    AttractionData = _db.BlogAttractions.Where(a => a.Blog.BlogGuid == blogGuid).Select(a => new
                    {
                        AttractionId = a.AttractionId,
                        AttractionName = _db.Attractions.FirstOrDefault(n => n.AttractionId == a.AttractionId).AttractionName,
                        Description = a.Description,
                        ImageUrl = _db.BlogImages.Where(i => i.BlogAttraction.BlogAttractionId == a.BlogAttractionId).Select(i => blogPath + i.ImageName)
                    }),
                    Comments = _db.BlogComments.Where(c => c.Blog.BlogGuid == blogGuid && c.Status == true).OrderByDescending(c => c.InitDate).Take(10).ToList().Select(c =>
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
                      })
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
        [HttpPost]
        [Route("comments")]
        public IHttpActionResult MoreBlogComments(MoreBlogCommentsView view)
        {
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            int pagesize = 10;
            int myUserId = 0;

            var allComments = _db.BlogComments.Where(c => c.Blog.BlogGuid == view.BlogGuid && c.Status == true).OrderByDescending(c => c.InitDate).ToList();

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
                var result = allComments.Skip((view.Page - 1) * pagesize).Take(pagesize).ToList().Select(c =>
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
