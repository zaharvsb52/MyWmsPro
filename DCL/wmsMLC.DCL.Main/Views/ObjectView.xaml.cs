namespace wmsMLC.DCL.Main.Views
{
    public partial class ObjectView : ObjectViewBase
    {
        public ObjectView()
        {
            InitializeComponent();
            Initialize();
        }

        #region .  Methods  .
        private void Initialize()
        {
            Group = objectDataLayout;
            MenuView = MmenuView;
        }
        #endregion .  Methods  .
    }
}
