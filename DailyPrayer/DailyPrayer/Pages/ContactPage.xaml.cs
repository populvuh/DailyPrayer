using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DailyPrayer
{
    public partial class ContactPage : ContentPage
    {
        public ContactPage()
        {
            InitializeComponent();

            BindingContext = new ContactPageModel();

            SetFontSizes();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);                //must be called
            App.GetDeviceStats();
            SetFontSizes();
        }

        private void SetFontSizes()
        {
            EmailLabelText.FontSize = (double)Application.Current.Properties["ContactSize"];
            EmailEntry.FontSize = EmailLabelText.FontSize;
            EmailEnterBtn.FontSize = EmailLabelText.FontSize;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
