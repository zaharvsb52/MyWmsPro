using System;
using System.Collections.Generic;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class PlaceViewModel : ObjectViewModelBase<Place>
    {
        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null || !Source.IsNew)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(Place.SegmentCodePropertyName) &&
                !e.PropertyName.EqIgnoreCase(Place.PlaceTypeCodePropertyName))
                return;

            var mgr = (IPlaceManager) GetManager();
            if (e.PropertyName.EqIgnoreCase(Place.PlaceTypeCodePropertyName))
            {
                mgr.FillFromPlaceType(Source);
                return;
            }

            if (!mgr.FillPlaceCode(Source))
                return;

            var placepk = Source.GetPrimaryKeyPropertyName();
            var fv = FormulaValues as IDictionary<string, Object>;
            if (fv.ContainsKey(placepk))
                fv[placepk] = Source.PlaceCode;
            else
                fv.Add(placepk, Source.PlaceCode);
            OnFormulaStateChanged(placepk);

            if (fv.ContainsKey(Place.PlaceNamePropertyName))
                fv[Place.PlaceNamePropertyName] = Source.PlaceName;
            else
                fv.Add(Place.PlaceNamePropertyName, Source.PlaceName);
            OnFormulaStateChanged(Place.PlaceNamePropertyName);
        }

        protected override void BeforeValidationFormulaValue(IEnumerable<Place> items)
        {
            base.BeforeValidationFormulaValue(items);
            if (items == null)
                return;

            var mgr = (IPlaceManager) GetManager();
            mgr.UpdateFormulasGroupProperties(items);
        }
    }
}