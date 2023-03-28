using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    ///     重設密碼
    /// </summary>
    public class ResetPasswordView
    {
        /// <summary>
        ///     原密碼
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(30, ErrorMessage = "{0}長度至少必須為{2}個字元。", MinimumLength = 6)]
        [Display(Name = "原密碼")]
        [DataType(DataType.Password)]
        public string OriginalPassword { get; set; }


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
    ///     產生隨機行程
    /// </summary>
    public class GetJourneysView
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
    ///     用戶修改原本行程按儲存-新建
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
    ///     取得我的收藏行程
    /// </summary>
    public class FavoriteTour
    {
        /// <summary>
        ///     行程Id
        /// </summary>
        [Display(Name = "行程Id")]
        public int TourId { get; set; }

        /// <summary>
        ///     行程名稱
        /// </summary>
        [Display(Name = "行程名稱")]
        public string TourName { get; set; }


        /// <summary>
        ///     景點數量
        /// </summary>
        [Display(Name = "景點數量")]
        public int AttrCounts { get; set; }


        /// <summary>
        ///     愛心數
        /// </summary>
        [Display(Name = "愛心數")]
        public int Likes { get; set; }


        /// <summary>
        ///     景點圖片路徑
        /// </summary>
        [Display(Name = "景點圖片路徑")]
        public List<string> ImageUrl { get; set; } 
    }








    /// <summary>
    ///     取得單一用戶收藏行程頁面
    /// </summary>
    public class TourView
    {
        /// <summary>
        ///     行程Id
        /// </summary>
        [Display(Name = "行程Id")]
        public int TourId { get; set; }

        /// <summary>
        ///     行程名稱
        /// </summary>
        [Display(Name = "行程名稱")]
        public string TourName { get; set; }

        /// <summary>
        ///     行程擁有者
        /// </summary>
        [Display(Name = "行程擁有者")]
        public string UserGuid { get; set; }

        /// <summary>
        ///     行程景點
        /// </summary>
        [Display(Name = "行程景點")]
        public List<object> Attractions { get; set; }
    }








    /// <summary>
    ///     複製行程
    /// </summary>
    public class DuplicateTourView
    {
        /// <summary>
        ///     行程Id
        /// </summary>
        [Display(Name = "行程Id")]
        public int TourId { get; set; }

        /// <summary>
        ///     行程名稱
        /// </summary>
        [Display(Name = "行程名稱")]
        public string TourName { get; set; }
    }





    /// <summary>
    ///     取得我的收藏行程
    /// </summary>
    public class TourModifyView: TourAddView
    {
        /// <summary>
        ///     行程Id
        /// </summary>
        [Display(Name = "行程Id")]
        public int TourId { get; set; }
    }
}