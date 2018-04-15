using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using System.Windows.Input;

// Для полноценной работы у пользователь необходимо добавить ссылки на библиотеки
// DevExpress.Dashboard.v13.2.Extensions.dll
// DevExpress.XtraBars.v13.2.dll
// DevExpress.DataAccess.v13.2.UI.dll
// DevExpress.XtraGauges.v13.2.Core.dll
// DevExpress.XtraGauges.v13.2.Win.dll
// DevExpress.XtraCharts.v13.2.UI.dll
// DevExpress.XtraCharts.v13.2.dll
// DevExpress.XtraPrinting.v13.2.dll
// DevExpress.XtraEditors.v13.2.dll
// DevExpress.XtraLayout.v13.2.dll
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class DashboardViewModel : PanelViewModelBase
    {
        public ICommand RefreshCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public List<object> ListDataSource { get; private set; }
        public event EventHandler ExportDashboard;
        public event EventHandler RefreshDashboard;
        
        protected virtual void OnExportDashboard()
        {
            var handler = ExportDashboard;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnRefreshDashboard()
        {
            var handler = RefreshDashboard;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public DashboardViewModel()
        {
            RefreshCommand = new DelegateCustomCommand(RefreshData);
            ExportCommand = new DelegateCustomCommand(ExportData);
            AddMenu();

            ListDataSource = new List<object>();
            RefreshData();
        }

        private void ExportData()
        {
            try
            {
                WaitStart();
                OnExportDashboard();
            }
            finally 
            {
                
                WaitStop();
            }
        }

        protected override void InitializeSettings()
        {
            //Используем глобальные настройки вида панели инструментов
            //MenuSuffix = GetType().GetFullNameWithoutVersion();

            base.InitializeSettings();
            IsMenuEnable = true;
        }

        private void AddMenu()
        {
            InitializeCustomizationBar();

            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1);

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
        }

        private void RefreshData()
        {
            try
            {
                WaitStart();
                GetSource();
                OnRefreshDashboard();
            }
            finally
            {
                WaitStop();
            }
        }


        private void GetSource()
        {
            ListDataSource.Clear();
            var mgr = IoC.Instance.Resolve<IBPProcessManager>();
            var query = string.Format(@"select w.warehousename, a.areaname, a.areatypecode_r
                                            ,count(pl.placecode)
                                            ,sum((case when pl.placecapacity=0 then 1 else 0 end)) busy
                                            ,sum((case when pl.placecapacity>0 then 1 else 0 end)) free
                                            from wmsarea a
                                            join wmswarehouse w on w.warehousecode=a.warehousecode_r
                                            join wmssegment s on s.areacode_r=a.areacode
                                            join wmsplace pl on pl.segmentcode_r=s.segmentcode
                                            where a.areatypecode_r = 'AREATYPE_STORAGE'
                                            group by w.warehousename, a.areaname, a.areatypecode_r");

            var dataArea = mgr.ExecuteDataTable(query);
            ListDataSource.Add(dataArea != null ? ConvertToInfoArea(dataArea) : null);

            query = "select externaltrafficplanarrived \"Планируемая дата\" , " +
                   "count(externaltrafficid) \"Количество машин\" " +
                   ", sum(cargoiwbcount) \"Грузовые места\" " +
                   ", sum(iwbposcount) \"Позиции\" " +
                   "from ( " +
                   "select et.externaltrafficid, et.vehicleid_r, et.externaltrafficplanarrived, c.internaltrafficid_r, et.statuscode_r " + 
                   ",nvl(c.cargoiwbcount, 0)cargoiwbcount " +
                   ",i2c.iwbid_r " +
                   ",nvl ((select count(ip.iwbposid) from wmsiwbpos ip where ip.iwbid_r=i2c.iwbid_r), 0) iwbposcount " +
                   "from yexternaltraffic et " +
                   "left join yinternaltraffic it on it.externaltrafficid_r=et.externaltrafficid " +
                   "left join wmscargoiwb c on c.internaltrafficid_r=it.internaltrafficid " +
                   "left join wmsiwb2cargo i2c on i2c.cargoiwbid_r=c.cargoiwbid " +
                   ") " +
                   "where externaltrafficplanarrived >= trunc(sysdate) " +
                   "group by externaltrafficplanarrived order by 1 ";

            var dataCar = mgr.ExecuteDataTable(query);
            ListDataSource.Add(dataCar != null ? ConvertToInfoCar(dataCar) : null);

            query = string.Format(@"select 
                                    (select w.warehousename from wmswarehouse w where w.warehousecode=g.warehousecode_r) WAREHOUSENAME
                                    ,g.gatenumber
                                    ,(select count(yi.internaltrafficid) from yinternaltraffic yi
                                    where 1=1
                                    and yi.warehousecode_r=g.warehousecode_r and yi.gatecode_r=g.gatecode
                                    and yi.internaltrafficfactarrived < sysdate
                                    and (yi.internaltrafficfactdeparted is null or yi.internaltrafficfactdeparted > sysdate) ) IsBusy
                                    from wmsgate g
                                    order by 1, 2");
            var dataGate = mgr.ExecuteDataTable(query);
            ListDataSource.Add(dataGate != null ? ConvertToInfoGate(dataGate) : null);
        }

        private List<InfoArea> ConvertToInfoArea(DataTable dataTable)
        {
            return (from row in dataTable.AsEnumerable()
                    select new InfoArea
                        {
                            WarehouseName = row["WAREHOUSENAME"].ToString(),
                            AreaName = row["AREANAME"].ToString(),
                            Count = Convert.ToInt32(row["COUNT(PL.PLACECODE)"]),
                            Free = Convert.ToInt32(row["FREE"]),
                            Busy = Convert.ToInt32(row["BUSY"]),
                        }).ToList();
        }

        private List<InfoCar> ConvertToInfoCar(DataTable dataTable)
        {
            return (from DataRow row in dataTable.Rows
                    select new InfoCar()
                        {
                            DateTime = Convert.ToDateTime(row[0].ToString()),
                            CountCars = Convert.ToInt32(row[1]),
                            Places = Convert.ToInt32(row[2]),
                            Position = Convert.ToInt32(row[3]),
                        }).ToList();
        }

        private List<InfoGate> ConvertToInfoGate(DataTable dataTable)
        {
            return (from DataRow row in dataTable.Rows
                    select new InfoGate()
                    {
                        WarehouseName = row[0].ToString(),
                        GateNumber = row[1].ToString(),
                        IsBusy = Convert.ToInt32(row[2]),
                        IsFree = Math.Abs(Convert.ToInt32(row[2]) - 1),
                    }).ToList();
        }


        public class InfoArea
        {
            public string WarehouseName { get; set; }
            public string AreaName { get; set; }
            public int Count { get; set; }
            public int Free { get; set; }
            public int Busy { get; set; }
        }

        public class InfoCar
        {
            public DateTime DateTime { get; set; }
            public int CountCars { get; set; }
            public int Places { get; set; }
            public int Position { get; set; }
        }

        public class InfoGate
        {
            public string WarehouseName { get; set; }
            public string GateNumber { get; set; }
            public int IsBusy { get; set; }
            public int IsFree { get; set; }
        }
    }
}