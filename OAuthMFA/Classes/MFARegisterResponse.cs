using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthMFA.Classes
{
  public class MFARegisterResponse
  {
    public string? Secret { get; set; }

    public string? QrCodeUrl { get; set; }

    public string? ManualEntryKey { get; set; }

    public string? Instructions { get; set; }

    public string? QrCode { get; set; }
  }
}
