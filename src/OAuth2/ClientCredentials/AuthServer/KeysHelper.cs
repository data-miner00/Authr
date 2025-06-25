namespace AuthServer;

public static class KeysHelper
{
    public static string PublicKey()
    {
        return File.ReadAllText("AuthServer.pem");
    }

    public static string PrivateKey()
    {
        return File.ReadAllText("AuthServer.key");
    }
}
