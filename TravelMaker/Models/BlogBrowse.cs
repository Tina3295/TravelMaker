using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記瀏覽記錄
    /// </summary>
    public class BlogBrowse
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogBrowseId { get; set; }


        /// <summary>
        ///     對應遊記
        /// </summary>
        [Display(Name = "對應遊記")]
        public int BlogId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }


        /// <summary>
        ///     用戶ID
        /// </summary>
        [Display(Name = "用戶ID")]
        public int UserId { get; set; }



        /// <summary>
        ///     瀏覽時間
        /// </summary>
        [Display(Name = "瀏覽時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }
    }
}