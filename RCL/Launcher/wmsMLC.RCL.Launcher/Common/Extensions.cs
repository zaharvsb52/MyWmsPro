using System;

namespace wmsMLC.RCL.Launcher.Common
{
    public static class Extensions
    {
        public static bool IsNullOrEmptyAfterTrim(this string source)
        {
            return (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(source.Trim()));
        }

        public static string GetTrim(this string source)
        {
            return (string.IsNullOrEmpty(source) ? string.Empty : source.Trim());
        }

        public static bool EqIgnoreCase(this string a, string b, bool usetrim)
        {
            const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;
            if (usetrim)
                return string.Equals(a.GetTrim(), b.GetTrim(), stringComparison);
            return string.Equals(a, b, stringComparison);
        }

        public static bool EqIgnoreCase(this string a, string b)
        {
            return EqIgnoreCase(a, b, false);
        }

        /// <summary>
        /// Возвращает текст заданной длины.
        /// </summary>
        public static string Left(this string text, int lenght, bool istrim)
        {
            if (lenght <= 0) throw new ArgumentException("Argument 'lenght' should be more than zero.");
            if (string.IsNullOrEmpty(text)) return text;
            var result = (istrim ? text.Trim() : text);
            return (result.Length > lenght ? result.Substring(0, lenght) : result);
        }
    }
}
