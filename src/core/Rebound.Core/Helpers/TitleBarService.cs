using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;
using WinUIEx;
using WinUIEx.Messaging;

namespace Rebound.Helpers;

public class TitleBarService
{
    private readonly WindowEx CurrentWindow;
    private readonly FrameworkElement AccentStrip;
    private readonly Image TitleBarIcon;
    private readonly TextBlock WindowTitle;
    private readonly Button Close;
    private readonly Button MaxRes;
    private readonly Button Minimize;
    private readonly FontIcon MaxResGlyph;
    private readonly FrameworkElement Content;

    private WindowMessageMonitor MessageMonitor
    {
        get; set;
    }
    private bool IsWindowFocused { get; set; } = false;
    private SolidColorBrush ButtonBrush
    {
        get; set;
    }

    private double AdditionalHeight = 0;

    public SelectedCaptionButton CurrentCaption = SelectedCaptionButton.None;

    public enum SelectedCaptionButton
    {
        None = 0,
        Minimize = 1,
        Maximize = 2,
        Close = 3
    }

    public TitleBarService(WindowEx win, FrameworkElement accentStrip, Image titleBarIcon, TextBlock windowTitle, Button close, Button maxres, Button min, FontIcon maxResGlyph, FrameworkElement content)
    {
        CurrentWindow = win;
        AccentStrip = accentStrip;
        TitleBarIcon = titleBarIcon;
        WindowTitle = windowTitle;
        Close = close;
        MaxRes = maxres;
        Minimize = min;
        MaxResGlyph = maxResGlyph;
        Content = content;
        CurrentWindow.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        CurrentWindow.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
        Content.PointerMoved += PointerMoved;
        Content.PointerReleased += PointerReleased;
        Content.PointerExited += PointerExited;
        CurrentWindow.WindowStateChanged += CurrentWindow_WindowStateChanged;
        LoadBounds();
        CheckWindow();
        Rehook();
    }

    public async void CheckWindow()
    {
        try
        {
            if (WindowTitle != null && CurrentWindow != null)
            {
                WindowTitle.Text = CurrentWindow.Title;
            }
            await Task.Delay(50);
            CheckFocus();
            CheckWindow();
        }
        catch
        {

        }
    }

    private void CurrentWindow_WindowStateChanged(object sender, WindowState e) => CheckMaximization();

