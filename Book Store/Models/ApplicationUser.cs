using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedDate { get; set; }
        [ForeignKey("CityId")]
        public City? City { get; set; }
        public int? CityId { get; set; }

    }
}
