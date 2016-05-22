using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace VacationExplorer
{
    public sealed partial class VenueDetailsPage : Page
    {
        public string foursquareUrl = "http://foursquare.com/";
        public string facebookUrl = "http://facebook.com/";

        public VenueDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string)
            {
                try
                {
                    await LoadVenue(e.Parameter as string);
                }
                catch (COMException ex)
                {
                    DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage("An error occurred when loading the venue. Please go back and try again.");
                }
            }
        }

        private void DisplayErrorMessage(string message)
        {
            ScrollViewerVenueDetails.Visibility = Visibility.Collapsed;
            StackPanelFoursquare.Visibility = Visibility.Visible;

            StackPanelError.Visibility = Visibility.Visible;
            TextBlockError.Text = message;
        }

        private async Task LoadVenue(string id)
        {
            Uri uri = new Uri("https://api.foursquare.com/v2/venues/" + id + "?client_id=" + App.foursquareClientId + "&client_secret=" + App.foursquareClientSecret + " &v=20160508");

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string jsonString = await response.Content.ReadAsStringAsync();

            VenueResult result = JsonConvert.DeserializeObject<VenueResult>(jsonString);
            if (result.meta != null)
            {
                ProgressRingVenues.IsActive = false;
                ProgressRingVenues.Visibility = Visibility.Collapsed;

                if (result.meta.code == 200 && result.response.venue != null)
                {
                    DisplayVenue(result.response.venue);
                }
                else if (result.meta.errorDetail != null)
                {
                    DisplayErrorMessage(result.meta.errorDetail);
                }
                else
                {
                    StackPanelError.Visibility = Visibility.Visible;
                }
            }
        }

        private async void DisplayVenue(Venue venue)
        {
            ScrollViewerVenueDetails.Visibility = Visibility.Visible;
            StackPanelFoursquare.Visibility = Visibility.Visible;

            TextBlockTitle.Text = "Viewing " + venue.name;

            foursquareUrl = venue.canonicalUrl + "?ref=" + App.foursquareClientId;

            if (venue.location != null)
            {
                MapControlLocation.MapServiceToken = App.bingMapsApiKey;

                BasicGeoposition position = new BasicGeoposition();
                position.Latitude = venue.location.lat;
                position.Longitude = venue.location.lng;
                Geopoint mapLocationPoint = new Geopoint(position);
                await MapControlLocation.TrySetViewAsync(mapLocationPoint, 16D);

                MapIcon icon = new MapIcon();
                icon.Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = venue.location.lat,
                    Longitude = venue.location.lng
                });
                icon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                icon.Title = venue.name;
                MapControlLocation.MapElements.Add(icon);

                MapControlLocation.Visibility = Visibility.Visible;
            }

            if (venue.location.formattedAddress.Length > 0)
            {
                StackPanelAddress.Visibility = Visibility.Visible;
                TextBlockAddress1.Text = venue.location.formattedAddress[0];
                TextBlockAddress1.Visibility = Visibility.Visible;

                if (venue.location.formattedAddress.Length >= 2)
                {
                    TextBlockAddress2.Text = venue.location.formattedAddress[1];
                    TextBlockAddress2.Visibility = Visibility.Visible;
                }

                if (venue.location.formattedAddress.Length >= 3)
                {
                    TextBlockAddress3.Text = venue.location.formattedAddress[2];
                    TextBlockAddress3.Visibility = Visibility.Visible;
                }
            }

            if (venue.description != null)
            {
                StackPanelAbout.Visibility = Visibility.Visible;
                TextBlockDescription.Text = venue.description;
                TextBlockDescription.Visibility = Visibility.Visible;
            }

            if (venue.url != null)
            {
                StackPanelAbout.Visibility = Visibility.Visible;
                TextBlockUrl.Text = venue.url;
                TextBlockUrl.Visibility = Visibility.Visible;
            }

            if (venue.contact != null)
            {
                if (venue.contact.formattedPhone != null)
                {
                    StackPanelContact.Visibility = Visibility.Visible;
                    TextBlockPhone.Text = venue.contact.formattedPhone;
                    TextBlockPhone.Visibility = Visibility.Visible;
                }

                if (venue.contact.facebookName != null)
                {
                    StackPanelContact.Visibility = Visibility.Visible;
                    StackPanelFacebook.Visibility = Visibility.Visible;
                    TextBlockFacebook.Text = venue.contact.facebookName;
                    TextBlockFacebook.Visibility = Visibility.Visible;

                    if (venue.contact.facebookUsername != null)
                    {
                        facebookUrl = facebookUrl + venue.contact.facebookUsername;
                    }
                    else if (venue.contact.facebook != null)
                    {
                        facebookUrl = facebookUrl + venue.contact.facebook;
                    }
                }
                else if (venue.contact.facebookUsername != null)
                {
                    StackPanelContact.Visibility = Visibility.Visible;
                    StackPanelFacebook.Visibility = Visibility.Visible;
                    TextBlockFacebook.Text = venue.contact.facebookUsername;
                    TextBlockFacebook.Visibility = Visibility.Visible;
                }

                if (venue.contact.twitter != null)
                {
                    StackPanelContact.Visibility = Visibility.Visible;
                    StackPanelTwitter.Visibility = Visibility.Visible;
                    TextBlockTwitter.Text = "@" + venue.contact.twitter;
                    TextBlockTwitter.Visibility = Visibility.Visible;
                }
            }
        }

        private async void StackPanelFoursquare_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(foursquareUrl));
        }

        private async void TextBlockUrl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(TextBlockUrl.Text));
        }

        private async void StackPanelTwitter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://twiter.com/" + TextBlockTwitter.Text.Substring(1)));
        }

        private async void StackPanelFacebook_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(facebookUrl));
        }
    }
}
