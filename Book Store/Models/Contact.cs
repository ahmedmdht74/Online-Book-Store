using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book_Store.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "*Name required*")]
        public string Name { get; set; }
        [EmailAddress,Required(ErrorMessage = "*Email required*")]
        public string Email { get; set; }
        [DataType(DataType.MultilineText), Required(ErrorMessage = "*Message required*"),MinLength(10)]
        public string Message { get; set; }
        public bool IsDeleted { get; set; } = false;

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser{ get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? Subject { get; set; }
        public string? Status { get; set; }
    }


    public enum ContactStatus
    {
        New,
        InProcess,
        Replied
    }
}
