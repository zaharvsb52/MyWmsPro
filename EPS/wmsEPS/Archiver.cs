#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="Archiver.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>08.10.2012 10:17:59</Date>
/// <Summary>Архивация, сжатие файла</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

// DotNetZip Library
// Home page: http://dotnetzip.codeplex.com/
// Download file: DotNetZipLib-DevKit-v1.9.zip 
// File: zip-v1.9\Release\Ionic.Zip.dll
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using wmsMLC.General.Services;
using wmsMLC.General.Types;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Класс, для создания сжатых, архивных файлов
    /// </summary>
    public class Archiver : BaseObject
    {
        /// <summary>
        /// Событие записи сообщения в лог
        /// </summary>
        //public event EventErrorHandler OnEventErrorHandler;

        public Archiver(string linkById)
        {
            LinkById = linkById;
        }

        public Archiver(string hostName, string userName, string linkById)
        {
            LinkById = linkById;
            HostName = hostName;
            UserName = userName;
        }

        /// <summary>
        /// Установка уровня компресии
        /// </summary>
        /// <param name="level">уровеь</param>
        /// <returns>уровень компресии</returns>
        private static CompressionLevel CompressionLevel(int level)
        {
            CompressionLevel compressionLevel;
            switch (level)
            {
                case 0:
                    compressionLevel = Ionic.Zlib.CompressionLevel.None;
                    break;
                case 1:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level9;
                    break;
                case 2:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level8;
                    break;
                case 3:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level7;
                    break;
                case 4:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level6;
                    break;
                case 5:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level5;
                    break;
                case 6:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level4;
                    break;
                case 7:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level3;
                    break;
                case 8:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level2;
                    break;
                case 9:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level1;
                    break;
                case 10:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Level0;
                    break;
                default:
                    compressionLevel = Ionic.Zlib.CompressionLevel.Default;
                    break;
            }
            return compressionLevel;
        }

        /// <summary>
        /// Сжатие файла в архивный файл
        /// </summary>
        /// <param name="fileName">полное имя файла, который нужно сжать</param>
        /// <param name="archiveFileName">полное имя файла, в который происходит запись архива</param>
        /// <param name="compressionLevel">уровень сжатия</param>
        public void ArchiveFileToFile(string fileName, string archiveFileName, int compressionLevel)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "FileName" }, "GENERAL_ARCHIVER_ARCHIVERFILETOFILE", LinkById, HostName, UserName);
                }
                return;
            }
            if (string.IsNullOrEmpty(archiveFileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "ArchiveFileName" }, "GENERAL_ARCHIVER_ARCHIVERFILETOFILE", LinkById, HostName, UserName);
                }
                return;
            }
            ArchiveFilesToFile(new string[] { fileName }, archiveFileName, compressionLevel);
        }

        /// <summary>
        /// Сжатие файлов в архивный файл
        /// </summary>
        /// <param name="fileNames">список полных имен файлов, которые необходимо сжать</param>
        /// <param name="archiveFileName">полное имя файла, в который происходит запись архива</param>
        /// <param name="compressionLevel">уровень сжатия</param>
        public void ArchiveFilesToFile(string[] fileNames, string archiveFileName, int compressionLevel)
        {
            if (string.IsNullOrEmpty(archiveFileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "ArchiveFileName" }, "GENERAL_ARCHIVER_ARCHIVERFILESTOFILE", LinkById, HostName, UserName);
                }
                return;
            }

            try
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionLevel = CompressionLevel(compressionLevel);
                    zip.AlternateEncoding = Encoding.UTF8;
                    zip.AlternateEncodingUsage = ZipOption.Always;
                    AddFile(zip, fileNames);
                    if (zip.Count > 0)
                        zip.Save(archiveFileName);
                }
            }
            catch (Exception ex)
            {
                if (OnEventErrorHandler != null)
                    OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { string.Empty }, "GENERAL_ARCHIVER_ARCHIVERFILESTOFILE", LinkById, HostName, UserName);
            }
        }

        /// <summary>
        /// Добавление файлов в zip
        /// Если файла нет - сообщение об отсутствии файла
        /// </summary>
        /// <param name="zip">элемент Ionic</param>
        /// <param name="fileNames">список имен файлов</param>
        private void AddFile(ZipFile zip, IEnumerable<string> fileNames)
        {
            foreach (string str in fileNames)
            {
                if (string.IsNullOrEmpty(str))
                {
                    if (OnEventErrorHandler != null)
                    {
                        OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "FileName" }, "GENERAL_ARCHIVER_ADDFILES", LinkById, HostName, UserName);
                    }
                    return;
                }
                if (File.Exists(str))
                {
                    zip.AddFile(str, @"\");
                }
                else
                {
                    if (OnEventErrorHandler != null)
                        OnEventErrorHandler(null, ErrorType.ERROR, "EUnknown", new string[] { string.Format("file: {0} not exists", str) }, "GENERAL_ARCHIVER_ADDFILES", LinkById, HostName, UserName);
                }
            }
        }

        /// <summary>
        /// Сжатие файла из памяти и сохранение в файл
        /// </summary>
        /// <param name="bytes">массив байт</param>
        /// <param name="fileName">имя файла для </param>
        /// <param name="archiveFileName"></param>
        /// <param name="compressionLevel"></param>
        public void ArchiveStreamToFile(byte[] bytes, string fileName, string archiveFileName, int compressionLevel)
        {
            if ((bytes == null) ||
                (bytes.Length == 0))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "" }, "GENERAL_ARCHIVER_ARCHIVERSTREAMTOFILE", LinkById, HostName, UserName);
                }
                return;
            }
            if (string.IsNullOrEmpty(fileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "FileName" }, "GENERAL_ARCHIVER_ARCHIVERFOLDERTOFILE", LinkById, HostName, UserName);
                }
                return;
            }
            if (string.IsNullOrEmpty(archiveFileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "ArchiveFileName" }, "GENERAL_ARCHIVER_ARCHIVERFOLDERTOFILE", LinkById, HostName, UserName);
                }
                return;
            }
            using (var stream = new MemoryStream(bytes))
            {
//                using (new BinaryWriter(stream))
//                {
                    using (var zip = new ZipFile())
                    {
                        zip.CompressionLevel = CompressionLevel(compressionLevel);
                        stream.Position = 1; //<<<<< changes
                        zip.AlternateEncoding = Encoding.UTF8;
                        zip.AlternateEncodingUsage = ZipOption.Always;
                        zip.AddEntry(fileName, stream);
                        zip.Save(archiveFileName);
                    }
                //}
            }
        }

        /// <summary>
        /// Сжатие файла из памяти и сохранение в файл
        /// </summary>
        /// <param name="fileName">имя файла для </param>
        /// <param name="compressionLevel"></param>
        public byte[] ArchiveFileToStream(string fileName, int compressionLevel)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "FileName" }, "GENERAL_ARCHIVER_ARCHIVERFILETOSTREAM", LinkById, HostName, UserName);
                }
                return null;
            }

            using (var zip = new ZipFile())
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        zip.CompressionLevel = CompressionLevel(compressionLevel);
                        zip.AlternateEncoding = Encoding.UTF8;
                        zip.AlternateEncodingUsage = ZipOption.Always;
                        zip.AddFile(fileName);
                        zip.Save(stream);
                        return stream.GetBuffer();
                    }
                }
                catch (Exception ex)
                {
                    if (OnEventErrorHandler != null)
                        OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { string.Empty }, "GENERAL_ARCHIVER_ARCHIVERFILETOSTREAM", LinkById, HostName, UserName);
                }
            }
            return null;
        }

        /// <summary>
        /// Сжатие папки и сохранение в файл
        /// </summary>
        /// <param name="folderName">имя папки</param>
        /// <param name="archiveFileName">имя результирующего файла</param>
        /// <param name="compressionLevel">уровень сжатия</param>
        public void ArchiveFolderToFile(string folderName, string archiveFileName, int compressionLevel)
        {
            if (!Directory.Exists(folderName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EUnknown", new string[] { string.Format("not exists {0}", folderName) }, "GENERAL_ARCHIVER_ARCHIVERFOLDERTOFILE", LinkById, HostName, UserName);
                }
                return;
            }
            if (string.IsNullOrEmpty(archiveFileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "ArchiveFileName" }, "GENERAL_ARCHIVER_ARCHIVERFOLDERTOFILE", LinkById, HostName, UserName);
                }
                return;
            }

            try
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionLevel = CompressionLevel(compressionLevel);
                    zip.AlternateEncoding = Encoding.UTF8;
                    zip.AlternateEncodingUsage = ZipOption.Always;
                    zip.AddDirectory(folderName);
                    zip.Save(archiveFileName);
                }
            }
            catch (Exception ex)
            {
                if (OnEventErrorHandler != null)
                    OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { ex.Message }, "GENERAL_ARCHIVER_ARCHIVERFILESTOFILE", LinkById, HostName, UserName);
            }
        }

        /// <summary>
        /// Сжатие потока данных в другой поток
        /// </summary>
        /// <param name="bytes">массив данных</param>
        /// <param name="fileName">имя файла, в который можно сохранить архив</param>
        /// <param name="compressionLevel">уровень сжатия</param>
        /// <returns>сжатые данные</returns>
        public byte[] ArchiveStreamToStream(byte[] bytes, string fileName, int compressionLevel)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "FileName" }, "GENERAL_ARCHIVER_ARCHIVERSTREAMTOSTREAM", LinkById, HostName, UserName);
                }
                return null;
            }
            if ((bytes == null) ||
               (bytes.Length == 0))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "" }, "GENERAL_ARCHIVER_ARCHIVERSTREAMTOSTREAM", LinkById, HostName, UserName);
                }
                return null;
            }
            if ((fileName == string.Empty))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EEmpty", new string[] { "FileName" }, "GENERAL_ARCHIVER_ARCHIVERSTREAMTOSTREAM", LinkById, HostName, UserName);
                }
                return null;
            }
            try
            {
                using (var zip = new ZipFile())
                {
                    using (var stream = new MemoryStream())
                    {
                        zip.CompressionLevel = CompressionLevel(compressionLevel);
                        zip.AlternateEncoding = Encoding.UTF8;
                        zip.AlternateEncodingUsage = ZipOption.Always;
                        zip.AddEntry(fileName, bytes);
                        zip.Save(stream);
                        return stream.GetBuffer();
                    }
                }
            }
            catch (Exception ex)
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { string.Empty }, "GENERAL_ARCHIVER_ARCHIVERFILESTOFILE", LinkById, HostName, UserName);
                }
            }
            return null;
        }

        /// <summary>
        /// Сжатие папки и передача результата в массив 
        /// </summary>
        /// <param name="folderName">путь к папке</param>
        /// <param name="compressionLevel">уровень сжатия</param>
        /// <returns>массив байт</returns>
        public byte[] ArchiveFolderToStream(string folderName, int compressionLevel)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "ENoPath", new string[] { "" }, "GENERAL_ARCHIVER_ARCHIVERFILESTOFILE", LinkById, HostName, UserName);
                }
                return null;
            }
            if (!Directory.Exists(folderName))
            {
                if (OnEventErrorHandler != null)
                {
                    OnEventErrorHandler(null, ErrorType.ERROR, "EUnknown", new string[] { string.Format("not exists {0}", folderName) }, "GENERAL_ARCHIVER_ARCHIVERFOLDERTOFILE", LinkById, HostName, UserName);
                }
                return null;
            }
            
            try
            {
                using (var zip = new ZipFile())
                {
                    using (var stream = new MemoryStream())
                    {
                        zip.CompressionLevel = CompressionLevel(compressionLevel);
                        zip.AlternateEncoding = Encoding.UTF8;
                        zip.AlternateEncodingUsage = ZipOption.Always;
                        zip.AddDirectory(folderName);
                        zip.Save(stream);
                        return stream.GetBuffer();
                    }
                }
            }
            catch (Exception ex)
            {
                if (OnEventErrorHandler != null)
                    OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { string.Empty }, "GENERAL_ARCHIVER_ARCHIVERFILESTOFILE", LinkById, HostName, UserName);
            }
            return null;
        }
    }
}