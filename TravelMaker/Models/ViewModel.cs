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
    ///     給參數搜尋行程(熱門話題取得所有行程)
    /// </summary>
    public class TourSearch : FavoriteTour
    {
        /// <summary>
        ///     是否按愛心
        /// </summary>
        [Display(Name = "是否按愛心")]
        public bool IsLike { get; set; }
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
    ///     房間資訊
    /// </summary>
    public class RoomContentView
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

        /// <summary>
        ///     房主Guid
        /// </summary>
        [Display(Name = "房主Guid")]
        public string CreaterGuid { get; set; }

        /// <summary>
        ///     房客
        /// </summary>
        [Display(Name = "房客")]
        public List<object> Users { get; set; }


        /// <summary>
        ///     投票日期
        /// </summary>
        [Display(Name = "投票日期")]
        public List<object> VoteDates { get; set; }



        /// <summary>
        ///     房間景點
        /// </summary>
        [Display(Name = "房間景點")]
        public List<object> AttrationsData { get; set; }
    }
    public class UsersData
    {
        public string UserGuid { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
    }
    public class AttrationsData
    {
        public int AttractionId { get; set; }
        public string UserGuid { get; set; }
        public string AttractionName { get; set; }
        public decimal Elong { get; set; }
        public decimal Nlat { get; set; }
        public string ImageUrl { get; set; }
        public int Order { get; set; }
    }
    public class VoteDatesData
    {
        public int VoteDateId { get; set; }
        public string Date { get; set; }
        public int Count { get; set; }
        public bool IsVoted { get; set; }
    }



    /// <summary>
    ///     取得我的房間
    /// </summary>
    public class MyRoom
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


        /// <summary>
        ///     景點數量
        /// </summary>
        [Display(Name = "景點數量")]
        public int AttrCounts { get; set; }


        /// <summary>
        ///     房主名字
        /// </summary>
        [Display(Name = "房主名字")]
        public string CreaterName { get; set; }


        /// <summary>
        ///     景點圖片路徑
        /// </summary>
        [Display(Name = "景點圖片路徑")]
        public List<string> ImageUrl { get; set; }
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





    /// <summary>
    ///     首頁-取得熱門行程*4、取得熱門景點*3
    /// </summary>
    public class HomePageView
    {
        /// <summary>
        ///     熱門行程*4
        /// </summary>
        [Display(Name = "熱門行程*4")]
        public List<HomePageTour> Tours { get; set; }

        /// <summary>
        ///     熱門景點*3
        /// </summary>
        [Display(Name = "熱門景點*3")]
        public List<AttractionView> Attractions { get; set; }
    }

    public class HomePageTour
    {
        public int TourId { get; set; }
        public string TourName { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int Likes { get; set; }
        public bool IsLike { get; set; }
        public List<HomePageTourAttraction> Attractions { get; set; }
    }
    public class HomePageTourAttraction
    {
        public string AttractionName { get; set; }
        public double Elong { get; set; }
        public double Nlat { get; set; }
    }






    //-------------------景點View-------------------
    /// <summary>
    ///     取得我的收藏景點
    /// </summary>
    public class MyAttractionCollectionsView
    {
        /// <summary>
        ///     景點id
        /// </summary>
        [Display(Name = "景點id")]
        public int AttractionId { get; set; }

        /// <summary>
        ///     景點名稱
        /// </summary>
        [Display(Name = "景點名稱")]
        public string AttractionName { get; set; }

        /// <summary>
        ///     景點地點
        /// </summary>
        [Display(Name = "景點地點")]
        public string CityDistrict { get; set; }

        /// <summary>
        ///     平均評分星數
        /// </summary>
        [Display(Name = "平均評分星數")]
        public int AverageScore { get; set; }

        /// <summary>
        ///     類別
        /// </summary>
        [Display(Name = "類別")]
        public List<string> Category { get; set; }


        /// <summary>
        ///     圖片
        /// </summary>
        [Display(Name = "圖片")]
        public string ImageUrl { get; set; }
    }

    /// <summary>
    ///     給參數搜尋景點(熱門話題取得所有景點)
    /// </summary>
    public class AttractionView:MyAttractionCollectionsView
    {
        /// <summary>
        ///     是否收藏
        /// </summary>
        [Display(Name = "是否收藏")]
        public bool IsCollect { get; set; }
    }






    /// <summary>
    ///     取得單一景點資訊
    /// </summary>
    public class AttractionInfoView 
    {
        /// <summary>
        ///     景點資訊
        /// </summary>
        public AttractionData AttractionData { get; set; }
        /// <summary>
        ///     評論資訊
        /// </summary>
        public CommentData CommentData { get; set; }
        /// <summary>
        ///     更多景點*3
        /// </summary>
        public List<object> MoreAttractions { get; set; }
    }
    public class AttractionData
    {
        public bool IsCollect { get; set; }
        public int AttractionId { get; set; }
        public string AttractionName { get; set; }
        public string Introduction { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string OfficialSite { get; set; }
        public string Facebook { get; set; }
        public string OpenTime { get; set; }
        public List<string> ImageUrl { get; set; }
    }




    public class CommentData
    {
        public int AverageScore { get; set; }
        public List<Comments> Comments { get; set; }
    }
    public class Comments
    {
        public int AttractionCommentId { get; set; }
        public bool IsMyComment { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }
        public string InitDate { get; set; }
    }



    /// <summary>
    ///     取得更多景點評論
    /// </summary>
    public class MoreCommentView
    {
        public int AttractionId { get; set; }
        public string Order { get; set; }
        public int Page { get; set; }
    }

}