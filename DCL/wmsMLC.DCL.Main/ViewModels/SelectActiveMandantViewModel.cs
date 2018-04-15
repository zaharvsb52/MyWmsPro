using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class SelectActiveMandantViewModel : PanelViewModelBase
    {
        public bool IsSave { get; private set; }

        private ObservableCollection<User2Mandant> _mandants;
        public ObservableCollection<User2Mandant> Mandants
        {
            get { return _mandants; }
            set
            {
                _mandants = value;
                OnPropertyChanged("Mandants");
            }
        }

        private User2Mandant _selectedMandant;

        public User2Mandant SelectedMandant
        {
            get { return _selectedMandant; }
            set
            {
                _selectedMandant = value;
                OnPropertyChanged("SelectedMandant");
            }
        }
        public ObservableCollection<DataField> Fields { get; private set; }
        public ICommand ChangeAllMandantCommand { get; private set; }
        public ICommand OnlyCurrentMandantCommand { get; private set; }
        public ICommand ActionAcceptCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        
        public SelectActiveMandantViewModel()
        {
            ChangeAllMandantCommand = new DelegateCustomCommand<bool?>(ChangeAllMandant);
            OnlyCurrentMandantCommand = new DelegateCustomCommand<User2Mandant>(OnlyCurrentMandant);
            ActionAcceptCommand = new DelegateCustomCommand(OnSave);
            EditCommand = new DelegateCustomCommand(OnEdit);

            SelectedMandant = null;
            Mandants = new ObservableCollection<User2Mandant>();

            var names = new[] { User2Mandant.User2MandantIsActivePropertyName, User2Mandant.MandantCodePropertyName, User2Mandant.MandantNamePropertyName };
           
            Fields = new ObservableCollection<DataField>(names.Select(propertyName =>
                    DataFieldHelper.Instance.GetDataFields(typeof (User2Mandant), SettingDisplay.List)
                        .ToList()
                        .FirstOrDefault(p => p.Name.EqIgnoreCase(propertyName)))
                .Where(f => f != null).ToList());
            Fields[0].AllowAddNewValue = true;

            GetMandants();
            PanelCaption = StringResources.SelectActiveMandant;
            AddMenu();
        }
      
        private void OnEdit()
        {
            if (SelectedMandant == null)
                return;

            var man = Mandants.First(m => m.MandantID == SelectedMandant.MandantID);
            man.User2MandantIsActive = !SelectedMandant.User2MandantIsActive;
        }

        private void AddMenu()
        {
            //IsMenuEnable = true;
            //IsCustomizeBarEnabled = true;
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1, "BarItemCommands");
            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.SelectCurrent,
                Command = OnlyCurrentMandantCommand,
                CommandParameter = SelectedMandant,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.SelectAll,
                Command = ChangeAllMandantCommand,
                CommandParameter = true,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 20
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.UnSelectAll,
                Command = ChangeAllMandantCommand,
                CommandParameter = false,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 30
            });
        }

        private void GetMandants()
        {
            try
            {
                WaitIndicatorVisible = true;

                var user = WMSEnvironment.Instance.AuthenticatedUser;

                Mandants.Clear();
                if (user == null)
                    return;

                using (var mgr = IoC.Instance.Resolve<IBaseManager<User2Mandant>>())
                {
                    var res =
                        mgr.GetFiltered(string.Format("USERCODE_R = '{0}'",
                            WMSEnvironment.Instance.AuthenticatedUser.GetSignature())).ToArray();
                    if (!res.Any())
                        return;

                    Array.Sort(res, (x, y) => string.Compare(x.MandantCode, y.MandantCode, StringComparison.Ordinal));
                    Mandants.AddRange(res);
                }
            }
            finally
            {
                WaitIndicatorVisible = false;
            }
        }
        
        private void ChangeAllMandant(bool? active)
        {
            if (!active.HasValue)
                return;

            foreach (var m in Mandants)
            {
                m.User2MandantIsActive = active.Value;
            }
        }

        private void OnlyCurrentMandant(User2Mandant mandant)
        {
            if (SelectedMandant == null) 
                return;

            try
            {
                WaitStart();

                ChangeAllMandant(false);
                SelectedMandant.User2MandantIsActive = true;
                OnSave();
            }
            finally
            {
                WaitStop();
            }
        }

        private void OnSave()
        {
            if (!Mandants.Any())
                return;

            try
            {
                WaitStart();

                Close();

                var saveArrey = Mandants.Where(m => m.IsDirty);

                using (var bpManager = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    foreach (var m in saveArrey)
                    {
                        bpManager.UpdateEntity(m);
                    }
                }

                IsSave = true;
            }
            finally
            {
                WaitStop();
            }
        }

        private void Close()
        {
            var service = IoC.Instance.Resolve<IViewService>();
            DispatcherHelper.Invoke(new Action(() => service.Close(this, true)));
        }

        protected override void Dispose(bool disposing)
        {
            if (Mandants != null)
                Mandants.Clear();

            if (Fields != null)
                Fields.Clear();
            
            SelectedMandant = null;
            base.Dispose(disposing);
        }

    }
}