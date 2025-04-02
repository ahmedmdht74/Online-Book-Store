using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Book"), Display(Name = "Book")]
        public int BookId { get; set; }
        public Book Book { get; set; }

        public int Quantity { get; set; }

    }
}
