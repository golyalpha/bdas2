namespace WebApp.Models;

public class PresentationRequest : Request
{
    public required uint PodiumSize { get; set; }

    public static PresentationRequest GetPresentationRequest(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<PresentationRequest> ListPresentationRequests()
    {
        throw new NotImplementedException();
    }
}

