using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;
using wmsMLC.General.Types;
using wmsMLC.General.wmsLogClient;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    public abstract class EpsOtcBase : IEpsOutputTask
    {
        public class ParamMap
        {
            public const string NullValue = "null";

            public EpsTaskParams Name { get; set; }
            public object Value { get; set; }
            public object Subvalue { get; set; }
        };

        /// <summary>
        /// Список параметров задачи
        /// </summary>
        public List<ParamMap> Params = new List<ParamMap>();

        /// <summary>
        /// Создать архивную версию файла
        /// </summary>
        public bool Ziped = false;

        /// <summary>
        /// Создать резервную копию файла
        /// </summary>
        public bool Reserve = false;

        /// <summary>
        /// Файлы задачи
        /// </summary>
        public List<FileStruct> Files;

        /// <summary>
        /// Архиватор zip
        /// </summary>
        public Archiver Archiver = null;

        /// <summary>
        /// Обработчик файлов
        /// </summary>
        public FilePumper FilePumper;

        private ExportType _exportType;

        public void SetExportType(ExportType et)
        {
            _exportType = et;
        }

        protected SubstMacros Sb;

        protected EpsOtcBase()
        {
            FilePumper = new FilePumper(Environment.MachineName, Environment.UserName, string.Empty);
            FilePumper.OnEventErrorHandler += LogClient.Logger.OnEventErrorHandler;
            Files = new List<FileStruct>();
            Sb = new SubstMacros();
            Sb.SetMacro("USERNAME", Environment.UserName);
            Sb.SetMacro("HOSTNAME", Environment.MachineName);
        }

        public void AddParameter(string name, object value, object subvalue)
        {
            var p = name.To(EpsTaskParams.None);
            if (p == EpsTaskParams.None) throw new DeveloperException(string.Format("Undefined EpsTaskParams '{0}' in EpsOtcBase.AddParameter.", name));
            var param = new ParamMap { Name = p, Subvalue = subvalue };
            value = value ?? ParamMap.NullValue;
            //param.value = value;
            // Коллеги, пожалуйста, делайте подстановку макросов непосредственно перед употреблением переменной. При загрузке
            // конфигурационного файла некоторые макросы будут не определены.
            param.Value = Sb.Substitute(value.ToString());
            Params.Add(param);
            Sb.SetMacro(name, value.ToString());
        }

        public abstract void DoTask(EpsFastReport[] reports);

        /// <summary>
        /// Поиск параметра по имени.
        /// </summary>
        /// <param name="paramType">имя типа параметра</param>
        /// <param name="subvalue"></param>
        /// <returns>объект параметра</returns>
        public ParamMap FindByName(EpsTaskParams paramType, object subvalue = null) // Коллеги, а не лучше ли использовать SortedList ?
        {
            return subvalue == null
                ? Params.FirstOrDefault(p => p.Name.To(EpsTaskParams.None) == paramType)
                : Params.FirstOrDefault(p => p.Name.To(EpsTaskParams.None) == paramType && Equals(p.Subvalue, subvalue));
        }

        protected T GetParamValue<T>(EpsTaskParams paramType, object subvalue = null, T defaultValue = default (T))
            where T : IConvertible
        {
            var param = FindByName(paramType, subvalue);
            if (param == null || ParamIsEmpty(param))
                return defaultValue;
            return param.Value.To<T>();
        }

        protected IList<T> GetAllParamValues<T>(EpsTaskParams paramType, object subvalue = null, T defaultValue = default (T))
            where T : IConvertible
        {
            var prms = subvalue == null
               ? Params.Where(p => p != null && p.Name.To(EpsTaskParams.None) == paramType)
               : Params.Where(p => p != null && p.Name.To(EpsTaskParams.None) == paramType && Equals(p.Subvalue, subvalue));

            var list = new List<T>();
            foreach (var prm in prms)
            {
                if (ParamIsEmpty(prm))
                    continue;

                list.Add(prm.Value.To<T>());
            }
            return list;
        }

        /// <summary>
        /// Обработка файлов задачи. 
        /// !!! Вызывать только после обработки параметров задачи !!!
        /// </summary>
        /// <param name="reports">отчеты</param>
        public void ProcessFiles(EpsFastReport[] reports)
        {
            if (reports != null && reports.Length > 0 && _exportType != null)
            {
                foreach (var epsFastReport in reports)
                {
                    if (epsFastReport == null) 
                        continue;

                    var extension = GetParamValue<string>(EpsTaskParams.FileExtension);
                    extension = string.IsNullOrEmpty(extension)
                        ? EpsHelper.GetFileExtension(_exportType.Format)
                        : extension.TrimStart('.');

                    if (string.IsNullOrEmpty(extension))
                        throw new Exception(string.Format("Неопределено расширение файла для формата '{0}'.", _exportType.Format));

                    var resultReportFile = Sb.Substitute(epsFastReport.ReportName);
                    byte[] data;

                    if (Archiver == null)
                    {
                        data = epsFastReport.GetExportStream(_exportType);
                    }
                    else
                    {
                        data = Archiver.ArchiveStreamToStream(epsFastReport.GetExportStream(_exportType),
                            resultReportFile + "." + extension,
                            Properties.Settings.Default.CompressionLevel);
                    }

                    Files.Add(extension.Length > 0
                        ? new FileStruct(data, resultReportFile, extension, Ziped)
                        : new FileStruct(data, resultReportFile, Ziped));
                }
            }
            else //Если отчетов нет, то проверяем парметр EpsTaskParams.SourceFolder
            {
                if (FindByName(EpsTaskParams.SourceFolder) == null)
                {
                    throw new Exception("SourceFolder parameter not found.");
                }
            }

            var fileList = FilePumper.GetFileList();
            if (FilePumper.PumperState)
            {
                if (fileList == null) 
                    return;

                foreach (var f in fileList)
                {
                    var data = (Archiver != null)
                        ? Archiver.ArchiveStreamToStream(FilePumper.GetFile(f), Path.GetFileName(f), Properties.Settings.Default.CompressionLevel)
                        : FilePumper.GetFile(f);

                    Files.Add(new FileStruct(data, f, Ziped));
                }
            }
            else
            {
                throw new Exception("Could not get source files.");
            }
        }

        /// <summary>
        /// Сделать архив и работать с ним
        /// </summary>
        /// <param name="zip">архивировать или нет</param>
        /// <param name="reserve">создать резервную копию</param>
        public void Zip(bool zip, bool reserve)
        {
            Ziped = zip;
            Archiver = Ziped ? new Archiver(Environment.MachineName, Environment.UserName, string.Empty) : null;
            Reserve = reserve;
        }

        public List<FileStruct> GetFiles()
        {
            return Files;
        }

        /// <summary>
        /// Проверка что параметр не пустой.
        /// </summary>
        /// <param name="p">объект параметра</param>
        /// <returns>true если пустой</returns>
        protected bool ParamIsEmpty(ParamMap p)
        {
            if (!p.Value.To<string>().EqIgnoreCase(ParamMap.NullValue))
            {
                return false;
            }
            return true;
        }
    }
}
