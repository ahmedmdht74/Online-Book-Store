using Book_Store.View_Models.Email;

namespace Book_Store.View_Models.Dashboard
{
    public class ReportDetailsVM
    {
        public int ReportId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public UserEmailOptions MakeResponse{ get; set; }
    }
}
