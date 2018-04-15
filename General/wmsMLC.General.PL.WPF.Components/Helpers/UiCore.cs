using System.Windows.Input;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public static class UiCore
    {
        //static UiCore()
        //{
        //    ReturnedScannerKey = Properties.Settings.Default.ReturnedScannerKey.To(Key.None);
        //    NextControlKey = Properties.Settings.Default.NextControlKey.To(Key.Enter);
        //}

        /// <summary>
        /// Завершающий код сканера.
        /// </summary>
        public static Key ReturnedScannerKey { get; set; }

        /// <summary>
        /// Код перехода на следующий контрол.
        /// </summary>
        public static Key NextControlKey { get; set; }
    }
}
