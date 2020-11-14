using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Valid4net.Rules
{
    /// <summary>
    /// Provides functionality to provide errors for the object if it is in an invalid state.
    /// </summary>
    /// <typeparam name="T">The type of this instance.</typeparam>
    public abstract class NotifyDataErrorInfo<T> : INotifyPropertyChanged, INotifyDataErrorInfo
        where T : NotifyDataErrorInfo<T>
    {
        #region Fields
        private const string HasErrorsPropertyName = "HasErrors";
        private static RuleCollection<T> rules = new RuleCollection<T>();
        private Dictionary<string, List<object>> errors;
        #endregion
        #region Public Events
        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire object.
        /// </summary>
        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { errorsChanged += value; }
            remove { errorsChanged -= value; }
        }
        #endregion
        #region Private Events
        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire object.
        /// </summary>
        private event EventHandler<DataErrorsChangedEventArgs> errorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region Public Properties
        /// <summary>
        /// Gets the when errors changed observable event. Occurs when the validation errors have changed for a property or for the entire object.
        /// </summary>
        /// <value>
        /// The when errors changed observable event.
        /// </value>
        public IObservable<string> WhenErrorsChanged
        {
            get
            {
                return Observable
                    .FromEventPattern<DataErrorsChangedEventArgs>(
                        h => errorsChanged += h,
                        h => errorsChanged -= h)
                    .Select(x => x.EventArgs.PropertyName);
            }
        }
        /// <summary>
        /// Gets a value indicating whether the object has validation errors.
        /// </summary>
        /// <value><c>true</c> if this instance has errors, otherwise <c>false</c>.</value>
        public virtual bool HasErrors
        {
            get
            {
                InitializeErrors();
                return errors.Count > 0;
            }
        }
        #endregion


        #region Protected Properties

        /// <summary>
        /// Gets the rules which provide the errors.
        /// </summary>
        /// <value>The rules this instance must satisfy.</value>
        protected static RuleCollection<T> Rules
        {
            get { return rules; }
        }

        /// <summary>
        /// Adds a new <see cref="Rule{T}"/> to this instance.
        /// </summary>
        /// <param name="propertyName">The name of the property the rules applies to.</param>
        /// <param name="error">The error if the object does not satisfy the rule.</param>
        /// <param name="rule">The rule to execute.</param>
        public void AddRule(string propertyName, object error, Func<T, bool> rule)
        {
            Rules.Add(propertyName, error, rule);
        }

        public void ClearRules()
        {
            rules.Clear();
        }

        public RuleCollection<T> GetRules() => rules;

        /// <summary>
        /// Gets the validation errors for the entire object.
        /// </summary>
        /// <returns>A collection of errors.</returns>
        public IEnumerable GetErrors()
        {
            return GetErrors(null);
        }
        /// <summary>
        /// Gets the validation errors for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">Name of the property to retrieve errors for. <c>null</c> to
        /// retrieve all errors for this instance.</param>
        /// <returns>A collection of errors.</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            Debug.Assert(
                string.IsNullOrEmpty(propertyName) ||
                (GetType().GetRuntimeProperty(propertyName) != null),
                "Check that the property name exists for this instance.");
            InitializeErrors();
            IEnumerable result;
            if (string.IsNullOrEmpty(propertyName))
            {
                List<object> allErrors = new List<object>();
                foreach (KeyValuePair<string, List<object>> keyValuePair in errors)
                {
                    allErrors.AddRange(keyValuePair.Value);
                }
                result = allErrors;
            }
            else
            {
                if (errors.ContainsKey(propertyName))
                {
                    result = errors[propertyName];
                }
                else
                {
                    result = new List<object>();
                }
            }
            return result;
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            //base.OnPropertyChanged(propertyName);
            if (string.IsNullOrEmpty(propertyName))
            {
                ApplyRules();
            }
            else
            {
                ApplyRules(propertyName);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(HasErrorsPropertyName));
            //base.OnPropertyChanged(HasErrorsPropertyName);
        }
        /// <summary>
        /// Notify a property change that uses CallerMemberName attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="backingField">Backing field of property</param>
        /// <param name="value">Value to give backing field</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual bool SetProperty<t>(ref t backingField, t value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<t>.Default.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        /// <summary>
        /// Called when the errors have changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnErrorsChanged([CallerMemberName] string propertyName = null)
        {
            Debug.Assert(
                string.IsNullOrEmpty(propertyName) ||
                (GetType().GetRuntimeProperty(propertyName) != null),
                "Check that the property name exists for this instance.");
            EventHandler<DataErrorsChangedEventArgs> eventHandler = errorsChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Applies all rules to this instance.
        /// </summary>
        private void ApplyRules()
        {
            InitializeErrors();
            foreach (string propertyName in rules.Select(x => x.PropertyName))
            {
                ApplyRules(propertyName);
            }
        }
        /// <summary>
        /// Applies the rules to this instance for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void ApplyRules(string propertyName)
        {
            InitializeErrors();
            List<object> propertyErrors = rules.Apply((T)this, propertyName).ToList();
            if (propertyErrors.Count > 0)
            {
                if (errors.ContainsKey(propertyName))
                {
                    errors[propertyName].Clear();
                }
                else
                {
                    errors[propertyName] = new List<object>();
                }
                errors[propertyName].AddRange(propertyErrors);
                OnErrorsChanged(propertyName);
            }
            else if (errors.ContainsKey(propertyName))
            {
                errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
        /// <summary>
        /// Initializes the errors and applies the rules if not initialized.
        /// </summary>
        private void InitializeErrors()
        {
            if (errors == null)
            {
                errors = new Dictionary<string, List<object>>();
                ApplyRules();
            }
        }
        #endregion
    }
}