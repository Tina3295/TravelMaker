using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     景點
    /// </summary>
    public class Attraction
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int AttractionId { get; set; }



        /// <summary>
        ///     景點名稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(30)]
        [Display(Name = "景點名稱")]
        public string AttractionName { get; set; }



        /// <summary>
        ///     景點介紹
        /// </summary>
        [Display(Name = "景點介紹")]
        public string Introduction { get; set; }



        /// <summary>
        ///     營業狀態
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "營業狀態")]
        public bool OpenStatus { get; set; }



        /// <summary>
        ///     地區
        /// </summary>
        [Display(Name = "地區")]
        public int DistrictId { get; set; }

        [ForeignKey("DistrictId")]
        public virtual District District { get; set; }



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
        [MaxLength(50)]
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
        ///     地理資料
        /// </summary>
        [Display(Name = "地理資料")]
        public DbGeography Location { get; set; }



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
        ///     景點建立日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]  //只顯示年月日
        [DataType(DataType.DateTime)]
        [Display(Name = "景點建立日期")]
        public DateTime? InitDate { get; set; }


        public virtual ICollection<CategoryAttraction> CategoryAttractions { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<TourAttraction> TourAttractions { get; set; }
        public virtual ICollection<RoomAttraction> RoomAttractions { get; set; }
        public virtual ICollection<AttractionCollection> AttractionCollections { get; set; }
    }
}