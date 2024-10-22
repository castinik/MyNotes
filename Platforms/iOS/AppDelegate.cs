using Foundation;
using MyNotes.Helpers;
using UIKit;

namespace MyNotes
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShow);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHide);
            return base.FinishedLaunching(app, options);
        }

        private void OnKeyboardDidShow(NSNotification notification)
        {
            var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification).Height;
            KeyboardHelper.TriggerKeyboardAppeared(keyboardFrame);
        }

        private void OnKeyboardWillHide(NSNotification notification)
        {
            KeyboardHelper.TriggerKeyboardDisappeared();
        }
    }
}
