namespace Book_Store.View_Models.Dashboard
{
    public class HomeDashVM
    {
        public string AdminName { get; set; }
        public decimal TotalSales { get; set; }
        public int Orders { get; set; }
        public int NewCustomers { get; set; }
        public decimal Revenue { get; set; }
        public List<PieChartData> pieChartData { get; set; }
        public List<RecentOrder> RecentOrders { get; set; }
        public List<StackedLineChartData> StackedLineChartDatas { get; set; }
        public List<TopSellingBook> TopSellingBooks { get; set; }
        public List<LowStockBook> LowStockBooks { get; set; }
        
        public string Period { get; set; }
    }

    public class PieChartData
    {
        public string GenreName;
        public decimal Quantity;
    }

    public class StackedLineChartData
    {
        public string Month;
        public int Order;
        public int User;
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string Date { get; set; }
        public string Customer { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
    }

    public class TopSellingBook
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Sold { get; set; }
    }

    public class LowStockBook
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}
