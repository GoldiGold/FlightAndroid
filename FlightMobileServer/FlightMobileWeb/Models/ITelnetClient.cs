using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightMobileWeb.Model
{
    public interface ITelnetClient
    {
        /// <summary>
        /// connecting to a server of this ip and port address.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void Connect(string ip, int port);
        /// <summary>
        /// write a xommand to the server..
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        Task<int> Write(string command);
        /// <summary>
        /// reading the buffer from the server
        /// </summary>
        /// <returns></returns>
        Task<string> Read(); // blocking call
        /// <summary>
        /// disconnect from the server.
        /// </summary>
        void Disconnect();
        /// <summary>
        /// the Property of the sonnected boolean.
        /// </summary>
        bool Connection { set; get; }


    }
}
