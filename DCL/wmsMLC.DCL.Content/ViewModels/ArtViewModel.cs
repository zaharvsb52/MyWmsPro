using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ArtView))]
    public class ArtViewModel : ObjectViewModelBase<Art>, ILoadImageHandler
    {
        private const string ImagePropertyName = "Image";

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            if (!InPropertyEditMode)
            {
                var field = new ValueDataField
                {
                    Name = ImagePropertyName,
                    FieldName = ImagePropertyName,
                    SourceName = ImagePropertyName,
                    FieldType = typeof (object),
                    Visible = true,
                    LabelPosition = ValueDataFieldConstants.None
                };

                field.Properties[ValueDataFieldConstants.IsSubImageView] = true;
                field.Properties[ValueDataFieldConstants.ShowMenu] = true;
                fields.Add(field);
            }
            return fields;
        }

        protected override void OnAfterRefresh(bool usewait)
        {
            base.OnAfterRefresh(usewait);
            if (!usewait)
                return;
            LoadImage(usewait: false, showerror: false);
        }

        public void LoadImage()
        {
            LoadImage(usewait: true, showerror: true);
        }

        public async void LoadImage(bool usewait, bool showerror)
        {
            try
            {
                if (usewait)
                    WaitStart();

                if (Source == null || Source.IsNew)
                    return;

                var artImage = Source as ArtImage;
                if (artImage == null)
                    return;

                artImage.RejectChanges();

                if (!artImage.ARTPICTURE.HasValue)
                    return;

                //Обновляем EntityFile. Безусловное получение картинки артикула.
                var data = await LoadImageAsync(artImage.ARTPICTURE.Value);
                artImage.SetImage(data);
            }
            catch (Exception ex)
            {
                if (showerror)
                {
                    if (!ExceptionHandler(ex, ExceptionResources.ItemCantRefresh))
                        throw;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (usewait)
                    WaitStop();
            }
        }

        private async Task<string> LoadImageAsync(decimal fileid)
        {
            return await Task.Factory.StartNew(() =>
            {
                //Используется для включения WaitIndicator'а
                System.Threading.Thread.Sleep(200);
                using (var mgr = GetEntityFileManager())
                {
                    return mgr.GetFileData(fileid);
                }
            });
        }

        protected override void OnSetSource(object source)
        {
            var art = source;
            if (!(art is ArtImage))
            {
                art = new ArtImage((Art) art);
                if (Source is ArtImage)
                {
                    ((ArtImage) art).SetImage((ArtImage) Source);
                }
            }
            base.OnSetSource(art);
        }

        protected override void MgrInsert(ref Art entity)
        {
            if (ValidateArtImage())
            {
                SaveImage(true, ref entity);
            }
            else
            {
                base.MgrInsert(ref entity);
                OnSetSource(entity);
            }
        }

        protected override void MgrUpdate()
        {
            if (ValidateArtImage())
            {
                var entity = Source;
                SaveImage(false, ref entity);
            }
            else
            {
                base.MgrUpdate();
            }
        }

        private bool ValidateArtImage()
        {
            if (InPropertyEditMode || InFormulaMode) //Не сохраняем в случае мультиредактирования и режима формул
                return false;

            var artImage = Source as ArtImage;
            if (artImage == null || !artImage.IsImageChanged ||
                (!artImage.ARTPICTURE.HasValue && artImage.Image == null))
                return false;

            return true;
        }

        private void SaveImage(bool sourceIsNew, ref Art entity)
        {
            if (!ValidateArtImage())
                return;

            var artImage = (ArtImage) Source;

            //Сохраняем Image
            try
            {
                using (var mgr = GetEntityFileManager())
                {
                    using (var uow = UnitOfWorkHelper.GetUnit())
                    {
                        try
                        {
                            mgr.SetUnitOfWork(uow);
                            var artmgr = IoC.Instance.Resolve<IBaseManager<Art>>();
                            artmgr.SetUnitOfWork(uow);

                            uow.BeginChanges();

                            if (artImage.ARTPICTURE.HasValue)
                            {
                                //Сохраняем артикул
                                artmgr.Update(Source);
                                if (artImage.Image == null)
                                {
                                    //Удаляем Image
                                    mgr.DeleteByKey(artImage.ARTPICTURE.Value);
                                }
                                else
                                {
                                    //Update Image
                                    mgr.SetFileData(artImage.ARTPICTURE.Value, ConvertByteArrayToString(artImage.Image));
                                }
                            }
                            else
                            {
                                //insert image
                                var entityFile = mgr.New();
                                entityFile.File2Entity = "ART";
                                entityFile.FileName =
                                    entityFile.FileKey =
                                        sourceIsNew
                                            ? string.Format("{0}_{1}", artImage.MANDANTID, artImage.ArtName)
                                            : artImage.GetKey<string>();
                                entityFile.SetProperty(EntityFile.FileDataPropertyName, null);
                                mgr.Insert(ref entityFile);
                                var entityFileKey = entityFile.GetKey<decimal>();

                                //Сохраняем art
                                artImage.ARTPICTURE = entityFileKey;

                                if (sourceIsNew)
                                {
                                    artmgr.Insert(ref entity);

                                    entityFile.FileName =
                                        entityFile.FileKey = entity.GetKey<string>();
                                    mgr.Update(entityFile);

                                    //OnSetSource(entity);
                                }
                                else
                                {
                                    artmgr.Update(Source);
                                }

                                mgr.SetFileData(entityFileKey, ConvertByteArrayToString(artImage.Image));
                            }

                            uow.CommitChanges();
                        }
                        catch
                        {
                            uow.RollbackChanges();
                            throw;
                        }
                    }
                }

                artImage.AcceptChanges();
                artImage.IsImageChanged = false;
            }
            catch
            {
                artImage.SetError();
                throw;
            }
        }

        protected override void OnSave()
        {
            if (!CanSave())
                return;

            if (!Validate())
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSaveAndClose())
                return;

            if (!Validate())
                return;

            base.OnSaveAndClose();
        }

        public bool ValidateFileSize(long imagesize)
        {
            var maxImageSize = Properties.Settings.Default.MaxImageSize;
            var maxImageSizeInByte = maxImageSize * 1048576;

            if (imagesize > maxImageSizeInByte)
            {
                GetViewService()
                       .ShowDialog(StringResources.Error,
                           string.Format(ExceptionResources.ImageSizeErrorFormat, maxImageSize), MessageBoxButton.OK,
                           MessageBoxImage.Error, MessageBoxResult.Yes);
                return false;
            }

            return true;
        }

        private bool Validate()
        {
            if ((Source.ArtCommercTime != null || !string.IsNullOrEmpty(Source.ArtCommercTimeMeasure)) &&
                (Source.ArtCommercTime == null || string.IsNullOrEmpty(Source.ArtCommercTimeMeasure)))
            {
                var fields = GetDataFields(SettingDisplay.Detail);
                var field = Source.ArtCommercTime == null
                    ? fields.FirstOrDefault(x => x.SourceName == Art.ARTCOMMERCTIMEPropertyName)
                    : fields.FirstOrDefault(x => x.SourceName == Art.ARTCOMMERCTIMEMEASUREPropertyName);

                if (field != null)
                {
                    GetViewService()
                        .ShowDialog(StringResources.Error,
                            string.Format(StringResources.ErrorSaveShouldNotBeEmpty, field.Caption), MessageBoxButton.OK,
                            MessageBoxImage.Error, MessageBoxResult.Yes);
                    return false;
                }
            }

            var artImage = Source as ArtImage;
            if (artImage == null || artImage.Image == null)
                return true;
            return ValidateFileSize(artImage.Image.Length);
        }

        private string ConvertByteArrayToString(byte[] buffer)
        {
            return buffer == null ? null : Convert.ToBase64String(buffer);
        }

        private static IEntityFileManager GetEntityFileManager()
        {
            return IoC.Instance.Resolve<IEntityFileManager>();
        }

        [SourceName("ART")]
        [SysObjectName("Art")]
        [XmlRoot("TENTART")]
        public class ArtImage : Art
        {
            public ArtImage()
            {
            }

            public ArtImage(Art art)
            {
                if (art == null)
                    return;

                try
                {
                    SuspendNotifications();
                    Copy(art, this);
                    AcceptChanges(art.IsNew);
                }
                finally
                {
                    ResumeNotifications();
                }
            }

            public bool IsImageChanged { get; set; }

            private byte[] _image;
            public byte[] Image
            {
                get { return _image; }
                set
                {
                    if (_image == value)
                        return;
                    _image = value;
                    IsImageChanged = true;
                    OnPropertyChanged(ImagePropertyName);
                    IsDirty = true;
                }
            }

            public new void RejectChanges()
            {
                Image = null;
                IsImageChanged = false;
                base.RejectChanges();
            }

            public void SetImage(string data)
            {
                _image = string.IsNullOrEmpty(data) ? null : Convert.FromBase64String(data);
                OnPropertyChanged(ImagePropertyName);
            }

            public void SetImage(ArtImage artImage)
            {
                _image = artImage._image;
                IsImageChanged = artImage.IsImageChanged;
                OnPropertyChanged(ImagePropertyName);
            }

            public void SetError()
            {
                //Даём возможность сохранения при ошибках
                if (IsImageChanged)
                    IsDirty = true;
            }
        }
    }
}