using System;
using wmsMLC.General.BL;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class InfoMenuTileViewModel : MenuTileViewModelBase
    {
        public const string TileMenuActionInfoOnTe = "InfoOnTe";
        public const string TileMenuActionInfoOnPlace = "InfoOnPlace";

        #region .  Properties  .

        private string _infoOnTeMenuHeader;
        public string InfoOnTeMenuHeader
        {
            get { return _infoOnTeMenuHeader; }
            set
            {
                if (_infoOnTeMenuHeader == value)
                    return;
                _infoOnTeMenuHeader = value;
                OnPropertyChanged("InfoOnTeMenuHeader");
            }
        }

        private string _infoOnPlaceMenuHeader;
        public string InfoOnPlaceMenuHeader
        {
            get { return _infoOnPlaceMenuHeader; }
            set
            {
                if (_infoOnPlaceMenuHeader == value)
                    return;
                _infoOnPlaceMenuHeader = value;
                OnPropertyChanged("InfoOnPlaceMenuHeader");
            }
        }

        protected override Action DefaultCompletedActionHandler
        {
            get { return null; }
        }

        #endregion .  Properties  .

        #region .  Methods  .

        #region . Menu .
        public override void InitializeMenu()
        {
            InfoOnTeMenuHeader = Resources.StringResources.InfoOnTe;
            InfoOnPlaceMenuHeader = Resources.StringResources.InfoOnPlace;
        }
        #endregion . Menu .

        #region . Commands .
        protected override void OnMenuClick(string parameter)
        {
            if (!CanMenuClick(parameter))
                return;

            switch (parameter)
            {
                case TileMenuActionInfoOnTe:
                    RunBp("RclInfoOnTe");
                    break;
                case TileMenuActionInfoOnPlace:
                    RunBp("RclInfoOnPlace");
                    break;
            }
        }

        #endregion . Commands .

        private async void RunBp(string wfname)
        {
            try
            {
                WaitIndicatorVisible = true;
                var context = new BpContext();
                await RunBpAsync(wfname, context, DefaultCompletedHandler);
            }
            catch (Exception)
            {
                WaitIndicatorVisible = false;
                throw;
            }
        }

        #endregion .  Methods  .
    }
}
