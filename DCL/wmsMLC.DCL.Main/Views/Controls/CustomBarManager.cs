using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DevExpress.Utils.Serializing;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.Serialization;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views.Controls
{
    //https://www.devexpress.com/Support/WhatsNew/DXperience/files/14.2.5.bc.xml
    public class CustomBarManager : BarManager, IDisposable, ICustomBarManager, ISaveRestore
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(CustomBarManager));
        private readonly Version _version = new Version(1, 0, 0, 9);
        private List<string> _xmlFileNamesPath;
        private List<string> _layoutsString;
        private bool _isglobalproperty;
        private bool _isLoaded;

        static CustomBarManager()
        {
            CheckBarItemNames = false;
        }

        public CustomBarManager()
        {
            _xmlFileNamesPath = new List<string>();
            _layoutsString = new List<string>();

            DXSerializer.SetLayoutVersion(this, _version.ToString());

            //Не работает в 15.2
            //DXSerializer.AddLayoutUpgradeHandler(this, OnLayoutUpgradeHandler);

            DXSerializer.AddShouldSerializeCollectionItemHandler(this, OnShouldSerializeCollectionItem);
            DXSerializer.AddAllowPropertyHandler(this, OnAllowProperty);

            //AddHandler(BarNameScope.ScopeChangedEvent, new ScopeChangedEventHandler(OnScopeChanged));

            //Нет в 15.2
            //ReuseRemovedItemsDuringDeserialization = false;
            AddNewItems = false;

            #region Свойства сохраняющиеся в глобальном файле вида
            BarItemDisplayMode = BarItemDisplayMode.ContentAndGlyph;
            UserGlyphAlignment = Dock.Top;
            //ToolbarGlyphSize = GlyphSize.Default;
            #endregion Свойства сохраняющиеся в глобальном файле вида

            //TODO: Необходимо разобраться - зачем LockKeyGestureEventAfterHandling в false.
            LockKeyGestureEventAfterHandling = false;

            //HACK: Другой функционал обраюотки горячих клавиш в версии 15.2
            KeyGestureWorkingMode = KeyGestureWorkingMode.AllKeyGestureFromRoot;
        }

        ~CustomBarManager()
        {
            Dispose(false);
        }

        #region .  Properties  .
        private BarItemDisplayMode _barItemDisplayMode;
        [XtraSerializableProperty]
        public BarItemDisplayMode BarItemDisplayMode
        {
            get
            {
                return _barItemDisplayMode;
            }
            set
            {
                if (_barItemDisplayMode == value)
                    return;

                _barItemDisplayMode = value;
                foreach (var item in Bars.SelectMany(bar => bar.ItemLinks.OfType<BarItemLink>()))
                {
                    item.BarItemDisplayMode = _barItemDisplayMode;
                }
            }
        }

        private Dock _userGlyphAlignment;

        [XtraSerializableProperty]
        public Dock UserGlyphAlignment
        {
            get
            {
                return _userGlyphAlignment;
            }
            set
            {
                if (_userGlyphAlignment == value)
                    return;

                _userGlyphAlignment = value;
                foreach (var item in Bars.SelectMany(bar => bar.ItemLinks.OfType<BarItemLink>()))
                {
                    item.UserGlyphAlignment = _userGlyphAlignment;
                }
            }
        }
        #endregion .  Properties  .

        #region .  Methods  .
        protected override void OnLoaded(object sender, EventArgs e)
        {
            base.OnLoaded(sender, e);
            if (_isLoaded)
                return;
            RestoreLayoutInternal();
            _isLoaded = true;
        }

        private void OnShouldSerializeCollectionItem(object sender, XtraShouldSerailizeCollectionItemEventArgs e)
        {
            var value = e.Value as RuntimePropertyCustomization;
            if (value != null)
            {
                switch (value.PropertyName)
                {
                    case "ToolbarGlyphSize":
                        e.ShouldSerailize = true;
                        break;
                    case "DockInfo.ContainerName":
                        e.ShouldSerailize = false;
                        break;
                    default:
                        e.ShouldSerailize = !_isglobalproperty;
                        break;
                }
            }
        }

        private void OnAllowProperty(object sender, AllowPropertyEventArgs e)
        {
            if (!e.Allow)
                return;

            if (e.IsSerializing)
            {
                if (e.Property != null)
                {
                    switch (e.Property.Name)
                    {
                        case "BarItemDisplayMode":
                        case "UserGlyphAlignment":
                            e.Allow = _isglobalproperty;
                            break;
                        //case "Items":
                        //    e.Allow = false;
                        //    break;
                        //case "Bars":
                        //    e.Allow = !_isglobalproperty;
                        //    break;
                    }
                }
            }
        }

        //Перенесено в CustomBar
        //protected override void AddLink(BarItemLinkBase link, bool linkItem)
        //{
        //    base.AddLink(link, linkItem);
        //    var barItemLink = link as BarItemLink;
        //    if (barItemLink != null)
        //    {
        //        if (barItemLink.BarItemDisplayMode != BarItemDisplayMode)
        //            barItemLink.BarItemDisplayMode = BarItemDisplayMode;
        //        if (barItemLink.UserGlyphAlignment != UserGlyphAlignment)
        //            barItemLink.UserGlyphAlignment = UserGlyphAlignment;
        //    }
        //}

        //Не работает в 15.2
        //private void OnLayoutUpgradeHandler(object sender, LayoutUpgradeEventArgs e)
        //{
        //    DXSerializer.RemoveLayoutUpgradeHandler(this, OnLayoutUpgradeHandler);

        //    Version version;
        //    if (Version.TryParse(e.RestoredVersion, out version))
        //    {
        //        if (version > _version)
        //            return;
        //    }
        //    //throw new PassThroughException(ExceptionResources.BadLayoutVersion, e.RestoredVersion, _version);
        //    Logger.WarnFormat(ExceptionResources.BadLayoutVersion, e.RestoredVersion, _version);
        //}

        public override void RestoreLayoutFromXml(string xmlFile)
        {
            if (string.IsNullOrEmpty(xmlFile))
                return;

            if (!IsLoaded)
            {
                _xmlFileNamesPath.Add(xmlFile);
                return;
            }
            base.RestoreLayoutFromXml(xmlFile);
        }

        public void RestoreLayoutFromString(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            if (!_isLoaded)
            {
                _layoutsString.Add(layout);
                return;
            }
            RestoreLayoutFromStringBase(layout);
        }

        private void RestoreLayoutInternal()
        {
            var globalBarItemDisplayMode = BarItemDisplayMode.Default;
            var globalUserGlyphAlignment = Dock.Top;

            Action saveGlobalPropertyHandler = () =>
            {
                globalBarItemDisplayMode = BarItemDisplayMode;
                globalUserGlyphAlignment = UserGlyphAlignment;
            };

            Action restoreGlobalPropertyHandler = () =>
            {
                BarItemDisplayMode = globalBarItemDisplayMode;
                UserGlyphAlignment = globalUserGlyphAlignment;
            };

            if (_xmlFileNamesPath.Count > 0)
            {
                switch (_xmlFileNamesPath.Count)
                {
                    case 2:
                        base.RestoreLayoutFromXml(_xmlFileNamesPath[1]);
                        saveGlobalPropertyHandler();
                        base.RestoreLayoutFromXml(_xmlFileNamesPath[0]);
                        restoreGlobalPropertyHandler();
                        break;
                    case 1:
                        base.RestoreLayoutFromXml(_xmlFileNamesPath[0]);
                        break;
                }

                if (Bars.Count > 0)
                    _xmlFileNamesPath.Clear();
                return;
            }

            if (_layoutsString.Count > 0)
            {
                switch (_layoutsString.Count)
                {
                    case 2:
                        RestoreLayoutFromStringBase(_layoutsString[1]);
                        saveGlobalPropertyHandler();
                        RestoreLayoutFromStringBase(_layoutsString[0]);
                        restoreGlobalPropertyHandler();
                        break;
                    case 1:
                        RestoreLayoutFromStringBase(_layoutsString[0]);
                        break;
                }

                if (Bars.Count > 0)
                    _layoutsString.Clear();
            }
        }

        protected override void OnBarsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnBarsSourceChanged(e);
            if (_isLoaded && Bars.Count > 0) //Дозагружаем вид. Используется для детальных форм
                Dispatcher.Invoke(new Action(RestoreLayoutInternal), DispatcherPriority.Background);
        }

        private void RestoreLayoutFromStringBase(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.AutoFlush = true;
                    sw.Write(layout);
                    ms.Position = 0;
                    RestoreLayoutFromStream(ms);
                }
            }
        }

        public void SaveLayoutToXml(string xmlFile, bool isglobalproperty)
        {
            _isglobalproperty = isglobalproperty;
            SaveLayoutToXml(xmlFile);
        }

        public string SaveLayoutToString(bool isglobalproperty)
        {
            _isglobalproperty = isglobalproperty;
            using (var ms = new MemoryStream())
            {
                SaveLayoutToStream(ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        #region Старый код 
        //protected override void OnOwnerKeyDown(object sender, KeyEventArgs e)
        //{
        //    //base.OnOwnerKeyDown(sender, e);
        //}

        //protected override void OnPreviewOwnerKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (!IsActive())
        //        return;
        //    CheckExecuteGesture(e);
        //    base.OnPreviewOwnerKeyDown(sender, e);
        //}

        //private bool IsActive()
        //{
        //    var ctx = DataContext as IActivatable;
        //    if (ctx != null)
        //        return ctx.IsActive;
        //    return false;
        //}
        #endregion Старый код 

        public void Dispose()
        {
           Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            DXSerializer.RemoveShouldSerializeCollectionItemHandler(this, OnShouldSerializeCollectionItem);
            DXSerializer.RemoveAllowPropertyHandler(this, OnAllowProperty);
        }
        #endregion .  Methods  .
    }
}
