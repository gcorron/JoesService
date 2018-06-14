using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp5
{
    public static class ObjectCopier
    {
        public static void CopyFields<T>(this T dest, T source)
        {
            PropertyInfo[] properties = dest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(dest, propertyInfo.GetValue(source, null), null);
            }
        }
    }

}
