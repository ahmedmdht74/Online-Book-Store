using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ApplicationUser"), Display(Name = "User")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ShippingDate { get; set; }
        public string PaymentMethod { get; set; }
        [ForeignKey("OrderStatus"), Display(Name = "Order Status")]
        public int StatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<OrderDetails> OrderDetails { get; set; }



        [ForeignKey("City")]
        public int CityId { get; set; }
        public City City { get; set; }


        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Address { get; set; }

        public string? AdditionNotice { get; set; }

        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }

        public double? DeliveryFee{ get; set; }


        public string? CityDeliveryId { get; set; }
        [ForeignKey("CityDeliveryId")]
        public ApplicationUser? CityDelivery { get; set; }


    }
}
