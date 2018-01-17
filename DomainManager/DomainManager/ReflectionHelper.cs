using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DomainManager
{
    public static class ReflectionHelper
    {
        public static T GetAttribue<T>(this MemberInfo memberInfo)
            where T : Attribute
        {
            var arrtibutes = memberInfo.GetCustomAttributes(typeof(T), false);

            if (arrtibutes == null || arrtibutes.Length == 0)
            {
                return default(T);
            }

            return arrtibutes[0] as T;
        }
    }
}
