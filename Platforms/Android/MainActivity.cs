using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MyNotes.Helpers;

namespace MyNotes
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Usa ViewTreeObserver per osservare il layout e rilevare la tastiera
            Window.DecorView.ViewTreeObserver.GlobalLayout += (sender, args) =>
            {
                Rect r = new Rect();
                Window.DecorView.GetWindowVisibleDisplayFrame(r);
                double screenHeight = Window.DecorView.RootView.Height;
                double keypadHeight = screenHeight - r.Bottom;

                if (keypadHeight > screenHeight * 0.15) // Se la tastiera copre più del 15% dello schermo
                {
                    KeyboardHelper.TriggerKeyboardAppeared(keypadHeight);
                }
                else
                {
                    KeyboardHelper.TriggerKeyboardDisappeared();
                }
            };
        }
    }
}
