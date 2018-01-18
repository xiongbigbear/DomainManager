using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DomainManager
{
    class JsonUtils : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty p = base.CreateProperty(member, memberSerialization);
            p.ShouldSerialize = (a) => 
            {
                object result = null;
                try
                {
                    result = p.ValueProvider.GetValue(a);
                }
                catch (Exception ex)
                {

                }
                return result == null ? false : true;
            };
            return p;
        }
    }
}
