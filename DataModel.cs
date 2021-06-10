using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ExampleQueryApp
{
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public double Price { get; set; }
        public string BuyerState { get; set; }
    }
}
