using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Acceptance.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.Acceptance.ViewModels
{
    [View(typeof(AcceptanceWorkingView))]
    public class AcceptanceWorkingViewModel : CustomObjectListViewModelBase<AcceptanceWorkingInfo>
    {
        private readonly AcceptanceViewModel _acceptanceViewModel;
        private decimal? _posID;

        public EditableBusinessObject ParentViewModelSource { get; set; }

        public AcceptanceWorkingViewModel(AcceptanceViewModel acceptanceViewModel)
        {
            _acceptanceViewModel = acceptanceViewModel;
            //var workings = RefreshWorking();
            RefreshData();
            //Source = new ObservableCollection<AcceptanceWorkingInfo>(workings);
        }

        protected override ObservableCollection<DataField> GetDataFields()
        {
            var res = base.GetDataFields();
            res.Add(new DataField
            {
                Name = wmsMLC.DCL.Content.ViewModels.InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                BindingPath = wmsMLC.DCL.Content.ViewModels.InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                SourceName = wmsMLC.DCL.Content.ViewModels.InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                FieldName = wmsMLC.DCL.Content.ViewModels.InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                FieldType = typeof(string),
                Caption = StringResources.Operation
            });
            return res;
        }

        public override void RefreshData()
        {
            Source = new ObservableCollection<AcceptanceWorkingInfo>(RefreshWorking());
        }

        private List<AcceptanceWorkingInfo> RefreshWorking()
        {
            if (!_posID.HasValue)
            {
                var posList = _acceptanceViewModel.Source as IEnumerable<IWBPosInput>;
                if (posList == null)
                    return new List<AcceptanceWorkingInfo>();

                var pos = posList.FirstOrDefault(i => i.IWBPosId > 0);
                if (pos == null)
                    return new List<AcceptanceWorkingInfo>();

                _posID = pos.GetKey<decimal>();
            }

            List<Working> workingList;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                workingList = mgr.GetFiltered(string.Format("workid_r in (select w2e.workid_r from wmswork2entity w2e " +
                                                            "join wmsiwb2cargo i2c on TO_CHAR(i2c.cargoiwbid_r) = w2e.work2entitykey " +
                                                            "join wmsiwbpos ip on ip.iwbid_r = i2c.iwbid_r where w2e.work2entityentity = 'CARGOIWB' " +
                                                            "and ip.iwbposid = {0})", _posID), GetModeEnum.Partial).ToList();
            }

            if (!workingList.Any())
                return new List<AcceptanceWorkingInfo>();

            var workIds = workingList.Where(p => p.WORKID_R.HasValue).Select(p => p.WORKID_R.Value).Distinct().ToArray();
            var works = new Dictionary<decimal, Work>();
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                foreach (var workid in workIds)
                {
                    works[workid] = mgr.Get(workid, GetModeEnum.Partial);
                }
            }
            return workingList.Where(p => p.WORKID_R.HasValue).Select(p => new AcceptanceWorkingInfo(p, works[p.WORKID_R.Value].Get<string>("VOPERATIONNAME"))).ToList();
        }
    }
}