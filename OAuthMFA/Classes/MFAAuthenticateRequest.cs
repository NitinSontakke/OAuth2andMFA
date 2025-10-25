using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthMFA.Classes
{
  public class MFAAuthenticateRequest
  {
    public String? UserId { get; set; }
    public String? Code { get; set; }
  }
}
