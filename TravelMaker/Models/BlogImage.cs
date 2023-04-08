using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記照片
    /// </summary>
    public class BlogImage
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogImageId { get; set; }



        /// <summary>
        ///     對應遊記
        /// </summary>
        [Display(Name = "對應遊記")]
        public int BlogAttractionId { get; set; }

        [ForeignKey("BlogAttractionId")]
        public virtual BlogAttraction BlogAttraction { get; set; }




        /// <summary>
        ///     圖片名稱
        /// </summary>
        [Display(Name = "圖片名稱")]
        public string ImageName { get; set; }




        /// <summary>
        ///     照片上傳日期
        /// </summary>
        [Display(Name = "照片上傳日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }
    }
}