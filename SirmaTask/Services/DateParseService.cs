using System.Globalization;

namespace SirmaTask.Services
{
    public interface IDateParseService
    {
        DateTime? ParseDate(string dateStr);
        DateTime ParseNonNullDate(string dateStr);
    }
    public class DateParseService : IDateParseService
    {
        private string[] dateFormats = {
            "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd",
            "dd-MM-yyyy", "yyyy/MM/dd", "dd MMM yyyy",
            "MMM dd, yyyy", "yyyyMMdd", "dd.MM.yyyy",
            "yyyy.MM.dd", "dddd, dd MMMM yyyy"
        };
        public DateTime? ParseDate(string dateStr)
        {
            if (string.Equals(dateStr, "NULL", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(dateStr))
            {
                return null;
            }
            DateTime parsedDate;

            bool success = DateTime.TryParseExact(
                        dateStr,
                        dateFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out parsedDate
                        );

            if(success)
                return parsedDate;
            throw new FormatException($"Invalid date format: {dateStr}");
        }
        public DateTime ParseNonNullDate(string dateStr)
        {
            DateTime parsedDate;

            bool success = DateTime.TryParseExact(
                        dateStr,
                        dateFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out parsedDate
                        );

            if (success)
                return parsedDate;
            throw new FormatException($"Invalid date format: {dateStr}");
        }
    }
}
