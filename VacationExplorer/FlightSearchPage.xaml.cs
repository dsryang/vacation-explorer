using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace VacationExplorer
{
    public sealed partial class FlightSearchPage : Page
    {
        private ObservableCollection<ComboBoxItem> cabinClassCollection;
        private ObservableCollection<ComboBoxItem> currencyCollection;
        private ObservableCollection<string> numAdultCollection;
        private ObservableCollection<string> numChildCollection;
        private ObservableCollection<string> numInfantCollection;

        private Place origin;
        private Place destination;

        public FlightSearchPage()
        {
            this.InitializeComponent();

            cabinClassCollection = new ObservableCollection<ComboBoxItem>();
            currencyCollection = new ObservableCollection<ComboBoxItem>();
            numAdultCollection = new ObservableCollection<string>();
            numChildCollection = new ObservableCollection<string>();
            numInfantCollection = new ObservableCollection<string>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadDates();
            LoadCabinClass();
            LoadCurrency();
            LoadNumberOfPassengers();
        }

        private void LoadDates()
        {
            DateTime now = DateTime.Today;
            DatePickerDeparture.Date = now.AddDays(1);
            DatePickerReturn.Date = now.AddDays(2);
        }

        private void LoadCabinClass()
        {
            ComboBoxCabinClass.ItemsSource = cabinClassCollection;
            cabinClassCollection.Add(new ComboBoxItem("Economy", "Economy"));
            cabinClassCollection.Add(new ComboBoxItem("PremiumEconomy", "Premium Economy"));
            cabinClassCollection.Add(new ComboBoxItem("Business", "Business"));
            cabinClassCollection.Add(new ComboBoxItem("First", "First"));

            ComboBoxCabinClass.SelectedIndex = 0;
        }

        private void LoadCurrency()
        {
            ComboBoxCurrency.ItemsSource = currencyCollection;
            currencyCollection.Add(new ComboBoxItem("CAD", "CAD ($)"));
            currencyCollection.Add(new ComboBoxItem("EUR", "EUR (€)"));
            currencyCollection.Add(new ComboBoxItem("GBP", "GBP (£)"));
            currencyCollection.Add(new ComboBoxItem("USD", "USD ($)"));

            ComboBoxCurrency.SelectedIndex = 0;
        }

        private void LoadNumberOfPassengers()
        {
            ComboBoxAdult.ItemsSource = numAdultCollection;
            ComboBoxChild.ItemsSource = numChildCollection;
            ComboBoxInfant.ItemsSource = numInfantCollection;

            numAdultCollection.Add("0");
            numAdultCollection.Add("1");
            numAdultCollection.Add("2");
            numAdultCollection.Add("3");
            numAdultCollection.Add("4");
            numAdultCollection.Add("5");
            numAdultCollection.Add("6");
            numAdultCollection.Add("7");
            numAdultCollection.Add("8");

            numChildCollection.Add("0");
            numChildCollection.Add("1");
            numChildCollection.Add("2");
            numChildCollection.Add("3");
            numChildCollection.Add("4");
            numChildCollection.Add("5");
            numChildCollection.Add("6");
            numChildCollection.Add("7");
            numChildCollection.Add("8");

            numInfantCollection.Add("0");
            numInfantCollection.Add("1");

            ComboBoxAdult.SelectedIndex = 1;
            ComboBoxChild.SelectedIndex = 0;
            ComboBoxInfant.SelectedIndex = 0;
        }

        private void ComboBoxAdult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int infantSelectedIndex = ComboBoxInfant.SelectedIndex;
            numInfantCollection.Clear();
            numInfantCollection.Add("0");

            for (int i = 1; i <= ComboBoxAdult.SelectedIndex; i++)
            {
                numInfantCollection.Add(i.ToString());
            }

            ComboBoxInfant.SelectedIndex = infantSelectedIndex > ComboBoxAdult.SelectedIndex ? ComboBoxAdult.SelectedIndex : infantSelectedIndex;
        }

        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (origin == null)
            {
                TextBlockError.Visibility = Visibility.Visible;
                TextBlockError.Text = "Please enter an origin! Make sure you select one from the suggestions list.";
                return;
            }

            if (destination == null)
            {
                TextBlockError.Visibility = Visibility.Visible;
                TextBlockError.Text = "Please enter a destination! Make sure you select one from the suggestions list.";
                return;
            }

            if (ComboBoxAdult.SelectedIndex + ComboBoxChild.SelectedIndex + ComboBoxInfant.SelectedIndex == 0)
            {
                TextBlockError.Visibility = Visibility.Visible;
                TextBlockError.Text = "The total number of passengers must be greater than 0!";
                return;
            }

            if (DatePickerDeparture.Date >= DatePickerReturn.Date)
            {
                TextBlockError.Visibility = Visibility.Visible;
                TextBlockError.Text = "The return date must be after the departure date!";
                return;
            }

            if (DatePickerDeparture.Date <= DateTime.Today)
            {
                TextBlockError.Visibility = Visibility.Visible;
                TextBlockError.Text = "The departure date must be after today!";
                return;
            }

            string url = "http://partners.api.skyscanner.net/apiservices/pricing/v1.0";

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            List<KeyValuePair<string, string>> searchParameters = new List<KeyValuePair<string, string>> { };
            searchParameters.Add(new KeyValuePair<string, string>("apiKey", App.skyscannerApiKey));
            searchParameters.Add(new KeyValuePair<string, string>("country", "CA"));
            searchParameters.Add(new KeyValuePair<string, string>("currency", "CAD"));
            searchParameters.Add(new KeyValuePair<string, string>("locale", "en-US"));
            searchParameters.Add(new KeyValuePair<string, string>("originplace", origin.PlaceId));
            searchParameters.Add(new KeyValuePair<string, string>("destinationplace", destination.PlaceId));
            searchParameters.Add(new KeyValuePair<string, string>("outbounddate", DatePickerDeparture.Date.Value.ToString("yyyy-MM-dd")));
            searchParameters.Add(new KeyValuePair<string, string>("inbounddate", DatePickerReturn.Date.Value.ToString("yyyy-MM-dd")));
            searchParameters.Add(new KeyValuePair<string, string>("cabinclass", ComboBoxCabinClass.SelectedValue.ToString()));
            searchParameters.Add(new KeyValuePair<string, string>("adults", ComboBoxAdult.SelectedValue.ToString()));
            searchParameters.Add(new KeyValuePair<string, string>("children", ComboBoxChild.SelectedValue.ToString()));
            searchParameters.Add(new KeyValuePair<string, string>("infants", ComboBoxInfant.SelectedValue.ToString()));
            searchParameters.Add(new KeyValuePair<string, string>("groupPricing", "true"));
            FormUrlEncodedContent content = new FormUrlEncodedContent(searchParameters);
            HttpResponseMessage response = await httpClient.PostAsync(new Uri(url), content);

            if (response.IsSuccessStatusCode)
            {
                string newUrl = response.Headers.Location.ToString() + "?apiKey=" + App.skyscannerApiKey;
                Frame.Navigate(typeof(FlightSearchResultsPage), newUrl);
            }
            else
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                SkyscannerError error = JsonConvert.DeserializeObject<SkyscannerError>(jsonString);

                TextBlockError.Visibility = Visibility.Visible;
                if (error.debugInformation != null && error.debugInformation.ValidationErrors.Length > 0)
                {
                    TextBlockError.Text = error.debugInformation.ValidationErrors[0].Message;
                }
                else
                {
                    TextBlockError.Text = "An error occurred, please try again later.";
                }
            }
        }

        private async Task<List<Place>> getPlaceSuggestions(string query)
        {
            Uri uri = new Uri("http://partners.api.skyscanner.net/apiservices/autosuggest/v1.0/CA/CAD/en-US?apiKey=" + App.skyscannerApiKey + "&query=" + query);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string jsonString = await response.Content.ReadAsStringAsync();

            PlaceResult result = JsonConvert.DeserializeObject<PlaceResult>(jsonString);

            List<Place> places = new List<Place>();
            foreach (Place place in result.Places)
            {
                places.Add(place);
            }

            return places;
        }

        private async void AutoSuggestBoxOrigin_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length >= 2)
                {
                    sender.ItemsSource = await getPlaceSuggestions(sender.Text);
                }
                else
                {
                    sender.ItemsSource = new List<Place> { };
                }
            }
        }

        private async void AutoSuggestBoxDestination_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length > 2)
                {
                    sender.ItemsSource = await getPlaceSuggestions(sender.Text);
                }
                else
                {
                    sender.ItemsSource = new List<Place> { };
                }
            }
        }

        private void AutoSuggestBoxOrigin_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            origin = args.SelectedItem as Place;
        }

        private void AutoSuggestBoxDestination_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            destination = args.SelectedItem as Place;
        }
    }
}
