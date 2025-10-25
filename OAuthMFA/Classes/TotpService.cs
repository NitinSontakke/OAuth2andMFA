using Net.Codecrete.QrCodeGenerator;
using System.Security.Cryptography;
using System.Text;

namespace OAuthMFA.Classes
{
  public class TotpService
  {
    private const int TimeStepSeconds = 30;
    private const int CodeDigits = 6;
    private const int SecretLength = 20; // 160 bits

    /// <summary>
    /// Generates a new TOTP secret for a user. TOTP stands for time based OTP.
    /// </summary>
    public MFARegisterResponse GenerateSecret(string userEmail, string appName = "OAuth and MFA Demo")
    {
      // Generate cryptographically secure random secret
      var secretBytes = new byte[SecretLength];
      using (var rng = RandomNumberGenerator.Create())
      {
        rng.GetBytes(secretBytes);
      }

      var secret = Base32Encode(secretBytes);
      var manualEntryKey = FormatSecretForManualEntry(secret);

      Console.WriteLine("userEmail  : " + userEmail);
      Console.WriteLine("appName  : " + appName);
      Console.WriteLine("secret  : " + secret);
      // Generate QR code URL for easy setup
      var qrCodeUrl = GenerateQrCodeUrl(userEmail, appName, secret);

      return new MFARegisterResponse
      {
        Secret = secret,
        QrCodeUrl = qrCodeUrl,
        ManualEntryKey = manualEntryKey,
        Instructions = "Scan the QR code or manually enter the key in your authenticator app",
        QrCode = GenerateQRCode(qrCodeUrl)
      };
    }

    /// <summary>
    /// Generates current TOTP code for a given secret
    /// </summary>
    public string GenerateCode(string secret)
    {
      var secretBytes = Base32Decode(secret);
      var timeStep = GetCurrentTimeStep();
      return GenerateCode(secretBytes, timeStep);
    }

    /// <summary>
    /// Generates current TOTP code for a given secret
    /// </summary>
    public string GenerateQRCode(string text)
    {
      var qr = QrCode.EncodeText(text, QrCode.Ecc.High);
      return qr.ToGraphicsPath();
    }

    /// <summary>
    /// Validates a TOTP code against a secret with time window tolerance
    /// </summary>
    public bool ValidateCode(string? secret, string? code, int windowSteps = 1)
    {
      if (string.IsNullOrEmpty(code) || code.Length != CodeDigits || String.IsNullOrEmpty(secret))
        return false;

      var secretBytes = Base32Decode(secret);
      var currentTimeStep = GetCurrentTimeStep();

      // Check current time step and adjacent steps for clock skew tolerance
      for (int i = -windowSteps; i <= windowSteps; i++)
      {
        var testCode = GenerateCode(secretBytes, currentTimeStep + i);
        if (code.Equals(testCode, StringComparison.Ordinal))
          return true;
      }

      return false;
    }

    private string GenerateCode(byte[] secret, long timeStep)
    {
      var timeStepBytes = BitConverter.GetBytes(timeStep);
      if (BitConverter.IsLittleEndian)
        Array.Reverse(timeStepBytes);

      using var hmac = new HMACSHA1(secret);
      var hash = hmac.ComputeHash(timeStepBytes);

      // Dynamic truncation
      var offset = hash[hash.Length - 1] & 0x0F;
      var binaryCode = ((hash[offset] & 0x7F) << 24) |
                      ((hash[offset + 1] & 0xFF) << 16) |
                      ((hash[offset + 2] & 0xFF) << 8) |
                      (hash[offset + 3] & 0xFF);

      var code = binaryCode % (int)Math.Pow(10, CodeDigits);
      return code.ToString($"D{CodeDigits}");
    }

    private long GetCurrentTimeStep()
    {
      var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      return unixTime / TimeStepSeconds;
    }

    private string GenerateQrCodeUrl(string userEmail, string appName, string secret)
    {
      var issuer = Uri.EscapeDataString(appName);
      var user = Uri.EscapeDataString(userEmail);
      return $"otpauth://totp/{issuer}:{user}?secret={secret}&issuer={issuer}&digits={CodeDigits}&period={TimeStepSeconds}";
    }

    private string FormatSecretForManualEntry(string secret)
    {
      // Format as groups of 4 characters for easier manual entry
      var formatted = new StringBuilder();
      for (int i = 0; i < secret.Length; i += 4)
      {
        if (i > 0) formatted.Append(" ");
        formatted.Append(secret.Substring(i, Math.Min(4, secret.Length - i)));
      }
      return formatted.ToString();
    }

    #region Base32 Encoding/Decoding
    private static readonly string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    private string Base32Encode(byte[] data)
    {
      if (data == null || data.Length == 0)
        return string.Empty;

      var result = new StringBuilder();
      int buffer = 0;
      int bitsLeft = 0;

      foreach (byte b in data)
      {
        buffer = (buffer << 8) | b;
        bitsLeft += 8;

        while (bitsLeft >= 5)
        {
          result.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
          bitsLeft -= 5;
        }
      }

      if (bitsLeft > 0)
      {
        result.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
      }

      return result.ToString();
    }

    private byte[] Base32Decode(string input)
    {
      if (string.IsNullOrEmpty(input))
        return new byte[0];

      input = input.ToUpperInvariant().Replace(" ", "");

      var result = new List<byte>();
      int buffer = 0;
      int bitsLeft = 0;

      foreach (char c in input)
      {
        int value = Base32Alphabet.IndexOf(c);
        if (value < 0) continue; // Skip invalid characters

        buffer = (buffer << 5) | value;
        bitsLeft += 5;

        if (bitsLeft >= 8)
        {
          result.Add((byte)((buffer >> (bitsLeft - 8)) & 255));
          bitsLeft -= 8;
        }
      }

      return result.ToArray();
    }
    #endregion
  }
}
