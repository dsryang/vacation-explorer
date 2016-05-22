using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VacationExplorer
{
    sealed partial class App : Application
    {
        public static string bingMapsApiKey;
        public static string foursquareClientId;
        public static string foursquareClientSecret;
        public static string skyscannerApiKey;
        
        public static string currency;
        public static int currencyIndex;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
        
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.Navigated += OnNavigated;
                rootFrame.NavigationFailed += OnNavigationFailed;

                await getBingMapsApiKey();
                await getFoursquareApiKey();
                await getSkyscannerApiKey();
                getLocalSettings();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                // Handle back button requests
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                // Check if the back button should be displayed or not
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void OnNavigated(Object sender, NavigationEventArgs e)
        {
            // Check if the back button should be displayed or not
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        private async Task getBingMapsApiKey()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///BingMapsApiKey.txt"));
            bingMapsApiKey = await FileIO.ReadTextAsync(file);
        }

        private async Task getFoursquareApiKey()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///FoursquareClientSecret.txt"));
            foursquareClientSecret = await FileIO.ReadTextAsync(file);
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///FoursquareClientId.txt"));
            foursquareClientId = await FileIO.ReadTextAsync(file);
        }

        private async Task getSkyscannerApiKey()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SkyscannerApiKey.txt"));
            skyscannerApiKey = await FileIO.ReadTextAsync(file);
        }

        private void getLocalSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("currency"))
            {
                currency = (string) localSettings.Values["currency"];
                currencyIndex = (int) localSettings.Values["currencyIndex"];
            }
            else
            {
                currency = "CAD";
                currencyIndex = 0;
            }
        }
    }
}
