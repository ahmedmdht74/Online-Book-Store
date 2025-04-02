using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class UserListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int UserCount { get; set; }
        public string RoleFilter { get; set; }
        public int RoleFilterId { get; set; }

        public List<IdentityRole> Roles { get; set; }
        public List<UserDetailsInUserList> UserDetailsInUserLists { get; set; }
        public List<RoleDetailsInUserList> RoleDetailsInUserList { get; set; }
        [Required(ErrorMessage = "*required*"),Display(Name = "Role")]
        public string ModalRoleInput { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserDetailsInUserList
    {
        public string Id { get; set; }
        public int OrdersNum { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> Role { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool IsLocked { get; set; }


        public string? AssignedCity { get; set; }
        public string? AssignedState { get; set; }
    }


    public class RoleDetailsInUserList
    {
        public string RoleId { get; set; }
        public string Name { get; set; }
        public int UserNumber { get; set; }
    }
}
