using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models
{
    public class CartDetails
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Cart"), Display(Name = "Cart")]
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        [ForeignKey("Book"), Display(Name = "Book")]
        public int BookId { get; set; }
        public Book Book { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
