#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSFTP.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>12.10.2012 8:37:23</Date>
/// <Summary>Для передачи данных по протоколу FTP</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;
using System.Net;
using System.Text;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.OutputTasks;
using wmsMLC.General.Types;
using wmsMLC.General.wmsLogClient;

namespace wmsMLC.EPS.wmsEPS.Helpers
{
    /// <summary>
    /// Протокол FTP
    /// </summary>
    public class EpsFtp
    {
        private readonly string _targetFolder;

        private EpsTaskProtect _fileRecordProtect;
        private string _supportTargetFolder;
        private string _supportFileName;
        private readonly string _userLogin;
        private readonly string _userPassword;
        private readonly string _serverName;

        public Encoding Encoding { get; set; }
        public int Timeout { get; set; }
        public int BufferSize { get; set; }
        public bool UsePassive { get; set; }

        /// <summary>
        /// Значения по умолчанию.
        /// </summary>
        /// <param name="linkById"></param>
        /// <param name="hostName"></param>
        /// <param name="userName"></param>
        public EpsFtp(string targetFolder, string serverName, string login, string password)
        {
            if (string.IsNullOrEmpty(targetFolder) || EpsOtcBase.ParamMap.NullValue.Equals(targetFolder))
                throw new ArgumentException("targetFolder is 'null'");

            _targetFolder = targetFolder;
            _serverName = serverName;
            _supportTargetFolder = string.Empty;
            _supportFileName = string.Empty;
            _fileRecordProtect = EpsTaskProtect.None;
            _userLogin = string.IsNullOrEmpty(login) || EpsOtcBase.ParamMap.NullValue.Equals(login) ? "anonymous" : login;
            _userPassword = string.IsNullOrEmpty(password) || EpsOtcBase.ParamMap.NullValue.Equals(password) ? "anonymous" : password;
            BufferSize = 2048; // The buffer size is set to 2kb
            UsePassive = true;
            Timeout = 20000; // set timeout for 20 seconds
        }

        public void SetFileRecordProtect(EpsTaskProtect mode, string supportTargetFolder, string supportFileName)
        {
            if (mode == EpsTaskProtect.FOLDER &&
                (string.IsNullOrEmpty(supportTargetFolder) || EpsOtcBase.ParamMap.NullValue.Equals(supportTargetFolder)))
            {
                throw new ArgumentException("supportTargetFolder");
            }

            if ((mode == EpsTaskProtect.CRC || mode == EpsTaskProtect.EXT) &&
                (string.IsNullOrEmpty(supportFileName) || EpsOtcBase.ParamMap.NullValue.Equals(supportFileName)))
            {
                throw new ArgumentException("supportFileName");
            }

            _fileRecordProtect = mode;
            _supportTargetFolder = supportTargetFolder;
            _supportFileName = supportFileName;
        }

        public void Write(string sourceFileName, byte[] data)
        {
            var sm = new SubstMacros();
            sm.SetMacro("FILENAME", sourceFileName);

            string dstFolder = (_fileRecordProtect == EpsTaskProtect.FOLDER) ? _supportTargetFolder : _targetFolder;
            string dstFileName = (_fileRecordProtect == EpsTaskProtect.EXT) ? sm.Substitute(_supportFileName) : Path.GetFileName(sourceFileName);
            string dstFile = String.Format("{0}/{1}", dstFolder, dstFileName);

            // Проверка наличия директории и ее создание, если ее нет
            if (!IsDirectoryExists(dstFolder))
            {
                CreateDirectory(dstFolder);
            }

            // Проверка существования файла и его удаление
            if (IsFileExists(dstFile))
            {
                DeleteFile(dstFile);
            }

            // Запись файла или временного файла
            UploadFile(dstFile, data);

            // Перемещение временного файла
            if (_fileRecordProtect == EpsTaskProtect.FOLDER)
            {
                if (!IsDirectoryExists(_targetFolder))
                {
                    CreateDirectory(_targetFolder);
                }
                MoveFile(dstFile, String.Format("{0}/{1}", _targetFolder, Path.GetFileName(sourceFileName)));
            }

            // Переименование временного файла
            if (_fileRecordProtect == EpsTaskProtect.EXT)
            {
                RenameFile(dstFile, sourceFileName);
            }

            // Создание файла - спутника
            if (_fileRecordProtect == EpsTaskProtect.CRC)
            {
                UploadFile(string.Format("{0}/{1}", dstFolder, sm.Substitute(_supportFileName)),
                    Encoding.Default.GetBytes(new HashCode().GetMD5(data)));
            }
        }

