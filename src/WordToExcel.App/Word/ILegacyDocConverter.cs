namespace WordToExcel.App.Word;

internal interface ILegacyDocConverter
{
    string ConvertToDocx(string sourceDocPath, string temporaryDirectory);
}
