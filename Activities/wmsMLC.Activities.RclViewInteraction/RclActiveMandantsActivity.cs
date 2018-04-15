using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclActiveMandantsActivity : NativeActivity
    {
        #region .  Fields  .

        //private const string IsSelectedPropertyName = "IsSelected";
        private const string FieldName = "ActiveMandant";

        private NativeActivityContext _context;

        //private List<MandantWithActive> _activeMandantList;
        private List<User2Mandant> _user2MandantList;

        #endregion .  Fields  .

        #region .ctors .

        public RclActiveMandantsActivity()
        {
            DisplayName = "ТСД: Выбор актив. мандантов";
        }

        #endregion .ctors .

        #region .  Properties  .

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        #endregion .  Properties  .

        #region .  Methods  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            _context = context;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<User2Mandant>>())
            {
                _user2MandantList = mgr.GetFiltered(string.Format("USERCODE_R = '{0}'", WMSEnvironment.Instance.AuthenticatedUser.GetSignature())).ToList();
            }

            _user2MandantList.Sort((x, y) => string.Compare(x.MandantCode, y.MandantCode, StringComparison.Ordinal));
            if (!ShowSelectMandantDialog())
                return;

            UpdateData();
        }

        private void UpdateData()
        {
            var updateList = _user2MandantList.Where(item => item.IsDirty).ToList();
            using (var bpManager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                foreach (var m in updateList)
                {
                    bpManager.UpdateEntity(m);
                }
            }
        }
        
        private bool ShowSelectMandantDialog()
        {
            var footerMenu = CreateDialogFooterMenu();
            var model = CreateDialogModel(footerMenu);
            var viewService = IoC.Instance.Resolve<IViewService>();

            footerMenu[2].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() =>
            {
                ChangeActiveAll(model, false);
            }));

            footerMenu[3].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() =>
            {
                ChangeActiveAll(model, true);
            }));

            footerMenu[1].Set(ValueDataFieldConstants.Command, new DelegateCustomCommand(() =>
            {
                ChangeActive(model);
            }));

            while (true)
            {
                if (viewService.ShowDialogWindow(model, false) != true)
                    return false;

                switch (model.MenuResult)
                {
                    case "Value": 
                    case "Return":
                        return true;
                    case "F1":
                        ChangeActiveAll(model, false);
                        ChangeActive(model);
                        return true;
                    case "F5":
                        if (!model.ContainsKey(ValueDataFieldConstants.Properties)) continue;
                        var properties = (IDictionary<string, object>)model[ValueDataFieldConstants.Properties];
                        if (properties.ContainsKey(ValueDataFieldConstants.ShowAutoFilterRow))
                        {
                            model.GetField(FieldName).Set(ValueDataFieldConstants.ShowAutoFilterRow, !properties[ValueDataFieldConstants.ShowAutoFilterRow].To(false));
                        }
                        continue;
                    default:
                        return false;
                }
            }
        }

        private void ChangeActiveAll(DialogSourceViewModel model, bool isValue)
        {
            foreach (var m in _user2MandantList)
            {
                m.User2MandantIsActive = isValue;
            }

        }

        private void ChangeActive(DialogSourceViewModel model)
        {
            var id = model[FieldName] as decimal?;
            if (!id.HasValue)
                return;
            var row = _user2MandantList.FirstOrDefault(p => p.GetKey<decimal>() == id.Value);
            if (row == null)
                return;
            row.User2MandantIsActive = !row.User2MandantIsActive;
        }
        
        private ValueDataField[] CreateDialogFooterMenu()
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Задать",
                Value = "F1"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenuItem.Set(ValueDataFieldConstants.IsNotMenuButton, true);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Изменить",
                Value = "F2"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenuItem.Set(ValueDataFieldConstants.IsNotMenuButton, true);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu2",
                Caption = "Снять все",
                Value = "F3"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenuItem.Set(ValueDataFieldConstants.IsNotMenuButton, true);            
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu3",
                Caption = "Выбрать все",
                Value = "F4"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 1);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu4",
                Caption = "Фильтр",
                Value = "F5"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu5",
                Caption = "Применить",
                Value = "Enter"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 2);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);
            return footerMenu.ToArray();
        }

        private DialogSourceViewModel CreateDialogModel(ValueDataField[] footerMenu)
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = "Активные манданты",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = true,
            };

            var field = new ValueDataField
            {
                Name = FieldName,
                FieldType = typeof (User2Mandant),
                Caption = string.Empty,
                LabelPosition = "Left",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true
            };
            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            field.Set(ValueDataFieldConstants.ShowTotalRow, false);
            field.Set(ValueDataFieldConstants.ShowControlMenu, false);
            field.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, true);
            field.Set(ValueDataFieldConstants.ShowAutoFilterRow, false);
            field.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, true);
            
            //Получаем поля для грида
            var names = new[] { User2Mandant.User2MandantIsActivePropertyName, User2Mandant.MandantCodePropertyName, User2Mandant.MandantNamePropertyName, User2Mandant.MandantIDPropertyName };
            var fieldList = names.Select(propertyName => DataFieldHelper.Instance.GetDataFields(typeof(User2Mandant), SettingDisplay.List).ToList().FirstOrDefault(p => p.Name.EqIgnoreCase(propertyName)))
                    .Where(f => f != null)
                    .ToArray();


            fieldList[0].Set(ValueDataFieldConstants.ColumnWidth, "60");
            fieldList[1].Set(ValueDataFieldConstants.ColumnWidth, "100");
            fieldList[2].Set(ValueDataFieldConstants.ColumnWidth, "250");

            field.Set(ValueDataFieldConstants.Fields, fieldList);
            field.Set(ValueDataFieldConstants.ItemsSource, _user2MandantList);

            if (footerMenu != null)
                field.Set(ValueDataFieldConstants.FooterMenu, footerMenu);

            field.FieldName = field.Name;
            field.SourceName = field.Name;
            result.Fields.Add(field);

            result.UpdateSource();
            return result;
        }
        
        #endregion .  Methods  .
    }
}