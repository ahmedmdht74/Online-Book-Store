using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class AuthorListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int AuthorCount { get; set; }

        public List<AuthorDetailInAuthorList> AuthorDetailInAuthorList { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }


        //Add - Edit Labels
        [Required(ErrorMessage = "Name is required..."), Display(Name = "New Author")]
        public string NewAuthor { get; set; }
        [Display(Name = "Current Author")]
        public string CurrentAuthor { get; set; }
    }

    public class AuthorDetailInAuthorList
    {
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int BookPublish { get; set; }
        public decimal Totalsales { get; set; }
    }
}
