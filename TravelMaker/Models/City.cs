using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     城市
    /// </summary>
    public class City
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int CityId { get; set; }



        /// <summary>
        ///     城市名稱
        /// </summary>
        [Required]
        [Display(Name = "城市名稱")]
        [MaxLength(10)]
        public string CittyName { get; set; }


        public virtual ICollection<District> District { get; set; }
    }
}