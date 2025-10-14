using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public class ParameterService : IParameterService
    {
        // Key generator
        private int _keySeed;

        // Object manager
        private Dictionary<int, object> _parameters;

        private int GenerateKey()
        {
            return Interlocked.Increment(ref _keySeed);
        }

        public ParameterService()
        {
            _keySeed = 0;
            _parameters = new Dictionary<int, object>();
        }

        #region IModelViewService implementation

        public ParameterKey Store(object parameter)
        {
            int key = GenerateKey();
            _parameters.Add(key, parameter);
            return new ParameterKey() { Key = key };
        }

        public object Retrieve(ParameterKey key)
        {
            object result;
            if (!_parameters.TryGetValue(key.Key, out result))
                return null;

            _parameters.Remove(key.Key);
            return result;
        }

        public object Retrieve(ParameterKey key, Type type)
        {
            var result = Retrieve(key);
            if (result.GetType() == type)
                return result;

            return null;
        }

        public T Retrieve<T>(ParameterKey key)
        {
            var result = Retrieve(key);
            if (result is T)
                return (T)result;

            return default(T);
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public int Count
        {
            get
            {
                return _parameters.Count;
            }
        }

        #endregion
    }
}