using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [ForeignKey("State")]
        public int StateId { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public State State{ get; set; }
        public ICollection<ApplicationUser>? CityDeliveries { get; set; }
    }
}
