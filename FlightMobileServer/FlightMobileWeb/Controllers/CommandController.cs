using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.Http;
using FlightMobileWeb.Models;

namespace FlightMobileWeb.Controllers
{
    [Route("api/command")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private IFlightServer ifs;
        private HttpClient _client;

        [HttpPost]
        public ActionResult SendCommand([FromBody] Command c)
        {
            bool isOk = ifs.isValidCommand(c); // not sure we need it since we use the Json checker extension.
            if (!isOk)
            {
                // return error 400 badRequest
                return BadRequest("NOT A GOOD JSON COMMAND FILE.");
            }
            return Ok();
        }
    }
}