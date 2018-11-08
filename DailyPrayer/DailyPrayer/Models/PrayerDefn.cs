using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using Debug = System.Diagnostics.Debug;


namespace DailyPrayer.Models
{
    public class PrayerDefn
	{
		[JsonProperty("seasons")]
		private List<Season> _seasons { get; set; }
		public string scriptName { get; set; }
	}

	public class Season
	{
		[JsonProperty("season")]
		private string _name { get; set; }

		[JsonProperty("days")]
		private List<Day> _days { get; set; }
	}

	public class Day
	{		
		[JsonProperty("day")]
		public int _dayNo { get; set; }

		[JsonProperty("sections")]
		private List<Section> __sections { get; set; }
	}

	public class Section
	{
		[JsonProperty("section")]
		private string _section { get; set; }

		[JsonProperty("dir")]
		private string _dir { get; set; }

		[JsonProperty("id")]
		private string _id { get; set; }

		[JsonProperty("file")]
		private string _file { get; set; }
	}
}