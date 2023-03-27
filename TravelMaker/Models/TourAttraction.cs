using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     行程景點
    /// </summary>
    public class TourAttraction
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int TourAttractionId { get; set; }



        /// <summary>
        ///     對應行程
        /// </summary>
        [Display(Name = "對應行程")]
        public int TourId { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour Tour { get; set; }



        /// <summary>
        ///     對應景點
        /// </summary>
        [Display(Name = "對應景點")]
        public int AttractionId { get; set; }

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; }



        /// <summary>
        ///     順序
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "順序")]
        public int OrderNum { get; set; }
    }
}