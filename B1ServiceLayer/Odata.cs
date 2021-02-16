using Newtonsoft.Json;
using SAPB1;
using System;
using System.Collections.Generic;
using System.Text;

namespace B1ServiceLayer
{
    public class Odata<T>
    {
        [JsonProperty("odata.nextLink")]
        public string nextLink { get; set; }
        public List<T> value { get; set; }
    }
}
