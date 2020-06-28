using FlightMobileWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FlightMobileWeb.Servers
{
	public class Server
	{
		private Command command;
		private TcpClient tcp;
		private HttpClient http;
	}
}
