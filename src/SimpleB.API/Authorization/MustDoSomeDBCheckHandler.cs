using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleB.API.Authorization
{
    public class MustDoSomeDBCheckHandler : AuthorizationHandler<MustDoSomeDBCheckRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MustDoSomeDBCheckHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustDoSomeDBCheckRequirement requirement)
        {
            /*
            var idStr = _httpContextAccessor.HttpContext.GetRouteValue("id").ToString();

            if (!int.TryParse(idStr, out int id))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            */

            var userId = context.User.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            
            if (false) // Do DB checks here
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
