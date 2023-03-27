using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     行政區
    /// </summary>
    public class District
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int DistrictId { get; set; }



        /// <summary>
        ///     行政區名稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "行政區名")]
        public string DistrictName { get; set; }



        /// <summary>
        ///     城市
        /// </summary>
        [Display(Name = "城市")]
        public int CityId { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }




        public virtual ICollection<Attraction> Attractions { get; set; }
    }
}