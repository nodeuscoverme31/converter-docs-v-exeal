using System.Globalization;
using WordToExcel.App.Model;

namespace WordToExcel.App.Conversion;

internal sealed class ValuePolicy
{
    private const int ExcelSignificantDigitLimit = 15;
    private const string IsoDateFormat = "yyyy-MM-dd";

    public OutputValuePlan Plan(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);

        if (MustRemainText(sourceText))
        {
            return Text(sourceText);
        }

        if (DateTime.TryParseExact(
                sourceText,
                IsoDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date) &&
            date.ToString(IsoDateFormat, CultureInfo.InvariantCulture) == sourceText)
        {
            return new OutputValuePlan(
                ValueMode.SafeDate,
                sourceText,
                DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified),
                "yyyy-mm-dd");
        }

        if (IsSafePositiveInteger(sourceText) &&
            long.TryParse(sourceText, NumberStyles.None, CultureInfo.InvariantCulture, out var integer))
        {
            return new OutputValuePlan(ValueMode.SafeNumber, sourceText, integer);
        }

        return Text(sourceText);
    }

    private static bool MustRemainText(string sourceText)
    {
        if (sourceText.Length == 0)
        {
            return true;
        }

        if (sourceText[0] is '=' or '+' or '-' or '@')
        {
            return true;
        }

        return !string.Equals(sourceText, sourceText.Trim(), StringComparison.Ordinal);
    }

    private static bool IsSafePositiveInteger(string sourceText)
    {
        if (sourceText.Length == 0 || sourceText.Length > ExcelSignificantDigitLimit)
        {
            return false;
        }

        if (sourceText.Length > 1 && sourceText[0] == '0')
        {
            return false;
        }

        return sourceText.All(char.IsAsciiDigit);
    }

    private static OutputValuePlan Text(string sourceText) =>
        new(ValueMode.Text, sourceText, sourceText);
}
