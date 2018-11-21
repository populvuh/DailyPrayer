using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DailyPrayer.Models;
using FreshMvvm;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DailyPrayer
{
    public class App : Application
    {
        public App()
        {
            GetDeviceStats();

            SetIOC();
            Xam.Behaviors.Infrastructure.Init();

            var page = FreshMvvm.FreshPageModelResolver.ResolvePageModel<PrayerPageModel>(DateTime.Now.ToString("yyyyMMdd HH:mm"));
            MainPage = new FreshMvvm.FreshNavigationContainer(page);
        }

        public void SetIOC()
        {
            FreshIOC.Container.Register<IPrayerModel, PrayerModel>();
            FreshIOC.Container.Register<IFeastsModel, FeastsModel>();
            FreshIOC.Container.Register<IDatabaseModel, DatabaseModel>();
            FreshIOC.Container.Register<IDominicanCalender, DominicanCalender>();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // set to today when app resumes
            var page = FreshMvvm.FreshPageModelResolver.ResolvePageModel<PrayerPageModel>(DateTime.Now.ToString("yyyyMMdd HH:mm"));
            MainPage = new FreshMvvm.FreshNavigationContainer(page);
        }

        public static bool SmallScreen { get; private set; } = false;
        public static void GetDeviceStats()
        {
            // Get Metrics
            var metrics = DeviceDisplay.ScreenMetrics;

            // Orientation (Landscape, Portrait, Square, Unknown)
            var orientation = metrics.Orientation;
            if (orientation == ScreenOrientation.Landscape)
            {
                FontSizeConverter fontSizeConverter = new FontSizeConverter();
                Application.Current.Properties["HeadingSize"] = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                Application.Current.Properties["ContactSize"] = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            }
            else
            {
                var rotation = metrics.Rotation;                    // Rotation (0, 90, 180, 270)                
                var width = metrics.Width;                          // Width (in pixels)                
                var height = metrics.Height;                        // Height (in pixels)                
                var density = metrics.Density;                      // Screen density

                // density == 3/ width<1000
                SmallScreen = (width < 1000);
                Application.Current.Properties["HeadingSize"] = Device.GetNamedSize(SmallScreen ? NamedSize.Small : NamedSize.Medium, typeof(Label));
                Application.Current.Properties["ContactSize"] = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            }
            Application.Current.SavePropertiesAsync();

            System.Diagnostics.Debug.WriteLine($"App.GetDeviceStats(): width={metrics.Width}, height={metrics.Height}, density={metrics.Density}");
        }
    }
}
