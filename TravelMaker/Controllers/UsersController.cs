﻿using NSwag.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        ///     進入會員中心要get左邊選單各項數量
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
            List<object> result = new List<object>();
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int pageSize = 20;

            var rooms = _db.RoomMembers.Where(r => r.UserId == userId && r.Room.Status == true)
                         .Select(r => new
                         {
                             r.Room.RoomGuid,
                             r.Room.RoomName,
                             r.Room.RoomId
                         }).OrderByDescending(r => r.RoomId)
                         .Skip(pageSize * (page - 1)).Take(pageSize).ToList();



            foreach (var room in rooms)
            {
                MyRoom myRoom = new MyRoom();
                myRoom.RoomGuid = room.RoomGuid;
                myRoom.RoomName = room.RoomName;
                myRoom.CreaterName = _db.RoomMembers.Where(r => r.RoomId == room.RoomId && r.Permission == 1).Select(r => r.User.UserName).FirstOrDefault();
                myRoom.AttrCounts = _db.RoomAttractions.Where(r => r.RoomId == room.RoomId).Count();

                myRoom.ImageUrl = new List<string>();

                //每個景點一張圖片，最多三張
                var attractionIds = _db.RoomAttractions.Where(r=>r.RoomId==room.RoomId)
                                                       .Select(t => t.AttractionId).ToList();

                for (int i = 0; i < Math.Min(attractionIds.Count, 3); i++)
                {
                    int attractionId = attractionIds[i];
                    string img = _db.Images.Where(a => a.AttractionId == attractionId).Select(a => a.ImageName).FirstOrDefault();

                    myRoom.ImageUrl.Add(imgPath + img);
                }
                result.Add(myRoom);
            }



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
            List<object> result = new List<object>();
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int pageSize = 20;

            var attractions = _db.AttractionCollections.Where(a => a.UserId == userId)
                                .OrderByDescending(a => a.AttractionCollectionId)
                                .Skip(pageSize * (page - 1)).Take(pageSize).ToList();

            foreach(var attraction in attractions)
            {
                MyAttractionCollectionsView myAttraction = new MyAttractionCollectionsView();
                myAttraction.AttractionId = attraction.AttractionId;
                myAttraction.AttractionName = _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).Select(a => a.AttractionName).FirstOrDefault();

                myAttraction.CityDistrict = _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).Select(a => a.District.City.CittyName).FirstOrDefault() 
                    + " " + _db.Attractions.Where(a => a.AttractionId == attraction.AttractionId).Select(a => a.District.DistrictName).FirstOrDefault();

                var scores = _db.AttractionComments.Where(c => c.Status == true && c.AttractionId == attraction.AttractionId).Select(c => c.Score);
                double averageScore = scores.Any() ? scores.Average() : 0;
                myAttraction.AverageScore = (int)Math.Round(averageScore);

                myAttraction.Category = _db.CategoryAttractions.Where(c => c.AttractionId == attraction.AttractionId && c.CategoryId != 8 && c.CategoryId != 9).Select(c => c.Category.CategoryName).ToList();
                if (myAttraction.Category.Count == 0)
                {
                    myAttraction.Category.Add("餐廳");
                }

                myAttraction.ImageUrl = imgPath + _db.Images.Where(i => i.AttractionId == attraction.AttractionId).Select(i => i.ImageName).FirstOrDefault();


                result.Add(myAttraction);
            }

            if (attractions.Count != 0)
            {
                return Ok(result);
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
            var result = _db.BlogCollections.Where(b => b.UserId == userId && b.Blog.Status == 1).OrderByDescending(b => b.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(b => new
            {
                BlogGuid = b.Blog.BlogGuid,
                Title = b.Blog.Title,
                Cover = b.Blog.Cover == null ? "" : blogPath + b.Blog.Cover,
                UserGuid = b.Blog.User.UserGuid,
                UserName = b.Blog.User.UserName,
                ProfilePicture = b.Blog.User.ProfilePicture == null ? "" : profilePath + b.Blog.User.ProfilePicture,
                InitDate = b.Blog.InitDate.Value.ToString("yyyy-MM-dd HH:mm"),
                Sees = 0,
                Likes = _db.BlogLikes.Where(l => l.BlogId == b.BlogId).Count(),
                Comments = _db.BlogComments.Where(c => c.BlogId == b.BlogId && c.Status == true).Count() + _db.BlogReplies.Where(c => c.BlogComment.BlogId == b.BlogId && c.Status == true).Count(),
                Category = b.Blog.Category == null ? new string[0] : b.Blog.Category.Split(',')
            }).ToList();

            if (result.Any())
            {
                return Ok(result);
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
            var result = _db.Blogs.Where(b => b.UserId == userId && b.Status == 0).OrderByDescending(b => b.InitDate).Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(b => new
            {
                BlogGuid = b.BlogGuid,
                Title = b.Title,
                Cover = b.Cover == null ? "" : imgPath + b.Cover,
                InitDate = b.InitDate.Value.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            if (result.Any())
            {
                return Ok(result);
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

            int totalItem = _db.BlogFollowers.Where(f => f.FollowingUserId == myId).Count();
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
                return Ok(new { TotalItem = totalItem, FollowData = result });
            }
            else
            {
                return BadRequest("已無我的追蹤");
            }
        }
    }
}
