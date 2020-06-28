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

        public Command() { }
    }
}
