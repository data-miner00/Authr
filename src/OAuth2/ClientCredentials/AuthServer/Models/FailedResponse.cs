namespace AuthServer.Models;

using System.Text.Json.Serialization;

public class FailedResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; }
}
