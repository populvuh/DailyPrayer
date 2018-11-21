using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

using Xamarin.Forms;

namespace DailyPrayer
{
    public partial class PrayerPage : ContentPage
    {
        private const string _Tag = "PrayerPage";

        public PrayerPage()
        {
            InitializeComponent();

            var tapGestureRecognizerPrev = new TapGestureRecognizer();
            tapGestureRecognizerPrev.Tapped += (sender, e) =>
            {
                IsBusy = false;
                try
                {
                    MessagingCenter.Send<PrayerPage, int>(this, "NextPrayer", -1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PrayerPage.tapGestureRecognizerPrev.Tapped()\n" + ex.Message + "\n" + ex.InnerException);
                }
            };
            prevBtn.GestureRecognizers.Clear();
            prevBtn.GestureRecognizers.Add(tapGestureRecognizerPrev);

            var tapGestureRecognizerNext = new TapGestureRecognizer();
            tapGestureRecognizerNext.Tapped += (sender, e) =>
            {
                IsBusy = true;
                try
                {
                    MessagingCenter.Send<PrayerPage, int>(this, "NextPrayer", 1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PrayerPage.tapGestureRecognizerNext.Tapped()\n"+ex.Message+"\n"+ex.InnerException);
                }
            };
            nextBtn.GestureRecognizers.Clear();
            nextBtn.GestureRecognizers.Add(tapGestureRecognizerNext);

            // ExtendedWebView thingies ...
            Browser.HorizontalOptions = LayoutOptions.Fill;
            Browser.VerticalOptions = LayoutOptions.StartAndExpand;

            //// Without these two lines, the view will not render on iOS, likewise, 
            //// the ExtendedWebView needs these to calculate content height.
            Browser.WidthRequest = 640d;
            Browser.HeightRequest = 640d;
        }

        protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanging(propertyName);
            //Debug.WriteLine($"PrayerPage.OnPropertyChanging({propertyName})");
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            //Debug.WriteLine($"PrayerPage.OnPropertyChanged({propertyName})");
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            // looks like binding bug, so I bind it myself
            //(Content as GestureView).BindingContext = BindingContext;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);                //must be called
            App.GetDeviceStats();
            SetFontSizes();
        }

        private void SetFontSizes()
        {
            prayerHeading.FontSize = (double)Application.Current.Properties["HeadingSize"];
        }

        void PinchGesture(object sender, PinchGestureUpdatedEventArgs e)
        {
            MessagingCenter.Send<PrayerPage>(this, "PinchGesture");
        }

        void OnDateSelected(object sender, DateChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"PrayerPage Command OnDateSelected {args.NewDate.ToString()}");
            MessagingCenter.Send(this, "NewDate", args.NewDate);
        }

        private void OnChangeDate(object sender, EventArgs e)
        {
            Debug.WriteLine("PrayerPage.OnChangeDate()");

            PrayerDatePicker.IsVisible = true;
            PrayerDatePicker.Focus();
        }

        private async void OnEmailClicked(object sender, EventArgs e)
        {
            // this is a bit ugly; the RequestPermissionsAsync in ContactsPermission() 
            // only seems to work if called from a ContentPage;
            // it doesn't work if called from a PageModel :(
            // so we have to get permission here, and then message the PageModel to display the ContactsPage
            Debug.WriteLine("PrayerPage.OnEmailClicked()");

            // we don't actually care if we get permission, we still display the eMail page, just without any contacts
            bool OK = await ContactsPermission();

            MessagingCenter.Send<PrayerPage>(this, "ContactsPage");
        }

        private async Task<bool> ContactsPermission()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            if (status != PermissionStatus.Granted)
            {
                status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Contacts))[Permission.Contacts];
            }
            Debug.WriteLine($"{_Tag}.ContactsPermission(): {status.ToString()}");
            //await DisplayAlert("Results", status.ToString(), "OK");

            bool OK = (status == PermissionStatus.Granted);

            return OK;
        }
    }
}
