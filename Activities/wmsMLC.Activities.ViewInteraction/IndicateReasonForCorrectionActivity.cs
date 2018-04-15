using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
    public class IndicateReasonForCorrectionActivity : NativeActivity<int>
    {
        #region .  Properties  .
        [DisplayName(@"Приходная накладная")]
        public InArgument<IWB> Source { get; set; }
        #endregion

        #region .  Consts  .
        private const string ReasonCpvCode = "IWBPosAdjustmentReason";
        private const string ReasonDescCpvCode = "IWBPosAdjustmentReasonDesc";
        #endregion

        #region . Methods  .

        public IndicateReasonForCorrectionActivity()
        {
            DisplayName = "Указать причину корректировки";
        }

        private void RegEvent(decimal? iwbPosId)
        {
            DateTime now;
            using (var bpProcManager = IoC.Instance.Resolve<IBPProcessManager>())
                now = bpProcManager.GetSystemDate();

            var eventHeader = new EventHeader
            {
                EventKindCode = "IWBPOS_CORRECTED",
                OperationCode = "OP_CORRECTION_REASON",
                StartTime = now,
                Instance = "none"
            };

            var evDetail = new EventDetail();
            evDetail.SetProperty("IWBPOSID_R", iwbPosId);

            using (var eventHeaderMgr = IoC.Instance.Resolve<IEventHeaderManager>())
                eventHeaderMgr.RegEvent(ref eventHeader, evDetail);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var iwb = Source.Get(context);

            #region .  Checks  .

            if (iwb == null)
                throw new NullReferenceException("Source");

            var vs = IoC.Instance.Resolve<IViewService>();

            if (iwb.GetProperty(IWB.STATUSCODE_RPropertyName).ToString() == "IWB_CANCELED")
            {
                vs.ShowDialog(string.Format("Указать причину корректировки"),
                    string.Format("Накладная {0} имеет неверный статус 'Отменена'", iwb.GetKey()),
                    MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK);
            }

            // получаем информацию по позициям
            List<IWBPos> iwbPosList;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>())
                iwbPosList =
                    mgr.GetFiltered(
                        string.Format(" IWBID_R = {0} and nvl(IWBPOSPRODUCTCOUNT,-999) <> nvl(IWBPOSCOUNT,-999) ",
                            iwb.GetKey())).ToList();

            if (iwbPosList.Count == 0)
            {
                vs.ShowDialog(string.Format("Указать причину корректировки"),
                    string.Format("Накладная {0} не имеет позиций с расхождениями", iwb.GetKey()),
                    MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK);
                return;
            }

            var checkIwbPos = iwbPosList.FirstOrDefault();

            // проверим настройку параметров по манданту
            if (checkIwbPos != null)
            {
                var cpvMgr = IoC.Instance.Resolve<ICustomParamManager>();
                var cpValid =
                    cpvMgr.GetCPByInstance("IWBPOS", checkIwbPos.IWBPosID.ToString(),
                        FilterHelper.GetAttrEntity<CustomParam>(CustomParam.CustomParamCodePropertyName)).ToArray();
                var allow = cpValid.FirstOrDefault(i => i.GetKey().ToString().EqIgnoreCase(ReasonCpvCode)) !=
                            null &&
                            cpValid.FirstOrDefault(i => i.GetKey().ToString().EqIgnoreCase(ReasonDescCpvCode)) !=
                            null;
                if (!allow)
                {
                    vs.ShowDialog("Указать причину корректировки",
                        string.Format(
                            "Для манданта '{0}' не настроены пользовательские параметры  'Причина корректировки' и/или 'Описание причины'",
                            iwb.GetProperty(IWB.VMANDANTCODEPropertyName)),
                        MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK);
                    return;
                }
            }

            #endregion

            //prepare data for grid
            var iwbPosListWithCpv =
                iwbPosList.Select(
                    iwbPos =>
                    {
                        var customParamVal = iwbPos.CustomParamVal;
                        var iwbPosAdj = customParamVal != null
                            ? iwbPos.CustomParamVal.FirstOrDefault(i => i.CustomParamCode == ReasonCpvCode)
                            : null;
                        var iwbPosAdjDesc = customParamVal != null
                            ? iwbPos.CustomParamVal.FirstOrDefault(
                                i => i.CustomParamCode == ReasonDescCpvCode)
                            : null;

                        return new IwbPosWithCpvGrd(iwbPos,
                            iwbPosAdj != null ? decimal.Parse(iwbPosAdj.CPVValue) : (decimal?) null,
                            iwbPosAdjDesc != null ? iwbPosAdjDesc.CPVValue : null
                            );
                    }).ToList();

            var obj = new IndicateReasonForCorrectionViewModel
            {
                PanelCaption = string.Format("Указать причину корректироки по накладной '{0}'", iwb.GetKey()),
                AllowEditing = true,
                Source = new ObservableCollection<IwbPosWithCpvGrd>(iwbPosListWithCpv)
            };

            var result = vs.ShowDialogWindow(obj, true, false, "50%", "50%");

            // сохраняем
            if (!result.HasValue || !result.Value)
            {
                Result.Set(context, -1);
            }
            else
            {
                SaveIwbposCpv(context, obj.Source);
                Result.Set(context, 0);
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));

            metadata.SetArgumentsCollection(collection);
        }

        private void SaveIwbposCpv(NativeActivityContext context, ICollection<IwbPosWithCpvGrd> items)
        {
            if (items.Count == 0)
                return;

            var iwb = context.GetValue(this.Source);
            var mgr = IoC.Instance.Resolve<IBaseManager<IWBPos>>();
            var mgrCpvIwbPos = IoC.Instance.Resolve<IBaseManager<IWBPosCpv>>();
            var iwbPosList = mgr.GetFiltered(
                string.Format(" IWBID_R = {0} and nvl(IWBPOSPRODUCTCOUNT,-999) <> nvl(IWBPOSCOUNT,-999) ",
                    iwb.GetKey())).ToList();

            foreach (var iwbPoseItem in iwbPosList)
            {
                var processedIwbPos = items.FirstOrDefault(i => i.IwbPosId == iwbPoseItem.IWBPosID);
                var needRegEvent = false;

                if (processedIwbPos == null)
                    continue;

                if (iwbPoseItem.CustomParamVal == null)
                    iwbPoseItem.CustomParamVal = new WMSBusinessCollection<IWBPosCpv>();
                
                //get cpv
                var existReasonCpv = iwbPoseItem.CustomParamVal.FirstOrDefault(c => c.CustomParamCode == ReasonCpvCode);
                var existReasonDescCpv = iwbPoseItem.CustomParamVal.FirstOrDefault(c => c.CustomParamCode == ReasonDescCpvCode);
                var corrReasonCpvValue = SerializationHelper.GetCorrectStringValue(processedIwbPos.IWBPosAdjustmentReason);
                var corrReasonDescCpvValue = SerializationHelper.GetCorrectStringValue(processedIwbPos.IWBPosAdjustmentReasonDesc);

                //update
                if (existReasonCpv != null )
                {
                    if (corrReasonCpvValue == null)
                    {
                        //delete child
                        if (existReasonDescCpv != null)
                        mgrCpvIwbPos.Delete(existReasonDescCpv);
                        //delete parent
                        mgrCpvIwbPos.Delete(existReasonCpv);
                        RegEvent(iwbPoseItem.IWBPosID);
                        continue;
                    }

                    if (corrReasonDescCpvValue == null && existReasonDescCpv != null)
                    {
                        //delete child only
                        mgrCpvIwbPos.Delete(existReasonDescCpv);
                        needRegEvent = true;
                    }
                    
                    if (existReasonCpv.CPVValue != corrReasonCpvValue)
                    {
                        //update parent
                        existReasonCpv.CPVValue = corrReasonCpvValue;
                        mgrCpvIwbPos.Update(existReasonCpv);
                        needRegEvent = true;
                    }

                    if (existReasonDescCpv != null && corrReasonDescCpvValue != existReasonDescCpv.CPVValue)
                    {
                        //update child
                        existReasonDescCpv.CPVValue = corrReasonDescCpvValue;
                        mgrCpvIwbPos.Update(existReasonDescCpv);
                        needRegEvent = true;
                    }

                    if (existReasonDescCpv == null && corrReasonDescCpvValue != null)
                    {
                        //insert child, if exists only parent
                        var iwbPosCpvIns = new IWBPosCpv()
                        {
                            CPVParent = existReasonCpv.CPVID,
                            CPV2Entity = "IWBPOS",
                            CPVKey = processedIwbPos.IwbPosId.ToString(),
                            CustomParamCode = ReasonDescCpvCode,
                            CPVValue = corrReasonDescCpvValue
                        };
                        mgrCpvIwbPos.Insert(ref iwbPosCpvIns);
                        needRegEvent = true;
                    }
                }

                //insert
                if (existReasonCpv == null && processedIwbPos.IWBPosAdjustmentReason != null)
                {
                    //insert parent
                    var iwbPosCpvIns = new IWBPosCpv()
                    {
                        CPVID = -1,
                        CPV2Entity = "IWBPOS",
                        CPVKey = processedIwbPos.IwbPosId.ToString(),
                        CustomParamCode = ReasonCpvCode,
                        CPVValue = corrReasonCpvValue
                    };
                    mgrCpvIwbPos.Insert(ref iwbPosCpvIns);

                    //insert child
                    var iwbPosCpvInsDesc = new IWBPosCpv()
                    {
                        CPVParent = iwbPosCpvIns.CPVID,
                        CPV2Entity = "IWBPOS",
                        CPVKey = processedIwbPos.IwbPosId.ToString(),
                        CustomParamCode = ReasonDescCpvCode,
                        CPVValue = corrReasonDescCpvValue
                    };
                    mgrCpvIwbPos.Insert(ref iwbPosCpvInsDesc);
                    needRegEvent = true;
                }

                if (needRegEvent)
                    RegEvent(iwbPoseItem.IWBPosID);
            }
        }

        #endregion
    }
}