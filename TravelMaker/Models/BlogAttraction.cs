using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     遊記中的景點
    /// </summary>
    public class BlogAttraction
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int BlogAttractionId { get; set; }




        /// <summary>
        ///     對應遊記
        /// </summary>
        [Display(Name = "對應遊記")]
        public int BlogId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }




        /// <summary>
        ///     景點Id
        /// </summary>
        [Display(Name = "景點Id")]
        public int AttractionId { get; set; }





        /// <summary>
        ///     心得
        /// </summary>
        [Display(Name = "心得")]
        [MaxLength(1000)]
        public string Description { get; set; }



        /// <summary>
        ///     遊記景點建立日期
        /// </summary>
        [Display(Name = "遊記景點建立日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }



        public virtual ICollection<BlogImage> BlogImages { get; set; }
    }
}