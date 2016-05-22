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
    public sealed partial class FlightDetailsPage : Page
    {
        private string url;
        private Agent[] agents;
        private ObservableCollection<FlightSegmentItem> outboundSegmentCollection;
        private ObservableCollection<FlightSegmentItem> inboundSegmentCollection;
        private ObservableCollection<AgentsListItem> agentsCollection;

        public FlightDetailsPage()
        {
            this.InitializeComponent();

            outboundSegmentCollection = new ObservableCollection<FlightSegmentItem>();
            inboundSegmentCollection = new ObservableCollection<FlightSegmentItem>();
            agentsCollection = new ObservableCollection<AgentsListItem>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is FlightDetailsParams)
            {
                FlightDetailsParams flightDetailsParams = e.Parameter as FlightDetailsParams;
                url = flightDetailsParams.url;
                agents = flightDetailsParams.agents;

                try
                {
                    await LoadFlightDetails();
                }
                catch (COMException ex)
                {
                    DisplayErrorMessage("Couldn't connect to the server. Please check your internet connection!");
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage("An error occurred when searching for flights. Please go back and try again.");
                }
            }
            else
            {
                DisplayErrorMessage("An error occurred when searching for flights. Please go back and try again.");
            }
        }

        private void DisplayErrorMessage(string message)
        {
            ProgressRing.IsActive = false;
            ProgressRing.Visibility = Visibility.Collapsed;
            ScrollViewerDetails.Visibility = Visibility.Collapsed;
            StackPanelError.Visibility = Visibility.Visible;
            TextBlockError.Text = message;
        }

        private async Task LoadFlightDetails()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(DateTime.UtcNow);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url));
            string jsonString = await response.Content.ReadAsStringAsync();

            SkyscannerBookingResponse result = JsonConvert.DeserializeObject<SkyscannerBookingResponse>(jsonString);
            DisplayFlightDetails(result);
            DisplayAgentDetails(result);
        }

        private void DisplayFlightDetails(SkyscannerBookingResponse result)
        {
            foreach (Segment segment in result.Segments)
            {
                FlightSegmentItem segmentItem = new FlightSegmentItem();
                segmentItem.directionality = segment.Directionality;

                DateTime departure = DateTime.Parse(segment.DepartureDateTime);
                DateTime arrival = DateTime.Parse(segment.ArrivalDateTime);
                segmentItem.flightTime = departure.ToString("H:mm") + " - " + arrival.ToString("H:mm");
                int daysDiff = (arrival - departure).Days;
                if (daysDiff > 0)
                {
                    if (daysDiff == 1)
                    {
                        segmentItem.addDays = "(+" + daysDiff + " day)";
                    }
                    else
                    {
                        segmentItem.addDays = "(+" + daysDiff + " days)";
                    }
                }

                TimeSpan duration = TimeSpan.FromMinutes(segment.Duration);
                if (duration.Hours > 0)
                {
                    segmentItem.duration = duration.Hours + "h " + duration.Minutes + "m";
                }
                else
                {
                    segmentItem.duration = duration.Minutes + "m";
                }

                int remaining = 2;
                string originName = "";
                string destinationName = "";
                foreach (TripPlace place in result.Places)
                {
                    if (remaining == 0)
                    {
                        break;
                    }

                    if (place.Id == segment.OriginStation)
                    {
                        originName = place.Name;
                        remaining--;
                    }

                    if (place.Id == segment.DestinationStation)
                    {
                        destinationName = place.Name;
                        remaining--;
                    }
                }
                segmentItem.airports = originName + " - " + destinationName;
                
                foreach (Carrier carrier in result.Carriers)
                {
                    if (carrier.Id == segment.OperatingCarrier)
                    {
                        segmentItem.airline = "Operated by " + carrier.Name;
                    }
                }

                if (segment.Directionality.Equals("Outbound"))
                {
                    outboundSegmentCollection.Add(segmentItem);
                }
                else
                {
                    inboundSegmentCollection.Add(segmentItem);
                }
            }
            ListViewOutboundSegments.ItemsSource = outboundSegmentCollection;
            ListViewInboundSegments.ItemsSource = inboundSegmentCollection;
        }

        private void DisplayAgentDetails(SkyscannerBookingResponse result)
        {
            foreach (BookingOption bookingOption in result.BookingOptions)
            {
                foreach (BookingItem bookingItem in bookingOption.BookingItems)
                {
                    AgentsListItem item = new AgentsListItem();

                    foreach (Agent agent in agents)
                    {
                        if (agent.Id == bookingItem.AgentID && !(bookingItem.Status.Equals("Failed") || bookingItem.Status.Equals("NotAvailable")))
                        {
                            item.name = agent.Name;
                            item.price = string.Format("{0:C2}", bookingItem.Price);
                            item.deeplink = bookingItem.Deeplink;
                        }
                    }
                    agentsCollection.Add(item);
                }
            }
            ListViewAgents.ItemsSource = agentsCollection;

            ProgressRing.IsActive = false;
            ProgressRing.Visibility = Visibility.Collapsed;
        }

        private async void ListViewAgents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((sender as ListView).SelectedItem as AgentsListItem).deeplink));
        }
    }
}
