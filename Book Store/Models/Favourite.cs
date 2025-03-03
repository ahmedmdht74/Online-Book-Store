using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Book_Store.Models
{
    public class Favourite
    {
        [ForeignKey("Book"), Display(Name = "Book")]
        public int BookId { get; set; }
        public Book Book { get; set; }

        [ForeignKey("ApplicationUser"), Display(Name = "User")]
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser{ get; set; }

    }
}