        private FtpWebRequest CreateFtpRequest(string uri)
        {
            var request = (FtpWebRequest)WebRequest.Create(new Uri(uri));
            request.Proxy = null;
            request.UsePassive = UsePassive;
            request.KeepAlive = false;
            request.UseBinary = true;
            request.ContentLength = 0;
            request.Credentials = new NetworkCredential(_userLogin, _userPassword);
            return request;
        }

        public bool IsFileExists(string fileName)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + fileName);
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                response = (FtpWebResponse)ex.Response;
                if (FtpStatusCode.ActionNotTakenFileUnavailable == response.StatusCode)
                {
                    return false;
                }
                else
                {
                    throw new IOException("FTP get file size error. See inner exception.", ex);
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public bool IsDirectoryExists(string folderName)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + folderName + "/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                response = (FtpWebResponse)ex.Response;
                if (FtpStatusCode.ActionNotTakenFileUnavailable == response.StatusCode)
                {
                    return false;
                }
                else
                {
                    throw new IOException("FTP list directory error. See inner exception.", ex);
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public void DeleteFile(string fileName)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + fileName);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new IOException("Could not delete file. See inner exception.", ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public void DeleteDirectory(string folderName)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + folderName);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new IOException("Could not delete directory. See inner exception.", ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public void UploadFile(string fileName, byte[] bytes)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + fileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.ContentLength = bytes.Length;
                using (var ftpStream = request.GetRequestStream())
                {
                    ftpStream.Write(bytes, 0, bytes.Length);
                }
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new IOException("Could not upload file. See inner exception.", ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private void CreateDirectoryInternal(string folderName)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + folderName);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new IOException("Could not create directory. See inner exception.", ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public void CreateDirectory(string folderName)
        {
            string[] arrayFolder = folderName.Split(new Char[] { '/', '\\' });
            if (arrayFolder.Length > 0)
            {
                string folder = "";
                for (int i = 0; i < arrayFolder.Length; i++)
                {
                    folder = (i == 0) ? arrayFolder[i] : string.Format("{0}/{1}", folder, arrayFolder[i]);
                    if (!IsDirectoryExists(folder))
                    {
                        CreateDirectoryInternal(folder);
                    }
                }
            }
        }

        public void RenameFile(string fileName, string newFileName)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest request = CreateFtpRequest("ftp://" + _serverName + "/" + fileName);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = newFileName;
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                throw new IOException("Could not rename file. See inner exception.", ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public void MoveFile(string fileName, string newFileName)
        {
            FtpWebResponse response = null;
            try
            {
                if (fileName.Equals(newFileName)) return;

                var uriSource = new Uri("ftp://" + _serverName + "/" + fileName);
                var uriDestination = new Uri("ftp://" + _serverName + "/" + newFileName);

                if (IsFileExists(newFileName))
                {
                    DeleteFile(newFileName);
                }

                if (!IsFileExists(fileName))
                    return;

                Uri targetUriRelative = uriSource.MakeRelativeUri(uriDestination);
                FtpWebRequest ftp = CreateFtpRequest(uriSource.AbsoluteUri);
                ftp.Method = WebRequestMethods.Ftp.Rename;
                ftp.RenameTo = Uri.UnescapeDataString(targetUriRelative.ToString());
                response = (FtpWebResponse)ftp.GetResponse();
            }
            catch (WebException ex)
            {
                throw new IOException("Could not move file. See inner exception.", ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
    }
}