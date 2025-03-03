using Book_Store.Models;

namespace Book_Store.View_Models.UserDash
{
    public class UserOrderdetailsVM
    {
        public int OrderId       { get; set; }
        public string CreatedDate   { get; set; }
        public int StatusId      { get; set; }
        public List<OrderStatus> StatusName    { get; set; }
        public string Firstname     { get; set; }
        public string Lastname      { get; set; }
        public string Email         { get; set; }
        public string Phone         { get; set; }
        public string Address       { get; set; }
        public string City          { get; set; }
        public string State         { get; set; }
        public string ShippingDate  { get; set; }
        public List<UserOrderItemDetailsVM> OrderItems    { get; set; }
        public decimal Subtotal      { get; set; }
        public double Shipping      { get; set; }
        public decimal Total { get; set; }
    }

    public class UserOrderItemDetailsVM
    {
        public int BookId    { get; set; }
        public string BookName  { get; set; }
        public string BookImage { get; set; }
        public int Quantity  { get; set; }
        public decimal Price     { get; set; }
        public decimal Total { get; set; }
    }

}
