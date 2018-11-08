using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;


namespace DailyPrayer.Models
{
    public class SeasonDefn
	{
		[JsonProperty("seasons")]
		public List<LitYearSeason> seasons { get; set; }
		public string scriptName { get; set; }
	}

	public class LitYearSeason
	{
		[JsonProperty("season")]
		public string name { get; set; }

		[JsonProperty("dateStart")]
		public string dateStart { get; set; }
		[JsonProperty("dateEnd")]
		public string dateEnd { get; set; }
	}
}