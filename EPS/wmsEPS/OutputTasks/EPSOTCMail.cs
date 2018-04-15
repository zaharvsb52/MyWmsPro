#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSOTCMail.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>04.10.2012 8:23:12</Date>
/// <Summary>Реализация задачи почтового уведомления</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using log4net;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.EPS.wmsEPS.Properties;
using wmsMLC.General;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    /// <summary>
    /// Реализация задачи почтового уведомления
    /// </summary>
    public class EpsOtcMail : EpsOtcBase
    {
        const string SqlMacroSuf = "${SQL=";

        public override void DoTask(EpsFastReport[] reports)
        {
            var mailHelper = new MailHelper();
            using (var mail = mailHelper.CreateMailSender())
            {
                var newLine = "\r\n\r\n";
                // обработка форматов тела письма
                if (Settings.Default.MailBodyFormat.Equals(MediaTypeNames.Text.Html))
                {
                    newLine = "<br><br>";
                }

                mailHelper.SetFrom(mail);
                mailHelper.SetCredentials(mail);

                var sendTo = GetAllParamValues<string>(EpsTaskParams.Email);
                foreach (var addrr in sendTo)
                {
                    mail.AddTo(addrr);
                }

                var asAttach = GetParamValue(EpsTaskParams.AsAttachment, defaultValue: 1) == 1;

                var sendBlankMail = GetParamValue(EpsTaskParams.SendBlankMail, defaultValue: 1) == 1;

                var fileFormat = GetParamValue<string>(EpsTaskParams.FileFormat);
                if (!string.IsNullOrEmpty(fileFormat))
                {
                    Sb.SetMacro("FileFormat", fileFormat);
                    var ext = EpsHelper.GetFileExtension(fileFormat);
                    if (ext != null)
                        Sb.SetMacro("FileExtension", ext);
                }

                var deleteSrcFile = FindByName(EpsTaskParams.MoveFile) != null &&
                                     FindByName(EpsTaskParams.CopyFile) == null;

                var fileMask = GetParamValue<string>(EpsTaskParams.FileMask);
                if (!string.IsNullOrEmpty(fileMask))
                {
                    FilePumper.AddSearchPattern(fileMask);
                }

                var sourceFolder = GetParamValue<string>(EpsTaskParams.SourceFolder);
                if (!string.IsNullOrEmpty(sourceFolder))
                {
                    FilePumper.SetSourcePath(sourceFolder);
                }

                FilePumper.SetDestinationMove(deleteSrcFile);

                var resultReportFileValue = string.Empty;
                if (FindByName(EpsTaskParams.ResultReportFile) != null)
                {
                    Sb.SetMacro("ResultReportFile", GetParamValue<string>(EpsTaskParams.ResultReportFile));
                }
                else
                {
                    if (reports != null)
                    {
                        foreach (var report in reports.Where(p => p != null))
                        {
                            resultReportFileValue += string.Format("{0}{1}",
                                resultReportFileValue != string.Empty ? ";" : null, report.ReportName);
                        }
                    }
                    if (resultReportFileValue != string.Empty)
                        Sb.SetMacro("ResultReportFile", Sb.Substitute(resultReportFileValue));
                }

                // заполняем тему
                var mailSubject = ProcessSqlMacro(GetParamValue(EpsTaskParams.MailSubject, defaultValue: Settings.Default.MailSubject));
                mail.SetSubject(mailSubject);

                // Вызывать ProcessFiles только после обработки параметров задачи
                ProcessFiles(reports);
                if (Files.Count < 1)
                {
                    if (!sendBlankMail)
                    {
                        return; // не отсылаем почту
                    }
                }

                // заполняем тело
                var mailBody = ProcessSqlMacro(GetParamValue(EpsTaskParams.MailBody, defaultValue: Settings.Default.MailHeader));
                mail.SetBody(mailBody + newLine, true);

                // добавляем файлы
                foreach (var a in Files)
                {
                    if (asAttach)
                    {
                        mail.AddAttach(new MemoryStream(a.Data), a.FileName);
                    }
                    else
                    {
                        if (!Ziped) // не будем совать в тело zip файл
                        {
                            var s = new StreamReader(new MemoryStream(a.Data));
                            mail.AppendBody(s.ReadToEnd());
                            s.Close();
                        }
                    }
                }

                // ставим подпись
                var mailSignature = ProcessSqlMacro(GetParamValue(EpsTaskParams.MailSignature, defaultValue: Settings.Default.MailSignature));
                mail.AppendBody(newLine + mailSignature);

                // отправляем
                mail.Send();

                var logger = LogManager.GetLogger(GetType());
                logger.Info(string.Format("Mail '{0}' send to '{1}'.", mailSubject, string.Join(", ", sendTo)));
            }
        }

        private string ProcessSqlMacro(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // вычисляем макросы, т.к. могут использоваться в запросе
            var subsValue = Sb.Substitute(value);
            if (!subsValue.StartsWith(SqlMacroSuf))
                return subsValue;

            try
            {
                // вычленяем запрос
                var sql = subsValue.Replace(SqlMacroSuf, String.Empty);
                sql = sql.Substring(0, sql.Length - 1);

//                foreach (var param in Params)
//                {
//                    var paramName = param.OutputParamCode[0] == '{' ? param.OutputParamCode : "{" + param.OutputParamCode + "}";
//                    sql = sql.Replace(paramName, param.Value);
//                }

                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var table = mgr.ExecuteDataTable(sql);
                    if (table == null || table.Rows.Count == 0)
                        throw new DeveloperException("Вернулось 0 строк данных");

                    if (table.Rows[0].IsNull(0))
                        throw new DeveloperException("Вернулось пустое значение");

                    var name = Convert.ToString(table.Rows[0][0], CultureInfo.InvariantCulture);
                    if (string.IsNullOrEmpty(name))
                        throw new DeveloperException("Вернулась пустая строка");

                    // текст может быть в ковычках
                    subsValue = name.Replace("'", "");
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Ошибка получения значения по макросу {0}. {1}", value, ex.Message);
                throw new DeveloperException(message, ex);
            }

            // приводим еще раз, т.к. могли появиться новые макросы
            return Sb.Substitute(subsValue);
        }
    }
}
