using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WordToExcel.App;

public partial class MainWindow
{
    private bool uiEnhancementsInstalled;

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (uiEnhancementsInstalled)
        {
            return;
        }

        uiEnhancementsInstalled = true;
        AllowDrop = true;
        PreviewDragOver += WindowPreviewDragOver;
        PreviewDragLeave += WindowPreviewDragLeave;
        PreviewDrop += WindowPreviewDrop;
        InstallClearSelectedFileButton();
    }

    private void WindowPreviewDragOver(object sender, DragEventArgs e)
    {
        if (!DropZoneBorder.AllowDrop)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var acceptable = TryGetWindowDroppedWordFile(e.Data, out _);
        e.Effects = acceptable ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;

        DropZoneBorder.BorderBrush = acceptable
            ? (Brush)FindResource("AppAccentBrush")
            : (Brush)FindResource("AppBorderBrush");
    }

    private void WindowPreviewDragLeave(object sender, DragEventArgs e)
    {
        ResetDropZoneBorder();
    }

    private void WindowPreviewDrop(object sender, DragEventArgs e)
    {
        ResetDropZoneBorder();

        if (DropZoneBorder.AllowDrop && TryGetWindowDroppedWordFile(e.Data, out var path))
        {
            SelectFile(path!);
        }

        e.Handled = true;
    }

    private void InstallClearSelectedFileButton()
    {
        if (SelectedFileCard.Child is not Grid grid || grid.ColumnDefinitions.Count >= 3)
        {
            return;
        }

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var button = new Button
        {
            Content = "×",
            Width = 28,
            Height = 28,
            Margin = new Thickness(12, 0, 0, 0),
            Padding = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = (Brush)FindResource("AppMutedTextBrush"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Cursor = Cursors.Hand,
            ToolTip = "Убрать файл из выбора",
        };

        AutomationProperties.SetName(button, "Убрать выбранный файл");
        button.Click += ClearSelectedFileButton_Click;
        Grid.SetColumn(button, 2);
        grid.Children.Add(button);
    }

    private void ClearSelectedFileButton_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();
        ChooseFileButton.Focus();
    }

    private static bool TryGetWindowDroppedWordFile(IDataObject data, out string? path)
    {
        path = null;
        if (!data.GetDataPresent(DataFormats.FileDrop) || data.GetData(DataFormats.FileDrop) is not string[] files)
        {
            return false;
        }

        return FileDropPolicy.TryGetSingleSupportedWordFile(files, out path);
    }
}
