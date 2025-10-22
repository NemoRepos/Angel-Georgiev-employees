using System.Globalization;

namespace SirmaTask.Services
{
    public interface IDateParseService
    {
        bool ParseDate(string dateStr,out DateTime? date);
    }
    public class DateParseService : IDateParseService
    {
        private string[] dateFormats = {
            "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd",
            "dd-MM-yyyy", "yyyy/MM/dd", "dd MMM yyyy",
            "MMM dd, yyyy", "yyyyMMdd", "dd.MM.yyyy",
            "yyyy.MM.dd", "dddd, dd MMMM yyyy"
        };
        public bool ParseDate(string dateStr,out DateTime? date)
        {
            date = null;
            if (string.Equals(dateStr, "NULL", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(dateStr))
            {
                return false;
            }

            if(DateTime.TryParseExact(
                        dateStr,
                        dateFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime parsedDate
                        ))
            {
                date = parsedDate;
                return true;
            }
            return false;
        }
    }
}
