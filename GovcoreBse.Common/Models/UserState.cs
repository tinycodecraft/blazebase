using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common.Models;

public class UserState: IN.IAuthResult
{
    public double exp { get; set; }
    public string UserName { get; set; }

    public string UserID { get; set; }

    public int Level { get; set; }

    public bool IsAdmin { get; set; }

    public string Post { get; set; }

    public string Division { get; set; }

    public string Email { get; set; }
}