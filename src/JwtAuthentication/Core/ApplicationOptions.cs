namespace Core;

using System.Net;
using System.Security;

public sealed class ApplicationOptions : IDisposable
{
    private SecureString? secretKey;
    private bool isDisposed;

    /// <summary>
    /// Gets or sets the secret key used for signing JWT tokens.
    /// </summary>
    public string SecretKey
    {
        get
        {
            return this.secretKey is null
                ? string.Empty
                : new NetworkCredential(string.Empty, this.secretKey).Password;
        }

        set
        {
            this.secretKey?.Dispose();
            this.secretKey = null;

            if (!string.IsNullOrWhiteSpace(value))
            {
                this.secretKey = new NetworkCredential(string.Empty, value).SecurePassword;
            }
        }
    }

    /// <summary>
    /// Gets or sets the issuer of the JWT tokens.
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the audience of the JWT tokens.
    /// </summary>
    public required string Audience { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the JWT tokens in minutes.
    /// </summary>
    public int TokenExpirationMinutes { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.secretKey?.Dispose();
        this.isDisposed = true;
    }
}
