using System;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Services;
using System.Collections.Generic;
using System.Linq;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    public class NewGuidController : ApiBaseController
    {
        public NewGuidController(
            IUserDataService userDataService, 
            IOptions<ApiConfigs> apiConfigs, 
            IHostingEnvironment env, 
            IAuthorizationService authService) 
            : base(userDataService, 
                  apiConfigs, 
                  env, 
                  authService)
        {
        }

        // GET /newguid
        [Authorize]
        [HttpGet(Name = nameof(GetNewGuid))]
        public IActionResult GetNewGuid(
            int amount = 1)
        {
            if(amount < 1 || amount > 100)
            {
                return BadRequest(new ApiError("Request new guid failed.",
                    $"The amount must be between 1 and 100."));
            }

            if (amount > 1)
            {
                return Ok(new Collection<GuidString>
                {
                    Self = Link.To(nameof(GetNewGuid), new { amount = amount }),
                    Value = GetManyGuidStrings(amount).ToArray()
                });
            }

            return Ok(GetOneGuidString());
        }

        private GuidString GetOneGuidString()
        {
            return new GuidString
            {
                Self = Link.To(nameof(GetNewGuid), null),
                Value = Guid.NewGuid().ToString()
            };
        }

        private IEnumerable<GuidString> GetManyGuidStrings(int amount)
        {
            while (amount != 0)
            {
                yield return GetOneGuidString();
                amount--;
            }
        }
    }
}
