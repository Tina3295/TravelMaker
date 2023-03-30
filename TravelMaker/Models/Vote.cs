using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     投票記錄
    /// </summary>
    public class Vote
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int VoteId { get; set; }


        /// <summary>
        ///     投票用戶Id
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "投票用戶Id")]
        public int UserId { get; set; }



        /// <summary>
        ///     對應投票日期
        /// </summary>
        [Display(Name = "對應投票日期")]
        public int VoteDateId { get; set; }

        [ForeignKey("VoteDateId")]
        public virtual VoteDate VoteDate { get; set; }
    }
}