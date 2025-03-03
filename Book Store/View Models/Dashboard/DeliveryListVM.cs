using Book_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class DeliveryListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int DeliveryCount { get; set; }

        public List<UserDetailsInUserList> UserDetailsInUserLists { get; set; }
        public AddDeliveryVM AddDelivery { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<State> States { get; set; }
        public string ChosenState { get; set; }
        public int ChosenStateId { get; set; }

        //Operation
        [Display(Name = "City")]
        public int ChangedCityId { get; set; }
    }

    public class AddDeliveryVM
    {
        [Required(ErrorMessage ="*required*"),Display(Name = "Firstname")]
        public string Firstname { get; set; }
        [Required(ErrorMessage = "*required*"), Display(Name = "Lastname")]
        public string Lastname { get; set; }
        [Required(ErrorMessage = "*required*"), Display(Name = "Email"),EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "*required*"), Display(Name = "Phone"), DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Required(ErrorMessage = "*required*"), Display(Name = "Password"),DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "*required*"), Display(Name = "City")]
        public int CityId { get; set; }

    }
}
