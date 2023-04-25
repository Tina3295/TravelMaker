using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using TravelMaker.Models;

namespace TravelMaker.Security
{
    /// <summary>
    /// JwtToken 生成功能
    /// </summary>
    public class JwtAuthUtil
    {
        private TravelMakerDbContext _db = new TravelMakerDbContext(); // DB 連線

        /// <summary>
        /// 生成 JwtToken
        /// </summary>
        /// <param name="UserGuid">用戶識別碼</param>
        /// <param name="Account">帳號</param>
        /// <param name="UserName">用戶名</param>
        /// <param name="ProfilePicture">頭貼</param>
        /// <returns>JwtToken</returns>
        public string GenerateToken(string UserGuid, string Account, string UserName, string ProfilePicture)
        {
            // 自訂字串，驗證用，用來加密送出的 key
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"];

            // payload 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>
            {
                { "UserGuid", UserGuid},
                { "Account", Account },
                { "UserName", UserName },
                { "ProfilePicture", ProfilePicture },
                { "Exp", DateTime.Now.AddDays(7).ToString() } // JwtToken 時效設定 7 天
            };

            // 產生 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成只刷新效期的 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            // payload 從原本 token 傳遞的資料沿用，並刷新效期
            var payload = new Dictionary<string, object>
            {
                { "UserGuid", tokenData["UserGuid"] },
                { "Account", tokenData["Account"].ToString() },
                { "UserName", tokenData["UserName"].ToString() },
                { "ProfilePicture", tokenData["ProfilePicture"].ToString() },
                { "Exp", DateTime.Now.AddHours(2).ToString() } // JwtToken 時效刷新設定 2 小時
            };

            //產生刷新時效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string RevokeToken()
        {
            string secretKey = "RevokeToken"; // 故意用不同的 key 生成
            var payload = new Dictionary<string, object>
            {
                { "UserGuid", 0 },
                { "Account", "None" },
                { "UserName", "None" },
                { "ProfilePicture", "None" },
                { "Exp", DateTime.Now.AddDays(-15).ToString() } // 使 JwtToken 過期 失效
            };

            // 產生失效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }




        /// <summary>
        ///     產生更改密碼的token
        /// </summary>
        /// <returns>JwtToken</returns>
        public static string ResetPwdToken(Dictionary<string, object> Account)
        {
            string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            var token = JWT.Encode(Account, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);

            return token;
        }
    }
}