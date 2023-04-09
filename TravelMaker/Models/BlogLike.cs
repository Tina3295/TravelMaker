using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記喜歡數
    /// </summary>
    public class BlogLike
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogLikeId { get; set; }



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
    }
}