namespace TimeBasedOTP.Controllers;

using Microsoft.AspNetCore.Mvc;
using OtpNet;
using QRCoder;

// Reference: https://www.youtube.com/watch?v=pNEaBQIJ_Dg
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly string Issuer = Uri.EscapeDataString("TimedBasedOTPApp");

    private static readonly Dictionary<string, string> UserSecrets = [];

    [HttpPost("otp/generate/{userid}")]
    public async Task<IActionResult> Generate(string userid)
    {
        var secretKey = KeyGeneration.GenerateRandomKey();
        var base32SecretKey = Base32Encoding.ToString(secretKey);
        var escapedUser = Uri.EscapeDataString(userid);

        UserSecrets.Add(escapedUser, base32SecretKey);

        var otpUri = $"otpauth://totp/{Issuer}:{escapedUser}?secret={base32SecretKey}&issuer={Issuer}&digits=6&period=30";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(10);

        return this.File(qrCodeImage, "images/png");
    }

    [HttpPost("otp/validate/{userid}/{code}")]
    public async Task<IActionResult> Validate(string userid, string code)
    {
        if (!UserSecrets.TryGetValue(userid, out var secretKey))
        {
            return this.Unauthorized(new { Error = "Invalid userid or secret." });
        }

        var totp = new Totp(Base32Encoding.ToBytes(secretKey));
        var isValid = totp.VerifyTotp(
            code,
            out var timeStepMatched,
            VerificationWindow.RfcSpecifiedNetworkDelay); // gives 1 window from past and future

        if (!isValid)
        {
            return this.Unauthorized(new { Error = "Invalid userid or secret." });
        }

        return this.Ok();
    }
}
