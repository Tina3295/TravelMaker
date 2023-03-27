using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     行程
    /// </summary>
    public class Tour
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int TourId { get; set; }


        /// <summary>
        ///     行程名稱
        /// </summary>
        [MaxLength(30)]
        [Display(Name = "行程名稱")]
        public string TourName { get; set; }




        /// <summary>
        ///     對應用戶
        /// </summary>
        [Display(Name = "對應用戶")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }



        /// <summary>
        ///     行程建立日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]  //只顯示年月日
        [DataType(DataType.DateTime)]
        [Display(Name = "行程建立日期")]
        public DateTime InitDate { get; set; }



        public virtual ICollection<TourAttraction> TourAttractions { get; set; }
    }
}