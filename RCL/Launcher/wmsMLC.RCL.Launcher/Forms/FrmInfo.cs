using System;
using System.Windows.Forms;
using wmsMLC.RCL.Launcher.Common;

namespace wmsMLC.RCL.Launcher.Forms
{
    public partial class FrmInfo : Form
    {
        public FrmInfo()
        {
            InitializeComponent();
        }

        public Info[] Values { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Values == null)
                return;

            foreach (var v in Values)
            {
                var item = new ListViewItem
                {
                    Text = v.Name
                };
                item.SubItems.Add(v.Value);
                lvInfo.Items.Add(item);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}