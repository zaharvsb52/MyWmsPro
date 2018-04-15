using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Editors.Filtering;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Helpers
{
    public class FilterExpressionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strVal = value as string;
            if (string.IsNullOrEmpty(strVal))
                return strVal;

            // hack: fix problem with filtercontrol (if set new CriteriaOperator filter tree will be recreated)
            if (parameter == null)
                throw new DeveloperException("Converter wait the control as parameter");

            var filterControl = ((FrameworkElement)parameter).DataContext as FilterControl;
            if ((filterControl != null) && !Equals(filterControl.ActualFilterCriteria, null) && (filterControl.ActualFilterCriteria.LegacyToString() == strVal))
                return filterControl.ActualFilterCriteria;
            try
            {
                return CriteriaOperator.Parse(strVal);
            }
            catch (DevExpress.Data.Filtering.Exceptions.CriteriaParserException)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var co = value as CriteriaOperator;
            if (ReferenceEquals(co, null))
                return null;

            return co.LegacyToString();
        }
    }

    public class FilterHelperEx
    {
#if SILVERLIGHT
        public static readonly DependencyProperty FilterControlProperty =
            DependencyProperty.RegisterAttached("FilterControl", typeof(object), typeof(FilterHelperEx), new PropertyMetadata(null, new PropertyChangedCallback(FilterControlPropertyChanged)));
#else
        public static readonly DependencyProperty FilterControlProperty =
            DependencyProperty.RegisterAttached("FilterControl", typeof (object), typeof (FilterHelperEx),
                new UIPropertyMetadata(null, FilterControlPropertyChanged));
#endif

        private static void FilterControlPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            var filterControl = (FilterControl) e.NewValue;
            //INFO: вызывается 100500 раз на каждый чих
            //filterControl.LayoutUpdated += (s, a) => CheckFilterCriteria(filterControl);
            filterControl.DataContextChanged += (s, a) =>
            {
                var obj = a.NewValue as INotifyPropertyChanged;
                if (obj != null)
                    obj.PropertyChanged += (o, n) =>
                    {
                        DispatcherHelper.BeginInvoke(() => { CheckFilterCriteria(filterControl); }, DispatcherPriority.ApplicationIdle);
                    };
            };
        }
        
        private static void CheckFilterCriteria(FilterControl filterControl)
        {
            if (Equals(filterControl.ActualFilterCriteria, filterControl.FilterCriteria))
                return;

            //HACK: Почему-то периодически одинаковые значения в criteria при сравнении не равны
            if ((!ReferenceEquals(filterControl.ActualFilterCriteria, null)) &&
                (!ReferenceEquals(filterControl.FilterCriteria, null))
                && (string.Equals(filterControl.ActualFilterCriteria.LegacyToString(), filterControl.FilterCriteria.LegacyToString())))
                return;

            filterControl.Dispatcher.BeginInvoke((Action)filterControl.ApplyFilter, DispatcherPriority.ApplicationIdle);
            //filterControl.ApplyFilter();
        }
    
        #region CLRs

        public static object GetFilterControl(DependencyObject obj)
        {
            return obj.GetValue(FilterControlProperty);
        }

        public static void SetFilterControl(DependencyObject obj, object value)
        {
            obj.SetValue(FilterControlProperty, value);
        }
     
        #endregion

        private static RemoveCriteriaVisitor _visitor;

        private static RemoveCriteriaVisitor Visitor
        {
            get { return _visitor ?? (_visitor = new RemoveCriteriaVisitor()); }
        }

        public static CriteriaOperator RemoveCriteriaWithNotSetValue(CriteriaOperator op)
        {
            return Visitor.RemoveCriteriaWithNotSetValue(op);
        }
    }

    /// <summary>
    /// http://www.devexpress.com/Support/Center/Example/Details/E3396
    /// </summary>
    public class RemoveCriteriaVisitor : IClientCriteriaVisitor<CriteriaOperator>
    {
        public CriteriaOperator RemoveCriteriaWithNotSetValue(CriteriaOperator op)
        {
            if (ReferenceEquals(op, null))
                return null;

            return op.Accept(this) as CriteriaOperator;
        }

        #region IClientCriteriaVisitor Members

        public CriteriaOperator Visit(JoinOperand theOperand)
        {
            var condition = theOperand.Condition.Accept(this) as CriteriaOperator;
            var expression = theOperand.AggregatedExpression.Accept(this) as CriteriaOperator;
            if (object.ReferenceEquals(condition, null) || object.ReferenceEquals(expression, null))
                return null;
            return new JoinOperand(theOperand.JoinTypeName, condition, theOperand.AggregateType, expression);
        }

        public CriteriaOperator Visit(OperandProperty theOperand)
        {
            return theOperand;
        }

        public CriteriaOperator Visit(AggregateOperand theOperand)
        {
            var operand = theOperand.CollectionProperty.Accept(this) as OperandProperty;
            var condition = theOperand.Condition.Accept(this) as CriteriaOperator;
            var expression = theOperand.AggregatedExpression.Accept(this) as CriteriaOperator;
            if (object.ReferenceEquals(condition, null) || object.ReferenceEquals(expression, null) ||
                object.ReferenceEquals(operand, null))
                return null;

            return new AggregateOperand(operand, expression, theOperand.AggregateType, condition);
        }
        #endregion

        #region ICriteriaVisitor Members

        public CriteriaOperator Visit(FunctionOperator theOperator)
        {
            var operators = new List<CriteriaOperator>();
            foreach (var op in theOperator.Operands)
            {
                var temp = op.Accept(this) as CriteriaOperator;
                if (object.ReferenceEquals(temp, null))
                {
                    operators.Clear();
                    continue;
                }
                operators.Add(temp);
            }
            if (operators.Count == 0)
                return null;

            return new FunctionOperator(theOperator.OperatorType, operators);
        }

        public CriteriaOperator Visit(OperandValue theOperand)
        {
            if (theOperand.Value == null)
                return null;
            return theOperand;
        }

        public CriteriaOperator Visit(GroupOperator theOperator)
        {
            List<CriteriaOperator> operators = new List<CriteriaOperator>();
            foreach (CriteriaOperator op in theOperator.Operands)
            {
                CriteriaOperator temp = op.Accept(this) as CriteriaOperator;
                if (object.ReferenceEquals(temp, null)) continue;
                operators.Add(temp);
            }
            return new GroupOperator(theOperator.OperatorType, operators);
        }

        public CriteriaOperator Visit(InOperator theOperator)
        {
            CriteriaOperator leftOperand = theOperator.LeftOperand.Accept(this) as CriteriaOperator;
            List<CriteriaOperator> operators = new List<CriteriaOperator>();
            foreach (CriteriaOperator op in theOperator.Operands)
            {
                CriteriaOperator temp = op.Accept(this) as CriteriaOperator;
                if (object.ReferenceEquals(temp, null)) continue;
                operators.Add(temp);
            }
            if (object.ReferenceEquals(leftOperand, null)) return null;
            return new InOperator(leftOperand, operators);
        }

        public CriteriaOperator Visit(UnaryOperator theOperator)
        {
            CriteriaOperator operand = theOperator.Operand.Accept(this) as CriteriaOperator;
            if (object.ReferenceEquals(operand, null)) return null;
            return new UnaryOperator(theOperator.OperatorType, operand);
        }

        public CriteriaOperator Visit(BinaryOperator theOperator)
        {
            CriteriaOperator leftOperand = theOperator.LeftOperand.Accept(this) as CriteriaOperator;
            CriteriaOperator rightOperand = theOperator.RightOperand.Accept(this) as CriteriaOperator;
            if (object.ReferenceEquals(leftOperand, null) || object.ReferenceEquals(rightOperand, null)) return null;
            return new BinaryOperator(leftOperand, rightOperand, theOperator.OperatorType);
        }

        public CriteriaOperator Visit(BetweenOperator theOperator)
        {
            CriteriaOperator test = theOperator.TestExpression.Accept(this) as CriteriaOperator;
            CriteriaOperator begin = theOperator.BeginExpression.Accept(this) as CriteriaOperator;
            CriteriaOperator end = theOperator.EndExpression.Accept(this) as CriteriaOperator;
            if (object.ReferenceEquals(test, null) || object.ReferenceEquals(begin, null) ||
                object.ReferenceEquals(end, null)) return null;
            return new BetweenOperator(test, begin, end);
        }

        #endregion
    }
}