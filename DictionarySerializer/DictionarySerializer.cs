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

            data.Add("type", Common.SerializeName(item));

            var fields = Common.GetFields(item.GetType());

            for (byte iField = 0; iField < fields.Count(); iField++)
            {
                var field = fields.ElementAt(iField);

                var fieldAtt = (Common.Field)field.GetCustomAttributes(typeof(Common.Field), true).FirstOrDefault();

                if (fieldAtt != null && (fieldAtt.mask & mask) != mask)
                    continue;

                var value = field.GetValue(item);

                if (value != null)
                    data.Add(field.Name, SerializeField(value, mask));
            }

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
        
        public static T Deserialize<T>(object data)
        {
            var type = typeof(T);

            if (type.IsPrimitive || type == typeof(string))
            {
                return (T)data;
            }

            if (type.GetInterfaces().Contains(typeof(IList)))
            {
                return (T)Common.InvokeGeneric<DictionarySerializer>("DeserializeList", type, data);
            }

            if (type.IsClass || type.IsInterface)
            {
                return (T)Common.InvokeGeneric<DictionarySerializer>("DeserializeObject", type, data);
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, (string)data);
            }

            throw new Exception(string.Format("Unsupported type {0}", type.Name));
        }

        private static T DeserializeObject<T>(Dictionary<string, object> dict)
        {
            var type = Common.GetTypeByName<T>((string)dict["type"]);
            var ctor = type.GetConstructor(Type.EmptyTypes);
            var serializable = (T)ctor.Invoke(null);
            var fields = Common.GetFields(serializable.GetType());
            for (byte iField = 0; iField < fields.Count(); iField++)
            {
                var field = fields.ElementAt(iField);

                object value;

                if (dict.TryGetValue(field.Name, out value))
                {
                    field.SetValue(serializable, Common.InvokeGeneric<DictionarySerializer>("Deserialize", field.FieldType, value));
                }

            }
            return serializable;
        }

        private static T DeserializeList<T>(object[] data) where T : IList
        {
            var type = typeof(T);
            var listCtor = type.GetConstructor(new Type[] { typeof(int) });
            var list = (IList)listCtor.Invoke(new object[] { data.Length });
            var elementType = Common.GetElementType(type);
            if (list.IsFixedSize)
            {
                for (int iEntry = 0; iEntry < data.Length; iEntry++)
                {
                    list[iEntry] = (Common.InvokeGeneric<DictionarySerializer>("Deserialize", elementType, data[iEntry]));
                }
            }
            else
            {
                foreach (var item in data)
                {
                    list.Add(Common.InvokeGeneric<DictionarySerializer>("Deserialize", elementType, item));
                }
            }
            return (T)list;
        }       
    }
}