    private void PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(CurrentWindow.Content).Properties.IsLeftButtonPressed != true)
        {
            CurrentCaption = SelectedCaptionButton.None;
        }
    }

    private void PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(CurrentWindow.Content).Properties.IsLeftButtonPressed != true)
        {
            CurrentCaption = SelectedCaptionButton.None;
        }
    }

    private void PointerExited(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            if (CurrentWindow != null)
            {
                if (CurrentWindow.Content != null)
                {
                    if (e.GetCurrentPoint(CurrentWindow.Content).Properties.IsLeftButtonPressed != true)
                    {
                        CurrentCaption = SelectedCaptionButton.None;
                    }
                }
            }
        }
        catch
        {

        }
    }

    public static bool IsInRect(double x, double xMin, double xMax, double y, double yMin, double yMax) => xMin <= x && x <= xMax && yMin <= y && y <= yMax;

    private async void Rehook()
    {
        MessageMonitor ??= new WindowMessageMonitor(CurrentWindow);
        MessageMonitor.WindowMessageReceived -= Event;
        MessageMonitor.WindowMessageReceived += Event;

        await Task.Delay(750);

        Rehook();
    }

    public void SetWindowIcon(string path)
    {
        TitleBarIcon.Source = new BitmapImage(new Uri($"ms-appx:///{path}"));
        CurrentWindow.SetIcon($"{AppContext.BaseDirectory}\\{path}");
    }

    public WindowEx GetWindow() => CurrentWindow;

    public static bool IsAccentColorEnabledForTitleBars()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Registry.AccentRegistryKeyPath);
            if (key != null)
            {
                var value = key.GetValue(Registry.AccentRegistryValueName);
                if (value is int intValue)
                {
                    // ColorPrevalence value of 1 means the accent color is used for title bars and window borders.
                    return intValue == 1;
                }
            }
        }
        catch
        {

        }

        return false; // Default to false if any issues occur
    }

    public void CheckFocus()
    {
        if (IsAccentColorEnabledForTitleBars() == true)
        {
            try
            {
                if (AccentStrip != null)
                {
                    AccentStrip.Visibility = Visibility.Visible;
                }
                ButtonBrush = IsWindowFocused == false ? new SolidColorBrush(Colors.White) : Application.Current.Resources["AccentTextFillColorDisabledBrush"] as SolidColorBrush;
                AccentStrip.Visibility = IsWindowFocused == false ? Visibility.Visible : Visibility.Collapsed;
            }
            catch { }
        }
        else
        {
            try
            {
                if (AccentStrip != null)
                {
                    AccentStrip.Visibility = Visibility.Collapsed;
                }
                ButtonBrush = IsWindowFocused == false ? Application.Current.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush : Application.Current.Resources["TextFillColorDisabledBrush"] as SolidColorBrush;
            }
            catch { }
        }
        UpdateBrush();
    }

    private async void Event(object sender, WindowMessageEventArgs e)
    {
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int WM_NCHITTEST = 0x0084;
        const int WM_NCLBUTTONUP = 0x00A2;
        const int WM_NCMOUSELEAVE = 0x02A2;
        const int WM_ACTIVATE = 0x0006;
        const int WA_INACTIVE = 0;

        // Gets the pointer's position relative to the screen's edge with DPI scaling applied
        var x = Win32.GET_X_LPARAM(e.Message.LParam);
        var y = Win32.GET_Y_LPARAM(e.Message.LParam);

        double minButtonLeftPos = 0;
        double maxButtonLeftPos = 0;
        double closeButtonLeftPos = 0;
        double buttonsMinY = 0;
        double buttonsMaxY = 0;

        double xMinimizeMin = 0;
        double xMinimizeMax = 0;

        double xMaximizeMin = 0;
        double xMaximizeMax = 0;

        double xCloseMin = 0;
        double xCloseMax = 0;

        double yMin = 0;
        double yMax = 0;

        try
        {
            if (Minimize != null && MaxRes != null && Close != null)
            {
                minButtonLeftPos = (Minimize.Width + MaxRes.Width + Close.Width) * Display.Scale(CurrentWindow);
                maxButtonLeftPos = (MaxRes.Width + Close.Width) * Display.Scale(CurrentWindow);
                closeButtonLeftPos = Close.Width * Display.Scale(CurrentWindow);
                buttonsMinY = (Close.Margin.Top * Display.Scale(CurrentWindow)) + 2;
                buttonsMaxY = (Close.Height + Close.Margin.Top) * Display.Scale(CurrentWindow);

                // Gets the X positions from: Window X + Window border + (Window size +/- button size)
                xMinimizeMin =
                    CurrentWindow.AppWindow.Position.X +
                    (7 * Display.Scale(CurrentWindow)) +
                    ((CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow)) - minButtonLeftPos);

                xMinimizeMax =
                    CurrentWindow.AppWindow.Position.X +
                    (7 * Display.Scale(CurrentWindow)) +
                    ((CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow)) - maxButtonLeftPos);

                xMaximizeMin =
                    CurrentWindow.AppWindow.Position.X +
                    (7 * Display.Scale(CurrentWindow)) +
                    ((CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow)) - maxButtonLeftPos);

                xMaximizeMax =
                    CurrentWindow.AppWindow.Position.X +
                    (7 * Display.Scale(CurrentWindow)) +
                    ((CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow)) - closeButtonLeftPos);

                xCloseMin =
                    CurrentWindow.AppWindow.Position.X +
                    (7 * Display.Scale(CurrentWindow)) +
                    ((CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow)) - closeButtonLeftPos);

                xCloseMax =
                    CurrentWindow.AppWindow.Position.X +
                    (7 * Display.Scale(CurrentWindow)) +
                    (CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow));

                // Gets the Y positions from: Window Y + Window border + (Window size +/- button size)
                yMin =
                    CurrentWindow.AppWindow.Position.Y +
                    (AdditionalHeight * Display.Scale(CurrentWindow)) +
                    buttonsMinY;

                yMax =
                    CurrentWindow.AppWindow.Position.Y +
                    (AdditionalHeight * Display.Scale(CurrentWindow)) +
                    buttonsMaxY;
            }
        }
        catch
        {
            return;
        }

        switch (e.Message.MessageId)
        {
            case WM_ACTIVATE:
                {
                    var wParam = e.Message.WParam.ToUInt32();
                    if (wParam == WA_INACTIVE)
                    {
                        // The window has lost focus
                        IsWindowFocused = true;
                        CheckFocus();
                    }
                    else
                    {
                        // The window has gained focus
                        IsWindowFocused = false;
                        CheckFocus();
                    }
                    break;
                }
            case WM_NCHITTEST:
                {
                    // Minimize Button
                    if (IsInRect(x, xMinimizeMin, xMinimizeMax, y, yMin, yMax))
                    {
                        e.Handled = true;
                        e.Result = new IntPtr(8);
                        _ = CurrentCaption == SelectedCaptionButton.Minimize
                            ? VisualStateManager.GoToState(Minimize, "Pressed", true)
                            : CurrentCaption == SelectedCaptionButton.None
                                ? VisualStateManager.GoToState(Minimize, "PointerOver", true)
                                : VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = VisualStateManager.GoToState(Close, "Normal", true);
                        await Task.Delay(1000);
                        e.Handled = false;
                    }

                    // Maximize Button
                    else if (IsInRect(x, xMaximizeMin, xMaximizeMax, y, yMin, yMax))
                    {
                        e.Handled = true;
                        e.Result = new IntPtr(9);
                        _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = CurrentCaption == SelectedCaptionButton.Maximize
                            ? VisualStateManager.GoToState(MaxRes, "Pressed", true)
                            : CurrentCaption == SelectedCaptionButton.None
                                ? VisualStateManager.GoToState(MaxRes, "PointerOver", true)
                                : VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = VisualStateManager.GoToState(Close, "Normal", true);
                        await Task.Delay(1000);
                        e.Handled = false;
                    }

                    // Close Button
                    else if (IsInRect(x, xCloseMin, xCloseMax, y, yMin, yMax))
                    {
                        e.Handled = true;
                        e.Result = new IntPtr(20);
                        _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = CurrentCaption == SelectedCaptionButton.Close
                            ? VisualStateManager.GoToState(Close, "Pressed", true)
                            : CurrentCaption == SelectedCaptionButton.None
                                ? VisualStateManager.GoToState(Close, "PointerOver", true)
                                : VisualStateManager.GoToState(Close, "Normal", true);
                        await Task.Delay(1000);
                        e.Handled = false;
                    }

                    // Title bar drag area
                    else
                    {
                        e.Handled = true;
                        e.Result = new IntPtr(1);
                        _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = VisualStateManager.GoToState(Close, "Normal", true);
                        e.Handled = false;
                    }

                    break;
                }
            case WM_NCLBUTTONDOWN:
                {
                    e.Handled = true;
                    e.Result = new IntPtr(1);
                    e.Handled = false;

                    // Minimize Button
                    if (IsInRect(x, xMinimizeMin, xMinimizeMax, y, yMin, yMax))
                    {
                        CurrentCaption = SelectedCaptionButton.Minimize;
                        _ = VisualStateManager.GoToState(Minimize, "Pressed", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = VisualStateManager.GoToState(Close, "Normal", true);
                    }

                    // Maximize Button
                    else if (IsInRect(x, xMaximizeMin, xMaximizeMax, y, yMin, yMax))
                    {
                        CurrentCaption = SelectedCaptionButton.Maximize;
                        _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Pressed", true);
                        _ = VisualStateManager.GoToState(Close, "Normal", true);
                    }

                    // Close Button
                    else if (IsInRect(x, xCloseMin, xCloseMax, y, yMin, yMax))
                    {
                        CurrentCaption = SelectedCaptionButton.Close;
                        _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = VisualStateManager.GoToState(Close, "Pressed", true);
                    }

                    // Title bar drag area
                    else
                    {
                        CurrentCaption = SelectedCaptionButton.None;
                        _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                        _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                        _ = VisualStateManager.GoToState(Close, "Normal", true);
                    }

                    break;
                }
            case WM_NCLBUTTONUP:
                {
                    e.Handled = true;
                    e.Result = new IntPtr(1);
                    e.Handled = false;

                    // Minimize Button
                    if (IsInRect(x, xMinimizeMin, xMinimizeMax, y, yMin, yMax))
                    {
                        if (CurrentCaption == SelectedCaptionButton.Minimize)
                        {
                            CurrentWindow.Minimize();
                            _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                            _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                            _ = VisualStateManager.GoToState(Close, "Normal", true);
                        }
                    }

                    // Maximize Button
                    else if (IsInRect(x, xMaximizeMin, xMaximizeMax, y, yMin, yMax))
                    {
                        if (CurrentCaption == SelectedCaptionButton.Maximize)
                        {
                            RunMaximization();
                        }
                    }

                    // Close Button
                    else if (IsInRect(x, xCloseMin, xCloseMax, y, yMin, yMax))
                    {
                        if (CurrentCaption == SelectedCaptionButton.Close)
                        {
                            CurrentWindow.Close();
                        }
                    }

                    // Title bar drag area
                    else
                    {

                    }

                    CurrentCaption = SelectedCaptionButton.None;

                    MessageMonitor.WindowMessageReceived -= Event;
                    MessageMonitor.Dispose();
                    break;
                }
            case WM_NCMOUSELEAVE:
                {
                    e.Handled = true;
                    e.Result = new IntPtr(1);
                    e.Handled = false;
                    _ = VisualStateManager.GoToState(Minimize, "Normal", true);
                    _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                    _ = VisualStateManager.GoToState(Close, "Normal", true);
                    break;
                }
            default:
                {
                    e.Handled = false;
                    break;
                }
        }
    }

    public void RunMaximization()
    {
        var state = (CurrentWindow.Presenter as OverlappedPresenter).State;
        if (state == OverlappedPresenterState.Restored)
        {
            CurrentWindow.Maximize();
            CheckMaximization();
            return;
        }
        else if (state == OverlappedPresenterState.Maximized)
        {
            CurrentWindow.Restore();
            CheckMaximization();
            return;
        }
    }

    public async void CheckMaximization()
    {
        if (CurrentWindow.Presenter is OverlappedPresenter)
        {
            var state = (CurrentWindow.Presenter as OverlappedPresenter).State;
            if (state == OverlappedPresenterState.Restored)
            {
                MaxResGlyph.Glyph = "";
                await Task.Delay(10);
                _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                AdditionalHeight = 0;
                return;
            }
            else if (state == OverlappedPresenterState.Maximized)
            {
                MaxResGlyph.Glyph = "";
                await Task.Delay(10);
                _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
                AdditionalHeight = 6;
                return;
            }
        }
        else
        {
            MaxResGlyph.Glyph = "";
            await Task.Delay(10);
            _ = VisualStateManager.GoToState(MaxRes, "Normal", true);
            return;
        }
    }

    public async void LoadBounds()
    {
        await Task.Delay(100);

        try
        {
            var appWindow = CurrentWindow.AppWindow;
            if (appWindow != null)
            {
                var titleBar = appWindow.TitleBar;

                var rect = new RectInt32(0, 0, (int)(CurrentWindow.Bounds.Width * Display.Scale(CurrentWindow)), (int)(31 * Display.Scale(CurrentWindow)));

                RectInt32[] rects = [rect];

                titleBar.SetDragRectangles(rects);
            }

            LoadBounds();
        }
        catch
        {
            return;
        }
    }

    private void UpdateBrush()
    {
        try
        {
            if (Close != null && MaxRes != null && Minimize != null && WindowTitle != null)
            {
                Close.Foreground = ButtonBrush;
                MaxRes.Foreground = ButtonBrush;
                Minimize.Foreground = ButtonBrush;
                WindowTitle.Foreground = ButtonBrush;
            }
        }
        catch
        {

        }
    }
}