using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     追蹤
    /// </summary>
    public class BlogFollower
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogFollowerId { get; set; }


        /// <summary>
        ///     對應用戶(被追蹤者)
        /// </summary>
        [Display(Name = "對應用戶(被追蹤者)")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }



        /// <summary>
        ///     追蹤者
        /// </summary>
        [Display(Name = "追蹤者")]
        public int FollowingUserId { get; set; }




        /// <summary>
        ///     追蹤日期
        /// </summary>
        [Display(Name = "追蹤日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }
    }
}