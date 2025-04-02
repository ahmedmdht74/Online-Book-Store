using System.ComponentModel.DataAnnotations;

namespace Book_Store.View_Models.Email
{
    public class UserEmailOptions
    {
        [Display(Name = "Receiver")]
        public List<string> RecieveEmails { get; set; }
        [Display(Name = "Subject")]
        public string Subject { get; set; }
        [Display(Name = "Message")]
        public string Body { get; set; }
        [Display(Name = "Pick an Attachment")]
        public IFormFile? Attachment { get; set; }
        public List<KeyValuePair<string, string>>? PlaceHolders { get; set; }
    }
}
