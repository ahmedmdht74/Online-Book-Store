using Book_Store.Models;

namespace Book_Store.View_Models.UserDash
{
    public class UserDashboardVM
    {
        public List<UserDashboardWishlist>? UserWishlist { get; set; }
        public List<UserDashboardRecommendation>? Recommendations { get; set; }
        public List<UserDashboardOrder>? Orders { get; set; }
    }



    public class UserDashboardWishlist
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string BookImage { get; set; }
        public string AuthorName { get; set; }
    }

    public class UserDashboardRecommendation
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string BookImage { get; set; }
        public string Description { get; set; }
    }


    public class UserDashboardOrder
    {
        public int OrderId { get; set; }
        public string OrderDate { get; set; }
        public string OrderedBy { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
        public List<OrderStatus> StatusNames { get; set; }
        public int StatusId { get; set; }
    }

    
    
    
    
    
    
    
}
