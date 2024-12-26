using System;
using System.Windows.Input;

namespace WpfApp4.ViewModels
{
    /// <summary>
    /// Represents a command that can be bound to UI elements in WPF applications.
    /// </summary>
    public class ViewModelCommand : ICommand
    {
        // Fields
        private readonly Action<object?>? _executeAction;
        private readonly Predicate<object?>? _canExecuteAction;

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelCommand"/> class.
        /// </summary>
        /// <param name="executeAction">The action to execute.</param>
        public ViewModelCommand(Action<object?>? executeAction)
            : this(executeAction, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelCommand"/> class.
        /// </summary>
        /// <param name="executeAction">The action to execute.</param>
        /// <param name="canExecuteAction">The predicate to determine if the action can execute.</param>
        public ViewModelCommand(Action<object?>? executeAction, Predicate<object?>? canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        // Events
        public event EventHandler? CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Methods
        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecuteAction == null || _canExecuteAction(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object? parameter)
        {
            _executeAction?.Invoke(parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}