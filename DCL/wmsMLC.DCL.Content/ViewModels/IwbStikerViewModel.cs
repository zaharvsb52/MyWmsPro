using System.Collections.ObjectModel;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View((typeof(CustomObjectListView)))]
    public class IwbStikerViewModel : CustomObjectListViewModelBase<IwbStickerCpv>
    {
        public IwbStikerViewModel()
        {
            AllowEditing = true;
        }

        protected override void InitializeSettings()
        {
            MenuSuffix = GetType().GetFullNameWithoutVersion();
            base.InitializeSettings();
        }

        protected override bool CanEdit()
        {
            // �� ���� ��������� �� �������� �����
            return false;
        }

        protected override ObservableCollection<DataField> GetDataFields()
        {
            var fields = new []{
            new DataField
            {
                FieldName = "ArtCode",
                Caption = "�������",
                Description = "�������� ��������",
                FieldType = typeof(string),
                Name = "ArtCode",
                SourceName = "ArtCode",
                IsEnabled = false,
                Visible = true
            },
            new DataField
            {
                FieldName = "ProductCount",
                Caption = "���-�� ������",
                Description = "���-�� ������",
                FieldType = typeof(decimal),
                Name = "ProductCount",
                SourceName = "ProductCount",
                IsEnabled = false,
                Visible = true
            },
            new DataField
            {
                FieldName = "StickerCount",
                Caption = "���-�� ����������",
                Description = "���-�� ����������",
                FieldType = typeof(decimal?),
                Name = "StickerCount",
                SourceName = "StickerCount",
                Visible = true,
                IsEnabled = true                
            }};
            return new ObservableCollection<DataField>(fields);
        }

        protected override void CreateMainMenu()
        {
            //INFO: ����� �� ����� ����
        }
    }

    public class IwbStickerCpv : EditableBusinessObject
    {
        //[Browsable(false)]
        public decimal? CpvCode { get; set; }

        public string ArtCode { get; set; }

        public decimal? ProductCount { get; set; }

        private decimal? _stickerCount;
        public decimal? StickerCount 
        {
            get
            {
                return _stickerCount;
            }
            set
            {
                _stickerCount = value;
                OnPropertyChanged("StickerCount");
            }
        }

        public IwbStickerCpv(decimal? cpvCode, string artCode, decimal? productCount, decimal? stickerCount)
        {
            CpvCode = cpvCode;
            ArtCode = artCode;
            ProductCount = productCount;
            StickerCount = stickerCount;
        }
    }
}