namespace Dfc.ProviderPortal.FindACourse.ApiModels
{
    public interface IPagedRequest
    {
        int? Limit { get; set; }
        int? Start { get; set; }
    }
}
