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

		public CommandController(IFlightServer flightServer)
		{
			this.ifs = flightServer;
		}

		[HttpPost]
		public async Task<ActionResult> SendCommand([FromBody] Command c)
		{
			//bool isOk = ifs.isValidCommand(c); // not sure we need it since we use the Json checker extension.
			if (c == null)
			{
				// return error 400 badRequest
				Console.WriteLine("the command is NULL");
				return BadRequest("NOT A GOOD JSON COMMAND FILE.");
			}
			else
			{
				// TODO: ADD THE AWAIT SEND_COMMAND OF THE SERVER (WITH THE TCP). WE WILL CALL THE SERVER SEND_COMMAND THAT
				// WILL USE THE TCP IN THE INSIDE. WE WON'T KNOW THE INNER IMPLEMENTATION.
				Console.WriteLine("The command we got:");
				c.toStringToConsole();
				//Result asyncCommandTaskResult = await this.ifs.Execute(c);
				//Console.WriteLine("the result is: " + asyncCommandTaskResult);
				if(await this.ifs.Execute(c) == Result.Ok)
				{
					Console.WriteLine("its ok nigga");
					return Ok();
				} else
				{
					Console.WriteLine("what defauk it failed");
					return StatusCode(500);
				}

				/*
				if (asyncCommandTaskResult == Result.Ok)
				{
					return Ok();

				}
				else
				{
					return StatusCode(500);
				}*/
			}
		}
	}
}