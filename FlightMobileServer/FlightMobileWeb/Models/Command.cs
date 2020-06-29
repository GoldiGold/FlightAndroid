using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;



namespace FlightMobileWeb.Models
{
    public enum Result { Ok, NotOk}
    public class Command
    {
        [JsonProperty(PropertyName = "aileron", Required = Required.Always)]
        [JsonPropertyName("aileron")]
        [Range(-1, 1)]
        public double Aileron { get; set; }
        [JsonProperty(PropertyName = "rudder", Required = Required.Always)]
        [JsonPropertyName("rudder")]
        [Range(-1, 1)]
        public double Rudder { get; set; }
        [JsonProperty(PropertyName = "elevator", Required = Required.Always)]
        [JsonPropertyName("elevator")]
        [Range(-1, 1)]
        public double Elevator { get; set; }
        [JsonProperty(PropertyName = "throttle", Required = Required.Always)]
        [JsonPropertyName("throttle")]
        [Range(0, 1)]
        public double Throttle { get; set; }

        public string AileronString()
        {
            return "Aileron";
        }
        public string RudderString()
        {
            return "Rudder";
        }
        public string ElevatorString()
        {
            return "Elevator";
        }
        public string ThrottleString()
        {
            return "Throttle";
        }

        public Command() { }

        public void toStringToConsole()
        {
            if (this == null)
            {
                Console.WriteLine("this is null boiii");
            }
            else
            {
                Console.WriteLine($"Aileron: {this.Aileron}, Rudder: {this.Rudder}, Elevator: {this.Elevator}, Throttle: {this.Throttle}");

            }
        }
    }

    public class AsyncCommand
    {
        public Command command { get; private set; }
        public TaskCompletionSource<Result> Completion { get; private set; }
        public Task<Result> Task { get => Completion.Task; }
        public AsyncCommand(Command c)
        {
            this.command = c;
            this.Completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
