using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     登入
    /// </summary>
    public class LoginView
    {
        /// <summary>
        ///     帳號
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [EmailAddress(ErrorMessage = "Email格式不符")]
        [MaxLength(100)]
        [Display(Name = "帳號")]
        [DataType(DataType.EmailAddress)]
        public string Account { get; set; }


        /// <summary>
        ///     密碼
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(30, ErrorMessage = "{0}長度至少必須為{2}個字元。", MinimumLength = 6)]
        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }




    /// <summary>
    ///     註冊
    /// </summary>
    public class SignUpView : LoginView
    {
        /// <summary>
        ///     用戶名
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "暱稱")]
        public string UserName { get; set; }
    }


    /// <summary>
    ///     重設密碼(透過寄信)
    /// </summary>
    public class ResetPasswordView
    {
        /// <summary>
        ///     新密碼
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(30, ErrorMessage = "{0}長度至少必須為{2}個字元。", MinimumLength = 6)]
        [Display(Name = "新密碼")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }

    /// <summary>
    ///     修改密碼(會員中心)
    /// </summary>
    public class ChangePasswordView:ResetPasswordView
    {
        /// <summary>
        ///     原密碼
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(30, ErrorMessage = "{0}長度至少必須為{2}個字元。", MinimumLength = 6)]
        [Display(Name = "原密碼")]
        [DataType(DataType.Password)]
        public string OriginalPassword { get; set; }
    }





    //-------------------行程View-------------------

    /// <summary>
    ///     產生隨機行程
    /// </summary>
    public class GetToursView
    {
        /// <summary>
        ///     類別Id(多選)
        /// </summary>
        [Display(Name = "類別Id")]
        public int[] CategoryId { get; set; }



        /// <summary>
        ///     景點個數
        /// </summary>
        [Display(Name = "景點個數")]
        public int AttrCounts { get; set; }



        /// <summary>
        ///     交通方式
        /// </summary>
        [Display(Name = "交通方式")]
        public string Transports { get; set; }



        /// <summary>
        ///     鄰近經度
        /// </summary>
        [Display(Name = "鄰近經度")]
        public double Elong { get; set; }



        /// <summary>
        ///     鄰近緯度
        /// </summary>
        [Display(Name = "鄰近緯度")]
        public double Nlat { get; set; }




        /// <summary>
        ///     地區
        /// </summary>
        [Display(Name = "地區")]
        public string[] DistrictName { get; set; }




        /// <summary>
        ///     回傳景點Id
        /// </summary>
        [Display(Name = "回傳景點Id")]
        public int[] AttractionId { get; set; }
    }

    /// <summary>
    ///     符合篩選器的景點清單
    /// </summary>
    public class AttractionList
    {
        public int AttractionId { get; set; }
        public int IsRestaurant { get; set; }
    }


    /// <summary>
    ///     用戶修改原本行程按儲存-新建、覆蓋
    /// </summary>
    public class TourAddView
    {
        /// <summary>
        ///     行程名稱
        /// </summary>
        [Display(Name = "行程名稱")]
        public string TourName { get; set; }

        /// <summary>
        ///     行程景點
        /// </summary>
        [Display(Name = "行程景點")]
        public int[] AttractionId { get; set; }
    }




    /// <summary>
    ///     編輯景點
    /// </summary>
    public class AttEditView
    {
        /// <summary>
        ///     景點Id
        /// </summary>
        [Display(Name = "景點Id")]
        public int AttractionId { get; set; }

        /// <summary>
        ///     景點名稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        [Display(Name = "景點名稱")]
        public string AttractionName { get; set; }

        /// <summary>
        ///     景點介紹
        /// </summary>
        [Display(Name = "景點介紹")]
        public string Introduction { get; set; }

        /// <summary>
        ///     地區
        /// </summary>
        [Display(Name = "地區")]
        public string District { get; set; }

        /// <summary>
        ///     地址
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "地址")]
        public string Address { get; set; }

        /// <summary>
        ///     電話
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "電話")]
        public string Tel { get; set; }

        /// <summary>
        ///     Email
        /// </summary>
        [EmailAddress(ErrorMessage = "Email格式不符")]
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        ///     經度
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "經度")]
        public decimal Elong { get; set; }

        /// <summary>
        ///     緯度
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "緯度")]
        public decimal Nlat { get; set; }

        /// <summary>
        ///     官網
        /// </summary>
        [MaxLength(300)]
        [Display(Name = "官網")]
        public string OfficialSite { get; set; }

        /// <summary>
        ///     Facebook
        /// </summary>
        [MaxLength(300)]
        [Display(Name = "Facebook")]
        public string Facebook { get; set; }

        /// <summary>
        ///     開放時間
        /// </summary>
        [MaxLength(150)]
        [Display(Name = "開放時間")]
        public string OpenTime { get; set; }

        /// <summary>
        ///     類別
        /// </summary>
        [Display(Name = "類別")]
        public string[] Category { get; set; }

        /// <summary>
        ///     圖片
        /// </summary>
        [Display(Name = "圖片")]
        public string[] ImageNames { get; set; }
    }




    //-------------------房間View-------------------

    /// <summary>
    ///     新增房間
    /// </summary>
    public class RoomAddView
    {
        /// <summary>
        ///     房間名稱
        /// </summary>
        [Display(Name = "房間名稱")]
        public string RoomName { get; set; }


        /// <summary>
        ///     景點Id
        /// </summary>
        [Display(Name = "景點Id")]
        public int[] Attractions { get; set; }
    }



    /// <summary>
    ///     修改房間名稱
    /// </summary>
    public class RoomNameView
    {
        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }
        /// <summary>
        ///     房間名稱
        /// </summary>
        [Display(Name = "房間名稱")]
        public string RoomName { get; set; }
    }



    /// <summary>
    ///     主揪,被揪編輯(儲存)房間-房間資訊(行程)
    /// </summary>
    public class RoomModifyView
    {
        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }


        /// <summary>
        ///     房間景點
        /// </summary>
        [Display(Name = "房間景點")]
        public List<RoomModifyData> AttrationsData { get; set; }
    }
    public class RoomModifyData
    {
        public int AttractionId { get; set; }
        public string UserGuid { get; set; }
        public int Order { get; set; }
    }









    /// <summary>
    ///     投票日期相關
    /// </summary>
    public class DateView
    {
        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }


        /// <summary>
        ///     投票日期(格式:2023-05-03)
        /// </summary>
        [Display(Name = "投票日期")]
        public string Date { get; set; }
    }




    /// <summary>
    ///     主揪新增被揪
    /// </summary>
    public class RoomMemberAddView
    {
        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }

        /// <summary>
        ///     被揪Emil
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [EmailAddress(ErrorMessage = "Email格式不符")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "被揪Emil")]
        public string UserEmail { get; set; }
    }




    /// <summary>
    ///     主揪刪除被揪
    /// </summary>
    public class RoomMemberDelView
    {
        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }

        /// <summary>
        ///     被揪Guid
        /// </summary>
        [Display(Name = "被揪Guid")]
        public string UserGuid { get; set; }
    }



    /// <summary>
    ///     主揪,被揪加景點進房間
    /// </summary>
    public class AttractionAddView
    {
        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }

        /// <summary>
        ///     景點Id
        /// </summary>
        [Display(Name = "景點Id")]
        public int AttractionId { get; set; }
    }



    /// <summary>
    ///     試玩行程
    /// </summary>
    public class TourTryView
    {
        /// <summary>
        ///     類別
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     四個景點
        /// </summary>
        public List<object> AttractionData { get; set; }
    }





    //-------------------景點View-------------------
 

    /// <summary>
    ///     取得更多景點評論
    /// </summary>
    public class MoreCommentView
    {
        public int AttractionId { get; set; }
        public string Order { get; set; }
        public int Page { get; set; }
    }

    /// <summary>
    ///     新增單一景點評論
    /// </summary>
    public class AddAttractionView
    {
        /// <summary>
        ///     景點Id
        /// </summary>
        public int AttractionId { get; set; }
        /// <summary>
        ///     景點評論
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        ///     景點評分
        /// </summary>
        public int Score { get; set; }
    }


    /// <summary>
    ///     編輯評論
    /// </summary>
    public class EditAttractionCommentsView
    {
        /// <summary>
        ///     景點評論Id
        /// </summary>
        public int AttractionCommentId { get; set; }
        /// <summary>
        ///     評論
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        ///     星數
        /// </summary>
        public int Score { get; set; }
    }

    //-------------------遊記View-------------------


    /// <summary>
    ///     編輯遊記
    /// </summary>
    public class BlogEditView
    {
        /// <summary>
        ///     遊記Guid
        /// </summary>
        public string BlogGuid { get; set; }
        /// <summary>
        ///     標題
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        ///     封面
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        ///     類別
        /// </summary>
        public string[] Category { get; set; }
        /// <summary>
        ///     遊記景點List
        /// </summary>
        public List<BlogAttractionList> BlogAttractionList { get; set; }
    }
    /// <summary>
    ///     遊記景點List
    /// </summary>
    public class BlogAttractionList
    {
        /// <summary>
        ///     景點Id
        /// </summary>
        public int AttractionId { get; set; }
        /// <summary>
        ///     心得描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        ///     圖片
        /// </summary>
        public string[] ImageUrl { get; set; }
    }




    /// <summary>
    ///     新增留言
    /// </summary>
    public class AddBlogCommentsView
    {
        /// <summary>
        ///     遊記Guid
        /// </summary>
        public string BlogGuid { get; set; }
        /// <summary>
        ///     留言內容
        /// </summary>
        public string Comment { get; set; }
    }
    /// <summary>
    ///     編輯留言
    /// </summary>
    public class EditBlogCommentsView
    {
        /// <summary>
        ///     遊記留言Id
        /// </summary>
        public int BlogCommentId { get; set; }
        /// <summary>
        ///     留言內容
        /// </summary>
        public string Comment { get; set; }
    }


    /// <summary>
    ///     新增回覆
    /// </summary>
    public class AddBlogCommentRepliesView
    {
        /// <summary>
        ///     遊記留言Id
        /// </summary>
        public int BlogCommentId { get; set; }
        /// <summary>
        ///     回覆內容
        /// </summary>
        public string Reply { get; set; }
    }
    /// <summary>
    ///     編輯留言
    /// </summary>
    public class EditBlogCommentRepliesView
    {
        /// <summary>
        ///     遊記回覆Id
        /// </summary>
        public int BlogReplyId { get; set; }
        /// <summary>
        ///     回覆內容
        /// </summary>
        public string Reply { get; set; }
    }




    /// <summary>
    ///     funtion
    /// </summary>
    public class Tool
    {
        /// <summary>
        ///     處理評論時間顯示 分鐘 小時 日 週 月
        /// </summary>
        /// <param name="dateTime">日期時間</param>
        /// <returns></returns>
        public static string CommentTime(DateTime dateTime)
        {
            TimeSpan timeSince = DateTime.Now.Subtract(dateTime);

            if (timeSince.TotalMinutes < 1)
            {
                return "剛剛";
            }
            else if (timeSince.TotalMinutes < 60)
            {
                return (int)timeSince.TotalMinutes + "分鐘前";
            }
            else if (timeSince.TotalHours < 24)
            {
                return (int)timeSince.TotalHours + "小時前";
            }
            else if (timeSince.TotalDays < 7)
            {
                return (int)timeSince.TotalDays + "天前";
            }
            else if (timeSince.TotalDays < 30)
            {
                return (int)timeSince.TotalDays / 7 + "週前";
            }
            else if (timeSince.TotalDays < 365)
            {
                return (int)timeSince.TotalDays / 30 + "個月前";
            }
            else
            {
                return (int)timeSince.TotalDays / 365 + "年前";
            }
        }



        /// <summary>
        ///     檢查是否為圖片檔
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public static bool IsImage(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp" || ext == ".svg" || ext == ".ico";
        }
    }






    
}