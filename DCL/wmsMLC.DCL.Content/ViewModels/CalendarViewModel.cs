using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class CalendarViewModel : ObjectViewModelBase<Calendar>
    {
        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(Calendar.CALENDARDATEPropertyName))
                return;

            if (Source.CalendarDate == null) 
                return;

            var dateFrom = Source.CalendarDate.Value.Date;
            var dateTill = Source.CalendarDate.Value.Date;

            var timeFrom = Source.CalendarTimeFrom.TimeOfDay;
            var timeTill = Source.CalendarTimeTill.TimeOfDay;

            dateFrom += timeFrom;
            dateTill += timeTill;

            Source.CalendarTimeFrom = dateFrom;
            Source.CalendarTimeTill = dateTill;
        }

        protected override void OnSave()
        {
            if (Source.CalendarDate != null && Source.CalendarDate.Value != null)
            {
                System.DateTime date;
                System.TimeSpan time;

                if (Source.CalendarDate.Value.Day != Source.CalendarTimeFrom.Day)
                {
                    date = Source.CalendarDate.Value.Date;
                    time = Source.CalendarTimeFrom.TimeOfDay;
                    date += time;
                    Source.CalendarTimeFrom = date;
                }

                if (Source.CalendarDate.Value.Day != Source.CalendarTimeTill.Day)
                {
                    date = Source.CalendarDate.Value.Date;
                    time = Source.CalendarTimeTill.TimeOfDay;
                    date += time;
                    Source.CalendarTimeTill = date;
                }
            }

            base.OnSave();
        }
    }
}