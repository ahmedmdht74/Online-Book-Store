namespace Book_Store.View_Models.Home
{
    public class CartDetailsVM
    {
        public List<BookDetailsVM> booksDetails { get; set; }
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
    }
}
