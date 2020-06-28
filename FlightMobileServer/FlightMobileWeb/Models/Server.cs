﻿using FlightMobileWeb.Model;
using FlightMobileWeb.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FlightMobileWeb.Servers
{
	public class Server : IFlightServer
	{
		private AsyncCommand _acommand; // we create it during the sendCommand function (since we need to check the vars have changed.
										//private TcpClient tcp;  the itc holds the tcp client
		private HttpClient _http;
		private ITelnetClient _itc;
		private TcpClient _client;
		private ConcurrentDictionary<string, Pair<double, string>> _valuesDict; //a dictionary build like: <name, <value, path>>.
		private string _toScreenshot;
		private string _ip;
		private int _port;
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
		public Server(ITelnetClient telnetClient, string ip, int httpPort)
		{
			this._itc = telnetClient;
			this._valuesDict = new ConcurrentDictionary<string, Pair<double, string>>();
			this._valuesDict["Elevator"] = new Pair<double, string>(double.NaN, "/controls/flight/elevator");
			this._valuesDict["Throttle"] = new Pair<double, string>(double.NaN, "/controls/engines/current-engine/throttle");
			this._valuesDict["Aileron"] = new Pair<double, string>(double.NaN, "/controls/flight/aileron");
			this._valuesDict["Rudder"] = new Pair<double, string>(double.NaN, "/controls/flight/rudder");
			this._toScreenshot = "http://" + ip + "/:" + httpPort + "/screenshot";
			this._queue = new BlockingCollection<AsyncCommand>();
			this._ip = ip;
			this._port = httpPort;
			this._client = new TcpClient();
			this._http = new HttpClient();

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
		public Result CheckIfUpdated(byte[] buffer, int length)
		{
			string allValues = Encoding.ASCII.GetString(buffer);
			string[] valuesArr = allValues.Split('\n');
			if (valuesArr.Length == 4)
			{
				// check values:
				/*try
				{
					if (this._valuesDict[this._acommand.command.AileronString()].First != Double.Parse(valuesArr[0]))
						return Result.NotOk;
				}
				catch (Exception)
				{
					Console.WriteLine("couldn't parse this as a double"); // DEBUGGING PURPUSES
					return Result.NotOk;
				}*/

				//RUNNIG WITH FOREACH
				int i = 0;
				foreach(Pair<double,string> pair in this._valuesDict.Values)
				{
					try
					{
						if (pair.First != Double.Parse(valuesArr[i]))
						{
							return Result.NotOk;
						}
					}
					catch (Exception)
					{
						Console.WriteLine("couldn't parse this as a double"); // DEBUGGING PURPUSES
						return Result.NotOk;
					}
					++i;
				}
				return Result.Ok; // EQUAL IN ALL THE VALUES.
			}
			else
			{
				return Result.NotOk; //DIDN'T GET ENOUGHT PARAMETERS FROM THE FG SERVER ALTHOUGH ESKEF FOR 4.
			}

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
		public string CreateCommandRequests(AsyncCommand cmd)
		{
				this.updateDictFromCommand(cmd);
				string send = "";
				send += $"set {this._valuesDict[cmd.command.AileronString()].Second} {this.Aileron}\r\n";
				send += $"set {this._valuesDict[cmd.command.ThrottleString()].Second} {this.Throttle}\r\n";
				send += $"set {this._valuesDict[cmd.command.ElevatorString()].Second} {this.Elevator}\r\n";
				send += $"set {this._valuesDict[cmd.command.RudderString()].Second} {this.Rudder}\r\n";
				send += $"get {this._valuesDict[cmd.command.AileronString()].Second} \r\n"; //GETTING THE VALUE OF THE PROPERTY
				send += $"get {this._valuesDict[cmd.command.ThrottleString()].Second} \r\n";
				send += $"get {this._valuesDict[cmd.command.ElevatorString()].Second} \r\n";
				send += $"get {this._valuesDict[cmd.command.RudderString()].Second} \n";

				return send;
		}

		public void ProccessCommands()
		{
			//this._itc.Connect(this._ip, this._port);
			this._client.Connect(this._ip, this._port);
			NetworkStream stream = this._client.GetStream();
			foreach (AsyncCommand cmd in this._queue.GetConsumingEnumerable())
			{
				//this.command = cmd;
				if (this.shouldUpdate(cmd.command))
				{
					byte[] sendBuffer = Encoding.ASCII.GetBytes(this.CreateCommandRequests(cmd));//cmmand to buffer; CREATE THE COMMAND IN THIS FUNCTION
					byte[] recvBuffer = new byte[1024];
					stream.Write(sendBuffer, 0, sendBuffer.Length);
					int nRead = stream.Read(recvBuffer, 0, 1024);
					Result res = this.CheckIfUpdated(recvBuffer, nRead);

					cmd.Completion.SetResult(res);
				}
			}
		}

		public Task<Result> Execute(Command cmd)
		{
			var asyncCommand = new AsyncCommand(cmd);
			this._queue.Add(asyncCommand);
			return asyncCommand.Task;
		}
		public void start()
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
