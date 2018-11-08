using System.Collections.Generic;

using FreshMvvm;
using Xamarin.Forms;

namespace DailyPrayer.Controls
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string IconSource { get; set; }
    }

    public class ThemedMasterDetailNavigationContainer : FreshMasterDetailNavigationContainer
    {
        List<MenuItem> pageIcons = new List<MenuItem>();

        public void AddPageWithIcon<T>(string title, string icon = "", object data = null) where T : FreshBasePageModel
        {
            base.AddPage<T>(title, data);
            pageIcons.Add(new MenuItem
            {
                Title = title,
                IconSource = icon
            });
        }

        protected override void CreateMenuPage(string menuPageTitle, string menuIcon = null)
        {
            var listview = new ListView();
            var _menuPage = new ContentPage();
            _menuPage.Title = menuPageTitle;
            _menuPage.BackgroundColor = Color.FromHex("#c8c8c8");

            listview.ItemsSource = pageIcons;

            var cell = new DataTemplate(typeof(ImageCell));
            cell.SetValue(TextCell.TextColorProperty, Color.Black);
            cell.SetBinding(ImageCell.TextProperty, "Title");
            cell.SetBinding(ImageCell.ImageSourceProperty, "IconSource");


            listview.ItemTemplate = cell;
            listview.ItemSelected += (sender, args) =>
            {
                if (Pages.ContainsKey(((MenuItem)args.SelectedItem).Title))
                {
                    Detail = Pages[((MenuItem)args.SelectedItem).Title];
                }
                IsPresented = false;
            };

            _menuPage.Content = listview;

            var navPage = new NavigationPage(_menuPage) { Title = "Menu" };

            if (!string.IsNullOrEmpty(menuIcon))
                navPage.Icon = menuIcon;

            Master = navPage;
        }

        protected override Page CreateContainerPage(Page page)
        {
            var navigation = new NavigationPage(page);
            navigation.BarTextColor = Color.White;

            return navigation;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
