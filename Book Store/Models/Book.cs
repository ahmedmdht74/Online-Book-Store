using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        [ForeignKey("Author"),Display(Name = "Author")]
        public int AuthorId { get; set; }
        public Author Author { get; set; }

        [ForeignKey("Genre"), Display(Name = "Genre")]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
        public Stock Stock { get; set; }

        public int Rating { get; set; } = 0;

        public int NumOfPeople { get; set; } = 0;

        public ICollection<CartDetails> CartDetails { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
        public bool IsDeleted { get; set; } = false;


    }
}
