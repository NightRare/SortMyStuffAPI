using System;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class NewGuidController : Controller
    {
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
