using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;


namespace DailyPrayer.Models
{
    public class DominicanFeasts
    {
        [JsonProperty("feasts")]
        public List<DominicanFeast> feasts { get; set; }

        public class DominicanFeast
        {
            [JsonProperty("feast")]
            public FeastDetails feast { get; set; }

            override public string ToString()
            {
                string toString = string.Format($"{feast.ToString()}");

                return toString;
            }
        }

        public class FeastDetails
        {
            //[JsonProperty("date")]
            //public string date { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }

            [JsonProperty("eTitle")]
            public string eTitle { get; set; }

            [JsonProperty("vTitle")]
            public string vTitle { get; set; }

            [JsonProperty("solemnityType")]
            public string solemnityType { get; set; }

            [JsonProperty("prevEvening")]
            public bool prevEvening { get; set; }

            override public string ToString()
            {
                string toString = string.Format($"{name}, {eTitle}, {vTitle}, {solemnityType}, {prevEvening}");

                return toString;
            }
        }

        override public string ToString()
        {
            string toString = "";
            foreach (DominicanFeast feast in feasts)
                toString += feast.ToString() + "\n";
            return toString;
        }

    }
}
