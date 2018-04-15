using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using log4net.Util;

namespace wmsMLC.General
{
    /// <summary>
    /// Класс SortByFolderFileAppender унаследован от RollingFileAppender. Основное отличие - перегружен метод AdjustFileBeforeAppend и изменена архивация лог-файлов.
    /// Есть возможность управлять архивацией (унаследовано от базового класса) по размеру, времени и т.д. параметром rollingStyle (<rollingStyle value="Composite" />), 
    /// по умолчанию RollingStyle == RollingMode.Composite.
    /// См. http://logging.apache.org/log4net/release/config-examples.html.
    /// </summary>
    public class SortByFolderFileAppender : log4net.Appender.RollingFileAppender
    {
        #region . Fields&Consts .

        private static readonly Type DeclaringType = typeof(SortByFolderFileAppender);
        private const string ArchiveFileTemplate = "yyyyMMdd_HHmmss";

        private DateTime _nextCheck = DateTime.MaxValue;
        private DateTime _now;
        private DateTime _prevDate;
        private RollPoint _rollPoint;
        private static readonly DateTime Date1970 = new DateTime(1970, 1, 1);
        private string _handler;

        //Значение свойства File после применения метода ActivateOptions
        private string _file;

        #endregion . Fields&Consts .

        public SortByFolderFileAppender() 
        {
            _handler = string.Empty;

             //Значения свойств по-умолчанию
            ArchivePathPattern = string.Format("{0}_LogArchive", SystemInfo.ApplicationFriendlyName);
            SubArchivePathPattern = "{0:yyyyMMdd}";
        }

        #region . Properties .

        /// <summary>
        /// Шаблон папки, куда перемещаются архивные файлы. К пути, определяемому из File добавляется данное свойство.
        /// Значение по-умолчанию - %appdomain_LogArchive. 
        /// Пример, <archivePath type="log4net.Util.PatternString" value="%appdomain_LogArchive"/>.
        /// </summary>
        public string ArchivePathPattern { get; set; }

        /// <summary>
        /// Шаблон подпапки, куда перемещаются архивные файлы, в случае когда определена архивная папка ArchivePathPattern.
        /// К пути, определяемому из File добавляется ArchivePathPattern и данное свойство. Значение по-умолчанию - {0:yyyyMMdd}. 
        /// Пример, <subArchivePathPattern value="{0:yyyyMMdd}"/>.
        /// </summary>
        public string SubArchivePathPattern { get; set; }

        /// <summary>
        /// Возможность не использовать отдельные файлы для логирования по свойству Handler. Пример, <doNotUseServiceHandler value="True"/>.
        /// </summary>
        public bool DoNotUseServiceHandler { get; set; }

        #endregion . Properties .

        #region . Methods .

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            //DateTimeStrategy инициализируется в base.ActivateOptions()
            _prevDate =
                _now = DateTimeStrategy.Now;
            _rollPoint = ComputeCheckPeriod(DatePattern);
            _nextCheck = NextCheckDate(_now, _rollPoint);
            _file = File;
        }

        protected override void AdjustFileBeforeAppend()
        {
            // полностью переопределяем поведение базового класса
            //base.AdjustFileBeforeAppend();

            // по времени
            var now = DateTimeStrategy.Now;
            var rollingStyleComposite = RollingStyle == RollingMode.Composite;
            if (rollingStyleComposite || RollingStyle == RollingMode.Date)
            {
                if (now >= _nextCheck)
                {
                    _nextCheck = NextCheckDate(now, _rollPoint);
                    _now = _prevDate; //Архивируем в папке, определяемой по времени предыдущей записи
                    RollOverFile();
                }
            }
            _prevDate = now;

            // по размеру
            var size = ((CountingQuietTextWriter)QuietWriter).Count;
            if (rollingStyleComposite || RollingStyle == RollingMode.Size)
            {
                if (File != null && size >= MaxFileSize)
                {
                    _now = now;
                    RollOverFile();
                }
            }

            // обрабатываем изменение имени файла
            if (DoNotUseServiceHandler || _file == null)
                return;

            var ohandler = GlobalContext.Properties[Log4NetHelper.ServiceNamePropertyName];
            if (ohandler == null)
                return;
            
            var handler = ohandler.ToString();
            if (_handler != handler)
            {
                _handler = handler;
                CloseFile();

                var newFileNamePath = Path.Combine(Path.GetDirectoryName(_file),
                    string.Format("{0}{1}{2}", Path.GetFileNameWithoutExtension(_file), handler,
                        Path.GetExtension(_file)));

                //Если в File есть записи
                if (size > 0)
                {
                    string text;
                    //читаем все данные из File
                    using (var fs = System.IO.File.Open(File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var sr = new StreamReader(fs, Encoding))
                        {
                            text = sr.ReadToEnd();
                        }
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine(text);

                    //записываем в newFileNamePath
                    using (var fs = System.IO.File.Open(newFileNamePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var sw = new StreamWriter(fs, Encoding))
                        {
                            sw.Write(sb.ToString());
                        }
                    }
                }

                File = newFileNamePath;
                base.ActivateOptions();

                //удаляем мусор
                if (_file != File && FileExists(_file))
                    DeleteFile(_file);

                SafeOpenFile(File, true);
            }
        }

        private void RollOverFile()
        {
            CloseFile();
            RollOverRenameFiles(File, _now);
            SafeOpenFile(File, false);
        }

        private void RollOverRenameFiles(string baseFileName, DateTime now)
        {
            var baseDirectory = Path.GetDirectoryName(File);
            if (string.IsNullOrEmpty(baseDirectory))
                return;

            var archiveDirectory = baseDirectory;
            var dateDirectory = archiveDirectory;
            if (!string.IsNullOrEmpty(ArchivePathPattern))
            {
                archiveDirectory = Path.Combine(baseDirectory, ArchivePathPattern);
                if (!string.IsNullOrEmpty(SubArchivePathPattern))
                    dateDirectory = Path.Combine(archiveDirectory, string.Format(SubArchivePathPattern, now));
            }

            var fileNameOnly = Path.GetFileName(File);
            var newFile = String.Format("{0}_{1}", fileNameOnly, now.ToString(ArchiveFileTemplate));
            var newFileNamePath = Path.Combine(dateDirectory, newFile);

            if (!Directory.Exists(archiveDirectory))
                Directory.CreateDirectory(archiveDirectory);

            if (!Directory.Exists(dateDirectory))
                Directory.CreateDirectory(dateDirectory);

            RollFile(baseFileName, newFileNamePath);
        }

        private RollPoint ComputeCheckPeriod(string datePattern)
        {
            var sdate = Date1970.ToString(datePattern, DateTimeFormatInfo.InvariantInfo);

            foreach (var p in Enum.GetValues(typeof (RollPoint)).Cast<RollPoint>().Where(p => p != RollPoint.InvalidRollPoint))
            {
                var nextdate = NextCheckDate(Date1970, p).ToString(datePattern, DateTimeFormatInfo.InvariantInfo);
                LogLog.Debug(DeclaringType, string.Concat(new object[] { "Type = [", p, "], r0 = [", sdate, "], r1 = [", nextdate, "]" }));
                if (sdate != nextdate)
                    return p;
            }

            return RollPoint.InvalidRollPoint;
        }

        #endregion . Methods .
    }
}