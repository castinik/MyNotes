using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using MyNotes.Pages;
using MyNotes.Services;
using MyNotes.ViewModels;

namespace MyNotes
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register ViewModels and Pages
            builder.Services.AddSingleton<INoteService, NoteService>();

            // Register the FolderPicker as a singleton
            //builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddTransient<NotePage>();
            builder.Services.AddTransient<NoteViewModel>();

            builder.Services.AddTransient<FolderPage>();
            builder.Services.AddTransient<FolderViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
