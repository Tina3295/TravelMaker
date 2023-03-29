using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     房間
    /// </summary>
    public class Room
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int RoomId { get; set; }



        /// <summary>
        ///     房間名稱
        /// </summary>
        [MaxLength(30)]
        [Display(Name = "房間名稱")]
        public string RoomName { get; set; }



        /// <summary>
        ///     房間狀態
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "房間狀態")]
        public bool Status { get; set; }



        /// <summary>
        ///     房間guid
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "房間guid")]
        public string RoomGuid { get; set; }



        /// <summary>
        ///     房間建立日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        [Display(Name = "房間建立日期")]
        public DateTime? InitDate { get; set; }



        public virtual ICollection<RoomMember> RoomMembers { get; set; }
        public virtual ICollection<RoomAttraction> RoomAttractions  { get; set; }
    }
}