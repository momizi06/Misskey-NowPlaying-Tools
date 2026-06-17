using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Misskey_SMTC.Media;
using Misskey_SMTC.Misskey;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace Misskey_SMTC
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? m_window;
        internal static Authentication MisskeyAuth { get; } = new();
        public static MediaService Media { get; } = new();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var keyInstance = AppInstance.FindOrRegisterForKey("main");

            if (!keyInstance.IsCurrent)
            {
                await keyInstance.RedirectActivationToAsync(
                    AppInstance.GetCurrent().GetActivatedEventArgs());

                Process.GetCurrentProcess().Kill();
                return;
            }

            keyInstance.Activated += async (_, e) =>
            {
                if (e.Kind == ExtendedActivationKind.Protocol &&
                    e.Data is ProtocolActivatedEventArgs protocolArgs)
                {
                    await MisskeyAuth.OnProtocolActivatedAsync(protocolArgs.Uri);
                }
            };
            App.Media.Start();
            m_window ??= new MainWindow();
            m_window.Activate();

            App.Media.NowPlaying.AppleMusicCompat =
                ApplicationData.Current.LocalSettings.Values["AppleMusicCompat"] is not bool b || b;

            // 初回起動自体がProtocolだった場合
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedArgs.Kind == ExtendedActivationKind.Protocol &&
                activatedArgs.Data is ProtocolActivatedEventArgs p)
            {
                await MisskeyAuth.OnProtocolActivatedAsync(p.Uri);
            }
        }
    }

}
