namespace Book_Store.View_Models.Dashboard
{
    public class ReportListVM
    {
        public string searchtext { get; set; }
        public int orderby { get; set; }
        public int ReportCount { get; set; }
        public string status { get; set; }

        public List<ReportDetailsInReportListVM> ReportDetailsInReportLists { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ReportDetailsInReportListVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }

    }
}
