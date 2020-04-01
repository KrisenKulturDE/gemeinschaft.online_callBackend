using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet("/CheckStatus")]
        [AllowAnonymous]
        public IActionResult CheckStatus()
        {
            if (ModelState.IsValid)
            {
                return Ok(DateTime.Now.ToLocalTime());
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

    }
}
