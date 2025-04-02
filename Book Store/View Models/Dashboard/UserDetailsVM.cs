using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class UserDetailsVM
    {
        public string UserId { get; set; }
        public string Image{ get; set; }
        public string FullName{ get; set; }
        [Required(ErrorMessage ="*required*"),Display(Name = "Email"),EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Firstname")]
        public string? Firstname { get; set; }
        [Display(Name = "Lastname")]
        public string? Lastname { get; set; }
        [Display(Name = "Phone"),DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }
        [Display(Name = "Roles")]
        public string UserRoles { get; set; }

        [DataType(DataType.Password),Required(ErrorMessage = "*required*")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password),Required(ErrorMessage = "*required*"),Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }

        public List<OrderDetailInOrderList> UserOrders { get; set; }

        public List<UserFavInUserDetailsVM> UserFav { get; set; }

        public List<UserRoleInUserDetailsVM> UserRoleInUserDetails { get; set; }
    }

    public class UserRoleInUserDetailsVM
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsExisted { get; set; }
    }

    public class UserFavInUserDetailsVM
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }

}
