using System.Collections.Generic;
using System.ComponentModel;

namespace wmsMLC.General.BL.Validation
{
    public interface IValidator : INotifyPropertyChanged
    {
        ValidateErrors Errors { get; }
        bool HasCriticalError();

        bool IsYourVlidatableObject(object obj);

        void Validate();
        void ValidateProperty(string propertyName);
        void ValidateChild(ValidateEventsArgs args);
        void AddSelfErrors(IEnumerable<ValidateErrorInfo> errors);
        ValidateErrorInfo[] GetSelfAndChildsErrors();
        void ClearSelfAndChildsErrors();

        void SuspendValidating();
        void ResumeValidating();

        string GetErrorDescription();
        string GetErrorDescription(string propertyName);
        ValidateErrorLevel GetMaxErrorLevel();
        ValidateErrorLevel GetMaxErrorLevel(string propertyName);
        string GetCriticalErrorDescription();

        bool HasChanges();

        void Revert();

        /// <summary>
        /// Событие, котое вызывается после всех собственных проверок объекта, чтобы подписчики проверили себя
        /// </summary>
        event ValidateEventHandler ValidateMe;
        void RefreshValidateMeSubscription();
        void ResetValidateMeSubscription();

        ValidateErrors GetAllErrors(string propertyName);
    }
}
