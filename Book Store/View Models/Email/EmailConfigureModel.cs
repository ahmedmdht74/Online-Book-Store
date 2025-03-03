namespace Book_Store.View_Models.Email
{
    public class EmailConfigureModel
    {
        public string SendingEmail { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string PassWord { get; set; }
        public bool EnableSSL { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public bool IsBodyHTML { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
