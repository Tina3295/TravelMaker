using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記
    /// </summary>
    public class Blog
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogId { get; set; }


        /// <summary>
        ///     遊記Guid
        /// </summary>
        [Display(Name = "遊記Guid")]
        public string BlogGuid { get; set; }



        /// <summary>
        ///     對應用戶
        /// </summary>
        [Display(Name = "對應用戶")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }


        /// <summary>
        ///     標題
        /// </summary>
        [MaxLength(30)]
        [Display(Name = "標題")]
        public string Title { get; set; }


        /// <summary>
        ///     類別
        /// </summary>
        [Display(Name = "類別")]
        public string Category { get; set; }


        /// <summary>
        ///     封面
        /// </summary>
        [Display(Name = "封面")]
        public string Cover { get; set; }


        /// <summary>
        ///     遊記狀態(0草稿1發佈2刪除)
        /// </summary>
        [Display(Name = "遊記狀態(0草稿1發佈2刪除)")]
        public int Status { get; set; }



        /// <summary>
        ///     遊記建立日期
        /// </summary>
        [Display(Name = "遊記建立日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }



        public virtual ICollection<BlogAttraction> BlogAttractions { get; set; }
    }
}