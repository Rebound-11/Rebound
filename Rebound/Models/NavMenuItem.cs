using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Media;

namespace Rebound.Models;

internal partial class NavMenuItem : ObservableObject
{
    [ObservableProperty]
    public partial string Id
    {
        get; set;
    }

    [ObservableProperty]
    public partial string NormalIcon
    {
        get; set;
    }

    [ObservableProperty]
    public partial string SelectedIcon
    {
        get; set;
    }

    [ObservableProperty]
    public partial FontFamily IconFontFamily
    {
        get; set;
    }

    [ObservableProperty]
    public partial string Title
    {
        get; set;
    }

    [ObservableProperty]
    public partial Type TargetType
    {
        get; set;
    }

    public string Icon
    {
        get
        {
            if (_isSelected)
            {
                return SelectedIcon;
            }

            return NormalIcon;
        }
    }

    private bool _isSelected = false;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;

            OnPropertyChanged(nameof(Icon));
        }
    }
}