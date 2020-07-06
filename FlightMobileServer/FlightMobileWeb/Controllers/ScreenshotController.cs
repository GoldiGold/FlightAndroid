using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FlightMobileWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightMobileWeb.Controllers
{
    [Route("screenshot")]
    [ApiController]
    public class ScreenshotController : ControllerBase
    {
        private IFlightServer ifs;


        public ScreenshotController(IFlightServer flightServer)
        {
            this.ifs = flightServer;
        }

        [HttpGet]
        public async Task<ActionResult> GetScreenshot()
        {
            var resp = await ifs.Http.GetAsync(ifs.ToScreenShot);
            byte[] response = await resp.Content.ReadAsByteArrayAsync();
            //byte[] response = await new HttpClient().GetByteArrayAsync(ifs.ToScreenShot);//await this.ifs.GetScreenShot();
            if (response != null) //success
                return File(response, "image/jpeg"); //200 Ok  + the image
            return BadRequest();
        }

    }
}