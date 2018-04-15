#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FileStruct.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>10.10.2012 11:28:52</Date>
/// <Summary>Структура файла</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;

namespace wmsMLC.General.Types
{
    public class FileStruct
    {
        /// <summary>
        ///     Признак того, что файл запакован
        /// </summary>
        private readonly bool _zip;

        /// <summary>
        ///     Конструктор класса
        /// </summary>
        /// <param name="data">массив данных</param>
        /// <param name="name">имя файла</param>
        /// <param name="zip">архивный файл</param>
        public FileStruct(byte[] data, string name, bool zip) : this(data, name, null, zip)
        {
        }

        /// <summary>
        ///     Конструктор класса
        /// </summary>
        /// <param name="data">массив данных</param>
        /// <param name="name">имя файла</param>
        /// <param name="ext">расширение файла без точки!!!</param>
        /// <param name="zip">архивный файл</param>
        public FileStruct(byte[] data, string name, string ext, bool zip)
        {
            Data = data;
            Name = Path.GetFileName(name);
            Ext = string.IsNullOrEmpty(ext) ? Path.GetExtension(name) : ext;
            _zip = zip;
            FilePath = Path.GetDirectoryName(name);

            if (!string.IsNullOrEmpty(Ext))
            {
                if (!string.IsNullOrEmpty(Name))
                    Name = Name.Replace(Ext, string.Empty);
                Ext = Ext.TrimStart('.');
            }
        }

        #region .  Properties  .

        public byte[] Data { get; private set; }

        public string Name { get; private set; }

        public string Ext { get; private set; }

        /// <summary>
        ///     Полное имя файла (с расширением)
        /// </summary>
        public string FileName
        {
            get { return GetFileName(); }
        }

        public string FilePath { get; private set; }

        public string FullFileName
        {
            get { return Path.Combine(FilePath, FileName); }
        }

        #endregion

        private string GetFileName()
        {
            string result;
            if (!_zip)
            {
                result = Name + ((Ext.Length > 0) ? "." : "") + Ext;
            }
            else
            {
                result = Name + ((Ext.Length > 0) ? "_" : "") + Ext + ".zip";
            }
            return result;
        }
    }
}