using System;
using System.Windows;
using System.Xml;
using DevExpress.Xpf.Core.Native;
using wmsMLC.General;

namespace wmsMLC.DCL.Main.Helpers
{
    public class SaveRestoreLayoutWindowHelper
    {
        private const string WindowSizeAttributeName = "WindowSize";
        private const string WindowPositionAttributeName = "WindowPosition";
        private const string VersionAttributeName = "Version";
        private const string WidthAttributeName = "Width";
        private const string HeightAttributeName = "Height";
        private const string WindowStateAttributeName = "WindowState";
        private const string WindowStartupLocationAttributeName = "WindowStartupLocation";
        private const string TopAttributeName = "Top";
        private const string LeftAttributeName = "Left";

        public static Version Version
        {
            get { return new Version(1, 1); }
        }

        public string CurrentVersion { get; private set; }

        public void SaveToXmlWindowSize(string file, Window window)
        {
            using (var xmlW = XmlWriter.Create(file))
            {
                xmlW.WriteStartElement(WindowSizeAttributeName);
                xmlW.WriteAttributeString(VersionAttributeName, Version.ToString());
                window.WritePropertyToXML(xmlW, FrameworkElement.WidthProperty, WidthAttributeName);
                window.WritePropertyToXML(xmlW, FrameworkElement.HeightProperty, HeightAttributeName);
                window.WritePropertyToXML(xmlW, Window.WindowStateProperty, WindowStateAttributeName);
                xmlW.WriteAttributeString(WindowStartupLocationAttributeName, window.WindowStartupLocation.ToString());
                xmlW.WriteEndElement();
                xmlW.Close();
            }
        }

        public void LoadFromXmlWindowSize(string file, Window window)
        {
            using (var xmlR = XmlReader.Create(file))
            {
                while (xmlR.Read())
                {
                    if (xmlR.Name == WindowSizeAttributeName)
                    {
                        CurrentVersion = xmlR[VersionAttributeName];
                        window.SizeToContent = SizeToContent.Manual;
                        window.ReadPropertyFromXML(xmlR, FrameworkElement.WidthProperty, WidthAttributeName, typeof(double));
                        window.ReadPropertyFromXML(xmlR, FrameworkElement.HeightProperty, HeightAttributeName, typeof(double));
                        window.ReadPropertyFromXML(xmlR, Window.WindowStateProperty, WindowStateAttributeName, typeof(WindowState));
                        var windowStartupLocation = xmlR[WindowStartupLocationAttributeName];
                        window.WindowStartupLocation = windowStartupLocation.To(WindowStartupLocation.CenterScreen);
                        ValidateSizeWindowToDesctop(window);
                        ValidateWindowStartupLocation(window);
                        break;
                    }
                }
            }
        }

        public void SaveToXmlWindowPosition(string file, Window window)
        {
            using (var xmlW = XmlWriter.Create(file))
            {
                xmlW.WriteStartElement(WindowPositionAttributeName);
                xmlW.WriteAttributeString(VersionAttributeName, Version.ToString());
                window.WritePropertyToXML(xmlW, Window.TopProperty, TopAttributeName);
                window.WritePropertyToXML(xmlW, Window.LeftProperty, LeftAttributeName); 
                xmlW.WriteEndElement();
                xmlW.Close();
            }
        }

        public void LoadFromXmlWindowPosition(string file, Window window)
        {
            using (var xmlR = XmlReader.Create(file))
            {
                while (xmlR.Read())
                {
                    if (xmlR.Name == WindowPositionAttributeName)
                    {
                        CurrentVersion = xmlR[VersionAttributeName];
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.ReadPropertyFromXML(xmlR, Window.TopProperty, TopAttributeName, typeof(double));
                        window.ReadPropertyFromXML(xmlR, Window.LeftProperty, LeftAttributeName, typeof(double));
                        //ValidateWindowPosition(window);
                        break;
                    }
                }
            }
        }

        private double GetPrimaryScreenWorkingAreaWidth()
        {
            //return System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            return SystemParameters.FullPrimaryScreenWidth;
        }

        private double GetPrimaryScreenWorkingAreaHeight()
        {
            //return System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            return SystemParameters.FullPrimaryScreenHeight;
        }

        public void ValidateSizeWindowToDesctop(Window window)
        {
            var windowState = window.WindowState;

            try
            {
                var screenWidth = GetPrimaryScreenWorkingAreaWidth();
                if (window.Width > screenWidth)
                    window.Width = screenWidth;

                var screenHeight = GetPrimaryScreenWorkingAreaHeight();
                if (window.Height > screenHeight)
                    window.Height = screenHeight;
            }
            finally
            {
                window.WindowState = windowState;
            }
        }

        //public void ValidateWindowPosition(Window window)
        //{
        //    var windowHeight = window.Height;
        //    var windowWidth = window.Width;

        //    if (window.Top + windowHeight / 2 > SystemParameters.VirtualScreenHeight)
        //        window.Top = SystemParameters.VirtualScreenHeight - windowHeight;

        //    if (window.Left + windowWidth / 2 > SystemParameters.VirtualScreenWidth)
        //        window.Left = SystemParameters.VirtualScreenWidth - windowWidth;

        //    if (window.Top < 0)
        //        window.Top = 0;

        //    if (window.Left < 0)
        //        window.Left = 0;
        //}

        public void ValidateWindowStartupLocation(Window window)
        {
            switch (window.WindowStartupLocation)
            {
                case WindowStartupLocation.CenterScreen:
                    CenterWindow(window, GetPrimaryScreenWorkingAreaWidth(), GetPrimaryScreenWorkingAreaHeight());
                    break;
                case WindowStartupLocation.CenterOwner:
                    if (window.Owner == null)
                        return;
                    CenterWindow(window, window.Owner.Width, window.Owner.Height);
                    break;
            }
        }

        private void CenterWindow(Window window, double width, double height)
        {
            if (!double.IsNaN(width) && !double.IsInfinity(width))
            {
                window.Left = (width - window.Width) / 2;   
            }
            if (!double.IsNaN(height) && !double.IsInfinity(height))
            {
                window.Top = (height - window.Height) / 2;
            }
        }
    }
}
