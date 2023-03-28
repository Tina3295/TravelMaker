using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
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
        public HttpResponseMessage Login(LoginView login)
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

                    //token放在response headers
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add("Authorization", $"Bearer {jwtToken}");

                    //頭貼
                    string profilePicture = "";
                    if (user.ProfilePicture != null)
                    {
                        profilePicture = "" + user.ProfilePicture;
                    }


                    var result = new
                    {
                        Message = "登入成功",
                        Account = user.Account,
                        UserName = user.UserName,
                        UserId = user.UserGuid,
                        ProfilePicture = profilePicture
                    };


                    var content = new ObjectContent(typeof(object), result, new JsonMediaTypeFormatter());
                    response.Content = content;

                    return response;
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "帳號或密碼有誤");
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "此帳號未註冊");
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
                var modelErrorMessage = ModelState.Values.Select(e => e.Errors).ToList();
                return BadRequest(modelErrorMessage.ToString()); //格式錯誤
            }
        }









        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!重設密碼網址尚無
        /// <summary>
        ///     忘記密碼、寄信
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
                                  "※提醒您，此連結有效期為10分鐘，若連結失效請再次點選「忘記密碼」按鈕重新寄送連結，感謝您。<br><br>前端給的網址?token=";
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
        ///     重設密碼
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
                string hashPwd = BitConverter
                                    .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.OriginalPassword)))
                                    .Replace("-", null);

                if (hashPwd == user.Password)
                {
                    string newHashPwd = BitConverter
                            .ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(resetPwd.NewPassword)))
                            .Replace("-", null);

                    user.Password = newHashPwd;
                    _db.SaveChanges();

                    return Ok("密碼修改完成");
                }
                else
                {
                    return BadRequest("不符原先密碼");
                }
            }
            else
            {
                return BadRequest("無此帳號");
            }
        }






        /// <summary>
        ///     取得我的收藏行程
        /// </summary>
        [HttpGet]
        [Route("tour/{page}")]
        [JwtAuthFilter]
        public IHttpActionResult FavoriteTour([FromUri] string page)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            string userGuuid = (string)userToken["UserGuid"];
            int userId = _db.Users.Where(u => u.UserGuid == userGuuid).Select(u => u.UserId).FirstOrDefault();

            //依照頁數取得我的行程
            List<object> result = new List<object>();
            string imgPath = "https://" + Request.RequestUri.Host + "/upload/AttractionImage/";
            int pagSize = 20;

            var tours = _db.Tours.Where(t => t.User.UserId == userId)
                         .Select(t => new
                         {
                             t.TourId,
                             t.TourName
                         }).OrderBy(t => t.TourId)
                         .Skip(pagSize * (Convert.ToInt32(page) - 1)).Take(pagSize).ToList();


            //每個景點一張圖片，最多三張
            foreach (var tour in tours)
            {
                FavoriteTour favoriteTour = new FavoriteTour();
                favoriteTour.TourId = tour.TourId;
                favoriteTour.TourName = tour.TourName;
                favoriteTour.ImageUrl = new List<string>();

                var attractionIds = _db.TourAttractions.Where(t => t.TourId == tour.TourId)
                                                       .Select(t => t.AttractionId).ToList();

                for (int i = 0; i < Math.Min(attractionIds.Count, 3); i++)
                {
                    int attractionId = attractionIds[i];
                    string img = _db.Images.Where(a => a.AttractionId == attractionId).Select(a => a.ImageName).FirstOrDefault();

                    favoriteTour.ImageUrl.Add(imgPath + img);
                }
                result.Add(favoriteTour);
            }



            if (tours.Count != 0)
            {
                return Ok(result);
            }
            else
            {
                return Ok("已無我的行程");
            }
        }






    }
}
