using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VacationExplorer
{
    public sealed partial class FlightSearchResultsPage : Page
    {
        private string url;
        private DispatcherTimer timer;
        private ObservableCollection<FlightsListItem> collection;
        private string sessionKey;
        private Query query;
        private Agent[] agents;

        public FlightSearchResultsPage()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            collection = new ObservableCollection<FlightsListItem>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string)
            {
                url = e.Parameter as string;

                try
                {
                    await LoadFlightsList();
                }
                catch (COMException ex)
                {
                    DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
                }
                catch (Exception ex)
                {
                    //DisplayErrorMessage("An error occurred when searching for flights. Please go back and try again.");
                    DisplayErrorMessage("Sorry, something unexpected occurred! Please send an email to dydevelopers@outlook.com describing what you did. Please include the following error message: " + ex.Message);
                }
            }
            else
            {
                DisplayErrorMessage("An error occurred when searching for flights. Please go back and try again.");
            }
        }

        private void DisplayErrorMessage(string message)
        {
            ProgressRingFlights.IsActive = false;
            StackPanelLoading.Visibility = Visibility.Collapsed;
            ListViewFlights.Visibility = Visibility.Collapsed;
            StackPanelError.Visibility = Visibility.Visible;
            TextBlockError.Text = message;
        }

        private async Task LoadFlightsList()
        {
            // Stop the timer if it's already running to prevent multiple server calls if the network's slow
            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(DateTime.UtcNow);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url));
            string jsonString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                // Nothing changed, wait for 15 seconds this time
                if (!timer.IsEnabled)
                {
                    timer.Tick += new EventHandler<object>(timerTick);
                    timer.Interval = new TimeSpan(0, 0, 15);
                    timer.Start();
                }
            }
            else
            {
                SkyscannerResponse result = JsonConvert.DeserializeObject<SkyscannerResponse>(jsonString);
                if (result.Status.Equals("UpdatesComplete"))
                {
                    if (result.Itineraries.Length == 0)
                    {
                        DisplayErrorMessage("No flights were found for your search.");
                    }
                    else
                    {
                        agents = result.Agents;
                        DisplayFlightsList(result);
                    }
                }
                else
                {
                    // Updates aren't complete yet, try again in 10 seconds
                    if (!timer.IsEnabled)
                    {
                        timer.Tick += new EventHandler<object>(timerTick);
                        timer.Interval = new TimeSpan(0, 0, 10);
                        timer.Start();
                    }
                }
            }
        }

        private async void timerTick(object sender, object e)
        {
            if (timer.IsEnabled)
            {
                await LoadFlightsList();
            }
        }

        private void DisplayFlightsList(SkyscannerResponse result)
        {
            sessionKey = result.SessionKey;
            query = result.Query;

            foreach (Itinerary itinerary in result.Itineraries)
            {
                FlightsListItem item = new FlightsListItem();
                int[] outboundCarriers = { };
                int[] inboundCarriers = { };
                int outboundOrigin = 0;
                int outboundDestination = 0;
                int inboundOrigin = 0;
                int inboundDestination = 0;

                item.outboundId = itinerary.OutboundLegId;
                item.inboundId = itinerary.InboundLegId;

                int remaining = 2;
                foreach (Leg leg in result.Legs)
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    if (leg.Id.Equals(itinerary.OutboundLegId))
                    {
                        DateTime departure = DateTime.Parse(leg.Departure);
                        DateTime arrival = DateTime.Parse(leg.Arrival);
                        item.outboundFlightTime = departure.ToString("H:mm") + " - " + arrival.ToString("H:mm");
                        int daysDiff = (arrival - departure).Days;
                        if (daysDiff > 0)
                        {
                            if (daysDiff == 1)
                            {
                                item.outboundAddDays = "(+" + daysDiff + " day)";
                            }
                            else
                            {
                                item.outboundAddDays = "(+" + daysDiff + " days)";
                            }
                        }

                        TimeSpan duration = TimeSpan.FromMinutes(leg.Duration);
                        if (duration.Hours > 0)
                        {
                            item.outboundDuration = ((duration.Days * 24) + duration.Hours) + "h " + duration.Minutes + "m";
                        }
                        else
                        {
                            item.outboundDuration = duration.Minutes + "m";
                        }

                        int stops = leg.SegmentIds.Length - 1;
                        if (stops == 0)
                        {
                            item.outboundStops = "Direct Flight";
                        }
                        else if (stops == 1)
                        {
                            item.outboundStops = stops + " Stop";
                        }
                        else
                        {
                            item.outboundStops = stops + " Stops";
                        }

                        outboundCarriers = leg.OperatingCarriers;
                        outboundOrigin = leg.OriginStation;
                        outboundDestination = leg.DestinationStation;
                        remaining--;
                    }
                    else if (leg.Id.Equals(itinerary.InboundLegId))
                    {
                        DateTime departure = DateTime.Parse(leg.Departure);
                        DateTime arrival = DateTime.Parse(leg.Arrival);
                        item.inboundFlightTime = departure.ToString("H:mm") + " - " + arrival.ToString("H:mm");
                        int daysDiff = (arrival - departure).Days;
                        if (daysDiff > 0)
                        {
                            if (daysDiff == 1)
                            {
                                item.inboundAddDays = "(+" + daysDiff + " day)";
                            }
                            else
                            {
                                item.inboundAddDays = "(+" + daysDiff + " days)";
                            }
                        }

                        TimeSpan duration = TimeSpan.FromMinutes(leg.Duration);
                        if (duration.Hours > 0)
                        {
                            item.inboundDuration = ((duration.Days * 24) + duration.Hours) + "h " + duration.Minutes + "m";
                        }
                        else
                        {
                            item.inboundDuration = duration.Minutes + "m";
                        }

                        int stops = leg.SegmentIds.Length - 1;
                        if (stops == 0)
                        {
                            item.inboundStops = "Direct Flight";
                        }
                        else if (stops == 1)
                        {
                            item.inboundStops = stops + " Stop";
                        }
                        else
                        {
                            item.inboundStops = stops + " Stops";
                        }

                        inboundCarriers = leg.OperatingCarriers;
                        inboundOrigin = leg.OriginStation;
                        inboundDestination = leg.DestinationStation;
                        remaining--;
                    }
                }

                remaining = 4;
                string outboundOriginName = "";
                string outboundDestinationName = "";
                string inboundOriginName = "";
                string inboundDestinationName = "";
                foreach (TripPlace place in result.Places)
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    if (place.Id == outboundOrigin)
                    {
                        outboundOriginName = place.Code;
                        remaining--;
                    }
                    else if (place.Id == outboundDestination)
                    {
                        outboundDestinationName = place.Code;
                        remaining--;
                    }

                    if (place.Id == inboundOrigin)
                    {
                        inboundOriginName = place.Code;
                        remaining--;
                    }
                    else if (place.Id == inboundDestination)
                    {
                        inboundDestinationName = place.Code;
                        remaining--;
                    }
                }
                item.outboundAirports = outboundOriginName + " - " + outboundDestinationName;
                item.inboundAirports = inboundOriginName + " - " + inboundDestinationName;

                remaining = inboundCarriers.Length + outboundCarriers.Length;
                foreach (Carrier carrier in result.Carriers)
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    for (int i = 0; i < outboundCarriers.Length; i++)
                    {
                        if (carrier.Id == outboundCarriers[i])
                        {
                            if (item.outboundAirlines.Length > 0)
                            {
                                item.outboundAirlines += ", " + carrier.Name;
                            }
                            else
                            {
                                item.outboundAirlines += "Operated by " + carrier.Name;
                            }
                            remaining--;
                        }
                    }

                    for (int i = 0; i < inboundCarriers.Length; i++)
                    {
                        if (carrier.Id == inboundCarriers[i])
                        {
                            if (item.inboundAirlines.Length > 0)
                            {
                                item.inboundAirlines += ", " + carrier.Name;
                            }
                            else
                            {
                                item.inboundAirlines += "Operated by " + carrier.Name;
                            }
                            remaining--;
                        }
                    }
                }

                item.lowestPrice = "From " + string.Format("{0:C2}", itinerary.PricingOptions[0].Price);

                collection.Add(item);
            }
            ListViewFlights.ItemsSource = collection;

            ProgressRingFlights.IsActive = false;
            StackPanelLoading.Visibility = Visibility.Collapsed;
        }

        private async void ListViewFlights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string url = "http://partners.api.skyscanner.net/apiservices/pricing/v1.0/" + sessionKey + "/booking?apiKey=" + App.skyscannerApiKey;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>> { };
            parameters.Add(new KeyValuePair<string, string>("apiKey", App.skyscannerApiKey));
            parameters.Add(new KeyValuePair<string, string>("outboundlegid", ((sender as ListView).SelectedItem as FlightsListItem).outboundId));
            parameters.Add(new KeyValuePair<string, string>("inboundlegid", ((sender as ListView).SelectedItem as FlightsListItem).inboundId));
            parameters.Add(new KeyValuePair<string, string>("adults", query.Adults.ToString()));
            parameters.Add(new KeyValuePair<string, string>("children", query.Children.ToString()));
            parameters.Add(new KeyValuePair<string, string>("infants", query.Infants.ToString()));
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await httpClient.PutAsync(new Uri(url), content);

            if (response.IsSuccessStatusCode)
            {
                FlightDetailsParams flightDetailsParams = new FlightDetailsParams();
                flightDetailsParams.url = response.Headers.Location.ToString() + "?apiKey=" + App.skyscannerApiKey;
                flightDetailsParams.agents = agents;
                Frame.Navigate(typeof(FlightDetailsPage), flightDetailsParams);
            }
        }

        private async void StackPanelSkyscanner_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://skyscanner.net"));
        }
    }
}
