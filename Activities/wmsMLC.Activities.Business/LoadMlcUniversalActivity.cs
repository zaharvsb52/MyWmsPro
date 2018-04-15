using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Business
{
    public class LoadMlcUniversalActivity : NativeActivity
    {
        private class Parameters
        {
            public string FileName { get; set; }
            public Decimal? MandantId { get; set; }
            public string ABCD { get; set; }
        }

        protected override void Execute(NativeActivityContext context)
        {
            var parameters = GetParameters(context);
            if (parameters == null)
                return;

            LoadData(parameters);
        }

        private Parameters GetParameters(NativeActivityContext context)
        {
            // отображаем окно запроса данных
            var fFile = new ValueDataField();
            fFile.Name = "File";
            fFile.FieldName = fFile.Name;
            fFile.FieldType = typeof(string);
            //fFile.SourceName = fFile.Name;
            fFile.Caption = "Файл";
            fFile.IsRequired = true;
            fFile.IsLabelFontWeightBold = true;

            var fMandant = new ValueDataField();
            fMandant.Name = "Mandant";
            fMandant.FieldName = fMandant.Name;
            //fMandant.SourceName = fMandant.Name;
            fMandant.FieldType = typeof(decimal?);
            fMandant.Caption = "Мандант";
            fMandant.IsRequired = true;
            fMandant.LookupCode = "MANDANT_MANDANTID";
            fMandant.IsLabelFontWeightBold = true;

            var fABCD = new ValueDataField();
            fABCD.Name = "ABCD";
            fABCD.FieldName = fABCD.Name;
            //fABCD.SourceName = fABCD.Name;
            fABCD.FieldType = typeof(string);
            fABCD.Caption = "ABCD";
            fABCD.IsRequired = true;
            fABCD.LookupCode = "ENUM_ART";
            fABCD.IsLabelFontWeightBold = true;

            // создадим модель
            var model = new ExpandoObjectViewModelBase
            {
                Fields = new ObservableCollection<ValueDataField>(new[] { fFile, fMandant, fABCD })
            };
            model.PanelCaption = "Загрузка файла My Universal";
            model.SetSuffix("5CF7EA23-7B12-4211-9F18-45878562C0A7");

            var vs = IoC.Instance.Resolve<wmsMLC.General.PL.WPF.Services.IViewService>();
            if (vs.ShowDialogWindow(model, true, false, "30%", "10%") == true)
                return new Parameters
                {
                    FileName = (string)model[fFile.FieldName],
                    ABCD = (string)model[fABCD.FieldName],
                    MandantId = (decimal?)model[fMandant.FieldName]
                };

            return null;
        }

        private void LoadData(Parameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.FileName))
                throw new OperationException("Не выбран файл");

            if (!File.Exists(parameters.FileName))
                throw new OperationException("Не найден файл " + parameters.FileName);

            var lines = File.ReadLines(parameters.FileName).ToArray();
            var errors = new Dictionary<int, Exception>();
            for (int i = 0; i < lines.Length; i++)
            {
                var items = lines[i].Split(';');

                // получаем артикул
                Art art;
                try
                {
                    art = GetOrCreateArt(items[5], parameters);
                }
                catch (Exception ex)
                {
                    errors.Add(i, ex);
                    continue;
                }

                // 
            }
        }

        private Art GetOrCreateArt(string artCode, Parameters parameters)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Art>>())
            {
                var filter = string.Format("{0} = '{1}'", SourceNameHelper.Instance.GetPropertySourceName(typeof(Art), Art.ArtNamePropertyName), artCode);
                var arts = mgr.GetFiltered(filter).ToArray();
                if (arts.Length > 1)
                    throw new DeveloperException("Найдено более одного артикула с номером " + artCode);
                if (arts.Length == 1)
                    return arts[0];

                // создаем артикул
                var art = new Art();
                art.ArtName = artCode;
                art.MANDANTID = parameters.MandantId;
                art.ARTABCD = parameters.ABCD;
                mgr.Insert(ref art);
                return art;
            }
        }
    }
}