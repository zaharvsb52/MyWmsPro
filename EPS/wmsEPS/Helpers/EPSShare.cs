#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSFTP.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>12.10.2012 8:37:23</Date>
/// <Summary>Для передачи данных по SMB</Summary>
/// --------------------------------------------------------------------------------------
/// 
#pragma warning restore 1587
using System;
using System.IO;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.OutputTasks;
using wmsMLC.General.Types;

namespace wmsMLC.EPS.wmsEPS.Helpers
{
    class EpsShare
    {
        private readonly string _targetFolder;

        private EpsTaskProtect _fileRecordProtect;
        private string _supportTargetFolder;
        private string _supportFileName;
        private readonly bool _deleteSrcFile;

        public EpsShare(string targetFolder, bool deleteSrcFile)
        {
            if (string.IsNullOrEmpty(targetFolder) || EpsOtcBase.ParamMap.NullValue.Equals(targetFolder))
                throw new ArgumentException("targetFolder is 'null'");

            _targetFolder = targetFolder;
            _deleteSrcFile = deleteSrcFile;
            _supportTargetFolder = string.Empty;
            _supportFileName = string.Empty;
            _fileRecordProtect = EpsTaskProtect.None;
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
            sm.SetMacro("FILENAME", Path.GetFileName(sourceFileName));

            string dstFolder = (_fileRecordProtect == EpsTaskProtect.FOLDER) ? _supportTargetFolder : _targetFolder;
            string dstFileName = (_fileRecordProtect == EpsTaskProtect.EXT) ? sm.Substitute(_supportFileName) : Path.GetFileName(sourceFileName);
            string dstFile = Path.Combine(dstFolder, dstFileName);

            CreateFile(dstFile, data);

            if (_fileRecordProtect == EpsTaskProtect.CRC)
            {
                var hc = new HashCode();
                string strHashCode = hc.GetMD5(dstFile);
                CreateFile(Path.Combine(_targetFolder, sm.Substitute(_supportFileName)), new[] { strHashCode });
            }
            
            if (_fileRecordProtect == EpsTaskProtect.EXT || _fileRecordProtect == EpsTaskProtect.FOLDER)
            {
                string newFileName = Path.Combine(_targetFolder, Path.GetFileName(sourceFileName));
                DeleteFileIfExists(newFileName);
                MoveFile(dstFile, newFileName);
            }

            if (_deleteSrcFile)
            {
                DeleteFileIfExists(sourceFileName);
            }
        }

        private void CreateFile(string fileName, string[] content)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("fileName");

            var dirName = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            File.WriteAllLines(fileName, content);
        }

        private void CreateFile(string fileName, byte[] content)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("fileName");

            var dirName = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            File.WriteAllBytes(fileName, content);
        }

        private void DeleteFileIfExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("fileName");

            if (!File.Exists(fileName))
                return;

            File.Delete(fileName);
        }

        private bool MoveFile(string sourceFile, string targetFile)
        {
            if (string.IsNullOrEmpty(sourceFile))
                throw new ArgumentException("sourceFile");

            if (string.IsNullOrEmpty(targetFile))
                throw new ArgumentException("targetFile");

            if (!File.Exists(sourceFile))
                throw new FileNotFoundException("Src file not found", sourceFile);

            if (File.Exists(targetFile))
                throw new IOException(string.Format("{0} already exists", targetFile));

            var dirName = Path.GetDirectoryName(targetFile);
            if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            try
            {
                File.Move(sourceFile, targetFile);
            }
            catch (Exception)
            {
                DeleteFileIfExists(sourceFile);
                throw;
            }

            return true;
        }
    }
}
