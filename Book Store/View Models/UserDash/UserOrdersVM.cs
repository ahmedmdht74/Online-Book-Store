using Book_Store.Models;

namespace Book_Store.View_Models.UserDash
{
    public class UserOrdersVM
    {
        public int OrderId     { get; set; }
        public int TotalItems  { get; set; }
        public double? TotalPrice  { get; set; }
        public string OrderedBy   { get; set; }
        public string Date        { get; set; }
        public List<string> Images      { get; set; }
        public int StatusId { get; set; }
        public List<OrderStatus> StatusName { get; set; }
    }
}
