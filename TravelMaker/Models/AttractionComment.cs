using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     景點評論
    /// </summary>
    public class AttractionComment
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int AttractionCommentId { get; set; }


        /// <summary>
        ///     對應景點
        /// </summary>
        [Display(Name = "對應景點")]
        public int AttractionId { get; set; }

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; }


        /// <summary>
        ///     對應用戶
        /// </summary>
        [Display(Name = "對應用戶")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }


        /// <summary>
        ///     評論內容
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "評論內容")]
        public string Comment { get; set; }


        /// <summary>
        ///     星數
        /// </summary>
        [Display(Name = "星數")]
        public int Score { get; set; }


        /// <summary>
        ///     留言狀態
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "留言狀態")]
        public bool Status { get; set; }


        /// <summary>
        ///     留言日期
        /// </summary>
        [Display(Name = "留言日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]  //只顯示年月日
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }



        /// <summary>
        ///     編輯日期
        /// </summary>
        [Display(Name = "編輯日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]  //只顯示年月日
        [DataType(DataType.DateTime)]
        public DateTime? EditDate { get; set; }
    }
}