using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Rebound.Models;

using Windows.System;

namespace Rebound.Views;

public sealed partial class ShellPage : Page
{
    private NavMenuItem _selectedMenuItem;

    public ShellPage()
    {
        InitializeComponent();

        Debug.WriteLine(@$"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\Programs\Rebound 11 Tools");

        BuildTopNavigationMenu();

        if (!CheckArgs())
        {
            MainFrame.Navigate(_selectedMenuItem.TargetType);
        }
    }

    private void BuildTopNavigationMenu()
    {
        var menuItems = new List<NavMenuItem>();

        menuItems.Add(new NavMenuItem
        {
            Id = "Home",
            NormalIcon = "",
            SelectedIcon = "",
            IconFontFamily = new FontFamily("ms-appx:///Fonts/FluentIcons.ttf#FluentSystemIcons-Resizable"),
            Title = "Home",
            TargetType = typeof(HomePage),
        });

        menuItems.Add(new NavMenuItem
        {
            Id = "Rebound11",
            NormalIcon = "",
            SelectedIcon = "",
            IconFontFamily = new FontFamily("ms-appx:///Fonts/FluentIcons.ttf#FluentSystemIcons-Resizable"),
            Title = "Rebound 11",
            TargetType = typeof(Rebound11Page),
        });

        menuItems.Add(new NavMenuItem
        {
            Id = "IviriusUI",
            NormalIcon = "",
            SelectedIcon = "",
            IconFontFamily = new FontFamily("ms-appx:///Fonts/FluentIcons.ttf#FluentSystemIcons-Resizable"),
            Title = "Ivirius.UI",
            TargetType = typeof(EmptyPage),
        });

        var navigationViewItems = BuildNavigationViewItems(menuItems);

        NavigationViewControl.MenuItemsSource = navigationViewItems;
        NavigationViewControl.SelectedItem = navigationViewItems.First();

        _selectedMenuItem = navigationViewItems.First().DataContext as NavMenuItem;
        _selectedMenuItem.IsSelected = true;
    }

    private List<NavigationViewItem> BuildNavigationViewItems(List<NavMenuItem> menuItems)
    {
        var navigationViewItems = new List<NavigationViewItem>();

        foreach (var item in menuItems)
        {
            var fontIcon = new FontIcon
            {
                FontFamily = item.IconFontFamily,
            };

            var iconBinding = new Binding()
            {
                Mode = BindingMode.OneWay,
                Source = item,
                Path = new Microsoft.UI.Xaml.PropertyPath("Icon"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            fontIcon.SetBinding(FontIcon.GlyphProperty, iconBinding);

            var navItem = new NavigationViewItem
            {
                DataContext = item,
                Content = item.Title,
                Icon = fontIcon,
            };

            navigationViewItems.Add(navItem);
        }

        return navigationViewItems;
    }


    public bool CheckArgs()
    {
        if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).ToUpperInvariant().Contains("INSTALLREBOUND11"))
        {
            var selectedItem = (NavigationViewControl.MenuItemsSource as List<NavigationViewItem>)[1];
            
            NavigationViewControl.SelectedItem = selectedItem;

            ChangeMenuSelection(selectedItem.DataContext as NavMenuItem);

            MainFrame.Navigate(_selectedMenuItem.TargetType);

            return true;
        }

        return false;
    }

    private void Navigate(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var navMenuItem = args.InvokedItemContainer.DataContext as NavMenuItem;

        if (navMenuItem.Id == _selectedMenuItem.Id)
        {
            return;
        }

        //// We need to have target type (target page) for all items. If not, we shouldn't add such items to the nav bar.
        if (navMenuItem.TargetType is null)
        {
            return;
        }

        MainFrame.Navigate(navMenuItem.TargetType);

        ChangeMenuSelection(navMenuItem);
    }

    private void ChangeMenuSelection(NavMenuItem newNavMenuItem)
    {
        _selectedMenuItem.IsSelected = false;
        _selectedMenuItem = newNavMenuItem;
        _selectedMenuItem.IsSelected = true;
    }

    private async void VisitDocsWebsite(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://ivirius.com/docs"));
    }

    private async void GoToDiscordServer(object sender, TappedRoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://dsc.gg/ivirius"));
    }
}