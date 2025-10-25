using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthMFA.Classes
{
  public class MFAAuthenticateResponse
  {
    public Boolean Success { get; set; }

    public String? Message { get; set; }
  }
}
