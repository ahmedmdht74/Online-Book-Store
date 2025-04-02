using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class GenreListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int GenreCount { get; set; }

        public List<GenreDetailInGenreList> GenreDetailInGenreLists { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }


        //Add - Edit Labels
        [Required(ErrorMessage = "Name is required..."),Display(Name = "New Genre")]
        public string NewGenre{ get; set; }
        [Display(Name = "Current Genre")]
        public string CurrentGenre { get; set; }
    }



    public class GenreDetailInGenreList
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int NumOfBooks { get; set; }
        public decimal Totalsales { get; set; }
        public int SoldBook { get; set; }

    }

}
