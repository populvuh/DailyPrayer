using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;


namespace DailyPrayer.Models
{
    class DominicanDates
    {
        [JsonProperty("years")]
        public List<DominicanYear> years { get; set; }

        public class DominicanYear
        {
            [JsonProperty("year")]
            public int year { get; set; }

            [JsonProperty("dates")]
            public YearDates dates { get; set; }

            override public string ToString()
            {
                string toString = string.Format("{0} = ({1})", year, dates.ToString());

                return toString;
            }
        }

        public class YearDates
        {
            [JsonProperty("OT1")]
            public string ot1 { get; set; }

            [JsonProperty("AshWed")]
            public string ashwed { get; set; }

            [JsonProperty("Easter")]
            public string easter { get; set; }

            [JsonProperty("Pentecost")]
            public string pentecost { get; set; }

            [JsonProperty("Advent")]
            public string advent { get; set; }

            public override string ToString()
            {
                string toString = string.Format("OT1={1}, AshWed={2}, Easter={3}, Pentecost={4}, Advent={0}", advent, ot1, ashwed, easter, pentecost);

                return toString;
            }

        }
    }
}
