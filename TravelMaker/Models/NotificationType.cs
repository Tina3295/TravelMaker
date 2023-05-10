using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     通知類型
    /// </summary>
    public class NotificationType
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int NotificationTypeId { get; set; }



        /// <summary>
        ///     類型
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Display(Name = "類型")]
        public string Type { get; set; }


        public virtual ICollection<Notification> Notifications { get; set; }
    }
}