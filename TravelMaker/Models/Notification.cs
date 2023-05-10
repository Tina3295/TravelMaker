using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     通知
    /// </summary>
    public class Notification
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int NotificationId { get; set; }


        /// <summary>
        ///     狀態
        /// </summary>
        [Required]
        [Display(Name = "狀態")]
        public bool Status { get; set; }


        /// <summary>
        ///     是否已讀
        /// </summary>
        [Required]
        [Display(Name = "是否已讀")]
        public bool IsRead { get; set; }



        /// <summary>
        ///     發通知的用戶ID
        /// </summary>
        [Display(Name = "發通知的人")]
        public int Sender { get; set; }



        /// <summary>
        ///     收通知的用戶ID
        /// </summary>
        [Display(Name = "收通知的人")]
        public int Receiver { get; set; }




        /// <summary>
        ///     對應通知類型
        /// </summary>
        [Display(Name = "對應通知類型")]
        public int NotificationTypeId { get; set; }

        [ForeignKey("NotificationTypeId")]
        public virtual NotificationType NotificationType { get; set; }



        /// <summary>
        ///     通知日期
        /// </summary>
        [Display(Name = "通知日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }



        /// <summary>
        ///     房間Guid
        /// </summary>
        [Display(Name = "房間Guid")]
        public string RoomGuid { get; set; }

        /// <summary>
        ///     舊房間名稱
        /// </summary>
        [MaxLength(30)]
        [Display(Name = "舊房間名稱")]
        public string OldRoomName { get; set; }

        /// <summary>
        ///     新房間名稱
        /// </summary>
        [MaxLength(30)]
        [Display(Name = "新房間名稱")]
        public string NewRoomName { get; set; }


        /// <summary>
        ///     新增的投票日期
        /// </summary>
        [MaxLength(10)]
        [Display(Name = "新增的投票日期")]
        public string AddVoteDate { get; set; }


        /// <summary>
        ///     遊記Guid
        /// </summary>
        [Display(Name = "遊記Guid")]
        public string BlogGuid { get; set; }
    }
}