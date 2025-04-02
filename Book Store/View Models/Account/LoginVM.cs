using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Account
{
    public class LoginVM
    {
        public string? returnURL{ get; set; }

        [MaxLength(50)]
        [Required]
        public string Email{ get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember me")]
        public bool RememberMe{ get; set; }
    }
}
