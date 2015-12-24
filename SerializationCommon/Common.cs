using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cpGames.Serialization
{
    public static class Common
    {
        public const string TYPE = "type";

        public static Type GetElementType(Type type)
        {
            Type elementType = type.GetElementType();
            if (elementType == null)
                elementType = type.GetGenericArguments()[0];
            return elementType;
        }

        public static string SerializeName(object item)
        {
            var classAtts = item.GetType().GetCustomAttributes(typeof(Class), true);
            if (classAtts.Length == 0)
            {
                return item.GetType().AssemblyQualifiedName;
            }
            else
            {
                var att = (Class)classAtts[0];
                return att.Name;
            }
        }

        public static Type GetTypeByName<T>(string name)
        {
            var assembly = Assembly.GetAssembly(typeof(T));
            foreach (Type type in assembly.GetTypes())
            {
                var classAtts = type.GetCustomAttributes(typeof(Class), true);
                if (classAtts.Length > 0)
                {
                    var att = (Class)classAtts[0];
                    if (att.Name.Equals(name))
                        return type;
                }
            }
            return Type.GetType(name);
        }

        public static object InvokeGeneric<T>(string methodName, Type t, object data)
        {
            MethodInfo method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo generic = method.MakeGenericMethod(t);
            return generic.Invoke(null, new object[] { data });
        }

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = type.GetFields().Where(x =>
              (x.GetCustomAttributes(typeof(Common.Field), false).Length == 0) ||
              (!(x.GetCustomAttributes(typeof(Common.Field), false)[0] as Common.Field).ignore));
            return fields;
        }

        [AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
        public class Field : Attribute
        {
            public bool ignore = false;

            public byte mask = 0;

            public Field()
            {
            }
        }

        [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class Class : Attribute
        {
            private string _name;

            public Class(string name)
            {
                _name = name;
            }

            public string Name { get { return _name; } }
        }
    }
}
