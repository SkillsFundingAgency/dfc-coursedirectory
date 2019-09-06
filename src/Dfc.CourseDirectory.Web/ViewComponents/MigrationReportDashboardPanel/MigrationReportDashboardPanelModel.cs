namespace Dfc.CourseDirectory.Web.ViewComponents.MigrationReportDashboardPanel
{
    public class MigrationReportDashboardPanelModel
    {
        public string Caption { get; set; }
        public int Value { get; set; }
        public double Percentage { get; set; }
        public int Total { get; set; }

        public MigrationReportDashboardPanelModel(string caption, int value = 0, double percentage = 0, int total = 0)
        {
            Caption = caption;
            Value = value;
            Percentage = percentage;
            Total = total;
        }
    }
}
