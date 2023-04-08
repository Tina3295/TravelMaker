using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TravelMaker.Models
{
    /// <summary>
    ///     用戶
    /// </summary>
    public class User
    {
        /// <summary>
        ///     編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "編號")]
        public int UserId { get; set; }


        /// <summary>
        ///     帳號
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [EmailAddress(ErrorMessage = "Email格式不符")]
        [MaxLength(100)]
        [Display(Name = "帳號")]
        [DataType(DataType.EmailAddress)]
        public string Account { get; set; }


        /// <summary>
        ///     密碼
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(100, ErrorMessage = "{0}長度至少必須為 {2} 個字元。", MinimumLength = 6)]
        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        /// <summary>
        ///     暱稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "暱稱")]
        public string UserName { get; set; }


        /// <summary>
        ///     頭貼
        /// </summary>
        [Display(Name = "頭貼")]
        public string ProfilePicture { get; set; }




        /// <summary>
        ///     全域唯一識別碼
        /// </summary>
        [Display(Name = "會員guid")]
        public string UserGuid { get; set; }




        /// <summary>
        ///     帳號建立日期
        /// </summary>
        [Display(Name = "帳號建立日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]  //只顯示年月日
        [DataType(DataType.DateTime)]
        public DateTime? InitDate { get; set; }





        public virtual ICollection<Tour> Tours { get; set; }
        public virtual ICollection<RoomMember> RoomMembers { get; set; }
        public virtual ICollection<VoteDate> VoteDates { get; set; }
        public virtual ICollection<AttractionComment> AttractionComments { get; set; }
        public virtual ICollection<Blog> Blogs { get; set; }
    }
}