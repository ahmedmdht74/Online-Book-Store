using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Order"), Display(Name = "Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }
        [ForeignKey("Book"), Display(Name = "Book")]
        public int BookId { get; set; }
        public Book Book { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
