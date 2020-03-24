using System;
using System.Windows.Input;

namespace BaiduSearch.helpers
{
    /// <summary>
    /// Taken from http://msdn.microsoft.com/en-us/magazine/dd419663.aspx#id0090030
    /// 这里对RelayCommad进行了改良，RelayCommand把更新CanExecuteChanged代理给RequerySuggested，由系统自动检测，
    /// 这就会造成当焦点改变时，会一直调用CanExcuteChanged，造成系统性能下降，DelegateCommand则提供手动调用
    /// 详细请参见 http://www.cnblogs.com/bear831204/archive/2012/05/31/2528484.html
    /// </summary>

    public class DelegateCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException($"{nameof(execute)}");

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged;

        // The CanExecuteChanged is automatically registered by command binding, we can assume that it has some execution logic 
        // to update the button's enabled\disabled state(though we cannot see). So raises this event will cause the button's state be updated.
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }

}
