using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using Debug = System.Diagnostics.Debug;

namespace DailyPrayer
{
    public partial class NewDatePage : ContentPage
    {
        public delegate void DateChangedEventHandler(object sender, DateChangedEventArgs e);


        public NewDatePage()
        {
            InitializeComponent();

            PrayerDatePicker.DateSelected += new EventHandler<DateChangedEventArgs>(OnDateChangedEvent);
        }

        protected void OnDateChangedEvent(object sender, DateChangedEventArgs e)
        {
            Debug.WriteLine("NewDatePage.OnDateChangedEvent() "+e.NewDate.ToString("yyyyMMdd"));
        }
    }
}
