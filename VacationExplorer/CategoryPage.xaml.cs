using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class CategoryPage : Page
    {
        private SearchParams searchParams;
        private List<ExploreImage> categories;
        private ObservableCollection<ExploreImage> collection;

        public CategoryPage()
        {
            this.InitializeComponent();

            categories = new List<ExploreImage>();
            collection = new ObservableCollection<ExploreImage>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is SearchParams)
            {
                searchParams = e.Parameter as SearchParams;
            }
            ListViewCategories.ItemsSource = collection;
            LoadExploreCategories();
        }

        private void LoadExploreCategories()
        {
            categories.Add(new ExploreImage("/Assets/Category-Arts-Entertainment.png", "Arts and Entertainment", "arts"));
            categories.Add(new ExploreImage("/Assets/Category-Outdoors-Recreation.png", "Outdoors and Recreation", "outdoors"));
            categories.Add(new ExploreImage("/Assets/Category-Sights.png", "Sights", "sights"));

            foreach (ExploreImage category in categories)
            {
                collection.Add(category);
            }
        }

        private void ListViewCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            searchParams.categoryName = categories[ListViewCategories.SelectedIndex].name;
            searchParams.query = categories[ListViewCategories.SelectedIndex].query;

            Frame.Navigate(typeof(ExploreResultsPage), searchParams);
        }
    }
}
