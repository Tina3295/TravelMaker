using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     用戶按喜歡的行程愛心(熱門依據)
    /// </summary>
    public class TourLike
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int TourLikeId { get; set; }




        /// <summary>
        ///     對應行程
        /// </summary>
        [Display(Name = "對應行程")]
        public int TourId { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour Tour { get; set; }



        /// <summary>
        ///     按愛心的用戶id
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "用戶Id")]
        public int UserId { get; set; }



        /// <summary>
        ///     按愛心日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]  //只顯示年月日
        [DataType(DataType.DateTime)]
        [Display(Name = "建立日期")]
        public DateTime? InitDate { get; set; }
    }
}