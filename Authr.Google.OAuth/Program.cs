using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;

var clientId = "clientId";
var clientSecret = "clientSecret";

var scopes = new[] { "https://www.googleapis.com/auth/gmail.readonly", "https://www.googleapis.com/auth/youtube" };

var credentials = GoogleWebAuthorizationBroker
    .AuthorizeAsync(
        new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
        scopes,
        "user",
        CancellationToken.None)
    .GetAwaiter().GetResult();

if (credentials.Token.IsExpired(SystemClock.Default))
{
    credentials.RefreshTokenAsync(CancellationToken.None).GetAwaiter().GetResult();
}

var service = new GmailService(new BaseClientService.Initializer()
{
    HttpClientInitializer = credentials
});

var profile = service.Users.GetProfile("mumkhong@skiff.com").Execute();

var youtubeService = new YouTubeService(new BaseClientService.Initializer()
{
    HttpClientInitializer = credentials,
});

var request = youtubeService.Channels.List("id");
request.Mine = true;

var list = request.Execute();

foreach (var channel in list.Items)
{
    Console.WriteLine(channel.Id);
}

Console.WriteLine(profile.MessagesTotal);
