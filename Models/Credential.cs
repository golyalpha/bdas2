namespace WebApp.Models;

public class Credential
{
    public int Id { get; set; }
    public required string CredentialType { get; set; }
    public required string Data { get; set; }
}
