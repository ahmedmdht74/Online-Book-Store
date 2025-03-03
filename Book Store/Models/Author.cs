using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? ImagePath { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<Book> Books { get; set; }

    }
}
