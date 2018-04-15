using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace wmsMLC.General.PL.WPF.Helpers
{
    public static class VisualTreeHelperExt
    {
        public class ProcessContext
        {
            public bool SkipChildren { get; set; }
            public bool StopEnumerate { get; set; }
        }

        private static bool ProcessElement(DependencyObject obj, bool isrestoredLayout, Action<DependencyObject, ProcessContext> action, out ProcessContext context)
        {
            context = new ProcessContext();
            if (isrestoredLayout && !context.StopEnumerate)
            {
                var fe = obj as FrameworkElement;
                if (fe != null)
                    context.StopEnumerate = IsElementRestored(fe.Parent);
                if (!context.StopEnumerate)
                    context.StopEnumerate = IsElementRestored(obj);
            }
            action(obj, context);
            return !context.StopEnumerate;
        }

        public static bool IsElementRestored(object o)
        {
            var restoreLayoutInfo = o as IRestoreLayoutInfo;
            return restoreLayoutInfo != null && restoreLayoutInfo.IsRestored;
        }

        public static bool ProcessElement(DependencyObject obj, Action<DependencyObject, ProcessContext> action)
        {
            return ProcessElement(obj, false, action);
        }

        public static bool ProcessElement(DependencyObject obj, bool isrestoredLayout, Action<DependencyObject, ProcessContext> action)
        {
            ProcessContext context;
            return ProcessElement(obj, isrestoredLayout, action, out context);
        }

        /// <summary>
        /// Пробежать по дереву элементов и для каждого выполнить действие
        /// </summary>
        /// <param name="obj">корневой элемент ветки</param>
        /// <param name="isrestoredLayout">метод используется для восстановления вида?</param>
        /// <param name="action">необходимое действие</param>
        /// <returns>True - если обшли все дерево</returns>
        public static bool ProcessVisualChildren(DependencyObject obj, bool isrestoredLayout, Action<DependencyObject, ProcessContext> action)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(obj, i);
                ProcessContext context;
                var result = ProcessElement(childVisual, isrestoredLayout, action, out context);
                if (!result)
                    return false;

                if (!context.SkipChildren)
                    if (!ProcessVisualChildren(childVisual, isrestoredLayout, action))
                        return false;
            }
            return true;
        }

        public static bool ProcessVisualChildren(DependencyObject obj, Action<DependencyObject, ProcessContext> action)
        {
            return ProcessVisualChildren(obj, false, action);
        }

        public static bool ProcessChildren(DependencyObject obj, Action<DependencyObject, ProcessContext> action)
        {
            return ProcessChildren(obj, false, action);
        }

        public static bool ProcessChildren(DependencyObject obj, bool isrestoredLayout, Action<DependencyObject, ProcessContext> action)
        {
            if (obj == null)
                return true;

            var children = LogicalTreeHelper.GetChildren(obj);
            foreach (var child in children.OfType<DependencyObject>())
            {
                ProcessContext context;
                var result = ProcessElement(child, isrestoredLayout, action, out context);
                if (!result)
                    return false;

                if (!context.SkipChildren)
                    if (!ProcessChildren(child, isrestoredLayout, action))
                        return false;
            }

            return true;
        }

        public static string PrintLogicalTree(object obj, int indent = 0)
        {
            var element = obj as DependencyObject;
            if (element == null)
                return string.Empty;

            var space = string.Empty;
            for (int count = 0; count < indent; count++)
                space += "-";

            var str = space + element.GetType() + Environment.NewLine;
            var children = LogicalTreeHelper.GetChildren(element);
            indent++;
            foreach (var child in children)
                str += PrintLogicalTree(child, indent);

            return str;
        }

        public static string PrintVisualTree(object obj, int indent = 0)
        {
            var element = obj as FrameworkElement;
            if (element == null)
                return string.Empty;

            var space = string.Empty;
            for (int count = 0; count < indent; count++)
                space += "-";

            var str = space + element.GetType() + Environment.NewLine;
            var childrenCount = VisualTreeHelper.GetChildrenCount(element);
            indent++;

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                str += PrintVisualTree(child, indent);
            }

            return str;
        }

        public static DependencyObject FindChild(DependencyObject obj, Type type)
        {
            if (obj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    var child = VisualTreeHelper.GetChild(obj, i);

                    if (child.GetType() == type)
                    {
                        return child;
                    }

                    var childReturn = FindChild(child, type);
                    if (childReturn != null)
                    {
                        return childReturn;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<DependencyObject> FindChildsByType(DependencyObject obj, Type type)
        {
            if (obj != null)
            {
                if (obj.GetType() == type)
                {
                    yield return obj;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    foreach (var child in FindChildsByType(VisualTreeHelper.GetChild(obj, i), type))
                    {
                        if (child != null)
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        public static T[] FindChildsByType<T>(DependencyObject obj)
        {
            return FindChildsByType(obj, typeof (T)).OfType<T>().ToArray();
        }

        public static T GetLogicalParent<T>(DependencyObject element) where T : DependencyObject
        {
            var oParent = element;
            var oTargetType = typeof(T);
            do
            {
                oParent = LogicalTreeHelper.GetParent(oParent);
            }
            while (
                !(
                    oParent == null
                    || oParent.GetType() == oTargetType
                    || oParent.GetType().IsSubclassOf(oTargetType)
                )
            );

            return oParent as T;
        }
    }

    public interface IRestoreLayoutInfo
    {
        bool IsRestored { get; }
    }
}