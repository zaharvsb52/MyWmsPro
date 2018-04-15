using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.DCL.General.ViewModels
{
    public class FilterViewModel : PanelViewModelBase, IFilterViewModel
    {
        #region .  Fields  .
        private string _prevfilterExpression;
        private string _filterExpression;
        private string _lastLoadedFilterFileName;
        private int? _maxRowCount;
        private decimal? _mandantId;
        private bool _isFilterMode;
        private bool _isRowCountEnabled;
        #endregion .  Fields  .

        public FilterViewModel()
        {
            Fields = new ObservableCollection<DataField>();
            Mandants = new MandantFilter();
            IsFilterMode = true;

            ToDefaultCommand = new DelegateCustomCommand(ToDefault, () => true);
            CancelCommand = new DelegateCustomCommand(RejectChanges, () => true);

            LoadCommand = new DelegateCustomCommand(Load, () => true);
            SaveCommand = new DelegateCustomCommand(Save, () => true);
        }

        #region .  Properties  .
        public string FilterExpression
        {
            get { return _filterExpression; }
            set
            {
                if (_filterExpression == value)
                    return;

                _filterExpression = value;
                OnPropertyChanged("FilterExpression");
            }
        }

        public int? MaxRowCount
        {
            get { return _maxRowCount; }
            set
            {
                _maxRowCount = value;
                OnPropertyChanged("MaxRowCount");
            }
        }

        public decimal? MandantId
        {
            get { return _mandantId; }
            set
            {
                _mandantId = value;
                OnPropertyChanged("MandantId");
            }
        }

        public bool IsFilterMode
        {
            get { return _isFilterMode; }
            set
            {
                _isFilterMode = value;
                OnPropertyChanged("IsFilterMode");
            }
        }

        public bool IsRowCountEnabled
        {
            get { return _isRowCountEnabled; }
            set
            {
                _isRowCountEnabled = value;
                OnPropertyChanged("IsRowCountEnabled");
            }
        }

        private string _defaultFilterExpression;
        public string DefaultFilterExpression
        {
            get { return _defaultFilterExpression; }
            set
            {
                if (_defaultFilterExpression == value)
                    return;

                _defaultFilterExpression = value;
                OnPropertyChanged("DefaultFilterExpression");
            }
        }

        private string _strongFilterExpression;
        public string StrongFilterExpression
        {
            get { return _strongFilterExpression; }
            set
            {
                if (_strongFilterExpression == value)
                    return;

                _strongFilterExpression = value;
                OnPropertyChanged("StrongFilterExpression");
            }
        }

        private string _sqlFilterExpression;
        public string SqlFilterExpression
        {
            get { return _sqlFilterExpression; }
            set
            {
                if (_sqlFilterExpression == value)
                    return;

                _sqlFilterExpression = value;
                OnPropertyChanged("SqlFilterExpression");
            }
        }

        public ObservableCollection<DataField> Fields { get; private set; }
        public MandantFilter Mandants { get; private set; }

        public ICommand ToDefaultCommand { get; private set; }
        public ICommand ApplyFilterCommand { get; set; }
        public ICommand CancelCommand { get; private set; }

        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public event EventHandler FixFilterExpressionRequest;

        public bool IsValid { get; private set; }
        #endregion .  Properties  .

        #region .  Methods  .

        public void RiseFixFilterExpression()
        {
            var h = FixFilterExpressionRequest;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        public string GetDefaultExpression()
        {
            var defFilter = string.Empty;
            foreach (var f in Fields)
            {
                if (!string.IsNullOrEmpty(defFilter))
                    defFilter += " OR ";
                defFilter += string.Format("[{0}] = ?", f.SourceName);
            }
            return defFilter;
        }

        public void ToDefault()
        {
            FilterExpression = GetDefaultExpression();
            SqlFilterExpression = null;
            MaxRowCount = Properties.Settings.Default.MaxRowCountDefault;
            AcceptChanges();
        }

        public string GetSqlExpression(string filterExpression)
        {
            var sqlHelper = IoC.Instance.Resolve<ISqlExpressionHelper>();
            string filterExp;
            IsValid = sqlHelper.TryGetSqlExpression(filterExpression, out filterExp);

            var filters = new List<string> { DefaultFilterExpression, filterExp };
            var filter = string.Join(" OR ", filters.Where(p => !string.IsNullOrEmpty(p)));
            if (!string.IsNullOrEmpty(filter) && !filter.StartsWith("("))
                return string.Format("({0})", filter);
            return filter;
        }

        public string GetSqlExpression()
        {
            var filterExpression = FilterExpression == "NONE" ? SqlFilterExpression : FilterExpression;
            var sqlHelper = IoC.Instance.Resolve<ISqlExpressionHelper>();

            string filterExp;
            IsValid = sqlHelper.TryGetSqlExpression(filterExpression, out filterExp);

            if (!string.IsNullOrEmpty(filterExp) && MaxRowCount.HasValue)
            {
                Func<string, string> formatHandler = str => string.Format("({0}) AND ", str);

                var filterExps = filterExp.Split(FilterHelper.Semicolon.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var length = filterExps.Length;
                if (length > 1)
                {
                    filterExps[length - 1] = formatHandler(filterExps[length - 1]);
                    filterExp = string.Join(FilterHelper.Semicolon, filterExps);
                }
                else
                {
                    filterExp = formatHandler(filterExp);
                }
            }

            if (MaxRowCount.HasValue)
                filterExp += string.Format("(ROWNUM <= {0})", MaxRowCount);

            if (Mandants.SelectedItem != null)
            {
                MandantId = Mandants.SelectedItem.MandantId;
                if (MandantId > 0)
                {
                    var f = Fields.FirstOrDefault(i => i.Name.ToUpper().EqIgnoreCase("MANDANTID"));
                    if (f != null)
                        filterExp += string.IsNullOrEmpty(filterExp) ? string.Format("({0} = {1})", f.SourceName, MandantId) : string.Format(" AND ({0} = {1})", f.SourceName, MandantId);
                    else 
                        filterExp += string.IsNullOrEmpty(filterExp) ? string.Format("(MANDANTID = {0})", MandantId) : string.Format(" AND (MANDANTID = {0})", MandantId);
                }
            }

            var filters = new List<string> { DefaultFilterExpression, filterExp };
            var filter = string.Join(" OR ", filters.Where(p => !string.IsNullOrEmpty(p)));
            if (!string.IsNullOrEmpty(filter) && !filter.StartsWith("("))
                filter = string.Format("({0})", filter);

            filters.Clear();

            //if (!string.IsNullOrEmpty(SqlFilterExpression) && !SqlFilterExpression.StartsWith("("))
            //    SqlFilterExpression = string.Format("({0})", SqlFilterExpression);
            //filters.AddRange(new[] { filter, SqlFilterExpression });

            if (!string.IsNullOrEmpty(StrongFilterExpression) && !StrongFilterExpression.StartsWith("("))
                StrongFilterExpression = string.Format("({0})", StrongFilterExpression);
            filters.AddRange(new[] { filter, StrongFilterExpression });

            return string.Join(" AND ", filters.Where(p => !string.IsNullOrEmpty(p)));
        }

        public string GetExpression()
        {
            var filterExp = GetSqlExpression();

            filterExp = Fields.Aggregate(filterExp, (current, f) => current.Replace(f.SourceName, f.Caption));
            filterExp = filterExp.Replace("ROWNUM", StringResources.FilterMaxRowCount);
            var ar = new[] { "AND", "And", "and", "OR", "Or", "or" };
            filterExp = ar.Aggregate(filterExp, (current, s) => current.Replace(s + " ", s + "\r"));
            filterExp = filterExp.Replace("(", "").Replace(")", "");
            return filterExp;
        }

        public void AcceptChanges()
        {
            _prevfilterExpression = FilterExpression;
        }

        public void RejectChanges()
        {
            FilterExpression = _prevfilterExpression;
        }

        private void Save()
        {
            RiseFixFilterExpression();

            var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = _lastLoadedFilterFileName,
                    DefaultExt = ".xml",
                    Filter = "Filters (.xml)|*.xml",
                    OverwritePrompt = true
                };


            var result = dlg.ShowDialog();
            if (result != true)
                return;

            var filename = dlg.FileName;
            if (File.Exists(filename))
                File.Delete(filename);

            var filter = new Filter
            {
                Expression = FilterExpression,
                MaxRowCount = MaxRowCount,
                MandantId = MandantId
            };

            var s = new XmlSerializer(typeof(Filter));
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                s.Serialize(fs, filter);
        }

        private void Load()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    FileName = _lastLoadedFilterFileName,
                    DefaultExt = ".xml",
                    Filter = "Filters (.xml)|*.xml",
                    CheckFileExists = true
                };

            var result = dlg.ShowDialog();
            if (result != true)
                return;

            var filename = dlg.FileName;
            Filter filter;
            var s = new XmlSerializer(typeof(Filter));
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                filter = (Filter)s.Deserialize(fs);

            MaxRowCount = filter.MaxRowCount;
            MandantId = filter.MandantId;
            FilterExpression = filter.Expression;

            _lastLoadedFilterFileName = filename;
        }
        #endregion .  Methods  .
    }

    public class FilterViewModel<TModel> : FilterViewModel, IFilterViewModel<TModel>
    {
        public FilterViewModel()
        {
            var properties = TypeDescriptor.GetProperties(typeof(TModel));
            Mandants.IsEnableMandant = properties.Cast<PropertyDescriptor>().Any(i => i.Name.EqIgnoreCase("MandantId"));
        }
    }

    public class Filter
    {
        public int? MaxRowCount { get; set; }
        public decimal? MandantId { get; set; }
        public string Expression { get; set; }
    }

    public class MandantFilter
    {
        private const string AllMandantsName = "Все манданты";
        private List<Mandant> _items;

        #region .  Properties  .
        public List<Mandant> Items
        {
            get
            {
                if (_items != null)
                    return _items;

                _items = CreateItems();
                SelectedItem = _items[0];
                return _items;
            }
        }
        public string DisplayMember { get; private set; }
        public string ValueMember { get; private set; }

        public Mandant SelectedItem { get; set; }
        public bool IsEnableMandant { get; set; }
        #endregion

        public MandantFilter()
        {
            DisplayMember = Mandant.MANDANTCODEPropertyName;
            ValueMember = Mandant.MANDANTIDPropertyName;
        }

        private static List<Mandant> CreateItems()
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Mandant>>())
            {
                var res = new List<Mandant>();
                res.AddRange(mgr.GetAll(GetModeEnum.Partial));
                res.Sort((a, b) => String.Compare(a.MandantCode, b.MandantCode, StringComparison.Ordinal));
                res.Insert(0, new Mandant { MandantCode = AllMandantsName, MandantId = 0 });
                return res;
            }
        }
    }
}
