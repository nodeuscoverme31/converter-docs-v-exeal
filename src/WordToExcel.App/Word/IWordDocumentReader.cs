using WordToExcel.App.Model;

namespace WordToExcel.App.Word;

internal interface IWordDocumentReader
{
    DocumentModel Read(string docxPath);
}
