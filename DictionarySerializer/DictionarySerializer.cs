using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cpGames.Serialization
{
    public class DictionarySerializer
    {
        public static Dictionary<string, object> Serialize(object item, byte mask = 0)
        {
            var data = new Dictionary<string, object>();

            var fields = item.GetType().GetFields().Where(x =>
                (x.GetCustomAttributes(typeof(Field), false).Length == 0) ||
                (!(x.GetCustomAttributes(typeof(Field), false)[0] as Field).ignore));

            for (byte iField = 0; iField < fields.Count(); iField++)
            {
                var field = fields.ElementAt(iField);

                var fieldAtt = (Field)field.GetCustomAttributes(typeof(Field), true).FirstOrDefault();

                if (fieldAtt != null && (fieldAtt.mask & mask) != mask)
                    continue;

                var value = field.GetValue(item);

                if (value != null)
                    data.Add(field.Name, SerializeField(value, mask));
            }

            data.Add("Type", SerializeName(item));

            return data;
        }

        private static object SerializeField(object value, byte mask = 0)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();

            if (type.IsPrimitive || type == typeof(string))
            {
                return value;
            }

            if (type.GetInterfaces().Contains(typeof(IList)))
            {
                var list = (IList)value;
                var listData = new object[list.Count];

                int iItem = 0;
                foreach (var item in list)
                {
                    listData[iItem++] = SerializeField(item, mask);
                }

                return listData;
            }
            
            if (type.IsClass)
            {
                return Serialize(value, mask);
            }

            if (type.IsEnum)
            {
                return value.ToString();
            }

            throw new Exception(string.Format("Unsupported type {0}", type.Name));
        }

        private static string SerializeName(object item)
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
        
        public static T Deserialize<T>(object data)
        {
            var type = typeof(T);

            if (type.IsPrimitive || type == typeof(string))
            {
                return (T)data;
            }

            if (type.GetInterfaces().Contains(typeof(IList)))
            {
                return (T)InvokeGeneric("DeserializeList", type, data);
            }

            if (type.IsClass || type.IsInterface)
            {
                return (T)InvokeGeneric("DeserializeObject", type, data);
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, (string)data);
            }

            throw new Exception(string.Format("Unsupported type {0}", type.Name));
        }

        private static T DeserializeObject<T>(Dictionary<string, object> dict)
        {
            var assembly = Assembly.GetAssembly(typeof(T));
            Type type = GetTypeByName((string)dict["Type"], assembly);
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            T serializable = (T)ctor.Invoke(null);

            var fields = serializable.GetType().GetFields().Where(x =>
               (x.GetCustomAttributes(typeof(Field), false).Length == 0) ||
               (!(x.GetCustomAttributes(typeof(Field), false)[0] as Field).ignore));

            for (byte iField = 0; iField < fields.Count(); iField++)
            {
                var field = fields.ElementAt(iField);

                object value;

                if (dict.TryGetValue(field.Name, out value))
                {
                    field.SetValue(serializable, InvokeGeneric("Deserialize", field.FieldType, value));
                }

            }
            return serializable;
        }

        private static T DeserializeList<T>(object[] data) where T : IList
        {
            var type = typeof(T);

            ConstructorInfo listCtor = type.GetConstructor(new Type[] { typeof(int) });
            IList list = (IList)listCtor.Invoke(new object[] { data.Length });

            Type elementType = type.GetElementType();
            if (elementType == null)
                elementType = type.GetGenericArguments()[0];

            foreach (var item in data)
            {
                list.Add(InvokeGeneric("Deserialize", elementType, item));
            }
            return (T)list;
        }

        private static object InvokeGeneric(string methodName, Type t, object data)
        {
            MethodInfo method = typeof(DictionarySerializer).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo generic = method.MakeGenericMethod(t);
            return generic.Invoke(null, new object[] { data });
        }

        private static Type GetTypeByName(string name, Assembly assembly)
        {
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
