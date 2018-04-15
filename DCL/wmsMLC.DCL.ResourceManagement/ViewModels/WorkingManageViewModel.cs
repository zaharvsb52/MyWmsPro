using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.ResourceManagement.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.ResourceManagement.ViewModels
{
    [View(typeof(WorkingManageView))]
    public class WorkingManageViewModel : PanelViewModelBase, IWorkingManageViewModel
    {
        private bool _isFilterVisible;
        private DateTime _startTime;
        private bool _showBorder;
        private string _description;
        private string _placeInformation;

        #region .  Properties  .
        public ObservableCollection<AppointmentModel> Appointments { get; private set; }
        public ObservableCollection<LabelModel> Labels { get; set; }

        public ObservableCollection<ResourceModel> Resources { get; private set; }
        public ObservableCollection<OperationModel> Operations { get; set; }
        public List<OperationModel> SelectedOperations { get; set; }
        public List<ResourceModel> SelectedResources { get; set; }
        

        public ICommand RefreshCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand FilterCommand { get; private set; } 
        public bool IsFilterVisible
        {
            get { return _isFilterVisible; }
            set
            {
                if (_isFilterVisible == value)
                    return;

                _isFilterVisible = value;
                OnPropertyChanged("IsFilterVisible");
            }
        }
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged("StartTime");
            }
        }
        public bool ShowBorder
        {
            get { return _showBorder; }
            set
            {
                _showBorder = value;
                OnPropertyChanged("ShowBorder");
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public string PlaceInformation
        {
            get { return _placeInformation; }
            set
            {
                _placeInformation = value;
                OnPropertyChanged("PlaceInformation");
            }
        }
        
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTill { get; set; }
        public bool AllowShowComleted { get; set; }

        #endregion

        public WorkingManageViewModel()
        {
            PanelCaption = "Работы";
            AllowClosePanel = true;
            ShowBorder = true;

            StartTime = DateTime.Today;

            Appointments = new ObservableCollection<AppointmentModel>();
            Resources = new ObservableCollection<ResourceModel>();

            Operations = new ObservableCollection<OperationModel>();
            Labels = new ObservableCollection<LabelModel>();
            SelectedOperations = new List<OperationModel>();
            SelectedResources = new List<ResourceModel>();

            RefreshCommand = new DelegateCustomCommand(Refresh);
            ExportCommand = new DelegateCustomCommand(ExportData);
            FilterCommand = new DelegateCustomCommand(Find, CanFind);

            SetDefaultFilterDates();

            FillReferences();
            Find();
        }

        private void FillReferences()
        {
            BillOperationClass[] classes;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<BillOperationClass>>())
                classes = mgr.GetAll().ToArray();

            foreach (var cl in classes)
            {
                var id = Labels.Count;
                var labelModel = new LabelModel
                {
                    Id = id+1,
                    Name = cl.OperationClassName,
                    Code = cl.OperationClassCode,
                    //Color = GetGradients(Colors.Blue, Colors.Green, classes.Length, id)
                };
                Labels.Add(labelModel);
            }
        }

        public Color GetGradients(Color start, Color end, int steps, int idx)
        {
            int stepA = ((end.A - start.A) / (steps - 1));
            int stepR = ((end.R - start.R) / (steps - 1));
            int stepG = ((end.G - start.G) / (steps - 1));
            int stepB = ((end.B - start.B) / (steps - 1));

            var dc = System.Drawing.Color.FromArgb(start.A + (stepA * idx),
                                                   start.R + (stepR * idx),
                                                   start.G + (stepG * idx),
                                                   start.B + (stepB * idx));
            return Color.FromArgb(dc.A, dc.R, dc.G, dc.B);
        }

        private void SetDefaultFilterDates()
        {
            var now = DateTime.Now;
            DateFrom = new DateTime(now.Year, now.Month, 1);
            DateTill = DateFrom.Value.AddMonths(1);
        }

        private void AddMenu()
        {
            InitializeCustomizationBar();
            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1);

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Filter,
                Command = FilterCommand,
                ImageSmall = ImageResources.DCLFilter16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilter32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F3),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.ExportData,
                Command = ExportCommand,
                ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 10
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.RefreshData,
                Command = RefreshCommand,
                ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F5),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 20
            });

            // меню вида
            var listMenuItem = new ListMenuItem
            {
                Caption = StringResources.SettingsInMenu,
                ImageSmall = ImageResources.DCLDefault16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDefault32.GetBitmapImage(),
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 30
            };
            bar.MenuItems.Add(listMenuItem);
        }

        public void Refresh()
        {
            WaitStart();
            try
            {
            }
            finally
            {
                WaitStop();
            }
        }

        private void ExportData()
        {

        }

        private bool CanFind()
        {
            return DateFrom.HasValue && DateTill.HasValue;
        }

        private void Find()
        {
            var workingFilter = string.Format("(workingfrom is null or workingfrom < '{0:yyyyMMdd HH:mm:ss}') and (workingtill is null or workingtill > '{1:yyyyMMdd HH:mm:ss}')", DateTill, DateFrom);
            var workFilter = string.Format("workid in (select workid_r from wmsworking where {0})", workingFilter);
            var operationFilter = string.Format("operationcode in (select operationcode_r from wmswork where workid in (select workid_r from wmsworking where {0}))", workingFilter);
            var workerFilter = string.Format("workerid in (select workerid_r from wmsworking where {0})", workingFilter);

            IEnumerable<Work> works;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
                works = mgr.GetFiltered(workFilter);

            IEnumerable<Working> workings;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                workings = mgr.GetFiltered(workingFilter);

            IEnumerable<Worker> workers;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
                workers = mgr.GetFiltered(workerFilter);

            IEnumerable<BillOperation> operations;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<BillOperation>>())
                operations = mgr.GetFiltered(operationFilter);

            // операции становятся
            Operations.Clear();
            foreach (var operation in operations)
            {
                var op = new OperationModel() {Id = Operations.Count, Code=operation.OperationCode, Name = operation.OperationName, ClassCode = operation.OperationClassCode};
                Operations.Add(op);
            }

            // работники становятся ресурсами
            Resources.Clear();
            foreach (var worker in workers)
            {
                var res = new ResourceModel(worker);
                Resources.Add(res);
            }

            // выполнения становятся событиями
            Appointments.Clear();
            foreach (var working in workings)
            {
                var work = works.First(i => i.GetKey<decimal>() == working.WORKID_R);
                var operation = Operations.First(i => i.Code == work.OPERATIONCODE_R);
                var worker = Resources.First(i => i.Id == working.WORKERID_R);
                var label = Labels.First(i => i.Code == operation.ClassCode);
                var a = new AppointmentModel(working, operation, worker, label);
                Appointments.Add(a);
            }
        }

        public bool IsAppointmentVisible(AppointmentModel a, OperationModel[] ops, ResourceModel[] res, bool showCompleted)
        {
            if (ops.Length == 0 || res.Length == 0)
                return false;

            if (!showCompleted && a.IsCompleted)
                return false;

            if (ops.All(i => i.Id != a.OperationId))
                return false;

            if (res.All(i => i.Id != a.WorkerId))
                return false;

            return true;
        }
    }

    #region #models
    public class OperationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ClassCode { get; set; }

    }
    public class AppointmentModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Subject { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public long Label { get; set; }
        public string Location { get; set; }
        public bool AllDay { get; set; }
        public int EventType { get; set; }
        public string RecurrenceInfo { get; set; }
        public string ReminderInfo { get; set; }
        public object ResourceId { get; set; }
        public decimal Price { get; set; }

        public int WorkerId { get; set; }
        public int OperationId { get; set; }
        public bool IsCompleted { get; set; }

        public Working Working { get; private set; }

        public AppointmentModel(Working working, OperationModel operationModel, ResourceModel resourceModel, LabelModel label)
        {
            Working = working;
            StartTime = working.WORKINGFROM.Value;
            WorkerId = Convert.ToInt32(working.WORKERID_R);
            OperationId = operationModel.Id;
            ResourceId = string.Format("<ResourceIds>\r\n<ResourceId Type=\"System.Int32\" Value=\"{0}\" />\r\n</ResourceIds>", WorkerId);

            if (working.WORKINGTILL.HasValue)
            {
                EndTime = working.WORKINGTILL.Value;
                IsCompleted = true;
            }
            else
                EndTime = DateTime.Now;

            Status = IsCompleted ? 1 : 0;
            Label = label.Id;
            Subject = string.Format("{0} ({1})", operationModel.Name, EndTime - StartTime);
        }
    }

    public class ResourceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }

        public ResourceModel(Worker worker)
        {
            var decId = worker.GetKey<decimal>();
            Id = Convert.ToInt32(decId);

            Name = worker.WorkerFIO;
        }
    }

    public class LabelModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Color Color { get; set; }
    }
    #endregion #models
}