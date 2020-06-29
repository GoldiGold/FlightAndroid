﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            byte[] response = await this.ifs.GetScreenShot();
            if (response != null) //success
                return File(response, "image/jpg"); //200 Ok  + the image
            return BadRequest();
        }

    }
}