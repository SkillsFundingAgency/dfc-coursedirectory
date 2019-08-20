namespace Dfc.CourseDirectory.Web.ViewComponents.BackLink
{
    public class BackLinkModel
    {
        public string Controller { get; set; }
        public string Action { get; set; }

        public BackLinkModel() { }
        public BackLinkModel(string controller, string action)
        {
            this.Controller = controller;
            this.Action = action;
        }
    }
}
