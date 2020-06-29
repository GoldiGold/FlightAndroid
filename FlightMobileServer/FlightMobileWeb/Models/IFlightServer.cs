using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace FlightMobileWeb.Models
{
	public interface IFlightServer
	{
		/// <summary>
		/// this function sends the values of a command object to a remote server.
		/// </summary>
		/// <returns>Task.CompletedTask if succeed to send to server. (MAYBE THIS IS WHAT WE RETURN.</returns>
		//Task SendCommand(AsyncCommand c);

		/// <summary>
		/// Checks if the command is ok, not sure we need it since we use the Json Extension.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		//bool isValidCommand(Command c);

		/// <summary>
		/// this function sends a get request for a screenshot to a remote server.
		/// </summary>
		/// <returns>an object type that represnt a JPG image.</returns>
		Task<byte[]> GetScreenShot();

		/// <summary>
		/// this tries to connect to a remote server using the ip and port from the configuration file.
		/// thats a blocking call, we don't want to start sending without being connected.
		/// </summary>
		/// <returns>true if succeed false if didn't.</returns>
		bool ConnectToServer();

		Task<Result> Execute(Command cmd);

		void Start();



	}
}
