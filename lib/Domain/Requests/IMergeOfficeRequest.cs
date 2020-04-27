namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IMergeOfficeRequest: IMergeRequest
    {
        IMergeOfficeRequest FilterByExtension();
    }
}