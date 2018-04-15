using System.ComponentModel;
using Microsoft.Win32;

namespace wmsMLC.RCL.Launcher.Common
{
    public abstract class WinRegistryBase
    {
        protected const string HivePathPropertyName = "HivePath";

        public string HivePath { get; set; }

        public virtual void Load()
        {
            RegistryKey rk = null;

            try
            {
                rk = OpenKey().OpenSubKey(HivePath);
                if (rk == null)
                    return;

                var type = GetType();
                var ds = TypeDescriptor.GetProperties(type);
                var typeWinRegistryBase = typeof(WinRegistryBase);
                foreach (PropertyDescriptor p in ds)
                {
                    if (p.IsReadOnly || p.Name.EqIgnoreCase(HivePathPropertyName) ||
                        typeWinRegistryBase.IsAssignableFrom(p.PropertyType))
                        continue;

                    var value = rk.GetValue(p.Name);
                    if (rk.GetValueKind(p.Name) == RegistryValueKind.DWord && value == null)
                        value = 0;
                    
                    p.SetValue(this, value);
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        }

        public virtual void Save()
        {
            RegistryKey rk = null;

            try
            {
                rk = OpenKey().OpenSubKey(HivePath, true) ?? OpenKey().CreateSubKey(HivePath);
                var type = GetType();
                var ds = TypeDescriptor.GetProperties(type);
                var typeWinRegistryBase = typeof (WinRegistryBase);
                foreach (PropertyDescriptor p in ds)
                {
                    if (p.IsReadOnly || p.Name.EqIgnoreCase(HivePathPropertyName) ||
                        typeWinRegistryBase.IsAssignableFrom(p.PropertyType))
                        continue;

                    var value = p.GetValue(this);
                    if (value == null)
                        continue;
                    rk.SetValue(p.Name, value);
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        }

        public virtual void Clear()
        {
        }

        public virtual void Delete()
        {
            RegistryKey rk = null;

            try
            {
                rk = OpenKey().OpenSubKey(HivePath, true);
                if (rk == null)
                    return;

                foreach (var p in rk.GetSubKeyNames())
                {
                    rk.DeleteSubKeyTree(p);
                }
            }
            finally
            {
                if (rk != null)
                    rk.Close();
            }
        }

        protected virtual RegistryKey OpenKey()
        {
            return Registry.CurrentUser;
        }
    }
}
