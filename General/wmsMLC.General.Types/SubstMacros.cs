#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="SubstMacros.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Alexander S. Shurakov</Author>
/// <Date>17.10.2012 9:51:07</Date>
/// <Summary>Класс для подстановки макросов в текстовые строки и не только </Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587


using System;
using System.Collections.Generic;
using System.Text;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Класс для подстановки макросов в строки String и StringBuilder
    /// </summary>
    [Serializable]
    public class SubstMacros : ISubstMacros
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private SortedList<string, string> _macroses; ///<remarks>Список макросов</remarks>
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        ///<summary>Свойство для доступа к макросам</summary>
        public SortedList<string, string> Macroses
        {
            get
            {
                return _macroses;
            }
            set
            {
                // Ибо нефиг. Пользуйтесь методом SetMacro()
                // _macroses = value;
            }
        }

        public const string Revision = "$Rev: 15 $"; ///<remarks>Версия в системе SVN</remarks>

        static readonly object ThreadLock = new object(); ///<remarks>Блокировка - чтобы сделать метод ThreadSafe</remarks>

        const int NewListCapacity = 8; ///<remarks>Размер списка макросов при создании</remarks>

        /// <summary>
        /// Конструктор
        /// </summary>
        public SubstMacros()
        {
            _macroses = new SortedList<string, string>(NewListCapacity);
            _macroses["LISTALLMACROSES"] = "";
        }

        /// <summary>
        /// Подставляет макросы в строку
        /// </summary>
        /// <param name="inputString">исходная строка</param>
        /// <returns>строка после подстановок</returns>
        public string Substitute(string inputString)
        {
            string result = string.Copy(inputString);
            result = SubstGlobalMacros.Substitute(result);
            result = SubstGlobalMacros.Substitute(_macroses, result);
            return result;
        }

        /// <summary>
        /// Подставляет макросы в строку StringBuilder
        /// </summary>
        /// <param name="inputSb">исходная строка</param>
        /// <returns>строка после подстановки</returns>
        public StringBuilder Substitute(StringBuilder inputSb)
        {
            const int reserve = 64;
            var result = new StringBuilder(inputSb.Length + reserve);
            result = result.Append(inputSb);
            result = SubstGlobalMacros.Substitute(result);
            result = SubstGlobalMacros.Substitute(_macroses, result);
            return result;
        }

        /// <summary>
        /// Добавляет новый макрос
        /// </summary>
        /// <param name="name">Имя макроса</param>
        /// <param name="value">Значение макроса</param>
        public void SetMacro(string name, string value)
        {
            if (name != null && value != null)
            {
                string myName = name.ToUpper();
                lock (ThreadLock)
                {
                    if (_macroses.ContainsKey(myName)) { _macroses[myName] = value; }
                    else { _macroses.Add(myName, value); }
                }
            }
        }


        /// <summary>
        /// Добавляет новый макрос (объект)
        /// </summary>
        /// <param name="name">Имя макроса</param>
        /// <param name="value">Значение макроса</param>
        public void SetMacro(string name, object value)
        {
            if (name != null && value != null)
            {
                string myValue = value.ToString();
                string myName = name.ToUpper();
                lock (ThreadLock)
                {
                    if (_macroses.ContainsKey(myName)) { _macroses[myName] = myValue; }
                    else { _macroses.Add(myName, myValue); }
                }
            }
        }

        /// <summary>
        /// Удаляет названный макрос
        /// </summary>
        /// <param name="name">Имя макроса</param>
        public void UnsetMacro(string name)
        {
            if (name != null)
            {
                string myName = name.ToUpper();
                if (_macroses.ContainsKey(myName))
                {
                    lock (ThreadLock) { _macroses.Remove(name); }
                }
            }
        }
    }
}
