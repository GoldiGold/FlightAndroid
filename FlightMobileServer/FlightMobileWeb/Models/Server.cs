using FlightMobileWeb.Model;
using FlightMobileWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FlightMobileWeb.Models
{
	public class Server : IFlightServer
	{
		private AsyncCommand _acommand; // we create it during the sendCommand function (since we need to check the vars have changed.
										//private TcpClient tcp;  the itc holds the tcp client
		private HttpClient _http;
		//private ITelnetClient _itc;
		private TcpClient _client;
		private ConcurrentDictionary<string, Pair<double, string>> _valuesDict; //a dictionary build like: <name, <value, path>>.
		private string _toScreenshot;
		private string _ip;
		private int _httpPort, _tcpPort;
		private BlockingCollection<AsyncCommand> _queue;
		public const double ailronPrecent = 0.02;
		public const double rudderPrecent = 0.02;
		public const double elevatorPrecent = 0.02;
		public const double throttlePrecent = 0.01;

		/// <summary>
		/// The ctor of the server class, gets a telnet client and ip&port to connect to.
		/// </summary>
		/// <param name="telnetClient"> the telnet client is responsible for the tcp communication, sending and getting the
		/// values of the command object.</param>
		/// <param name="ip"> the ip of the FG server to connect</param>
		/// <param name="httpPort"> the port of the FG server to connect</param>
		public Server(/*ITelnetClient telnetClient,string ip, int httpPort,*/ IConfiguration con)
		{
			//this._itc = telnetClient;
			this._httpPort = Int32.Parse(con["ServerHttpPort"]);
			this._tcpPort = Int32.Parse(con["ServerTcpPort"]);
			this._valuesDict = new ConcurrentDictionary<string, Pair<double, string>>();
			this._valuesDict["Aileron"] = new Pair<double, string>(double.PositiveInfinity, "/controls/flight/aileron");
			this._valuesDict["Rudder"] = new Pair<double, string>(double.PositiveInfinity, "/controls/flight/rudder");
			this._valuesDict["Elevator"] = new Pair<double, string>(double.PositiveInfinity, "/controls/flight/elevator");
			this._valuesDict["Throttle"] = new Pair<double, string>(double.PositiveInfinity, "/controls/engines/current-engine/throttle");
			this._toScreenshot = "http://" + con["ServerHostIP"] + "/:" + this._httpPort + "/screenshot";
			this._queue = new BlockingCollection<AsyncCommand>();
			this._ip = con["ServerHostIP"];
			this._client = new TcpClient();
			this._http = new HttpClient();
			this._acommand = new AsyncCommand(new Command());// creates a defult asyncCommand ( with infinities)
			this._acommand.command.Aileron = this.Aileron;
			this._acommand.command.Rudder = this.Rudder;
			this._acommand.command.Elevator = this.Elevator;
			this._acommand.command.Throttle = this.Throttle;


			Console.WriteLine("created a server");

			/*MAYBE ADD TO HERE: */
			this.Start();

		}

		/// <summary>
		/// check if the values of the command have been updated. 
		/// THE ORDER OF SENDING AND RECIEVING IS:
		///		1. Aileron
		///		2. Throttle
		///		3. Elevator
		///		4. Rudder
		/// </summary>
		/// <param name="buffer"> the string response from the tcp server that holds the 4 currently values.</param>
		/// <param name="length"> the length of the buffer </param>
		/// <returns></returns>
		public Result CheckIfUpdated(/*byte[] buffer, int length*/ NetworkStream stream)
		{
			/*string allValues = Encoding.ASCII.GetString(buffer);
			Console.WriteLine("output from server: " + allValues);
			string[] valuesArr = System.Text.RegularExpressions.Regex.Split(allValues, "\n");
			Console.WriteLine("the arr length is:" + valuesArr.Length);
			if (valuesArr.Length == 5)//the last one is empty since we finish the text with \n
			{*/
			byte[] recvBuffer = new byte[1024];
				// check values:
				//RUNNIG WITH FOREACH
				Console.WriteLine("started comparing");
				int i = 0;
			/*foreach (Pair<double, string> pair in this._valuesDict.Values)
			{*/
			byte[] sendMessage =Encoding.ASCII.GetBytes($"get {this._valuesDict[this._acommand.command.AileronString()].Second}\n");
			stream.Write(sendMessage, 0, sendMessage.Length);
			stream.Read(recvBuffer, 0, 1024);
				Console.WriteLine($"comparing between Aileron and the value: {Encoding.ASCII.GetString(recvBuffer)}");
				try
				{
					if (this.Aileron != Double.Parse(Encoding.ASCII.GetString(recvBuffer)))
						return Result.NotOk;
				}
				catch (Exception)
				{
					Console.WriteLine("couldn't parse this as a double"); // DEBUGGING PURPUSES
					return Result.NotOk;
				}
				++i;
			sendMessage = Encoding.ASCII.GetBytes($"get {this._valuesDict[this._acommand.command.RudderString()].Second}\n");
			stream.Write(sendMessage, 0, sendMessage.Length);
			recvBuffer = new byte[1024];
			stream.Read(recvBuffer, 0, 1024);
				Console.WriteLine($"comparing between Rudder and the value: {Encoding.ASCII.GetString(recvBuffer)}");
				try
				{
					if (this.Rudder != Double.Parse(Encoding.ASCII.GetString(recvBuffer)))
						return Result.NotOk;
				}
				catch (Exception)
				{
					Console.WriteLine("couldn't parse this as a double"); // DEBUGGING PURPUSES
					return Result.NotOk;
				}
				++i;
			sendMessage = Encoding.ASCII.GetBytes($"get {this._valuesDict[this._acommand.command.ElevatorString()].Second}\n");
			stream.Write(sendMessage, 0, sendMessage.Length);
			recvBuffer = new byte[1024];
			stream.Read(recvBuffer, 0, 1024);
				Console.WriteLine($"comparing between Elevator and the value: {Encoding.ASCII.GetString(recvBuffer)}");
				try
				{
					if (this.Elevator != Double.Parse(Encoding.ASCII.GetString(recvBuffer)))
						return Result.NotOk;
				}
				catch (Exception)
				{
					Console.WriteLine("couldn't parse this as a double"); // DEBUGGING PURPUSES
					return Result.NotOk;
				}
				++i;
			sendMessage = Encoding.ASCII.GetBytes($"get {this._valuesDict[this._acommand.command.ThrottleString()].Second}\n");
			stream.Write(sendMessage, 0, sendMessage.Length);
			recvBuffer = new byte[1024];
			stream.Read(recvBuffer, 0, 1024);
				Console.WriteLine($"comparing between Throttle and the value: {Encoding.ASCII.GetString(recvBuffer)}");
				try
				{
					if (this.Throttle != Double.Parse(Encoding.ASCII.GetString(recvBuffer)))
						return Result.NotOk;
				}
				catch (Exception)
				{
					Console.WriteLine("couldn't parse this as a double"); // DEBUGGING PURPUSES
					return Result.NotOk;
				}
				//++i;
				//}
				return Result.Ok; // EQUAL IN ALL THE VALUES.
			
			/*else
			{
				Console.WriteLine("the length is not 5");
				return Result.NotOk; //DIDN'T GET ENOUGHT PARAMETERS FROM THE FG SERVER ALTHOUGH ESKEF FOR 4.
			}*/

		}

		public void updateDictFromCommand(AsyncCommand cmd)
		{
			this._acommand = cmd;
			this.Aileron = this._acommand.command.Aileron;
			this.Throttle = this._acommand.command.Throttle;
			this.Elevator = this._acommand.command.Elevator;
			this.Rudder = this._acommand.command.Rudder;
		}

		public bool shouldUpdate(Command cmd)
		{
			return ((Math.Abs(this.Aileron - cmd.Aileron) >= ailronPrecent)
				|| (Math.Abs(this.Rudder - cmd.Rudder) >= rudderPrecent)
				|| (Math.Abs(this.Elevator - cmd.Elevator) >= elevatorPrecent)
				|| (Math.Abs(this.Throttle - cmd.Throttle) >= throttlePrecent));
		}

		/// <summary>
		/// Creates the string we send to FG in order to update and get the new values - to check if updated.
		/// </summary>
		/// <param name="cmd"> the Async command we get and exert the properties from.</param>
		/// <returns> a string of the message we send to the FG server through TCP.</returns>
		public string CreateCommandSet(AsyncCommand cmd)
		{
			this.updateDictFromCommand(cmd);
			string send = "";
			send += $"set {this._valuesDict[cmd.command.AileronString()].Second} {this.Aileron}\r\n";
			send += $"set {this._valuesDict[cmd.command.RudderString()].Second} {this.Rudder}\r\n";
			send += $"set {this._valuesDict[cmd.command.ElevatorString()].Second} {this.Elevator}\r\n";
			send += $"set {this._valuesDict[cmd.command.ThrottleString()].Second} {this.Throttle}\n";
			/*send += $"get {this._valuesDict[cmd.command.AileronString()].Second} \r\n"; //GETTING THE VALUE OF THE PROPERTY
			send += $"get {this._valuesDict[cmd.command.RudderString()].Second} \n";
			send += $"get {this._valuesDict[cmd.command.ElevatorString()].Second} \r\n";
			send += $"get {this._valuesDict[cmd.command.ThrottleString()].Second} \r\n";*/

			//Console.WriteLine("Message is: " + send);

			return send;
		}

		public void ProccessCommands()
		{
			//this._itc.Connect(this._ip, this._port);
			this._client.Connect(this._ip, this._tcpPort);
			NetworkStream stream = this._client.GetStream();
			stream.Write(Encoding.ASCII.GetBytes("data\n", 0, "data\n".Length));
			foreach (AsyncCommand cmd in this._queue.GetConsumingEnumerable())
			{
				//this.command = cmd;
				//Console.WriteLine("the current command is:");
				//this._acommand.command.toStringToConsole();
				if (this.shouldUpdate(cmd.command))
				{
					byte[] sendBuffer = Encoding.ASCII.GetBytes(this.CreateCommandSet(cmd));//cmmand to buffer; CREATE THE COMMAND IN THIS FUNCTION
					//byte[] recvBuffer = new byte[1024];
					stream.Write(sendBuffer, 0, sendBuffer.Length);
					//int nRead = stream.Read(recvBuffer, 0, 1024);
					/*do
					{
						nRead = stream.Read(recvBuffer, 0, 1024);
						Console.WriteLine("tried to read");
					} while (nRead == 0);*/
					Result res = this.CheckIfUpdated(stream);
					Console.WriteLine("the res after checking update: " + res);
					//Console.WriteLine("command after change is:");
					//this._acommand.command.toStringToConsole();
					cmd.Completion.SetResult(res);
				}
				else
				{
					cmd.Completion.SetResult(Result.Ok);//We just don't need to upadet it's fine.
				}
			}
		}

		public Task<Result> Execute(Command cmd)
		{
			var asyncCommand = new AsyncCommand(cmd);
			this._queue.Add(asyncCommand);
			return asyncCommand.Task;
		}
		public void Start()
		{
			Task.Factory.StartNew(ProccessCommands);
		}
		/*
		public Task IFlightServer.SendCommand(AsyncCommand c)
		{
			this._acommand = c;
			string fullCommand = "";
			fullCommand += $"GET";

			throw new NotImplementedException();
		}*/

		public async Task<byte[]> GetScreenShot()
		{
			//throw new NotImplementedException();
			return await this._http.GetByteArrayAsync(this._toScreenshot);
		}

		public bool ConnectToServer()
		{
			throw new NotImplementedException();
		}

		public double Elevator
		{
			get => this._valuesDict["Elevator"].First;
			set
			{
				if (this._valuesDict["Elevator"].First != value)
				{
					this._valuesDict["Elevator"].First = value;
					//this.NotifyPropertyChanged("Latitude");
				}
			}
		}

		public double Throttle
		{
			get => this._valuesDict["Throttle"].First;
			set
			{
				if (this._valuesDict["Throttle"].First != value)
				{
					this._valuesDict["Throttle"].First = value;
					//this.NotifyPropertyChanged("Latitude");
				}
			}
		}

		public double Aileron
		{
			get => this._valuesDict["Aileron"].First;
			set
			{
				if (this._valuesDict["Aileron"].First != value)
				{
					this._valuesDict["Aileron"].First = value;
					//this.NotifyPropertyChanged("Latitude");
				}
			}
		}

		public double Rudder
		{
			get => this._valuesDict["Rudder"].First;
			set
			{
				if (this._valuesDict["Rudder"].First != value)
				{
					this._valuesDict["Rudder"].First = value;
					//this.NotifyPropertyChanged("Latitude");
				}
			}
		}

	}

	public class Pair<T, U>
	{
		public Pair()
		{
		}
		public Pair(T first, U second)
		{
			this.First = first;
			this.Second = second;
		}
		public T First { get; set; }
		public U Second { get; set; }
	};
}
