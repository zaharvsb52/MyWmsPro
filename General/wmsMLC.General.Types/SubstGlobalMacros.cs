using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wmsMLC.General.Types
{
    /// <summary>
    ///     Класс для подстановки глобальных макросов в строки; содержит также методы, используемые классом SubstMacros
    /// </summary>
    public static class SubstGlobalMacros
    {
        // ReSharper disable InconsistentNaming

        /*
                static object _threadLock = new object(); */

        /// <remarks>Блокировка для доступа к списку макросов</remarks>
        public const string Revision = "$Rev: 16 $";

        /// <remarks>Версия в системе SVN</remarks>
        private const int NewListCapacity = 16;

        private const string MacroBegin = "${"; // Начало макроса
        private const string MacroEnd = "}"; // Конец макроса
        private static SortedList<string, string> _macroses;

        /// <remarks>Размер списка макросов при создании</remarks>
        //
        private static readonly string[] Predefined =
            {
                "LISTGLOBALMACROSES", "DATE", "LONGDATE", "TIME", "LONGTIME",
                "PATHDATE", "PATHTIME", "PATHLONGTIME", "PATHDATETIME", "SYSUSERNAME", "SYSDOMAINNAME",
                "SYSCURRENTDIRECTORY", "SYSMACHINENAME", "NEWLINE", "GUID"
            };

        /// <summary>
        ///     Конструктор
        /// </summary>
        static SubstGlobalMacros()
        {
            _macroses = new SortedList<string, string>(NewListCapacity);
            foreach (string t in Predefined)
            {
                _macroses.Add(t, "");
            }
            string vsp = Environment.UserName;
            _macroses["SYSUSERNAME"] = vsp;
            vsp = Environment.UserDomainName;
            _macroses["SYSDOMAINNAME"] = vsp;
            vsp = Environment.MachineName;
            _macroses["SYSMACHINENAME"] = vsp;
            vsp = Environment.NewLine;
            _macroses["NEWLINE"] = vsp;
        }

        /// <summary>Свойство для доступа к макросам. Только для чтения!</summary>
        public static SortedList<string, string> Macroses
        {
            get { return _macroses; }
            set
            {
                _macroses = value;
                // _macroses = value;
            }
        }

        /// <summary>
        ///     Возвращает значение для названного макроса
        /// </summary>
        /// <param name="name">Имя макроса</param>
        /// <returns>Значение макроса</returns>
        private static string DecodeString(string name)
        {
            string _return;
            string myName = name.ToUpper();
            DateTime dateTime = DateTime.Now;
            switch (myName)
            {
                case "DATE":
                    _return = dateTime.ToShortDateString();
                    break;
                case "LONGDATE":
                    _return = dateTime.ToLongDateString();
                    break;
                case "TIME":
                    _return = dateTime.ToShortTimeString();
                    break;
                case "LONGTIME":
                    _return = dateTime.ToLongTimeString();
                    break;
                case "PATHDATE":
                    _return = dateTime.ToShortDateString();
                    break;
                case "PATHTIME":
                    _return = dateTime.ToShortTimeString().Replace(":", ".");
                    break;
                case "PATHLONGTIME":
                    _return = dateTime.ToLongTimeString().Replace(":", ".");
                    break;
                case "PATHDATETIME":
                    _return = dateTime.ToShortDateString() + "-" + dateTime.ToShortTimeString().Replace(":", ".");
                    break;
                case "SYSCURRENTDIRECTORY":
                    _return = Environment.CurrentDirectory;
                    break;
                case "LISTGLOBALMACROSES":
                    _return = "Global macroses:" + _macroses["NEWLINE"];
                    _return =
                        _macroses.Keys.Select(
                            vspStr =>
                            string.Format("Global Macro[{0}]='{1}';", vspStr, _macroses[vspStr]) + _macroses["NEWLINE"])
                                 .Aggregate(_return, (current, vspS2) => current + vspS2);
                    break;
                case "GUID":
                    // В формате 32 цифры
                    _return = Guid.NewGuid().ToString("N");
                    break;
                default:
                    _return = _macroses.ContainsKey(name) ? _macroses[name] : "";
                    break;
            }
            return _return;
        }

        /// <summary>
        ///     Возвращает значение для макроса, используя заданный словарь
        /// </summary>
        /// <param name="glossary">Словарь</param>
        /// <param name="name">Имя макроса</param>
        /// <returns>Значение макроса</returns>
        private static string DecodeString(SortedList<string, string> glossary, string name)
        {
            string _return;
            string myName = name.ToUpper();
            switch (myName)
            {
                case "DATE":
                    _return = DecodeString(myName);
                    break;
                case "LONGDATE":
                    _return = DecodeString(myName);
                    break;
                case "TIME":
                    _return = DecodeString(myName);
                    break;
                case "LONGTIME":
                    _return = DecodeString(myName);
                    break;
                case "PATHDATE":
                    _return = DecodeString(myName);
                    break;
                case "PATHTIME":
                    _return = DecodeString(myName);
                    break;
                case "PATHDATETIME":
                    _return = DecodeString(myName);
                    break;
                case "SYSCURRENTDIRECTORY":
                    _return = DecodeString(myName);
                    break;
                case "LISTALLMACROSES":
                    _return = "Global macroses:" + _macroses["NEWLINE"];
                    string vspS2;
                    foreach (string vspStr in _macroses.Keys)
                    {
                        vspS2 = string.Format("Global Macro[{0}]='{1}';", vspStr, _macroses[vspStr]) +
                                _macroses["NEWLINE"];
                        _return += vspS2;
                    }
                    _return += "Local macroses:" + _macroses["NEWLINE"];
                    foreach (string vspStr in glossary.Keys)
                    {
                        vspS2 = string.Format("Macro[{0}]='{1}';", vspStr, glossary[vspStr]) + _macroses["NEWLINE"];
                        _return += vspS2;
                    }
                    break;
                case "GUID":
                    _return = DecodeString(myName);
                    break;
                default:
                    _return = glossary.ContainsKey(name) ? glossary[name] : "";
                    break;
            }
            return _return;
        }

        // static string[] _stringSeparator = { "${", "}" };
        /*
                const int Reserve = 64;
        */

        /// <summary>
        ///     Подставляет макросы в строку
        /// </summary>
        /// <param name="inputString">исходная строка</param>
        /// <returns>строка после подстановок</returns>
        public static string Substitute(string inputString)
        {
            string result = string.Copy(inputString);
            foreach (string key in _macroses.Keys)
            {
                string substr = MacroBegin + key + MacroEnd;
                if (result.Contains(substr))
                {
                    string newstr = DecodeString(key);
                    result = result.Replace(substr, newstr);
                }
            }
            return result;
        }

        /// <summary>
        ///     Подставляет макросы в строку, используя указанный словарь
        /// </summary>
        /// <param name="glossary">Словарь</param>
        /// <param name="inputString">Исходная строка</param>
        /// <returns>Строка после подстановки</returns>
        public static string Substitute(SortedList<string, string> glossary, string inputString)
        {
            string result = string.Copy(inputString);
            foreach (string key in glossary.Keys)
            {
                string substr = MacroBegin + key + MacroEnd;
                if (result.Contains(substr))
                {
                    string newstr = DecodeString(glossary, key);
                    result = result.Replace(substr, newstr);
                }
            }
            return result;
        }

        /// <summary>
        ///     Подставляет макросы в строку StringBuilder
        /// </summary>
        /// <param name="inputSb">исходная строка</param>
        /// <returns>строка после подстановки</returns>
        public static StringBuilder Substitute(StringBuilder inputSb)
        {
            const int reserve = 64;
            var result = new StringBuilder(inputSb.Length + reserve);
            result = result.Append(inputSb);
            string testString = result.ToString();
            foreach (string key in _macroses.Keys)
            {
                string substr = MacroBegin + key + MacroEnd;
                if (testString.Contains(substr))
                {
                    string newstr = DecodeString(key);
                    result = result.Replace(substr, newstr);
                }
            }
            return result;
        }

        /// <summary>
        ///     Подставляет макросы в строку StringBuilder, используя указанный словарь
        /// </summary>
        /// <param name="glossary">Словарь</param>
        /// <param name="inputSb">исходная строка</param>
        /// <returns>Строка после подстановки</returns>
        public static StringBuilder Substitute(SortedList<string, string> glossary, StringBuilder inputSb)
        {
            const int reserve = 64;
            var result = new StringBuilder(inputSb.Length + reserve);
            result = result.Append(inputSb);
            string testString = result.ToString();
            foreach (string key in glossary.Keys)
            {
                string substr = MacroBegin + key + MacroEnd;
                if (testString.Contains(substr))
                {
                    string newstr = DecodeString(glossary, key);
                    result = result.Replace(substr, newstr);
                }
            }
            return result;
        }

        /// <summary>
        ///     Для глобальных макросов - только для чтения
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetMacro(string name, object value)
        {
            // if (name != null)
            // {
            //  lock (_threadLock)
            //  {
            //   if (_macroses.ContainsKey(name)) { _macroses[name] = value; }
            //   else { _macroses.Add(name, value); }
            //  }
            //  }
        }


        /// <summary>
        ///     Для глобальных макросов - только для чтения
        /// </summary>
        /// <param name="name"></param>
        public static void UnsetMacro(string name)
        {
            // if (name != null) { lock (_threadLock) { _macroses.Remove(name); } }
        }
    }
}