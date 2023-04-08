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
                        AttractionList = _db.BlogAttractions.Where(a => a.Blog.BlogGuid == blogGuid).Select(a => new
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



    }
}
