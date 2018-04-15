using System.Windows;
using System.Windows.Input;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.General;
using System.Windows.Forms;
using System.Windows.Media;

namespace wmsMLC.DCL.Main.Helpers
{
    public static class Help
    {
        static Help()
        {
            CommandManager.RegisterClassCommandBinding(typeof (FrameworkElement),
                                                       new CommandBinding(ApplicationCommands.Help, Executed, CanExecute));
        }

        #region Filename

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.RegisterAttached("FileName", typeof (string), typeof (Help));

        public static string GetFileName(DependencyObject d)
        {
            return (string) d.GetValue(FileNameProperty);
        }

        public static void SetFileName(DependencyObject d, string value)
        {
            d.SetValue(FileNameProperty, value);
        }

        #endregion

        #region Keyword

        public static readonly DependencyProperty KeywordProperty =
            DependencyProperty.RegisterAttached("Keyword", typeof (string), typeof (Help));

        public static string GetKeyword(DependencyObject d)
        {
            return (string) d.GetValue(KeywordProperty);
        }

        public static void SetKeyword(DependencyObject d, string value)
        {
            d.SetValue(KeywordProperty, value);
        }

        #endregion

        #region SpecialKeyword

        public static readonly DependencyProperty SpecialKeywordProperty =
            DependencyProperty.RegisterAttached("SpecialKeyword", typeof(string), typeof(Help));

        public static string GetSpecialKeyword(DependencyObject d)
        {
            return (string)d.GetValue(SpecialKeywordProperty);
        }

        public static void SetSpecialKeyword(DependencyObject d, string value)
        {
            d.SetValue(SpecialKeywordProperty, value);
        }

        #endregion

        #region Helpers

        private static void CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            var el = sender as FrameworkElement;
            if (el == null)
                return;

            var fileName = FindFileName(el);
            if (!string.IsNullOrEmpty(fileName))
                args.CanExecute = true;

        }

        private static void Executed(object sender, ExecutedRoutedEventArgs args)
        {
            var parent = args.OriginalSource as DependencyObject;
            var keyword = GetKeyword(parent);
            var specword = GetSpecialKeyword(parent);
            var fileName = FindFileName(parent);
            if (!System.IO.File.Exists(fileName))
                throw new DeveloperException(string.Format("Не найден файл справки"));
            if (string.IsNullOrEmpty(keyword))
                keyword = FindKeyword(parent, keyword);
            if (string.IsNullOrEmpty(specword))
                specword = FindSpecialKeyword(parent);
            if (!string.IsNullOrEmpty(specword))
                keyword = string.Format("{0}{1}", keyword, specword);
            if (!string.IsNullOrEmpty(keyword))
                System.Windows.Forms.Help.ShowHelp(null, fileName, HelpNavigator.KeywordIndex, keyword);
            else
                System.Windows.Forms.Help.ShowHelp(null, fileName);
        }

        private static string FindFileName(DependencyObject sender)
        {
            if (sender == null)
                return null;
            var fileName = GetFileName(sender);
            return !string.IsNullOrEmpty(fileName) ? fileName : FindFileName(VisualTreeHelper.GetParent(sender));
        }

        //private static string FindKeyword(DependencyObject sender)
        //{
        //    if (sender == null)
        //        return null;
        //    var keyword = GetKeyword(sender);
        //    return !string.IsNullOrEmpty(keyword) ? keyword : FindKeyword(VisualTreeHelper.GetParent(sender));
        //}

        private static string FindSpecialKeyword(DependencyObject sender)
        {
            if (sender == null)
                return null;
            var word = GetSpecialKeyword(sender);
            if (!string.IsNullOrEmpty(word))
                return word;
            var code = sender as IHelpHandler;
            if (code != null)
            {
                var str = code.GetHelpEntity();
                if (!string.IsNullOrEmpty(str))
                    return str;
            }
            return FindSpecialKeyword(VisualTreeHelper.GetParent(sender));
        }


        private static string FindKeyword(DependencyObject sender, string word)
        {
            if (sender == null)
                return word;
            var newWord = GetKeyword(sender);
            if (!string.IsNullOrEmpty(newWord))
                word = string.Format("{0}.{1}", newWord, word);
            else
            {
                var code = sender as IHelpHandler;
                if (code != null)
                {
                    var str = code.GetHelpLink();
                    if (!string.IsNullOrEmpty(str))
                        word = string.Format("{0}.{1}", str, word);
                }
            }
            return FindKeyword(VisualTreeHelper.GetParent(sender), word);
        }


        #endregion
    }
}

