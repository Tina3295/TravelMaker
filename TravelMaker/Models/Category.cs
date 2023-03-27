using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     類別
    /// </summary>
    public class Category
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int CategoryId { get; set; }



        /// <summary>
        ///     類別名稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "類別名稱")]
        public string CategoryName { get; set; }



        public virtual ICollection<CategoryAttraction> CategoryAttractions { get; set; }
    }
}