using System;
using System.Windows.Input;

/// <summary>
/// This class provides a simple
/// delegate command that implements the ICommand
/// interface
/// </summary>
namespace booruReader.Helpers
{
    /// <summary>
    /// Implements the ICommand and wraps up all the verbose stuff so that you 
    /// can just pass 2 delegates 1 for the CanExecute and one for the Execute
    /// </summary>
    public class DelegateCommand : ICommand
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether [command succeeded].
        /// </summary>
        /// <value><c>true</c> if [command succeeded]; otherwise, <c>false</c>.</value>
        public Boolean CommandSucceeded { get; set; }

        /// <summary>
        /// Gets or sets the Predicate to execute when the 
        /// CanExecute of the command gets called
        /// </summary>
        public Predicate<object> CanExecuteDelegate { get; set; }

        /// <summary>
        /// Gets or sets the action to be called when the 
        /// Execute method of the command gets called
        /// </summary>
        public Action<object> ExecuteDelegate { get; set; }
        #endregion

        #region ICommand Members

        /// <summary>
        /// Checks if the command Execute method can run
        /// </summary>
        /// <param name="parameter">The command parameter to be passed</param>
        /// <returns>Returns true if the command can execute. 
        /// By default true is returned so that if the user of SimpleCommand 
        /// does not specify a CanExecuteCommand delegate the command 
        /// still executes.</returns>
        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
            {
                return CanExecuteDelegate(parameter);
            }
            else
            {
                // if there is no can execute default to true
                return true;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Executes the actual command
        /// </summary>
        /// <param name="parameter">The command parameter to be passed</param>
        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
            {
                ExecuteDelegate(parameter);
                CommandSucceeded = true;
            }
        }

        #endregion
    }
}
