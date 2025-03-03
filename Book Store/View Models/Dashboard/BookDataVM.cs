using Book_Store.Models;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Dashboard
{
    public class BookDataVM
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Book'Title is required"), Display(Name = "Title")]
        public string BookName { get; set; }


        [Required(ErrorMessage = "Price is required"), Display(Name = "Price")]
        public double Price { get; set; }



        [Display(Name = "Description"),DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [ Display(Name = "Quantity")]
        public int? Quantity { get; set; }


        public List<Genre>? Genres { get; set; }


        [Required(ErrorMessage = "Genre is required"), Display(Name = "Genre")]
        public int GenreId { get; set; }



        public List<Author>? Authors { get; set; }


        [Required(ErrorMessage = "Author is required"), Display(Name = "Author")]
        public int AuthorId { get; set; }

        public string? ImagePath { get; set; }

        public IFormFile? BookImageFile  { get; set; }
    }
}
