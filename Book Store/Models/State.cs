using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models
{
    public class State
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public ICollection<City> Cities { get; set; }
    }
}
