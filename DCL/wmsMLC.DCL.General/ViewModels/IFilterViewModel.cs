using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IFilterViewModel : IViewModel, IDisposable
    {
        int? MaxRowCount { get; set; }
        string DefaultFilterExpression { get; set; }
        string StrongFilterExpression { get; set; }
        string SqlFilterExpression { get; set; }
        string FilterExpression { get; set; }
        decimal? MandantId { get; set; }

        ObservableCollection<DataField> Fields { get; }
        ICommand ApplyFilterCommand { get; set; }

        void RiseFixFilterExpression();
        event EventHandler FixFilterExpressionRequest;

        void ToDefault();
        string GetSqlExpression();
        string GetSqlExpression(string filterExpression);
        string GetExpression();

        void AcceptChanges();
        void RejectChanges();

        bool IsValid { get; }

        bool IsFilterMode { get; set; }
        bool IsRowCountEnabled { get; set; }
    }

    public interface IFilterViewModel<T> : IFilterViewModel
    {
    }
}