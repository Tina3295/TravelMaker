using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     投票日期
    /// </summary>
    public class VoteDate
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int VoteDateId { get; set; }



        /// <summary>
        ///     對應房間
        /// </summary>
        [Display(Name = "對應房間")]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }




        /// <summary>
        ///     投票日期
        /// </summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        [Display(Name = "投票日期")]
        public DateTime Date { get; set; }




        /// <summary>
        ///     提出用戶Id
        /// </summary>
        [Display(Name = "提出用戶Id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}