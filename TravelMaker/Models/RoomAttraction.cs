using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     房間景點
    /// </summary>
    public class RoomAttraction
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int RoomAttractionId { get; set; }



        /// <summary>
        ///     對應房間
        /// </summary>
        [Display(Name = "對應房間")]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }





        /// <summary>
        ///     對應景點
        /// </summary>
        [Display(Name = "對應景點")]
        public int AttractionId { get; set; }

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; }




        /// <summary>
        ///     主要景點1~8/備用景點0
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "主要景點/備用景點")]
        public int AttrOrder { get; set; }



        /// <summary>
        ///     提出的用戶Id
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "提出的用戶Id")]
        public int UserId { get; set; }
    }
}