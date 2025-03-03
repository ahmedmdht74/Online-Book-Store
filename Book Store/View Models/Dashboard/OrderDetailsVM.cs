using Book_Store.Models;

namespace Book_Store.View_Models.Dashboard
{
    public class OrderDetailsVM
    {
        public int OrderId { get; set; }
        public int StatusStage { get; set; }
        public int StatusId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public int TotalAmount { get; set; }
        public string OrderDate { get; set; }
        public string? ShippingDate { get; set; }
        public string OrderStatus { get; set; }

        public List<BookDetailsInOrderVM> BookDetailsInOrder { get; set; }
        public List<ApplicationUser> CityDeliveries { get; set; }
        public decimal SubTotal { get; set; }
        public double? DeliveryFee { get; set; }
        public decimal? Total{ get; set; }

        //Operation Labels
        public string DeliveyId { get; set; }
    }

    public class BookDetailsInOrderVM
    {
        public int BookId { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
