using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name{ get; set; }

        public ICollection<Book> Books { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
