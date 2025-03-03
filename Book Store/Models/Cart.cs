using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ApplicationUser"), Display(Name = "User")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
