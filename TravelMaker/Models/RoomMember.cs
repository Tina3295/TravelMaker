using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     房間成員
    /// </summary>
    public class RoomMember
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int RoomMemberId { get; set; }



        /// <summary>
        ///     對應用戶
        /// </summary>
        [Display(Name = "對應用戶")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }



        /// <summary>
        ///     對應房間
        /// </summary>
        [Display(Name = "對應房間")]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }




        /// <summary>
        ///     房主1房客2
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "房主1房客2")]
        public int Permission { get; set; }




        /// <summary>
        ///     加入房間日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        [Display(Name = "加入房間日期")]
        public DateTime? InitDate { get; set; }
    }
}