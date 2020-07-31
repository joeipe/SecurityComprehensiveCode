using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleB.API.Authorization
{
    public class MustDoSomeDBCheckRequirement : IAuthorizationRequirement
    {
        public MustDoSomeDBCheckRequirement()
        {

        }
    }
}
