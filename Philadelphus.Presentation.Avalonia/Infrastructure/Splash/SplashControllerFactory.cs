using Philadelphus.Presentation.Avalonia.Views.Windows;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Splash
{
    internal static class SplashControllerFactory
    {
        public static ISplashController Create()
        {
            var externalSplash = ExternalSplashController.TryStart();
            if (externalSplash != null)
            {
                return externalSplash;
            }

            var splash = new SplashWindow();
            splash.Show();
            return new WindowSplashController(splash);
        }
    }
}
