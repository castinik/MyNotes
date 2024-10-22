namespace MyNotes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
#if ANDROID
                var entryColor = Android.Graphics.Color.ParseColor("#fdb44b");
                var highlightColor = Android.Graphics.Color.ParseColor("#E6A9A9A9");
                handler.PlatformView.TextCursorDrawable.SetColorFilter(entryColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.TextSelectHandle.SetColorFilter(entryColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.TextSelectHandleLeft.SetColorFilter(entryColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.TextSelectHandleRight.SetColorFilter(entryColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.SetHighlightColor(highlightColor);
                handler.PlatformView.Background = null;
#endif
            });
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("EditorCustomization", (handler, view) =>
            {
#if ANDROID
                var editorColor = Android.Graphics.Color.ParseColor("#fdb44b");
                var highlightColor = Android.Graphics.Color.ParseColor("#E6A9A9A9");
                handler.PlatformView.TextCursorDrawable.SetColorFilter(editorColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.TextSelectHandle.SetColorFilter(editorColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.TextSelectHandleLeft.SetColorFilter(editorColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.TextSelectHandleRight.SetColorFilter(editorColor, Android.Graphics.PorterDuff.Mode.SrcIn);
                handler.PlatformView.SetHighlightColor(highlightColor);
                handler.PlatformView.Background = null;
#endif
            });

            MainPage = new AppShell();
        }
    }
}
