using System;
using System.IO;
using System.Text;
using BLToolkit.Data;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.General;

namespace MLC.Wms.Tests.Integration
{
    [SetUpFixture]
    public class IntegrationTestsSetUpClass
    {
        private const string DeleteScriptFilePath = "Integration\\sql\\delete.sql";
        private const string FillScriptFilePath = "Integration\\sql\\fill.sql";

        private string _deletescript;

        public static StringBuilder Log { get; private set; }

        static IntegrationTestsSetUpClass()
        {
            Log = new StringBuilder();
        }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);

            //BLHelper.InitBL(dalType: DALType.Service);
            //BLHelper.RegisterServiceClient("Auto", ClientTypeCode.DCL, Properties.Settings.Default.SDCL_Endpoint);

            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");

            BLHelper.FillInitialCaches();

            FillTestData(true);
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            DeleteTestData();
            WriteFixtureLogFile();
        }

        private void FillTestData(bool deleteBeforeFill)
        {
            var fillscript = GetScriptFromFile(FillScriptFilePath);
            if (deleteBeforeFill)
                fillscript = GetDeleteScript() + fillscript;
            fillscript = PrepareScript(fillscript);

            try
            {
                using (var db = new DbManager())
                {
                    db.SetCommand(fillscript).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибки при выполнении скрипта '{0}'.", FillScriptFilePath), ex);
            }
        }

        private void DeleteTestData()
        {
            var deletescript = GetDeleteScript();
            deletescript = PrepareScript(deletescript);
            try
            {
                using (var db = new DbManager())
                {
                    db.SetCommand(deletescript).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибки при выполнении скрипта '{0}'.", DeleteScriptFilePath), ex);
            }
        }

        private string GetDeleteScript()
        {
            if (string.IsNullOrEmpty(_deletescript))
                _deletescript = GetScriptFromFile(DeleteScriptFilePath);
            return _deletescript;
        }

        private string GetScriptFromFile(string path)
        {
            using (TextReader reader = new StreamReader(path))
            {
                var result = reader.ReadToEnd();
                reader.Close();
                return result;
            }
        }

        private string PrepareScript(string sql)
        {
            if (sql == null)
                return string.Empty;

            var result = sql.Replace("/", string.Empty);
            //result = sql.Replace(";", string.Empty);
            result = string.Format("BEGIN{0}{1}{0}END;", Environment.NewLine, result);
            return result;
        }

        private void WriteFixtureLogFile()
        {
            var content = Log.ToString();
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("В лог писать нечего.");
                return;
            }

            // try to save report
            try
            {
                var fileName = string.Format("{0}.{1:yyyyMMddHHmmss}.log", GetType().FullName, DateTime.Now);
                var fullFileName = Path.Combine(Path.GetFullPath(@".\"),fileName);
                if (File.Exists(fullFileName))
                    File.Delete(fullFileName);

                File.WriteAllText(fullFileName, Log.ToString());
            }
            catch (Exception)
            {
                // упало - ничего срашного
                Console.WriteLine("Не смогли записать лог в файл.");
            }
        }
    }
}