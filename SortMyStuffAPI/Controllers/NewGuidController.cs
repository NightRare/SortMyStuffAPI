using System;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Services;

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
        public IActionResult GetNewGuid()
        {
            return Ok(new GuidString
            {
                Self = Link.To(nameof(GetNewGuid), null),
                Value = Guid.NewGuid().ToString()
            });
        }
    }
}
