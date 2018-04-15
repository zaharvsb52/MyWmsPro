#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FilePumper.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>10.09.2012 12:43:23</Date>
/// <Summary>Файлонасосная. Считывает файлы из указанной директории, читает и перемещает по указанному пути
/// Методы, заканчивающиеся на _WF, предназначены для wmsWorkFlowEngine 
/// Исключительные ситуации контролируются wmsWorkFlowEngine
/// </Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using wmsMLC.General.Services;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Объект файлонасосной
    /// </summary>
    public class FilePumper : BaseObject
    {
        ///// <summary>
        ///// Событие записи сообщения в лог
        ///// </summary>
        //public event EventErrorHandler OnEventErrorHandler;
        /// <summary>
        /// папка источник
        /// </summary>
        private string _sourcePath = "";

        /// <summary>
        /// папка назначения
        /// </summary>
        private string _destinationPath = "";

        /// <summary>
        /// маска поиска
        /// </summary>
        private readonly List<string> _searchPatterns = new List<string>();

        /// <summary>
        /// удаление источника
        /// true - удаление, false - без изменений
        /// </summary>
        private bool _move;

        /// <summary>
        /// список файлов
        /// </summary>
        private List<string> _fileList = new List<string>();

        /// <summary>
        /// Результат закачки файлов
        /// </summary>
        private bool _state = true;
        public bool PumperState
        {
            get { return _state; }
        }

        public FilePumper(string linkById)
        {
            LinkById = linkById;
        }

        public FilePumper(string hostName, string userName, string linkById)
        {
            LinkById = linkById;
            HostName = hostName;
            UserName = userName;
        }

        /// <summary>
        /// Инициализация путей
        /// </summary>
        /// <param name="sourcePath">string</param>
        /// <param name="destinationPath">string</param>
        public void Initialize_WF(object sourcePath, object destinationPath)
        {
            Initialize(sourcePath.ToString(), destinationPath.ToString(), new string[] { "*.*" });
        }

        /// <summary>
        /// Инициализация путей
        /// </summary>
        /// <param name="sourcePath">string</param>
        /// <param name="destinationPath">string</param>
        /// <param name="searchPatterns">string</param>
        public void Initialize(string sourcePath, string destinationPath, string[] searchPatterns)
        {            
            _fileList.Clear();
            SetSourcePath(sourcePath);
            SetDestinationPath(destinationPath);
            SetSearchPatterns(searchPatterns);           
        }

        /// <summary>
        /// Формирование списка файлов
        /// </summary>
        private void Update()
        {
            try
            {
                if (_state)
                {
                    if (_searchPatterns.Count < 1) // use default value
                    {
                        _searchPatterns.Add("*.*");
                    }
                    _fileList.Clear();
                    if (!string.IsNullOrEmpty(_sourcePath))
                    {
// ReSharper disable PossiblyMistakenUseOfParamsMethod
                        _fileList = _searchPatterns.SelectMany(i => _sourcePath != null ? Directory.GetFiles(Path.Combine(_sourcePath), i, SearchOption.TopDirectoryOnly) : null).
// ReSharper restore PossiblyMistakenUseOfParamsMethod
                                Where(j => (File.GetAttributes(j) & FileAttributes.Hidden) != FileAttributes.Hidden).Distinct().ToList();
                    }
                    else
                    {
                        _state = true;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(ex, ErrorType.ERROR, "ENoPath", new string[] { _sourcePath }, "GENERAL_FILEPUMPER_UPDATE", LinkById, HostName, UserName);
                }
                _state = false;
            }
            catch (Exception ex)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { "" }, "GENERAL_FILEPUMPER_UPDATE", LinkById, HostName, UserName);
                }
                _state = false;
            }
        }

        /// <summary>
        /// Получение списка файлов
        /// </summary>
        /// <returns>массив имен файлов с полным путем</returns>
        public string[] GetFileList()
        {
            Update();
            return _fileList.ToArray();
        }

        /// <summary>
        /// Задание свойства удаление источника
        /// </summary>
        /// <param name="move">удаление источника</param>
        public void SetDestinationMove(bool move)
        {
            _move = move;
        }

        /// <summary>
        /// Задание папки источника
        /// </summary>
        /// <param name="sourcePath">папка источник</param>
        public void SetSourcePath(string sourcePath)
        {
            if (sourcePath == null)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "sourcePath" }, "GENERAL_FILEPUMPER_SETSOURCEPATH", LinkById, HostName, UserName);                    
                }
                _state = false;
                return;
            }
            if (sourcePath == string.Empty)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "sourcePath" }, "GENERAL_FILEPUMPER_SETSOURCEPATH", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            _sourcePath = sourcePath;
            if (!Directory.Exists(_sourcePath))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EPathExist", new string[] { _sourcePath }, "GENERAL_FILEPUMPER_INITIALIZE", LinkById, HostName, UserName);
                }
                _state = false;
            }
        }

        /// <summary>
        /// Задание папки назначения
        /// </summary>
        /// <param name="destinationPath">папка назначения</param>
        public void SetDestinationPath(string destinationPath)
        {
            if (destinationPath == null)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "destinationPath" }, "GENERAL_FILEPUMPER_SETDESTINATIONPATH", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            if (destinationPath.Length < 1)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "destinationPath" }, "GENERAL_FILEPUMPER_SETDESTINATIONPATH", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            _destinationPath = destinationPath;
            if (!Directory.Exists(_destinationPath))
            {
                try
                {
                    Directory.CreateDirectory(_destinationPath);
                }
                catch (Exception ex)
                {
                    if (OnEventErrorHandler != null)
                    {
                        OnEventErrorHandler(ex, ErrorType.ERROR, "EMakeDir", new string[] { _destinationPath }, "GENERAL_FILEPUMPER_INITIALIZE", LinkById, HostName, UserName);
                    }
                    _state = false;
                }
            }
        }

        /// <summary>
        /// Задание маски поиска
        /// </summary>
        /// <param name="searchPatterns">маска поиска</param>
        public void SetSearchPatterns(string[] searchPatterns)
        {
            if (searchPatterns == null)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "searchPatterns" }, "GENERAL_FILEPUMPER_SETSEARCHPATTERNS", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            if (searchPatterns.Length < 1)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "searchPatterns" }, "GENERAL_FILEPUMPER_SETSEARCHPATTERNS", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            _searchPatterns.Clear();
            _searchPatterns.AddRange(searchPatterns);
            Update();            
        }

        /// <summary>
        /// Добавление маски поиска в массив
        /// </summary>
        /// <param name="searchPattern">маска поиска</param>
        public void AddSearchPattern(string searchPattern)
        {
            if (searchPattern == null)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "searchPatterns" }, "GENERAL_FILEPUMPER_ADDSEARCHPATTERN", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            if (searchPattern.Length < 1)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "searchPatterns" }, "GENERAL_FILEPUMPER_ADDSEARCHPATTERN", LinkById, HostName, UserName);
                }
                _state = false;
                return;
            }
            _searchPatterns.Add(searchPattern);
        }

        /// <summary>
        /// Получение содержимого файла по имени
        /// </summary>
        /// <param name="fileName">полный путь к файлу</param>
        /// <param name="replaceToDestination">переместить в папку назначения</param>
        /// <returns>содержимое файла</returns>
        public byte[] GetFile_WF(object fileName, object replaceToDestination)
        {
            bool bReplace = Convert.ToBoolean(replaceToDestination);
            return GetFile(fileName.ToString(), bReplace);
        }

        /// <summary>
        /// Получение содержимого файла по имени
        /// Перегрузка этой функции с двумя аргументами
        /// необходима для компиляции wmsMLC.APS.wmsDeliveryWorkFlow
        /// Какой вредитель сделал её private ?!
        /// </summary>
        /// <param name="fileName">полный путь к файлу</param>
        /// <param name="replaceToDestination">переместить в папку назначения</param>
        /// <returns>содержимое файла</returns>
        public byte[] GetFile(string fileName, bool replaceToDestination)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(fileName.ToString(CultureInfo.InvariantCulture));
                    if (replaceToDestination)
                    {
                        File.Delete(fileName);
                        string destFileName = Path.Combine(_destinationPath, Path.GetFileName(fileName));
                        try
                        {
                            File.WriteAllBytes(destFileName, fileBytes);
                        }
                        catch (Exception ex)
                        {
                            if (OnEventErrorHandler != null)
                            {
                                OnEventErrorHandler(ex, ErrorType.ERROR, "EFileWrite", new string[] { destFileName }, "GENERAL_FILEPUMPER_GETFILE", LinkById, HostName, UserName);
                            }
                            _state = false;
                        }
                    }
                    return fileBytes;
                }
                catch (Exception ex)
                {
                    if (OnEventErrorHandler != null)
                    {
                        OnEventErrorHandler(ex, ErrorType.ERROR, "EFileRead", new string[] { fileName }, "GENERAL_FILEPUMPER_GETFILE", LinkById, HostName, UserName);
                    }
                    _state = false;
                }
            }
            return null;
        }

        /// <summary>
        /// Получение содержимого файла по имени
        /// Если файл скрытый, то возвращаем null
        /// </summary>
        /// <param name="fileName">полное имя файла</param>
        /// <returns>содержимое файла</returns>
        public byte[] GetFile(string fileName)
        {
            byte[] fileBytes = null;
            if ((File.Exists(fileName)) &&
                ((File.GetAttributes(fileName) & FileAttributes.Hidden) != FileAttributes.Hidden))
            {
                try
                {
                    fileBytes = File.ReadAllBytes(fileName);
                    //if (_move)
                    //{
                    //    File.Delete(fileName);
                    //}                    
                }
                catch(Exception ex)
                {
                    if (OnEventErrorHandler != null)
                    {
                        OnEventErrorHandler(ex, ErrorType.ERROR, "EFileRead", new string[] { fileName }, "GENERAL_FILEPUMPER_GETFILE", LinkById, HostName, UserName);
                    }
                    _state = false;
                }
                try
                {                    
                    if (_move)
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    if (OnEventErrorHandler != null)
                    {
                        OnEventErrorHandler(ex, ErrorType.ERROR, "EFileDelete", new string[] { fileName }, "GENERAL_FILEPUMPER_GETFILE", LinkById, HostName, UserName);
                    }
                    _state = false;
                    // TODO: экстренно сохранить файл куда-нибудь
                }
            }
            return fileBytes;
        }
    }
}
