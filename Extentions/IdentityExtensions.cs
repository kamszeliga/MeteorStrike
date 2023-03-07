﻿using System.Security.Claims;
using System.Security.Principal;

namespace MeteorStrike.Extentions
{
    public static class IdentityExtensions 
    {
        public static int GetCompanyId(this IIdentity identity) 
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId")!;

            return int.Parse(claim.Value);
        }
    }
}
