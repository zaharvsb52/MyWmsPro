using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Grid;
using DevExpress.XtraEditors.DXErrorProvider;
using log4net;
using wmsMLC.DCL.Configurator.ViewModels;
using wmsMLC.General;

namespace wmsMLC.DCL.Configurator.Views
{
    public class PmMethodCellTemplateSelector : DataTemplateSelector
    {
        private ILog _log = LogManager.GetLogger(typeof(PmMethodCellTemplateSelector));

        public DataTemplate NotAllowedPmMethodsCellTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try
            {
                var cellData = item as GridCellData;
                if (cellData?.RowData?.RowHandle != null && cellData.RowData.RowHandle.Value != DataControlBase.AutoFilterRowHandle && cellData.RowData?.Row != null)
                {
                    var column = cellData.Column;
                    if (column != null)
                    {
                        var row = cellData.RowData.Row as PmConfiguratorData;
                        if (!string.IsNullOrEmpty(column.FieldName) && row?.Owner != null)
                        {
                            if (!row.Owner.ValidatePmMethods(column.FieldName, row))
                            {
                                if (IsNotAccessible(cellData.Value as List<object>))
                                    return NotAllowedPmMethodsCellTemplate;

                                var validationError = BaseEdit.GetValidationError(cellData);
                                if (validationError == null)
                                {
                                    var newError =
                                        new GridCellValidationError(
                                            Properties.Resources.PmMethodIsUnavailableButExists,
                                            null,
                                            ErrorType.Critical, cellData.RowData.RowHandle.Value,
                                            (GridColumn) column);
                                    BaseEditHelper.SetValidationError(cellData, newError);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WarnFormat("Error in PmMethodCellTemplateSelector {0}", ExceptionHelper.ExceptionToString(ex));
                _log.Debug(ex);
            }
            return base.SelectTemplate(item, container);
        }

        private bool IsNotAccessible(IList<object> values)
        {
            if (values == null || !values.Any())
                return true;

            return values.Count == 1 && values.Any(p => Properties.Resources.PmMethodIsUnavailable.EqIgnoreCase(p.To<string>()));
        }
    }
}
