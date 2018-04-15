using System;
using System.ComponentModel;
using System.Windows.Input;

namespace wmsMLC.General.PL.WPF.Commands
{
    /// <summary>
    /// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
    ///// It also implements the <see cref="IActiveAware"/> interface, which is useful when registering this command in a <see cref="CompositeCommand"/> that monitors command's activity.
    /// </summary>
    /// <typeparam name="T">Parameter type.</typeparam>
    /// <remarks>
    /// The constructor deliberately prevent the use of value types.
    /// Because ICommand takes an object, having a value type for T would cause unexpected behavior when CanExecute(null) is called during XAML initialization for command bindings.
    /// Using default(T) was considered and rejected as a solution because the implementor would not be able to distinguish between a valid and defaulted values.
    /// <para/>
    /// Instead, callers should support a value type by using a nullable value type and checking the HasValue property before using the Value property.
    /// <example>
    ///     <code>
    /// public MyClass()
    /// {
    ///     this.submitCommand = new DelegateCustomCommand&lt;int?&gt;(this.Submit, this.CanSubmit);
    /// }
    /// 
    /// private bool CanSubmit(int? customerId)
    /// {
    ///     return (customerId.HasValue &amp;&amp; customers.Contains(customerId.Value));
    /// }
    ///     </code>
    /// </example>
    /// </remarks>
    public class DelegateCustomCommand<T> : DelegateCustomCommandBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCustomCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public DelegateCustomCommand(Action<T> executeMethod)
            : this(executeMethod, o => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCustomCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command.  This can be null.</param>
        /// <exception cref="ArgumentNullException">When both <paramref name="executeMethod"/> and <paramref name="canExecuteMethod"/> ar <see langword="null" />.</exception>
        public DelegateCustomCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base(executeMethod == null ? null : new Action<object>(o => executeMethod((T) o)),
                canExecuteMethod == null ? null : new Func<object, bool>(o => canExecuteMethod((T) o)))
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.StringResources.DelegateCommandDelegatesCannotBeNull);

            var genericType = typeof (T);

            // DelegateCustomCommand allows object or Nullable<>.  
            // note: Nullable<> is a struct so we cannot use a class constraint.
            if (genericType.IsValueType)
            {
                if ((!genericType.IsGenericType) ||
                    (!typeof (Nullable<>).IsAssignableFrom(genericType.GetGenericTypeDefinition())))
                    throw new InvalidCastException(Resources.StringResources.DelegateCommandInvalidGenericPayloadType);
            }
        }

         public DelegateCustomCommand(INotifyPropertyChanged model, Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod)
        {
            if (model != null)
                model.PropertyChanged += delegate { RaiseCanExecuteChanged(); };
        }

        ///<summary>
        ///Determines if the command can execute by invoked the <see cref="Func{T,Bool}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command to determine if it can execute.</param>
        ///<returns>
        ///<see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        ///</returns>
        public bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
        }

        ///<summary>
        ///Executes the command and invokes the <see cref="Action{T}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command.</param>
        public void Execute(T parameter)
        {
            base.Execute(parameter);
        }
    }

    /// <summary>
    /// An <see cref="ICommand"/> whose delegates do not take any parameters for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// </summary>
    /// <seealso cref="DelegateCustomCommandBase"/>
    /// <seealso cref="DelegateCustomCommand{T}"/>
    public class DelegateCustomCommand : DelegateCustomCommandBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="DelegateCustomCommand"/> with the <see cref="Action"/> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        public DelegateCustomCommand(Action executeMethod)
            : this(executeMethod, () => true)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DelegateCustomCommand"/> with the <see cref="Action"/> to invoke on execution
        /// and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.</param>
        /// <param name="canExecuteMethod">The <see cref="Func{TResult}"/> to invoke when <see cref="ICommand.CanExecute"/> is called</param>
        public DelegateCustomCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.StringResources.DelegateCommandDelegatesCannotBeNull);
        }

        public DelegateCustomCommand(INotifyPropertyChanged model, Action executeMethod, Func<bool> canExecuteMethod)
            : this(executeMethod, canExecuteMethod)
        {
            if (model != null)
                model.PropertyChanged += delegate { RaiseCanExecuteChanged(); };
        }

        ///<summary>
        /// Executes the command.
        ///</summary>
        public void Execute()
        {
            Execute(null);
        }

        /// <summary>
        /// Determines if the command can be executed.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command can execute,otherwise returns <see langword="false"/>.</returns>
        public bool CanExecute()
        {
            return CanExecute(null);
        }
    }
}
