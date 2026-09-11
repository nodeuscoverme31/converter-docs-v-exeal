using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using WordToExcel.App.Conversion;
using WordToExcel.App.Model;

namespace WordToExcel.App;

public partial class MainWindow : Window
{
    private readonly ConversionOrchestrator orchestrator;
    private string? selectedFilePath;
    private string? outputFilePath;

    internal MainWindow(ConversionOrchestrator orchestrator)
    {
        this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        InitializeComponent();
        ResetUi();
    }

    private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите документ Word",
            Filter = "Документы Word (*.doc;*.docx)|*.doc;*.docx",
            CheckFileExists = true,
            Multiselect = false,
        };

        if (dialog.ShowDialog(this) == true)
        {
            SelectFile(dialog.FileName);
        }
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        var acceptable = TryGetDroppedWordFile(e.Data, out _);
        e.Effects = acceptable ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;

        DropZoneBorder.BorderBrush = acceptable
            ? (Brush)FindResource("AppAccentBrush")
            : (Brush)FindResource("AppBorderBrush");
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        ResetDropZoneBorder();
    }

    private void DropZone_Drop(object sender, DragEventArgs e)
    {
        ResetDropZoneBorder();
        if (TryGetDroppedWordFile(e.Data, out var path))
        {
            SelectFile(path!);
        }

        e.Handled = true;
    }

    private async void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        await ConvertSelectedAsync(destinationDirectory: null);
    }

    private async void AlternateFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedFilePath is null)
        {
            return;
        }

        var dialog = new OpenFolderDialog
        {
            Title = "Выберите папку для Excel-файла",
            Multiselect = false,
        };

        if (dialog.ShowDialog(this) == true)
        {
            await ConvertSelectedAsync(dialog.FolderName);
        }
    }

    private async Task ConvertSelectedAsync(string? destinationDirectory)
    {
        var sourcePath = selectedFilePath;
        if (sourcePath is null)
        {
            return;
        }

        SetProcessing(true);
        ConversionResult result;

        try
        {
            result = await Task.Run(() => orchestrator.Convert(sourcePath, destinationDirectory));
        }
        catch (Exception)
        {
            SetProcessing(false);
            ShowUnexpectedFailure();
            return;
        }

        SetProcessing(false);
        ShowResult(result);
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            return;
        }

        try
        {
            var canonicalOutput = Path.GetFullPath(outputFilePath);
            if (File.Exists(canonicalOutput))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{canonicalOutput}\"",
                    UseShellExecute = true,
                });
                return;
            }

            var directory = Path.GetDirectoryName(canonicalOutput);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true,
                });
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception)
        {
            ResultMessageText.Text = "Файл создан, но папку не удалось открыть автоматически.";
        }
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();
        ChooseFileButton.Focus();
    }

    private void SelectFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !IsSupportedWordExtension(path))
        {
            return;
        }

        var canonicalPath = Path.GetFullPath(path);
        selectedFilePath = canonicalPath;
        outputFilePath = null;

        SelectedFileNameText.Text = Path.GetFileName(canonicalPath);
        SelectedFilePathText.Text = canonicalPath;
        SelectedFileCard.Visibility = Visibility.Visible;
        ConvertButton.IsEnabled = true;
        ResultCard.Visibility = Visibility.Collapsed;
        WarningDetailsPanel.Visibility = Visibility.Collapsed;
        ResultPathText.Text = string.Empty;
        OpenFolderButton.Visibility = Visibility.Visible;
        AlternateFolderButton.Visibility = Visibility.Collapsed;
        DropTitleText.Text = "Файл выбран — можно преобразовывать";
    }

    private void SetProcessing(bool isProcessing)
    {
        ProcessingPanel.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
        ChooseFileButton.IsEnabled = !isProcessing;
        ConvertButton.IsEnabled = !isProcessing && selectedFilePath is not null;
        DropZoneBorder.AllowDrop = !isProcessing;
        ResetButton.IsEnabled = !isProcessing;
        OpenFolderButton.IsEnabled = !isProcessing;
        AlternateFolderButton.IsEnabled = !isProcessing;

        if (isProcessing)
        {
            ResultCard.Visibility = Visibility.Collapsed;
            DropTitleText.Text = "Преобразование выполняется";
        }
        else if (selectedFilePath is not null)
        {
            DropTitleText.Text = "Файл выбран — можно преобразовывать";
        }
    }

    private void ShowResult(ConversionResult result)
    {
        outputFilePath = result.OutputPath;
        ResultCard.Visibility = Visibility.Visible;
        ResultMessageText.Text = result.UserMessage;

        var hasOutput = !string.IsNullOrWhiteSpace(result.OutputPath);
        ResultPathText.Visibility = hasOutput ? Visibility.Visible : Visibility.Collapsed;
        ResultPathText.Text = result.OutputPath ?? string.Empty;
        OpenFolderButton.Visibility = hasOutput ? Visibility.Visible : Visibility.Collapsed;
        AlternateFolderButton.Visibility = result.Status == ConversionStatus.Error && result.ErrorCategory == ConversionErrorCategory.OutputWrite
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (result.Status == ConversionStatus.Success)
        {
            ResultIcon.Visibility = Visibility.Visible;
            ResultIcon.Data = (Geometry)FindResource("SuccessIconGeometry");
            ResultIcon.Stroke = (Brush)FindResource("AppSuccessBrush");
            ResultTitleText.Text = "Готово";
            WarningDetailsPanel.Visibility = Visibility.Collapsed;
            WarningItemsControl.ItemsSource = null;
            return;
        }

        if (result.Status == ConversionStatus.Warning)
        {
            ResultIcon.Visibility = Visibility.Visible;
            ResultIcon.Data = (Geometry)FindResource("WarningIconGeometry");
            ResultIcon.Stroke = (Brush)FindResource("AppWarningBrush");
            ResultTitleText.Text = "Готово с предупреждениями";
            WarningItemsControl.ItemsSource = result.EffectiveWarnings
                .Select(UserFacingWarning)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            WarningDetailsPanel.Visibility = Visibility.Visible;
            return;
        }

        ResultIcon.Visibility = Visibility.Collapsed;
        ResultTitleText.Text = result.ErrorCategory == ConversionErrorCategory.OutputWrite
            ? "Не удалось сохранить Excel-файл"
            : "Не удалось преобразовать";
        WarningDetailsPanel.Visibility = Visibility.Collapsed;
        WarningItemsControl.ItemsSource = null;
    }

    private void ShowUnexpectedFailure()
    {
        outputFilePath = null;
        ResultCard.Visibility = Visibility.Visible;
        ResultIcon.Visibility = Visibility.Collapsed;
        ResultTitleText.Text = "Не удалось преобразовать";
        ResultMessageText.Text = "Во время преобразования произошла непредвиденная ошибка. Исходный документ не изменён.";
        ResultPathText.Visibility = Visibility.Collapsed;
        WarningDetailsPanel.Visibility = Visibility.Collapsed;
        WarningItemsControl.ItemsSource = null;
        OpenFolderButton.Visibility = Visibility.Collapsed;
        AlternateFolderButton.Visibility = Visibility.Collapsed;
    }

    private void ResetUi()
    {
        selectedFilePath = null;
        outputFilePath = null;
        SelectedFileNameText.Text = string.Empty;
        SelectedFilePathText.Text = string.Empty;
        ResultMessageText.Text = string.Empty;
        ResultPathText.Text = string.Empty;
        WarningItemsControl.ItemsSource = null;
        SelectedFileCard.Visibility = Visibility.Collapsed;
        ProcessingPanel.Visibility = Visibility.Collapsed;
        ResultCard.Visibility = Visibility.Collapsed;
        WarningDetailsPanel.Visibility = Visibility.Collapsed;
        ResultPathText.Visibility = Visibility.Visible;
        ResultIcon.Visibility = Visibility.Visible;
        ResultIcon.Data = (Geometry)FindResource("SuccessIconGeometry");
        ResultIcon.Stroke = (Brush)FindResource("AppSuccessBrush");
        OpenFolderButton.Visibility = Visibility.Visible;
        AlternateFolderButton.Visibility = Visibility.Collapsed;
        ChooseFileButton.IsEnabled = true;
        ConvertButton.IsEnabled = false;
        DropZoneBorder.AllowDrop = true;
        DropTitleText.Text = "Перетащите сюда файл .doc или .docx";
        ResetDropZoneBorder();
    }

    private void ResetDropZoneBorder()
    {
        DropZoneBorder.BorderBrush = (Brush)FindResource("AppBorderBrush");
    }

    private static string UserFacingWarning(ConversionFinding finding) => finding.Code switch
    {
        "UNSUPPORTED_OBJECT" => "Изображение или встроенный объект не перенесён в Excel. Таблицы и обычный текст сохранены.",
        "TABLE_VERTICAL_MERGE_AMBIGUOUS" => "В одной из таблиц неоднозначное объединение ячеек. Данные сохранены без догадок — проверьте этот участок.",
        "NO_TABLES" => "Таблицы не найдены. Обычный текст сохранён на листе «Контекст».",
        _ => "В исходном документе есть особенность, которую стоит проверить в созданном Excel-файле.",
    };

    private static bool TryGetDroppedWordFile(IDataObject data, out string? path)
    {
        path = null;
        if (!data.GetDataPresent(DataFormats.FileDrop) || data.GetData(DataFormats.FileDrop) is not string[] files || files.Length != 1)
        {
            return false;
        }

        if (!IsSupportedWordExtension(files[0]))
        {
            return false;
        }

        path = files[0];
        return true;
    }

    private static bool IsSupportedWordExtension(string path)
    {
        var extension = Path.GetExtension(path);
        return string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extension, ".doc", StringComparison.OrdinalIgnoreCase);
    }
}
