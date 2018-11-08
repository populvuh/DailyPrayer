using System;

using Android.Content;
using Android.Views;

using DailyPrayer.Controls;
using DailyPrayer.Droid.Controls;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]
namespace DailyPrayer.Droid.Controls
{
    public class GestureViewRenderer : ViewRenderer
    {
        private readonly DroidGestureListener _listener;
        private readonly GestureDetector _detector;

        public GestureViewRenderer(Context context) : base(context)
        {
            _listener = new DroidGestureListener();
            _detector = new GestureDetector(_listener);
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                this.GenericMotion -= HandleGenericMotion;
                this.Touch -= HandleTouch;
                _listener.OnSwipeLeft -= HandleOnSwipeLeft;
                _listener.OnSwipeRight -= HandleOnSwipeRight;
                _listener.OnSwipeUp -= HandleOnSwipeTop;
                _listener.OnSwipeDown -= HandleOnSwipeDown;
            }

            if (e.OldElement == null)
            {
                this.GenericMotion += HandleGenericMotion;
                this.Touch += HandleTouch;
                _listener.OnSwipeLeft += HandleOnSwipeLeft;
                _listener.OnSwipeRight += HandleOnSwipeRight;
                _listener.OnSwipeUp += HandleOnSwipeTop;
                _listener.OnSwipeDown += HandleOnSwipeDown;
            }
        }

        void HandleTouch(object sender, TouchEventArgs e)
        {
            _detector.OnTouchEvent(e.Event);
        }

        void HandleGenericMotion(object sender, GenericMotionEventArgs e)
        {
            _detector.OnTouchEvent(e.Event);
        }

        void HandleOnSwipeLeft(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeLeft();
        }

        void HandleOnSwipeRight(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeRight();
        }

        void HandleOnSwipeTop(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeUp();
        }

        void HandleOnSwipeDown(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeDown();
        }
    }
}