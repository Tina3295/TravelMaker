using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     景點圖片
    /// </summary>
    public class Image
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int ImageId { get; set; }



        /// <summary>
        ///     對應景點
        /// </summary>
        [Display(Name = "對應景點")]
        public int AttractionId { get; set; }

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; }



        /// <summary>
        ///     圖片名稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "圖片名稱")]
        public string ImageName { get; set; }
    }
}