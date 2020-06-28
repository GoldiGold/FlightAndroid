using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace FlightMobileWeb.Model
{

	// TODO: CHECK HOW TO IMPLEMENT THE ASYNC FUNCTIONALITY - PAVEL'S TCP ASYNCHRONLY TIRGUL.
	class MyTelnetClient : ITelnetClient
	{
		private IPEndPoint ep;
		TcpClient /*Socket*/ socket;
		volatile bool connected = false;
		private uint timeoutCounter;
		private NetworkStream stream;
		private StreamReader streamReader;
		private Mutex timoutMutex = new Mutex();
		/// <inheritdoc />
		void ITelnetClient.Connect(string ip, int port)
		{
			//connected = false;
			timeoutCounter = 0;
			try
			{
				// Establish the remote endpoint for the socket
				///ep = new IPEndPoint(IPAddress.Parse(ip), port);
				// Create TCP client 
				socket = new TcpClient(ip, port)/* SocketType.Stream, ProtocolType.Tcp)*/
				{
					SendTimeout = 10000,
					ReceiveTimeout = 10000
				};
				stream = socket.GetStream();
				streamReader = new StreamReader(stream);
				// define the timeout to 10 seconds - if we didn't get an answer after 10 seconds return length of 0 and continue.
				// connect to Client socket
				///socket.Connect(ep);
				Console.WriteLine("connected to the server");
				//connected = true;
			}
			catch (Exception e)
			{
				//connected = false;
				//Console.WriteLine("error connecting:" + e.Message);
				throw e;
			}
			//throw new NotImplementedException();
		}

		/// <inheritdoc />
		async Task<int> ITelnetClient.Write(string command)
		{
			// Code from geeksforgeeks. just a sample code.

			// Creation of messagge that 
			// we will send to Server 
			//byte[] messageSent = Encoding.ASCII.GetBytes("Test Client\n");
			try
			{

				int byteSent = Encoding.ASCII.GetByteCount(command);
				byte[] sendData = Encoding.ASCII.GetBytes(command);
				lock (stream)
				{
					stream.Write(sendData, 0, byteSent);
				}
				//this.timeoutCounter = 0;
				return byteSent;
			}
			catch (Exception)
			{
				timoutMutex.WaitOne();
				this.timeoutCounter += 1;
				timoutMutex.ReleaseMutex();
				return 0;
			}
			//throw new NotImplementedException();
		}
		/// <inheritdoc />
		async Task<string> ITelnetClient.Read() //blocking call
		{
			// Code from geeksforgeeks. just a sample code.

			// Data buffer 
			//byte[] messageReceived = new byte[1024];

			// We receive the messagge using  
			// the method Receive(). This  
			// method returns number of bytes 
			// received, that we'll use to  
			// convert them to string 
			string Recv;
			try
			{
				lock (socket)
				{
					timoutMutex.WaitOne();
					for (int i = 0; i < this.timeoutCounter; ++i)
					{
						streamReader.ReadLine(); // read the previous data and don;t save it
					}
					timoutMutex.ReleaseMutex();
					Recv = streamReader.ReadLine(); // read the last message that counts (if there wasn't a timeout then counter = 0.
													//byteRecv = socket.Receive(messageReceived);
				}
				//timoutMutex.WaitOne();
				//string message = Encoding.ASCII.GetString(messageReceived).Split('\n')[this.timeoutCounter]; // Check if this works when there is a delay.
				//timoutMutex.ReleaseMutex();
				//if (this.timeoutCounter > 0)
				//{
				//Console.WriteLine("message from delay: " + Encoding.ASCII.GetString(messageReceived));
				//Console.WriteLine("info we take in: " + Recv);
				//}
				//else
				//{
				//Console.WriteLine("message without delay: " + Encoding.ASCII.GetString(messageReceived));
				//}
				timoutMutex.WaitOne();
				this.timeoutCounter = 0;
				timoutMutex.ReleaseMutex();
				return Recv;
			}
			catch (Exception)
			{
				timoutMutex.WaitOne();
				this.timeoutCounter += 1;
				timoutMutex.ReleaseMutex();
				//There was a problem with the Recieve function.
				return "";//string with length of 0 to know we got nothing.
			}

		}

		/// <inheritdoc />
		void ITelnetClient.Disconnect()
		{
			if (connected == true) // be extra sure.
			{
				//connected = false;
				lock (socket)
				{
					socket.GetStream().Close();
					socket.Client.Close();
					//socket.Dispose();
					//socket.Close()/*(SocketShutdown.Both)*/;
					socket.Close();
				}
			}
			//throw new NotImplementedException();
		}
		bool ITelnetClient.Connection { get => connected; set => connected = value; }
	}
}

