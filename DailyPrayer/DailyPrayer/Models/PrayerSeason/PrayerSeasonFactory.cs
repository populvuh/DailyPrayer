using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPrayer.Models.PrayerSeason
{
    static class PrayerSeasonFactory
    {
        static public PrayerSeason CreatePrayerSeason(Place place, bool testMode)
        {
            PrayerSeason prayerSeason = null;
            switch (place.DomSeason)
            {
                case DominicanSeasons.XMas:
                    prayerSeason = new XMas(place, testMode);
                    break;
                case DominicanSeasons.OT1:
                    prayerSeason = new OrdinaryTime(place, testMode);
                    break;
                case DominicanSeasons.Ash_Wednesday_Week:
                    prayerSeason = new AshWednesdayWeek(place, testMode);
                    break;
                case DominicanSeasons.Lent:
                    prayerSeason = new Lent(place, testMode);
                    break;
                case DominicanSeasons.Holy_Week:
                    prayerSeason = new HolyWeek(place, testMode);
                    break;
                case DominicanSeasons.Easter:
                    prayerSeason = new Easter(place, testMode);
                    break;
                case DominicanSeasons.OT2:
                    prayerSeason = new OrdinaryTime(place, testMode);
                    break;
                case DominicanSeasons.Advent:
                    prayerSeason = new Advent(place, testMode);
                    break;
                case DominicanSeasons.XMas_II:
                    prayerSeason = new XMas(place, testMode);
                    break;
                //case DominicanSeasons.EndOfYear:
                //break;
                case DominicanSeasons.Feasts:
                default:
                    prayerSeason = new FeastDay(place, testMode);
                    break;
            }

            return prayerSeason;
        }
    }
}
