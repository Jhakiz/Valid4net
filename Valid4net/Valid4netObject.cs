using Valid4net.Rules;

using System.ComponentModel;

namespace Valid4net
{
    /// <summary>
    /// Base class of Valid4net
    /// </summary>
    /// <typeparam name="T">Model type</typeparam>
    public class Valid4netObject<T> : NotifyDataErrorInfo<T>, INotifyPropertyChanged
        where T : NotifyDataErrorInfo<T>
    {
        /// <summary>
        /// Base instance
        /// </summary>
        public Valid4netObject()
        {
        }
    }
}
