using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    /// <summary>
    /// Parameter key.
    /// </summary>
    public class ParameterKey
    {
        public static ParameterKey NullKey { get { return new ParameterKey() { Key = 0 }; } }

        public int Key { get; set; }
    }

    /// <summary>
    /// Generic object manager container
    /// Support superior finding on object key and flexible object type-based query 
    /// </summary>
    /// <typeparam name="TKey">Key of the object</typeparam>
    /// <typeparam name="TObj">The single object itself</typeparam>
    public interface IParameterService
    {
        /// <summary>
        /// Stores the parameter.
        /// </summary>
        /// <returns>The parameter key</returns>
        ParameterKey Store(object parameter);

        /// <summary>
        /// Retrieves the parameter.
        /// </summary>
        /// <returns>The parameter.</returns>
        /// <param name="key">Key.</param>
        object Retrieve(ParameterKey key);

        /// <summary>
        /// Retrieves the parameter.
        /// </summary>
        /// <returns>The parameter object</returns>
        /// <param name="key">Parameter key</param>
        object Retrieve(ParameterKey key, System.Type type);

        /// <summary>
        /// Retrieves the parameter.
        /// </summary>
        /// <returns>The parameter.</returns>
        /// <param name="key">Key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T Retrieve<T>(ParameterKey key);

        /// <summary>
        /// Return the parameter count
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Clears all parameters.
        /// </summary>
        void Clear();

    }
}
