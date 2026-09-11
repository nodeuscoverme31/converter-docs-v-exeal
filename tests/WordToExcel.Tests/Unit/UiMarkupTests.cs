using Xunit;

namespace WordToExcel.Tests.Unit;

public sealed class UiMarkupTests
{
    [Fact]
    public void MainWindow_DeclaresClearSelectedFileButtonInXaml()
    {
        var repositoryRoot = FindRepositoryRoot();
        var xamlPath = Path.Combine(repositoryRoot, "src", "WordToExcel.App", "MainWindow.xaml");
        var xaml = File.ReadAllText(xamlPath);

        Assert.Contains("x:Name=\"ClearSelectedFileButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"ClearSelectedFileButton_Click\"", xaml, StringComparison.Ordinal);
        Assert.Contains("automation:AutomationProperties.Name=\"Убрать выбранный файл\"", xaml, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WordToExcel.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing WordToExcel.sln.");
    }
}
