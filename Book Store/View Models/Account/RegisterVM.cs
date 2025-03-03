using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Account
{
    public class RegisterVM 
    {
        [EmailAddress]
        [Required(ErrorMessage = "*required*")]
        public string Email{ get; set; }

        [Required(ErrorMessage = "*required*")]
        [DataType(DataType.Password)]
        public string Password{ get; set; }


        [Required(ErrorMessage = "*required*")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "*required*")]
        public string Firstname { get; set; }
        [Required(ErrorMessage = "*required*")]
        public string Lastname { get; set; }
        public string? PhoneNumber { get; set; }



    }
}
