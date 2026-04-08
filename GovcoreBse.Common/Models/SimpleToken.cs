using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common.Models;

public class SimpleToken
{
    public SimpleToken(string hours = null)
    {
        var expireseconds = TypeHelper.TryValue<int>(hours, 4) * 60 * 60;

        iss = "HYD";
        iat = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        exp = iat + expireseconds;
        aud = "";
        sub = "HYD.ENG";
        jti = "HYD." + DateTime.Now.ToString("yyyyMMddhhmmss");
    }

    public string iss { get; set; }
    public double iat { get; set; }
    public double exp { get; set; }
    public string aud { get; set; }
    public double nbf { get; set; }
    public string sub { get; set; }
    public string jti { get; set; }

}