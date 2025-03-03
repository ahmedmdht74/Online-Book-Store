namespace Book_Store.View_Models.Dashboard
{
    public class BookListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public List<BookDetailInBookList> bookDetailInBookLists { get; set; }
        public int BookCount { get; set; }


        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }


    public class BookDetailInBookList
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public decimal Price{ get; set; }
        public int Stock { get; set; }
        public int Sold { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        
    }

}
