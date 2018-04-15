using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(CargoIWBPosObjectView))]
    public class CargoIWBPosViewModel : ObjectViewModelBase<CargoIWBPos>
    {
        public static bool IsCheckedFact{get; set; }
        public bool IsNewProp { get { return IsNew(); } }

        protected override void MgrInsert(ref IEnumerable<CargoIWBPos> entities)
        {
            base.MgrInsert(ref entities);
    
            if  (!IsCheckedFact || entities.FirstOrDefault().CargoIwbPosType != "CLIENT") 
                return;

            foreach (var newItem in entities)
            {
                newItem.CargoIwbPosType = "FACT";
            }
            base.MgrInsert(ref entities);
        }

        private bool IsNew()
        {
            var obj = Source as IIsNew;
            return obj != null && obj.IsNew;
        }

        protected override void MgrInsert(ref CargoIWBPos entitie)
        {
            var factIWBPos = (CargoIWBPos)entitie.Clone();
            base.MgrInsert(ref entitie);

            if (!IsCheckedFact || entitie.CargoIwbPosType != "CLIENT") 
                return;
            factIWBPos.CargoIwbPosType = "FACT";
            base.MgrInsert(ref factIWBPos);
        }

        protected override bool Save()
        {
            var result = base.Save();
            if (!result || Source == null || InPropertyEditMode || !Source.IWBID_R.HasValue)
                return result;

            try
            {
                WaitStart();

                //Привязать накладную к грузу
                using (var mgrIwb2Cargo = IoC.Instance.Resolve<IBaseManager<IWB2Cargo>>())
                {
                    if (mgrIwb2Cargo.GetFiltered(
                        string.Format("IWBID_R = {0} AND CARGOIWBID_R = {1}", Source.IWBID_R, Source.CARGOIWBID_R),
                        FilterHelper.GetAttrEntity(typeof (IWB2Cargo), new IWB2Cargo().GetPrimaryKeyPropertyName()))
                        .ToArray()
                        .Length == 0)
                    {
                        var iwb2Cargonew = mgrIwb2Cargo.New();
                        iwb2Cargonew.SetProperty(IWB2Cargo.IWB2CARGOIWBIDPropertyName, Source.IWBID_R);
                        iwb2Cargonew.SetProperty(IWB2Cargo.IWB2CARGOCARGOIWBIDPropertyName, Source.CARGOIWBID_R);
                        mgrIwb2Cargo.Insert(ref iwb2Cargonew);
                        mgrIwb2Cargo.RiseManagerChanged();
                    }
                }

                using (var mgrIwb = IoC.Instance.Resolve<IBaseManager<IWB>>())
                {
                    mgrIwb.RiseManagerChanged();
                }

                return true;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, string.Format(ExceptionResources.CargoIwb2IwbErrorFormat, Source.CARGOIWBID_R)))
                    throw;
                return false;
            }
            finally
            {
                WaitStop();
            }
        }
    }
}