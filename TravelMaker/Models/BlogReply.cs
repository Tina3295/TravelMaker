using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記留言回復
    /// </summary>
    public class BlogReply
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogReplyId { get; set; }



        /// <summary>
        ///     對應遊記留言
        /// </summary>
        [Display(Name = "對應遊記留言")]
        public int BlogCommentId { get; set; }

        [ForeignKey("BlogCommentId")]
        public virtual BlogComment BlogComment { get; set; }



        /// <summary>
        ///     用戶Id
        /// </summary>
        [Required]
        [Display(Name = "用戶Id")]
        public int UserId { get; set; }



        /// <summary>
        ///     回覆內容
        /// </summary>
        [MaxLength(500)]
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "回覆內容")]
        public string Reply { get; set; }



        /// <summary>
        ///     回覆狀態
        /// </summary>
        [Required]
        [Display(Name = "回覆狀態")]
        public bool Status { get; set; }



        /// <summary>
        ///     回覆建立日期
        /// </summary>
        [Display(Name = "回覆建立日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }



        /// <summary>
        ///     回覆編輯日期
        /// </summary>
        [Display(Name = "回覆編輯日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? EditDate { get; set; }
    }
}