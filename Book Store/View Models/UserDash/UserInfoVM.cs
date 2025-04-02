using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.UserDash
{
    public class UserInfoVM
    {
        public PersonalInfo PersonalInfo { get; set; }
        public ChangePassword ChangePassword { get; set; }
    }

    public class PersonalInfo
    {
        public string? Image { get; set; }
        [Display(Name = "Firstname")]
        public string? Firstname { get; set; }
        [Display(Name = "Lastname")]
        public string? Lastname { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }
    }

    public class ChangePassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Passward")]
        public string OldPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Passward")]
        public string NewPassword { get; set; }
        [Required]
        [Display(Name = "Confirm New Passward")]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }


}
