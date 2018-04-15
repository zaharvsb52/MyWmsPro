using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class IwbStickerActivity : NativeActivity<int>
    {
        [DisplayName(@"Приходная накладная")]
        public InArgument<IWB> Source { get; set; }

        public IwbStickerActivity()
        {
            DisplayName = "Стикеровка";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var iwb = Source.Get(context);
            if (iwb == null)
                throw new NullReferenceException("Source");

            var vs = IoC.Instance.Resolve<IViewService>();

            // проверим настройку параметров по манданту
            var cpvMgr = IoC.Instance.Resolve<ICustomParamManager>();
            var cpValid = cpvMgr.GetCPByInstance("IWB", iwb.GetKey().ToString(), FilterHelper.GetAttrEntity<CustomParam>(CustomParam.CustomParamCodePropertyName)).ToArray();
            var allow = cpValid.FirstOrDefault(i => i.GetKey().ToString().EqIgnoreCase("IWBStickerArtL1")) != null && cpValid.FirstOrDefault(i => i.GetKey().ToString().EqIgnoreCase("IWBStickerArtValueL2")) != null;
            if (!allow)
            {
                vs.ShowDialog(string.Format("Стикеровка по накладной '{0}'", iwb.GetKey()), string.Format("Для манданта '{0}' отсутствуют параметры стикеровки!", iwb.GetProperty(IWB.VMANDANTCODEPropertyName)), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information,
                    System.Windows.MessageBoxResult.OK);
                Result.Set(context, -1);
                return;
            }

            // получаем информацию по товарам

            var stickerList = new List<IwbStickerCpv>();

            cpValid = cpvMgr.GetCPByInstance("ART", iwb.GetKey().ToString(), FilterHelper.GetAttrEntity<CustomParam>(CustomParam.CustomParamCodePropertyName)).ToArray();
            allow = cpValid.FirstOrDefault(i => i.GetKey().ToString().EqIgnoreCase("ARTStickerNeedL2")) != null;

            if (allow)
            {
                DataTable table;
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                    table =
                        mgr.ExecuteDataTable(
                            string.Format(
                                "select ARTCODE_R, SUM(QTYSKU) as QTY from vwinput inner join wmscustomparamvalue on vwinput.artcode_r=wmscustomparamvalue.cpvkey where wmscustomparamvalue.cpv2entity='ART' and wmscustomparamvalue.customparamcode_r='ARTStickerNeedL2' and wmscustomparamvalue.cpvvalue=1 and iwbid_r = {0} group by ARTCODE_R",
                                iwb.GetKey()));
                var rows = table.Rows.Cast<DataRow>();
                foreach (var r in rows)
                {
                    decimal? cpvCode = null;
                    var artCode = r["ARTCODE_R"].ToString();
                    decimal? stickerCount = null;

                    var exist = iwb.CustomParamVal == null
                        ? null
                        : iwb.CustomParamVal.FirstOrDefault(
                            i => i.CustomParamCode.EqIgnoreCase("IWBStickerArtL1") && i.CPVValue.EqIgnoreCase(artCode));
                    if (exist != null)
                    {
                        cpvCode = exist.GetKey<decimal>();
                        var child =
                            iwb.CustomParamVal.FirstOrDefault(
                                i => i.CPVParent == cpvCode && i.CustomParamCode.EqIgnoreCase("IWBStickerArtValueL2"));
                        if (child != null)
                            stickerCount =
                                (decimal) SerializationHelper.ConvertToTrueType(child.CPVValue, typeof (decimal));
                    }
                    var productCount = (decimal) SerializationHelper.ConvertToTrueType(r["QTY"], typeof (decimal));
                    stickerList.Add(new IwbStickerCpv(cpvCode, artCode, productCount, stickerCount));
                }
            }

            if (iwb.CustomParamVal != null)
            {
                var parentIwb =
                    iwb.CustomParamVal.ToArray()
                        .Where(i => i.CPVParent == null && i.CustomParamCode.EqIgnoreCase("IWBStickerArtL1"));
                foreach (var parent in parentIwb)
                {
                    var exist = stickerList.FirstOrDefault(i => i.ArtCode.EqIgnoreCase(parent.CPVValue));
                    if (exist == null)
                    {
                        decimal? cpvCode = parent.CPVID;
                        var artCode = parent.CPVValue;
                        decimal? stickerCount = null;
                        var child =
                            iwb.CustomParamVal.FirstOrDefault(
                                i =>
                                    i.CPVParent == parent.CPVID &&
                                    i.CustomParamCode.EqIgnoreCase("IWBStickerArtValueL2"));
                        if (child != null)
                            stickerCount =
                                (decimal) SerializationHelper.ConvertToTrueType(child.CPVValue, typeof (decimal));
                        stickerList.Add(new IwbStickerCpv(cpvCode, artCode, null, stickerCount));
                    }
                }
            }
            var obj = new IwbStikerViewModel();
            obj.PanelCaption = string.Format("Стикеровка по накладной '{0}'", iwb.GetKey());
            obj.AllowEditing = true;
            obj.Source = new ObservableCollection<IwbStickerCpv>(stickerList);

            var result = vs.ShowDialogWindow(obj, true, false, "50%", "50%");

            // сохраняем
            if (!result.HasValue || !result.Value)
            {
                Result.Set(context, -1);
            }
            else
            {
                SaveStickerCpv(context, obj.Source);
                Result.Set(context, 0);
            }
        }

        private void SaveStickerCpv(NativeActivityContext context, ICollection<IwbStickerCpv> items)
        {
            if (items.Count == 0)
                return;

            var iwb = context.GetValue(this.Source);
            var parentId = -1;

            if (iwb.CustomParamVal == null)
                iwb.CustomParamVal = new WMSBusinessCollection<IWBCpv>();

            var mgr = IoC.Instance.Resolve<IBaseManager<IWBCpv>>();

            foreach (var item in items)
            {
                // проверяем есть ли у нас такой CPV
                var existItem = item.CpvCode == null ? null : iwb.CustomParamVal.FirstOrDefault(i => i.CPVID == item.CpvCode);
                // обновляем
                if (existItem != null)
                {
                    // значения нет - удаляем, есть - обновляем
                    if (!item.StickerCount.HasValue)
                    {
                        // ищем детей и удаляем
                        var child = iwb.CustomParamVal.Where(i => i.CPVParent == existItem.CPVID).ToArray();
                        mgr.Delete(child);

                        // удаляем родителя
                        mgr.Delete(existItem);
                    }
                    else
                    {
                        var child = iwb.CustomParamVal.FirstOrDefault(i => i.CPVParent == existItem.CPVID && i.CustomParamCode.EqIgnoreCase("IWBStickerArtValueL2"));
                        if (child == null)
                            continue;

                        child.CPVValue = SerializationHelper.GetCorrectStringValue(item.StickerCount);
                        mgr.Update(child);
                    }
                }
                // добавляем
                else
                {
                    if (item.StickerCount.HasValue)
                    {
                        var iwbId = SerializationHelper.GetCorrectStringValue(iwb.GetKey());

                        var cpvArt = new IWBCpv
                        {
                            CPVID = parentId,
                            CPV2Entity = "IWB",
                            CPVKey = iwbId,
                            CustomParamCode = "IWBStickerArtL1",
                            CPVValue = item.ArtCode
                        };
                        mgr.Insert(ref cpvArt);

                        var cpvCount = new IWBCpv
                        {
                            CPVParent = cpvArt.CPVID,
                            CPV2Entity = "IWB",
                            CPVKey = iwbId,
                            CustomParamCode = "IWBStickerArtValueL2",
                            CPVValue = SerializationHelper.GetCorrectStringValue(item.StickerCount)
                        };
                        mgr.Insert(ref cpvCount);
                    }
                    parentId -= 1;
                }
            }
        }
    }
}
