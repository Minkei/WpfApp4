using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace WpfApp4.ViewModels
{
    /// <summary>
    /// Provides a base class for ViewModel with property change notification.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifies that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. Automatically provided by the compiler.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the property value and notifies listeners if the value changes.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">A reference to the property's backing field.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">The name of the property. Automatically provided by the compiler.</param>
        /// <returns>True if the value was changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
