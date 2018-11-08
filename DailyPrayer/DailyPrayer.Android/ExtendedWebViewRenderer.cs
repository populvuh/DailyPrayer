using Android.Content;
using Android.Webkit;

using DailyPrayer;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using DroidWebView = Android.Webkit.WebView;

[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(DailyPrayer.Droid.ExtendedWebViewRenderer))]

namespace DailyPrayer.Droid
{
    public class ExtendedWebViewRenderer : WebViewRenderer
    {
        DroidWebView _webView;
        static ExtendedWebView _xwebView = null;

        public ExtendedWebViewRenderer(Context context) : base(context)
        { }

        class ExtendedWebViewClient : WebViewClient
        {
            public ExtendedWebViewClient()
            {
                System.Diagnostics.Debug.WriteLine("ExtendedWebViewClient.ExtendedWebViewClient()");
            }

            public override void OnPageStarted(DroidWebView webView, string url, Android.Graphics.Bitmap bitmap)
            {
                System.Diagnostics.Debug.WriteLine("ExtendedWebViewClient.OnPageStarted() "+url);
            }

            public override async void OnPageFinished(DroidWebView view, string url)
            {
                System.Diagnostics.Debug.WriteLine("ExtendedWebViewClient.OnPageFinished() - "+ view.ContentHeight.ToString() + " " +url);
                if (_xwebView != null)
                {
                    int i = 10;
                    while (view.ContentHeight == 0 && i-- > 0)                                                  // wait here till content is rendered
                        await System.Threading.Tasks.Task.Delay(100);
                    _xwebView.HeightRequest = view.ContentHeight;
                }

                base.OnPageFinished(view, url);
            }

            public override bool ShouldOverrideUrlLoading(DroidWebView webView, string url)
            {
                System.Diagnostics.Debug.WriteLine("ExtendedWebViewClient.ShouldOverrideUrlLoading() - " + url);
                return false;
            }

            public override void OnReceivedError(DroidWebView view, IWebResourceRequest request, WebResourceError error)
            {
                System.Diagnostics.Debug.WriteLine("ExtendedWebViewClient.OnReceivedError(): "+error.DescriptionFormatted);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            System.Diagnostics.Debug.WriteLine("ExtendedWebViewRenderer.OnElementChanged()");
            base.OnElementChanged(e);
            _xwebView = e.NewElement as ExtendedWebView;
            _webView = Control;

            if (e.OldElement == null)
            {
                _webView.SetWebViewClient(new ExtendedWebViewClient());
                //WebSettings settings = _webView.Settings;
                //settings.JavaScriptEnabled = true;
            }
        }
    }
}