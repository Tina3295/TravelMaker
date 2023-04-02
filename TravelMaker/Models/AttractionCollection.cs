using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     景點收藏
    /// </summary>
    public class AttractionCollection
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int AttractionCollectionId { get; set; }


        /// <summary>
        ///     對應景點
        /// </summary>
        [Display(Name = "對應景點")]
        public int AttractionId { get; set; }

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; }



        /// <summary>
        ///     用戶Id
        /// </summary>
        [Display(Name = "用戶Id")]
        public int UserId { get; set; }
    }
}