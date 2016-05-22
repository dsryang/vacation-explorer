using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VacationExplorer
{
    public sealed partial class MainPage : Page
    {
        private List<ExploreImage> cities;
        private ObservableCollection<ExploreImage> collection;

        public MainPage()
        {
            this.InitializeComponent();

            cities = new List<ExploreImage>();
            collection = new ObservableCollection<ExploreImage>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ListViewExplore.ItemsSource = collection;
            LoadExploreCities();
        }

        private void LoadExploreCities()
        {
            cities.Add(new ExploreImage("/Assets/Explore-Toronto.png", "Toronto", "43.6425662", "-79.3870568"));
            cities.Add(new ExploreImage("/Assets/Explore-London.png", "London", "51.5007292", "-0.1246254"));
            cities.Add(new ExploreImage("/Assets/Explore-Singapore.png", "Singapore", "1.3323467", "103.8119114"));
            cities.Add(new ExploreImage("/Assets/Explore-Paris.png", "Paris", "48.8583736", "2.2922926"));

            foreach (ExploreImage city in cities)
            {
                collection.Add(city);
            }
        }

        private void ButtonExplore_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxDestination.Text.Length > 0)
            {
                SearchParams searchParams = new SearchParams();
                searchParams.cityName = TextBoxDestination.Text;

                Frame.Navigate(typeof(CategoryPage), searchParams);
            }
        }

        private void ListViewExplore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchParams searchParams = new SearchParams();
            searchParams.cityName = cities[ListViewExplore.SelectedIndex].name;
            searchParams.latlon = cities[ListViewExplore.SelectedIndex].latitude + "," + cities[ListViewExplore.SelectedIndex].longitude;

            Frame.Navigate(typeof(CategoryPage), searchParams);
        }

        private void TextBlockCopyright_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(CopyrightInfoPage));
        }
    }
}
