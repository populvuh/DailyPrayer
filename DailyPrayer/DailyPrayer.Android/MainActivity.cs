using Android.App;
using Android.Content.PM;
using Android.OS;

using Plugin.Permissions;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace DailyPrayer.Droid
{
    [Activity(Label = "Kinh Thần Vụ",
                Icon = "@drawable/appicon",
                Theme = "@style/MyTheme.Splash",                                      // Indicates the theme to use for this activity
                MainLauncher = true,                                                  // Set it as boot activity
                ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

