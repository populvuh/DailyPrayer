using System;

using FreshMvvm;
using Xamarin.Forms;

using Debug = System.Diagnostics.Debug;


namespace DailyPrayer
{
    public class NewDatePageModel : FreshBasePageModel
    {

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("NewDatePageModel.ViewIsDisappearing()");
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("NewDatePageModel.ViewIsAppearing()");
        }

        public Command UpdateDateCommand
        {
            get
            {
                return new Command( (param) => {
                    var datePicker = param as Xamarin.Forms.DatePicker;
                    DateTime date = datePicker.Date;
                    string dateString = String.Format("{0} {1}", datePicker.Date.ToString("yyyyMMdd"), DateTime.Now.ToString("HH:mm"));
                    //System.Diagnostics.Debug.WriteLine("NewDatePageModel.UpdateDateCommand - popping with new date; " + dateString);

                    CoreMethods.PopPageModel(dateString);

                    //var datePicker = param as Xamarin.Forms.DatePicker;
                    //DateTime date = datePicker.Date.AddHours(6);                // returns a time of 12am
                    //MessagingCenter.Send(this, "NewDate", date); 
                    //var app = App.Current as App;
                    //app.ResetMainDetail();
                });
            }
        }
    }
}
