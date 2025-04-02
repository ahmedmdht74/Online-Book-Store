namespace Book_Store.View_Models.Dashboard
{
    public class OrderListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int OrderCount { get; set; }

        public string method { get; set; }
        public string status { get; set; }


        public List<OrderDetailInOrderList> OrderDetailInOrderList { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class OrderDetailInOrderList
    {
        public int OrderId { get; set; }
        public string Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod  { get; set; }
        public string OrderStatus { get; set; }
        public int OrderStatusId { get; set; }
        public string OrderDate { get; set; }
    }
}
