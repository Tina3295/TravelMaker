using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記評論
    /// </summary>
    public class BlogComment
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogCommentId { get; set; }



        /// <summary>
        ///     對應遊記
        /// </summary>
        [Display(Name = "對應遊記")]
        public int BlogId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }




        /// <summary>
        ///     用戶Id
        /// </summary>
        [Required]
        [Display(Name = "用戶Id")]
        public int UserId { get; set; }




        /// <summary>
        ///     評論內容
        /// </summary>
        [MaxLength(500)]
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "評論內容")]
        public string Comment { get; set; }



        /// <summary>
        ///     評論狀態
        /// </summary>
        [Required]
        [Display(Name = "評論狀態")]
        public bool Status { get; set; }



        /// <summary>
        ///     評論建立日期
        /// </summary>
        [Display(Name = "評論建立日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }



        /// <summary>
        ///     評論編輯日期
        /// </summary>
        [Display(Name = "評論編輯日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? EditDate { get; set; }



        public virtual ICollection<BlogReply> BlogReplies { get; set; }
    }
}