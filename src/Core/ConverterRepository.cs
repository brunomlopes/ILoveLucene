using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;
using Core.Lucene;

namespace Core
{
    public class ConverterRepository : IConverterRepository
    {
        private Dictionary<string, IConverter> _convertersPerId;

        private IEnumerable<IConverter> _converters;

        [ImportMany(typeof(IConverter))]
        public IEnumerable<IConverter> Converters
        {
            get { return _converters; }
            set
            {
                _converters = value;
                _convertersPerId = _converters.ToDictionary(c => c.GetType().FullName);
            }
        }

        public ConverterRepository()
        {
        }

        public ConverterRepository(params IConverter[] converters)
        {
            Converters = converters;
        }

        public IConverter<T> GetConverterForType<T>()
        {
            var converter = _convertersPerId.Select(kvp => kvp.Value).OfType<IConverter<T>>().FirstOrDefault();
            if (converter == null)
            {
                throw new NotImplementedException(string.Format("No converter for {0} found ", typeof(T)));
            }
            return converter;
        }

        public IConverter GetConverterForId(string id)
        {
            if (!_convertersPerId.ContainsKey(id))
            {
                throw new NotImplementedException(string.Format("No converter for id {0} found", id));
            }
            return _convertersPerId[id];
        }
    }
}