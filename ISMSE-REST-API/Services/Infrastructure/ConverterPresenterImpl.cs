using ISMSE_REST_API.Contracts.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ISMSE_REST_API.Services.Infrastructure
{
    public class ConverterPresenterImpl : IConverterPresenter
    {
        private const string DATE_FORMAT = "yyyy-MM-dd";
        public DateTime GetDateTime(string src)
        {
            if (DateTime.TryParseExact(src, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date;
            throw new FormatException($"Ошибка в формате даты {src}. Ожидаемый формат: {DATE_FORMAT}, принятое занчение: {src}");
        }

        public int GetHours(string timeStr)
        {
            checkInputTime(timeStr);
            return int.Parse(timeStr.Split(':')[0]);
        }

        public int GetMinutes(string timeStr)
        {
            checkInputTime(timeStr);
            return int.Parse(timeStr.Split(':')[1]);
        }
        private void checkInputTime(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr))
                throw new ArgumentNullException($"timeStr (выражение времени) не передан");
            var timeRegex = new Regex("^(?:[01]?[0-9]|2[0-3]):[0-5][0-9]$");
            if (!timeRegex.IsMatch(timeStr))
                throw new FormatException($"timeStr указан в неверном формате: {timeStr}, ожидаемый формат: HH:mm");
        }
    }
}