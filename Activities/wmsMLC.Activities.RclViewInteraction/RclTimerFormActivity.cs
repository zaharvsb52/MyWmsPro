using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclTimerFormActivity : NativeActivity
    {
        #region .  Fields  .
        private const string TimerFieldName = "fldTimer";
        private const string DefaultDateTimeFormat = "dd.MM.yy HH:mm:ss";

        private NativeActivityContext _context;
        #endregion .  Fields  .

        public RclTimerFormActivity()
        {
            FontSize = 14;
            DisplayName = "ТСД: форма таймера";
            DialogTitle = null;
            Message = null;
            DateFrom = (DateTime?) null;
            TimerOffset = 0;
        }

        #region .  Properties  .

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Заголовок диалога")]
        public InArgument<string> DialogTitle { get; set; }

        [DisplayName(@"Сообщение")]
        public InArgument<string> Message { get; set; }

        [DisplayName(@"Дата с")]
        public InArgument<DateTime?> DateFrom { get; set; }

        public InArgument<double> TimerOffset { get; set; }

        public OutArgument<TimeSpan> TimerValue { get; set; }

        #endregion .  Properties  .

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DialogTitle, type.ExtractPropertyName(() => DialogTitle));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Message, type.ExtractPropertyName(() => Message));
            ActivityHelpers.AddCacheMetadata(collection, metadata, DateFrom, type.ExtractPropertyName(() => DateFrom));
            ActivityHelpers.AddCacheMetadata(collection, metadata, TimerOffset, type.ExtractPropertyName(() => TimerOffset));
            ActivityHelpers.AddCacheMetadata(collection, metadata, TimerValue, type.ExtractPropertyName(() => TimerValue));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            _context = context;
            ShowTimerForm();
        }

        private void ShowTimerForm()
        {
            var model = GetTimerFormModel();
            var field = model.GetField(TimerFieldName);
            var timervalue = field == null || field.Value == null ? TimeSpan.FromSeconds(0) : (TimeSpan)field.Value;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            timer.Tick += delegate
            {
                if (field != null)
                {
                    timervalue = timervalue.Add(timer.Interval);
                    model[TimerFieldName] = timervalue;
                }
            };

            while (true)
            {
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    continue;

                if (menuResult == "1Return")
                {
                    timer.IsEnabled = false;
                    if (TimerValue != null && field != null)
                        TimerValue.Set(_context, field.Value == null ? TimeSpan.FromSeconds(0) : (TimeSpan)field.Value);
                    break;
                }
            }
        }

        private DialogSourceViewModel GetTimerFormModel()
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = DialogTitle.Get(_context),
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var footerMenu = new List<ValueDataField>();
            var menuNext = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Далее",
                Value = Key.Enter.ToString()
            };
            menuNext.Set(ValueDataFieldConstants.Row, 0);
            menuNext.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(menuNext);

            ValueDataField field;
            var message = Message.Get(_context);
            if (!string.IsNullOrEmpty(message))
            {
                field = new ValueDataField
                {
                    Name = "txtMessage",
                    FieldType = typeof(string),
                    LabelPosition = "None",
                    IsEnabled = false,
                    Value = message
                };
                field.FieldName = field.Name;
                field.SourceName = field.Name;
                result.Fields.Add(field);
            }

            var dateFrom = DateFrom.Get(_context);
            if (dateFrom.HasValue)
            {
                field = new ValueDataField
                {
                    Name = "dtDateFrom",
                    Caption = "С",
                    FieldType = typeof(DateTime),
                    LabelPosition = "Left",
                    Value = dateFrom.Value,
                    DisplayFormat = DefaultDateTimeFormat,
                    IsEnabled = false,
                    SetFocus = false,
                    CloseDialog = false
                };
                field.FieldName = field.Name;
                field.SourceName = field.Name;
                result.Fields.Add(field);
            }

            var timevalue = TimeSpan.FromSeconds(dateFrom.HasValue ? (DateTime.Now - dateFrom.Value).TotalSeconds - TimerOffset.Get(_context) : 0);
            field = new ValueDataField
            {
                Name = TimerFieldName,
                Caption = "Таймер",
                FieldType = typeof(TimeSpan),
                LabelPosition = "Left",
                DisplayFormat = "hh\\:mm\\:ss",
                Value = timevalue,
                IsEnabled = false,
                SetFocus = false,
                CloseDialog = false
            };
            field.FieldName = field.Name;
            field.SourceName = field.Name;
            result.Fields.Add(field);

            var fieldFooterMenu = new ValueDataField
            {
                Name = "footerMenu",
                FieldType = typeof(IFooterMenu)
            };
            fieldFooterMenu.FieldName = fieldFooterMenu.Name;
            fieldFooterMenu.SourceName = fieldFooterMenu.Name;
            fieldFooterMenu.Properties["FooterMenu"] = footerMenu.ToArray();
            result.Fields.Add(fieldFooterMenu);

            result.UpdateSource();
            return result;
        }

        #endregion .  Methods  .
    }
}
