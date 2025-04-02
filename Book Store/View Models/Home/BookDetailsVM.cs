namespace Book_Store.View_Models.Home
{
    public class BookDetailsVM
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public string BookImage { get; set; }
        public decimal Price { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int Quantity { get; set; }
        public int Rating { get; set; }
        public int NumOfPeaple { get; set; }
    }
}
