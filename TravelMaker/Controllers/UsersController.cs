using NSwag.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TravelMaker.Models;
using TravelMaker.Security;

namespace TravelMaker.Controllers
{
    /// <summary>
    ///     用戶服務
    /// </summary>
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext();


        /// <summary>
        ///     登入
        /// </summary>
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginView login)
        {
            User user = _db.Users.Where(c => c.Account == login.Account).FirstOrDefault();

            if (user != null)
            {
                login.Password = BitConverter
                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(login.Password)))
                    .Replace("-", null);

                if (user.Password == login.Password)
                {
                    // GenerateToken() 生成新 JwtToken 用法
                    JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                    string jwtToken = jwtAuthUtil.GenerateToken(user.UserGuid, login.Account, user.UserName, user.ProfilePicture);


                    //頭貼
                    string profilePicture = "";
                    if (user.ProfilePicture != null)
                    {
                        profilePicture = "https://" + Request.RequestUri.Host + "/upload/profile/" + user.ProfilePicture;
                    }


                    var result = new
                    {
                        Message = "登入成功",
                        Account = user.Account,
                        UserName = user.UserName,
                        UserGuid = user.UserGuid,
                        JwtToken= jwtToken,
                        ProfilePicture = profilePicture
                    };

                    return Ok(result);
                }
                else
                {
                    return BadRequest("帳號或密碼有誤");
                }
            }
            else
            {
                return BadRequest("此帳號未註冊");
            }
        }




        /// <summary>
        ///     註冊
        /// </summary>
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(SignUpView signUp)
        {
            var registered = _db.Users.Where(u => u.Account == signUp.Account).FirstOrDefault();
            if (registered != null)
            {
                return BadRequest("此帳號已註冊");
            }
            else if (ModelState.IsValid)
            {
                User user = new User();

                user.Account = signUp.Account.ToLower();
                user.Password = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(signUp.Password))).Replace("-", null);
                user.UserName = signUp.UserName;
                user.UserGuid = Guid.NewGuid().ToString().Trim() + DateTime.Now.ToString("ff");
                user.InitDate = DateTime.Now;

                _db.Users.Add(user);
                _db.SaveChanges();

                var result = new
                {
                    Message = "註冊成功",
                    Account = signUp.Account,
                    UserName = signUp.UserName
                };
                return Ok(result);
            }
            else
            {
                var errorMessages = string.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(errorMessages);
            }
        }







        /// <summary>
        ///     忘記密碼-寄信
        /// </summary>
        [HttpPost]
        [Route("forgotPassword")]
        public IHttpActionResult ForgotPassword(LoginView forgetPwd)
        {
            User user = _db.Users.Where(u => u.Account == forgetPwd.Account).FirstOrDefault();

            if (user != null)
            {
                string fromAddress = ConfigurationManager.AppSettings["FromAddress"];
                string toAddress = forgetPwd.Account;
                string subject = "TravelMaker重設密碼連結";
                string mailBody = "親愛的TravelMaker會員&nbsp;" + user.UserName + "&nbsp;您好："
                                + "<br>此封信件為您在TravelMaker點選「忘記密碼」時所發送之信件，"
                                + "<br>若您沒有發出此請求，請忽略此電子郵件。"
                                + "<br>若您確實要重設密碼，請點選下列連結進入頁面重設密碼。<br><br>" +
                                  "※提醒您，此連結有效期為10分鐘，若連結失效請再次點選「忘記密碼」按鈕重新寄送連結，感謝您。<br><br>https://travel-maker.vercel.app/reset-password?token=";
                string mailBodyEnd = "<br><br>-----此為系統發出信件，請勿直接回覆，感謝您的配合。-----";

                Dictionary<string, object> account = new Dictionary<string, object>();
                account.Add("Account", forgetPwd.Account.ToLower());
                string token = JwtAuthUtil.ResetPwdToken(account);

                bool success = Mail.SendGmail(fromAddress, toAddress, subject, mailBody + token + mailBodyEnd);

                if (success)
                {
                    return Ok(new
                    {
                        Message = $"連結已寄到{forgetPwd.Account}"
                    });
                }
                else
                {
                    return BadRequest("寄信失敗");
                }
            }
            else
            {
                return BadRequest("此帳號未註冊");
            }
        }



        /// <summary>
        ///     忘記密碼-重設
        /// </summary>
        [HttpPut]
        [Route("resetPassword")]
        public IHttpActionResult ResetPassword(ResetPasswordView resetPwd)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string account = userToken["Account"].ToString();
            var user = _db.Users.Where(u => u.Account == account).FirstOrDefault();

            if (user != null)
            {
                string newHashPwd = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.NewPassword))).Replace("-", null);

                user.Password = newHashPwd;
                _db.SaveChanges();

                return Ok(new { Message = "密碼修改完成" });
            }
            else
            {
                return BadRequest("無此帳號");
            }
        }







        /// <summary>
        ///     修改密碼(會員中心)
        /// </summary>
        [HttpPut]
        [JwtAuthFilter]
        [Route("changePassword")]
        public IHttpActionResult ChangePassword(ChangePasswordView resetPwd)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            var user = _db.Users.Where(u => u.UserGuid == userGuid).FirstOrDefault();

            if (user != null) 
            {
                string hashPwd = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.OriginalPassword))).Replace("-", null);

                if (hashPwd == user.Password)
                {
                    string newHashPwd = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.NewPassword))).Replace("-", null);

                    user.Password = newHashPwd;
                    _db.SaveChanges();

                    return Ok(new { Message = "密碼修改完成" });
                }
                else
                {
                    return BadRequest("與原先密碼不符");
                }
            }
            else
            {
                return BadRequest("無此帳號");
            }
        }



        /// <summary>
        ///     會員中心左邊選單各項數量
        /// </summary>
        [HttpGet]
        [Route("dataCounts")]
        [JwtAuthFilter]
        public IHttpActionResult MyCounts()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            var result = new
            {
                TourCounts = _db.Tours.Where(t => t.UserId == userId).Count() + _db.RoomMembers.Where(r => r.UserId == userId && r.Room.Status == true).Count(),
                AttCounts = _db.AttractionCollections.Where(a => a.UserId == userId && a.Attraction.OpenStatus == true).Count(),
                BlogCounts = _db.Blogs.Where(b => b.UserId == userId && b.Status == 0).Count() + _db.BlogCollections.Where(b => b.UserId == userId && b.Blog.Status == 1).Count(),
                FollowCounts = _db.BlogFollowers.Where(f => f.FollowingUserId == userId).Count(),
                AttCommentCounts = _db.AttractionComments.Where(c => c.UserId == userId && c.Status == true && c.Attraction.OpenStatus == true).Count()
            };

            return Ok(result);
        }





        /// <summary>
        ///     取得我的收藏行程
        /// </summary>
        [HttpGet]
        [Route("tours/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult FavoriteTour([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == userGuid).UserId;

            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int pageSize = 20;

            //依照頁數取得我的行程
            var tours = _db.Tours.Where(t => t.User.UserId == userId).OrderByDescending(t => t.TourId).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(t => new
            {
                TourId = t.TourId,
                TourName = t.TourName,
                AttrCounts = _db.TourAttractions.Where(a => a.TourId == t.TourId).Count(),
                Likes = _db.TourLikes.Where(l => l.TourId == t.TourId).Count(),
                ImageUrl = _db.TourAttractions.Where(a => a.TourId == t.TourId).SelectMany(a => _db.Images.Where(i => i.AttractionId == a.AttractionId).Take(1)).Take(3).Select(a => imgPath + a.ImageName).ToList()
            });

            var result = new {
                TourCounts = _db.Tours.Where(t => t.UserId == userId).Count(),
                RoomCounts = _db.RoomMembers.Where(r => r.UserId == userId && r.Room.Status == true).Count(),
                TourData = tours
            };


            if (tours.Any())
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("已無我的行程");
            }
        }





        /// <summary>
        ///     取得我的收藏房間
        /// </summary>
        [HttpGet]
        [Route("rooms/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult MyRoom([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //依照頁數取得我的房間
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int pageSize = 20;

            var rooms = _db.RoomMembers.Where(r => r.UserId == userId && r.Room.Status == true).OrderByDescending(r => r.RoomId).Skip(pageSize * (page - 1)).Take(pageSize).Select(r => new
            {
                RoomGuid = r.Room.RoomGuid,
                RoomName = r.Room.RoomName,
                AttrCounts = _db.RoomAttractions.Where(a => a.RoomId == r.RoomId).Count(),
                CreaterName = _db.RoomMembers.FirstOrDefault(u => u.RoomId == r.RoomId && u.Permission == 1).User.UserName,
                ImageUrl=_db.RoomAttractions.Where(a=>a.RoomId==r.RoomId).SelectMany(a=>_db.Images.Where(i=>i.AttractionId==a.AttractionId).Take(1)).Take(3).Select(a=>imgPath+a.ImageName).ToList()
            }).ToList();

            var result = new
            {
                TourCounts = _db.Tours.Where(t => t.UserId == userId).Count(),
                RoomCounts = _db.RoomMembers.Where(r => r.UserId == userId && r.Room.Status == true).Count(),
                RoomData = rooms
            };


            if (rooms.Count != 0)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("已無我的房間");
            }
        }




        /// <summary>
        ///     取得我的收藏景點
        /// </summary>
        [HttpGet]
        [Route("attractions/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult MyAttractionCollections([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();

            //依照頁數取得我的收藏景點
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int pageSize = 20;

            int attCounts = _db.AttractionCollections.Where(a => a.UserId == userId && a.Attraction.OpenStatus == true).Count();

            var attractions = _db.AttractionCollections.Where(a => a.UserId == userId).OrderByDescending(a => a.AttractionCollectionId).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(a =>
            {
                var scores = _db.AttractionComments.Where(c => c.Status == true && c.AttractionId == a.AttractionId).Select(c => c.Score);
                double averageScore = scores.Any() ? scores.Average() : 0;


                return new
                {
                    AttractionId = a.AttractionId,
                    AttractionName = a.Attraction.AttractionName,
                    CityDistrict = a.Attraction.District.City.CittyName + " " + a.Attraction.District.DistrictName,
                    AverageScore = (int)Math.Round(averageScore),
                    Category = _db.CategoryAttractions.Where(c => c.AttractionId == a.AttractionId && c.CategoryId != 8 && c.CategoryId != 9).Select(c => c.Category.CategoryName).DefaultIfEmpty("餐廳").ToList(),
                    ImageUrl = imgPath + _db.Images.FirstOrDefault(i => i.AttractionId == a.AttractionId).ImageName
                };
            }).ToList();

            if (attractions.Any())
            {
                return Ok(new { AttCounts = attCounts, AttractionData = attractions });
            }
            else
            {
                return BadRequest("已無我的收藏景點");
            }
        }



        /// <summary>
        ///     修改暱稱
        /// </summary>
        [HttpPut]
        [Route("name")]
        [JwtAuthFilter]
        public IHttpActionResult Rename([FromBody] string name)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];

            var user = _db.Users.Where(u => u.UserGuid == userGuid).FirstOrDefault();
            user.UserName = name;
            _db.SaveChanges();

            return Ok(new { Message = "修改成功", UserName = name });
        }









        /// <summary>
        ///     上傳頭貼
        /// </summary>
        [Route("profile")]
        [HttpPost]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> UploadProfile() //非同步執行
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            var user = _db.Users.Where(u => u.UserGuid == userGuid).FirstOrDefault();

            // 檢查請求是否包含 multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string imgPath = HttpContext.Current.Server.MapPath(@"~/Upload/profile");
            try
            {
                // 讀取 MIME 資料
                var provider = new MultipartMemoryStreamProvider();
                try
                {
                    await Request.Content.ReadAsMultipartAsync(provider);
                }
                catch
                {
                    return BadRequest("檔案超過限制大小");
                }

                // 取得檔案副檔名，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                // 定義檔案名稱
                string fileName = user.UserName + "Profile" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 儲存圖片，單檔用.FirstOrDefault()直接取出，多檔需用迴圈
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(imgPath, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                // 使用 SixLabors.ImageSharp 調整圖片尺寸 (正方形大頭貼)
                var image = SixLabors.ImageSharp.Image.Load<Rgba32>(outputPath);
                int size = Math.Min(image.Width, image.Height);
                int x = (image.Width - size) / 2;
                int y = (image.Height - size) / 2;
                Rectangle cropArea = new Rectangle(x, y, size, size);

                if (size > 180)
                {
                    image.Mutate(i => i.Crop(cropArea).Resize(180, 180));
                }
                else
                {
                    image.Mutate(i => i.Crop(cropArea));
                }
                image.Save(outputPath);





                //更新資料庫
                user.ProfilePicture = fileName;
                _db.SaveChanges();

                return Ok(new { Message = "照片上傳成功", ProfilePicture = "https://travelmaker.rocket-coding.com/upload/profile/" + fileName });
            }
            catch (Exception)
            {
                return BadRequest("照片上傳失敗或未上傳");
            }
        }






        /// <summary>
        ///     取得我的收藏遊記
        /// </summary>
        [HttpGet]
        [Route("blogCollections/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult MyBlogCollections([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            string blogPath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";
            int pageSize = 20;

            //依照頁數取得我的收藏遊記
            var blogs = _db.BlogCollections.Where(b => b.UserId == userId && b.Blog.Status == 1).OrderByDescending(b => b.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(b => new
            {
                BlogGuid = b.Blog.BlogGuid,
                Title = b.Blog.Title,
                Cover = b.Blog.Cover == null ? "" : blogPath + b.Blog.Cover,
                UserGuid = b.Blog.User.UserGuid,
                UserName = b.Blog.User.UserName,
                ProfilePicture = b.Blog.User.ProfilePicture == null ? "" : profilePath + b.Blog.User.ProfilePicture,
                InitDate = b.Blog.InitDate.Value.ToString("yyyy-MM-dd HH:mm"),
                Sees = b.Blog.PageViewCounts,
                Likes = _db.BlogLikes.Where(l => l.BlogId == b.BlogId).Count(),
                Comments = _db.BlogComments.Where(c => c.BlogId == b.BlogId && c.Status == true).Count() + _db.BlogReplies.Where(c => c.BlogComment.BlogId == b.BlogId && c.Status == true).Count(),
                Category = b.Blog.Category == null ? new string[0] : b.Blog.Category.Split(',')
            }).ToList();

            int draftCounts = _db.Blogs.Where(b => b.UserId == userId && b.Status == 0).Count();
            int collectCounts = _db.BlogCollections.Where(b => b.UserId == userId && b.Blog.Status == 1).Count();

            if (blogs.Any())
            {
                return Ok(new { DraftCounts = draftCounts, CollectCounts = collectCounts, BlogData = blogs });
            }
            else
            {
                return BadRequest("已無我的收藏遊記");
            }
        }




        /// <summary>
        ///     取得我的草稿遊記
        /// </summary>
        [HttpGet]
        [Route("blogDrafts/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult MyBlogDrafts([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuid).Select(u => u.UserId).FirstOrDefault();
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/blogImage/";
            int pageSize = 20;

            //依照頁數取得我的草稿遊記
            var blogs = _db.Blogs.Where(b => b.UserId == userId && b.Status == 0).OrderByDescending(b => b.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(b => new
            {
                BlogGuid = b.BlogGuid,
                Title = b.Title,
                Cover = b.Cover == null ? "" : imgPath + b.Cover,
                InitDate = b.InitDate.Value.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            int draftCounts = _db.Blogs.Where(b => b.UserId == userId && b.Status == 0).Count();
            int collectCounts = _db.BlogCollections.Where(b => b.UserId == userId && b.Blog.Status == 1).Count();

            if (blogs.Any())
            {
                return Ok(new { DraftCounts = draftCounts, CollectCounts = collectCounts, BlogData = blogs });
            }
            else
            {
                return BadRequest("已無我的草稿遊記");
            }
        }




        /// <summary>
        ///     取得我的追蹤
        /// </summary>
        [HttpGet]
        [Route("followers/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult MyFollow([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = (string)userToken["UserGuid"];
            int myId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";
            int pageSize = 20;

            int followCounts = _db.BlogFollowers.Where(f => f.FollowingUserId == myId).Count();
            var result = _db.BlogFollowers.Where(f => f.FollowingUserId == myId).OrderByDescending(f=>f.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(f =>
            {
                var blogger = _db.Users.FirstOrDefault(u => u.UserId == f.UserId);
                return new
                {
                    UserGuid = blogger.UserGuid,
                    UserName = blogger.UserName,
                    ProfilePicture = blogger.ProfilePicture == null ? "" : profilePath + blogger.ProfilePicture,
                    Blogs = _db.Blogs.Where(b => b.User.UserId == blogger.UserId && b.Status == 1).Count(),
                    Follows = _db.BlogFollowers.Where(b => b.FollowingUserId == blogger.UserId).Count(),
                    Fans = _db.BlogFollowers.Where(b => b.UserId == blogger.UserId).Count()
                };
            });

            if (result.Any())
            {
                return Ok(new { FollowCounts = followCounts, FollowData = result });
            }
            else
            {
                return BadRequest("已無我的追蹤");
            }
        }



        /// <summary>
        ///     取得我的景點評論
        /// </summary>
        [HttpGet]
        [Route("comments/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult MyAttractionComments([FromUri] int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            int pageSize = 20;

            int attCommentCounts = _db.AttractionComments.Where(c => c.UserId == userId && c.Status == true && c.Attraction.OpenStatus == true).Count();

            var commentData = _db.AttractionComments.Where(c => c.UserId == userId && c.Status == true && c.Attraction.OpenStatus == true).OrderByDescending(c => c.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(c => new
            {
                AttractionCommentId = c.AttractionCommentId,
                AttractionName = c.Attraction.AttractionName,
                Score = c.Score,
                InitDate = Tool.CommentTime((DateTime)c.InitDate) + (c.EditDate == null ? "" : " (已編輯)"),
                Comment = c.Comment
            }).ToList();

            if (commentData.Any())
            {
                return Ok(new { AttCommentCounts = attCommentCounts, CommentData = commentData });
            }
            else
            {
                return BadRequest("已無我的景點評論");
            }
        }



        /// <summary>
        ///     將訊息數歸0(status1→0)
        /// </summary>
        [HttpPut]
        [Route("notifications/reset")]
        [JwtAuthFilter]
        public IHttpActionResult NotificationsReset()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;

            var notifications = _db.Notifications.Where(n => n.Receiver == userId && n.Status == true);

            foreach (var notification in notifications)
            {
                notification.Status = false;
            }
            _db.SaveChanges();

            return Ok();
        }



        /// <summary>
        ///     取得(更多)通知
        /// </summary>
        /// <param name="page">頁數</param>
        /// <returns></returns>
        [HttpGet]
        [Route("notifications/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult GetNotifications([FromUri]int page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;
            int pageSize = 13;
            string profilePath = "https://" + Request.RequestUri.Host + "/upload/profile/";

            int newNotificationCounts = _db.Notifications.Where(n => n.Receiver == userId && n.Status == true).Count();
            DateTime now = DateTime.Now;

            var notifications = _db.Notifications.Where(n => n.Receiver == userId).OrderByDescending( n => !n.IsRead && DbFunctions.DiffDays(n.InitDate, DateTime.UtcNow) <= 14).ThenByDescending(n => n.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(n =>
            {
                User user = _db.Users.FirstOrDefault(u => u.UserId == n.Sender);

                return new
                {
                    //新消息定義if(>2週or已讀過)=false
                    IsNew = !n.IsRead && (now - n.InitDate).TotalDays <= 14,
                    IsRead = n.IsRead,
                    Type = n.NotificationType.Type,
                    NotificationId = n.NotificationId,
                    UserGuid = user.UserGuid,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture == null ? null : profilePath + user.ProfilePicture,
                    InitDate = Tool.CommentTime(n.InitDate),

                    //房間相關
                    RoomGuid = n.RoomGuid,
                    RoomName = n.RoomGuid == null ? null : _db.Rooms.FirstOrDefault(r => r.RoomGuid == n.RoomGuid).RoomName,
                    OldRoomName = n.OldRoomName,
                    NewRoomName = n.NewRoomName,
                    AddVoteDate = n.AddVoteDate,

                    //遊記相關
                    BlogGuid = n.BlogGuid,
                    Title = n.BlogGuid == null ? null : _db.Blogs.FirstOrDefault(b => b.BlogGuid == n.BlogGuid).Title,

                    //行程相關
                    TourId = n.TourId,
                    TourName = n.TourId == 0 ? null : _db.Tours.FirstOrDefault(t => t.TourId == n.TourId).TourName
                };
            });

            var result = new
            {
                Status = newNotificationCounts != 0 ? true : false,
                Counts = newNotificationCounts,
                NotificationData = notifications
            };


            if (notifications.Any())
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("已無更多通知");
            }
        }



        /// <summary>
        ///     通知未讀→已讀
        /// </summary>
        /// <param name="notificationId">通知Id</param>
        /// <returns></returns>
        [HttpPut]
        [Route("notifications/{notificationId}")]
        [JwtAuthFilter]
        public IHttpActionResult IsRead([FromUri] int notificationId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = (string)userToken["UserGuid"];
            int userId = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid).UserId;

            Notification notification = _db.Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
            {
                if (notification.Receiver != userId)
                {
                    return BadRequest("非此通知接收者");
                }

                notification.IsRead = true;
                _db.SaveChanges();

                return Ok("通知已讀");
            }
            else
            {
                return BadRequest("查無此通知");
            }
        }



        /// <summary>
        ///     【維護】變更管理權限
        /// </summary>
        /// <param name="userGuid">userGuid</param>
        /// <returns></returns>
        [HttpPut]
        [JwtAuthFilter]
        [Route("permission/{userGuid}")]
        public IHttpActionResult ChangePermission([FromUri] string userGuid)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string myGuid = userToken["UserGuid"].ToString();
            var user = _db.Users.FirstOrDefault(u => u.UserGuid == myGuid);

            if (user != null && user.Permission == 2)
            {
                var person = _db.Users.FirstOrDefault(u => u.UserGuid==userGuid);
                if (person != null)
                {
                    if (person.Permission == 0)
                    {
                        person.Permission = 1;
                        _db.SaveChanges();
                        return Ok(person.Account + " " + person.UserName + " 授權成功");
                    }
                    else
                    {
                        person.Permission = 0;
                        _db.SaveChanges();
                        return Ok(person.Account + " " + person.UserName + " 權限移除");
                    }
                }
                else
                {
                    return BadRequest("查無此用戶");
                }
            }
            else
            {
                return BadRequest("權限不足");
            }
        }
    }
}
