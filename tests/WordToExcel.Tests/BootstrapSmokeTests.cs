namespace WordToExcel.Tests;

public sealed class BootstrapSmokeTests
{
    [Fact]
    public void AppAssembly_IsLoadable()
    {
        Assert.Equal("WordToExcel.App", typeof(WordToExcel.App.MainWindow).Assembly.GetName().Name);
    }
}
