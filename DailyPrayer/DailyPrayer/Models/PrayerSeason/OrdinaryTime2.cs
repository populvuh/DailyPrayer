using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPrayer.Models.PrayerSeason
{
    class OrdinaryTime2 : PrayerSeason
    {
        new private const string _Tag = "OrdinaryTime2";

        public OrdinaryTime2(Place place, bool testMode) : base(place, testMode)
        {
        }
    }
}
