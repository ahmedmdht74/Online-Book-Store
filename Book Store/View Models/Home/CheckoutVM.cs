using Book_Store.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Home
{
    public class CheckoutVM
    {
        [Required]
        [Display(Name = "City")]
        public int CityId { get; set; }


        [Required]
        [Display(Name = "Firstname")]
        public string Firstname { get; set; }


        [Required]
        [Display(Name = "Lastname")]
        public string Lastname { get; set; }


        [Display(Name = "PhoneNumber")]
        [DataType(DataType.PhoneNumber)]
        [Required]
        public string Phone { get; set; }


        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; }


        [Required]
        [Display(Name = "Delivery Address")]
        public string Address { get; set; }


        [Display(Name = "Additional Delivery Instructions")]
        public string? AdditionNotice { get; set; }


        [Required(ErrorMessage = "Ypu should choose payment method...")]
        public string PaymentMethod { get; set; }


        public List<State>? States { get; set; }


        public int CartItems { get; set; }


        public dynamic? CartBookDetails { get; set; }



        public double? Subtotal { get; set; }
        public double? Discount { get; set; }
        public double? TotalAmountDue { get; set; }
        public double? DeliveryFee { get; set; }




    }
}
