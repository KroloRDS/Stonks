namespace Stonks.CQRS.Queries.ViewModels;

public record ErrorViewModel : BaseViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}