using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace VacationExplorer
{
    public sealed partial class ExploreResultsPage : Page
    {
        private SearchParams searchParams;
        private ObservableCollection<VenueListItem> collection;
        private string cityName;

        public ExploreResultsPage()
        {
            this.InitializeComponent();

            collection = new ObservableCollection<VenueListItem>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is SearchParams)
            {
                searchParams = e.Parameter as SearchParams;
                TextBlockTitle.Text = "Exploring " + searchParams.categoryName + " in " + searchParams.cityName;
                cityName = searchParams.cityName;
                TextBlockMessage.Text = "Check out flights to " + cityName + "!";
            }

            ListViewVenues.ItemsSource = collection;

            try
            {
                await LoadVenuesList();
            }
            catch (COMException ex)
            {
                DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("An error occurred when getting venues. Please go back and try again.");
            }
        }

        private void DisplayErrorMessage(string message)
        {
            StackPanelError.Visibility = Visibility.Visible;
            TextBlockError.Text = message;
        }

        private async Task LoadVenuesList()
        {
            Uri uri;

            if (searchParams.latlon != null)
            {
                uri = new Uri("https://api.foursquare.com/v2/venues/explore?client_id=" + App.foursquareClientId + "&client_secret=" + App.foursquareClientSecret + " &v=20160508&ll=" + searchParams.latlon + "&section=" + searchParams.query);
            }
            else
            {
                uri = new Uri("https://api.foursquare.com/v2/venues/explore?client_id=" + App.foursquareClientId + "&client_secret=" + App.foursquareClientSecret + " &v=20160508&near=" + searchParams.cityName + "&section=" + searchParams.query);
            }

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string jsonString = await response.Content.ReadAsStringAsync();

            VenueResult result = JsonConvert.DeserializeObject<VenueResult>(jsonString);
            if (result.meta != null)
            {
                ProgressRingVenues.IsActive = false;
                ProgressRingVenues.Visibility = Visibility.Collapsed;

                if (result.meta.code == 200)
                {
                    GridFlights.Visibility = Visibility.Visible;

                    if (searchParams.latlon == null && result.response.geocode != null)
                    {
                        if (result.response.geocode != null)
                        {
                            TextBlockTitle.Text = "Exploring " + searchParams.categoryName + " in " + result.response.geocode.displayString;
                            cityName = result.response.geocode.displayString;
                        }
                        else
                        {
                            TextBlockTitle.Text = "Exploring " + searchParams.categoryName + " in " + result.response.headerFullLocation;
                            cityName = result.response.headerFullLocation;
                        }
                        TextBlockMessage.Text = "Check out flights to " + cityName + "!";
                    }
                    DisplayVenuesList(result.response);
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

        private void DisplayVenuesList(Response response)
        {
            if (response.groups.Length > 0 && response.groups[0].items.Length > 0)
            {
                GroupItem[] items = response.groups[0].items;
                foreach (GroupItem item in items)
                {
                    VenueListItem venueListItem = new VenueListItem();

                    venueListItem.id = item.venue.id;
                    venueListItem.name = item.venue.name;

                    if (item.venue.rating != 0)
                    {
                        venueListItem.rating = string.Format("{0:0.0}", item.venue.rating);
                        venueListItem.ratingColor = "#" + item.venue.ratingColor;
                    }
                    else
                    {
                        venueListItem.rating = "N/A";
                        venueListItem.ratingColor = "DarkGray";
                    }

                    if (item.venue.categories.Length > 0)
                    {
                        venueListItem.category = item.venue.categories[0].name;
                        if (item.venue.categories[0].icon != null)
                        {
                            venueListItem.imageUrl = new Uri(item.venue.categories[0].icon.prefix + "bg_44" + item.venue.categories[0].icon.suffix);
                        }
                    }
                    collection.Add(venueListItem);
                }
            }
        }

        private void ListViewVenues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string id = ((sender as ListView).SelectedItem as VenueListItem).id;
            Frame.Navigate(typeof(VenueDetailsPage), id);
        }

        private void GridFlights_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(FlightSearchPage));
        }
    }
}
