using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Skyb.Extensions.Caching.MemCached
{
    public class MemCachedXmlRepository : IXmlRepository
    {
        private readonly IMemcachedClient _client;
        private readonly string _key;
        public MemCachedXmlRepository(IMemcachedClient client, string key)
        {
            _client = client;
            _key = key;
        }
        private IEnumerable<XElement> GetAllElementsCore()
        {
            var list = _client.GetValue<List<string>>(_key);

            if (list == null)
            {
                yield break;
            }
            foreach (var xml in list) {
                yield return XElement.Parse(xml) ;
            }
        }



        public void StoreElement(XElement element, string friendlyName)
        {

            var list = _client.GetValue<List<string>>(_key);
            if (list == null)
            {
                list = new List<string>();
            }

            var xml = element.ToString();
            list.Add(xml);
            _client.SetValue(_key, list, DateTime.Now.AddHours(1));
            
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return GetAllElementsCore().ToList().AsReadOnly();
        }
    }
}
