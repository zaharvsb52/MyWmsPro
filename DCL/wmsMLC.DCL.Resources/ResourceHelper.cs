using System;
using System.Drawing;
using System.Windows.Media;
using WPFLocalizeExtension.Extensions;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Resources
{
    public static class ResourceHelper
    {
        #region старая версия
        //public static string DefaultUnknownImageName = "Error64.png";

        //public static string GetImageResourceName(string imageName, bool fromExternal = true)
        //{
        //    if (fromExternal)
        //        return string.Format("pack://application:,,,/wmsMLC.DCL.Resources;component/Resources/{0}", imageName);

        //    return string.Format("pack://application:,,,/Resources/{0}", imageName);
        //}

        //public static ImageSource GetImageByName(string name)
        //{
        //    var uri = new Uri(GetImageResourceName(name, true));
        //    return new BitmapImage(uri);
        //}

        //public static ImageSource GetImageForAction(string actionName)
        //{
        //    var name = DefaultUnknownImageName;
        //    switch (actionName)
        //    {
        //        case ActionNames.Add:
        //            name = "DCLAddNew16.png";
        //            break;

        //        case ActionNames.Delete:
        //            name = "DCLDelete16.png";
        //            break;

        //        case ActionNames.Edit:
        //            name = "DCLEdit16.png";
        //            break;

        //        case ActionNames.Save:
        //            name = "DCLSave16.png";
        //            break;

        //        case ActionNames.List:
        //            name = "DCLDefault16.png";
        //            break;
        //    }

        //    var uri = new Uri(GetImageResourceName(name, true));
        //    return new BitmapImage(uri);
        //}
        #endregion старая версия

        public static string DefaultUnknownImageKey = "wmsMLC.DCL.Resources:ImageResources:Error64";

        public static ImageSource GetImageForAction(string actionName)
        {
            switch (actionName)
            {
                case ActionNames.Add:
                    return GetImage("wmsMLC.DCL.Resources:ImageResources:DCLAddNew16");

                case ActionNames.Delete:
                    return GetImage("wmsMLC.DCL.Resources:ImageResources:DCLDelete16");

                case ActionNames.Edit:
                    return GetImage("wmsMLC.DCL.Resources:ImageResources:DCLEdit16");

                case ActionNames.Save:
                    return GetImage("wmsMLC.DCL.Resources:ImageResources:DCLSave16");

                case ActionNames.List:
                    return GetImage("wmsMLC.DCL.Resources:ImageResources:DCLDefault16");

                default:
                    return GetImage(DefaultUnknownImageKey);
            }
        }

        public static ImageSource GetImageByName(string assemblyname, string resourcename, string resourcekey)
        {
            if (string.IsNullOrEmpty(assemblyname)) throw new ArgumentNullException("assemblyname");
            if (string.IsNullOrEmpty(resourcename)) throw new ArgumentNullException("resourcename");
            if (string.IsNullOrEmpty(resourcekey)) throw new ArgumentNullException("resourcekey");
            return GetImage(string.Format("{0}:{1}:{2}", assemblyname, resourcename, resourcekey));
        }

        private static ImageSource GetImage(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            var keyinternal = GetRightKey(key, ".png");
            object bitmap;
            var res = new LocExtension(keyinternal).ResolveLocalizedValue(out bitmap);
            if (res)
            {
                if (bitmap is Bitmap)
                    return ((Bitmap)bitmap).GetBitmapImage();

                throw new DeveloperException("Bad Image format.");
            }
            throw new DeveloperException("Can't find resource by the key '{0}'.", keyinternal);
        }

        //HACK: Убираем .png из ключа
        private static string GetRightKey(string key, string badendwith)
        {
            if (key.IsNullOrEmptyAfterTrim() || badendwith.IsNullOrEmptyAfterTrim()) return key;
            var result = key.GetTrim();
            return result.ToLower().EndsWith(badendwith.ToLower()) ? result.Left(result.Length - badendwith.Length) : key;
        }
    }

    public class ActionNames
    {
        public const string Add = "Add";
        public const string Delete = "Delete";
        public const string Edit = "Edit";
        public const string Save = "Save";
        public const string List = "List";
    }
}