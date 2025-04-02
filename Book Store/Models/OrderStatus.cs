using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
