using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using wmsMLC.General;
using wmsMLC.RCL.Launcher.Common;
using wmsMLC.RCL.Launcher.Properties;

namespace wmsMLC.RCL.Launcher.Forms
{
    public partial class MainForm : Form
    {
        private const int MinTimerIntervalMs = 100;
        private readonly CryptoKeyProvider _cryptoKeyProvider;
        private RdpProcessBase _process;         

        public MainForm()
        {
            _cryptoKeyProvider = new CryptoKeyProvider();
            _cryptoKeyProvider.AddOrChangeKey(0,
               new byte[] { 0x54, 0x90, 0xd8, 0xab, 0xbc, 0xd3, 0xf7, 0xe4, 0x58, 0x37, 0xb8, 0xb3, 0x45 });
            _cryptoKeyProvider.AddOrChangeKey(1,
                new byte[] { 0x37, 0x56, 0x3e, 0x4b, 0xc6, 210, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0xcd, 0xf4 });
            InitializeComponent();

            timer1.Enabled = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            Text = Global.AppNamePlusVersion;

            base.OnLoad(e);

            var clearWaitMs = Settings.Default.ClearWaitMs;
            if (clearWaitMs < MinTimerIntervalMs)
                clearWaitMs = MinTimerIntervalMs;
            timer1.Interval = clearWaitMs;

            if (Settings.Default.SetLoginOnLoad)
            {
                try
                {
                    var settings = new WinRegistrySettings();
                    settings.Load();
                    if (!string.IsNullOrEmpty(settings.Login))
                    {
                        txtLogin.Text = settings.Login;
                        txtPwd.Focus();
                    }
                }
                catch (Exception ex)
                {
                    Global.ShowError(ex);
                }
            }

            var computerName = Settings.Default.ComputerName;
            if (!string.IsNullOrEmpty(computerName))
            {
                try
                {
                    var settings = new WinRegistrySystem();
                    settings.Load();
                    if (!computerName.EqIgnoreCase(settings.Name))
                    {
                        settings.Name = computerName;
                        settings.Save();
                    }
                }
                catch (Exception ex)
                {
                    Global.ShowError(ex);
                }
            }

            var badComputerNames = Settings.Default.BadComputerNames;
            if (badComputerNames != null && badComputerNames.Length > 0)
            {
                try
                {
                    var net = new NetInfo();
                    var cn = net.GetHostName();
                    if (string.IsNullOrEmpty(cn) || badComputerNames.Where(p => !string.IsNullOrEmpty(p)).Any(p => p.EqIgnoreCase(cn)))
                    {
                        Global.ShowError(string.Format(Resources.BadComputerName, cn, Environment.NewLine, Resources.FatalErrorExit));
                        Application.Exit();
                    }
                }
                catch (Exception ex)
                {
                    Global.ShowError(ex);
                }
            }

            if (Settings.Default.RdcType == RdcType.CetscHoneywell && Settings.Default.Hw6500NeedWriteToRegister)
            {
                try
                {
                    var hwregister = new WinRegistryHw6500();
                    hwregister.Load();

                    if (hwregister.RemapToVkF1 != 1)
                    {
                        hwregister.RemapToVkF1 = 1;
                        hwregister.Save();
                    }
                }
                catch (Exception ex)
                {
                    Global.ShowError(ex);
                }
            }           
        }

        protected override void OnClosed(EventArgs e)
        {
            ProcessDispose();
            base.OnClosed(e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var control = sender as Control;
            if (control == null)
                return;

            switch (e.KeyData)
            {
                case Keys.Up:
                    e.Handled = true;
                    SelectNextControl(control, false, true, true, true);
                    return;
                case Keys.Down:
                    e.Handled = true;
                    SelectNextControl(control, true, true, true, true);
                    return;
                case Keys.Enter:
                    if (control.Name.ToLower() == "txtlogin")
                    {
                        try
                        {
                            control.Text = DecryptText(control.Text, 0);
                        }
                        catch (Exception ex)
                        {
                            Global.ShowError(ex);
                        }
                    }
                    else if (control.Name.ToLower() == "txtpwd")
                    {
                        e.Handled = true;
                        CreateRdpParameters();
                        return;
                    }
                    e.Handled = true;
                    SelectNextControl(control, true, true, true, true);
                    return;
                case Keys.Escape:
                    Exit();
                    return;
            }
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;
            if (!string.IsNullOrEmpty(textBox.Text))
                textBox.SelectAll();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            try
            {
                var net = new NetInfo();
                var ip4 = string.Empty;
                var ip4Info = net.GetIp4();
                if (ip4Info.Length > 0)
                    ip4 = ip4Info[0];

                var values = new List<Info>
                {
                    new Info {Name = Resources.ComputerName, Value = net.GetHostName()},
                    //new Info {Name = "IP", Value = string.Join(", ", net.GetIp4())},
                    new Info {Name = "IP", Value = ip4},
                    new Info {Name = "Server", Value = Settings.Default.Server},
                    new Info {Name = Resources.DomainName, Value = Settings.Default.Domain},
                };
                var form = new FrmInfo {Values = values.ToArray()};
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                Global.ShowError(ex);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            CreateRdpParameters();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void CreateRdpParameters()
        {
            try
            {
                ProcessDispose();
                if (!Validate())
                    return;

                var login = DecryptText(txtLogin.Text, 0);
                new WinRegistrySettings {Login = login}.Save();
                var pwd = DecryptText(txtPwd.Text, 1);

                _process = CreateProcess(login, pwd);
                if (_process != null)
                {
                    _process.Run();
                    timer1.Enabled = true;
                }                
            }
            catch (Exception ex)
            {
                ProcessDispose();
                Global.ShowError(ex);
            }
        }

        private RdpProcessBase CreateProcess(string login, string pwd)
        {
            RdpProcessBase result = null;
            switch (Settings.Default.RdcType)
            {
                case RdcType.MotoTscClient:
                    result = new MotoTscClientProcess
                    {
                        Login = login,
                        Pwd = pwd
                    };
                    break;
                case RdcType.CetscHoneywell:
                    result = new CetscHoneywellProcess
                    {
                        Login = login,
                        Pwd = pwd
                    };
                    break;
            }

            return result;
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(txtLogin.Text))
            {
                txtLogin.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtPwd.Text))
            {
                txtPwd.Focus();
                return false;
            }

            return true;
        }

        private string DecryptText(string txt, int index)
        {
            var result = txt;
            if (result == null)
                return string.Empty;
            string value;
            return ParseBarcode(result, index, out value) ? value : result;
        }

        private bool ParseBarcode(string code, int index, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(code))
                return false;

            var descr = _cryptoKeyProvider.GetKey(index);
            var txt = CryptoHelper.Decrypt(code, descr);
            if (!string.IsNullOrEmpty(txt))
            {
                value = txt;
                return true;
            }
            return false;
        }

        private void Exit()
        {
            Close();
        }

        private void ProcessDispose()
        {
            if (_process != null)
            {
                _process.Clear();
                _process.Dispose();
                _process = null;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (_process != null)
                ProcessDispose();
            Exit();
        }
    }
}