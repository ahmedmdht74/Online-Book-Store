using Book_Store.Models;

namespace Book_Store.View_Models.Home
{
    public class SearchPageVM
    {
        public List<BookDetailsVM> Books { get; set; }
        public string SearchText { get; set; }

        public List<Author>? PotentialAuthors { get; set; }
        public List<Genre>? PotentialGenres { get; set; }
    }
}
