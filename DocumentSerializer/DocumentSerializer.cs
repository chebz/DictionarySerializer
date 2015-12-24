using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cpGames.Serialization
{
    public class DocumentSerializer
    {
        public static Document Serialize(object item, byte mask = 0)
        {
            var doc = new Document();
            doc.Add("type", Common.SerializeName(item));
            var fields = Common.GetFields(item.GetType());

            for (byte iField = 0; iField < fields.Count(); iField++)
            {
                var field = fields.ElementAt(iField);

                var fieldAtt = (Common.Field)field.GetCustomAttributes(typeof(Common.Field), true).FirstOrDefault();

                if (fieldAtt != null && (fieldAtt.mask & mask) != mask)
                    continue;

                var value = field.GetValue(item);

                if (value != null)
                    doc.Add(field.Name, SerializeField(value, mask));
            }

            return doc;
        }

        private static DynamoDBEntry SerializeField(object value, byte mask = 0)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();

            if (IsPrimitive(type))
            {
                return ValueToPrimitive(value);
            }

            if (type.GetInterfaces().Contains(typeof(IList)))
            {
                var list = (IList)value;
                //var elementType = Common.GetElementType(type);
                //if (IsPrimitive(elementType))
                //{
                //    var listOfPrimitives = new List<Primitive>();
                //    foreach (var item in list)
                //    {
                //        listOfPrimitives.Add(ValueToPrimitive(item));
                //    }
                //    return listOfPrimitives;
                //}
                //else
                //{
                //    var listOfEntries = new DynamoDBList();
                //    foreach (var item in list)
                //    {
                //        listOfEntries.Add(SerializeField(item, mask));
                //    }
                //    return listOfEntries;
                //}               
                var listOfEntries = new DynamoDBList();
                foreach (var item in list)
                {
                    listOfEntries.Add(SerializeField(item, mask));
                }
                return listOfEntries;
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

        private static bool IsPrimitive(Type type)
        {
            var converter = typeof(Primitive).GetMethod("op_Implicit", new[] { type });
            return (converter != null);
        }

        private static Primitive ValueToPrimitive(object val)
        {
            var converter = typeof(Primitive).GetMethod("op_Implicit", new[] { val.GetType() });
            return (Primitive)converter.Invoke(null, new[] { val });
        }

        public static T Deserialize<T>(object data)
        {
            var type = typeof(T);



            if (type.IsPrimitive || type == typeof(string))
            {
                return (T)(dynamic)data;
            }

            if (type.GetInterfaces().Contains(typeof(IList)))
            {
                return (T)Common.InvokeGeneric<DocumentSerializer>("DeserializeList", type, data);
            }

            if (type.IsClass || type.IsInterface)
            {
                return (T)Common.InvokeGeneric<DocumentSerializer>("DeserializeDocument", type, data);
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, (string)data);
            }

            throw new Exception(string.Format("Unsupported type {0}", type.Name));
        }

        private static T DeserializeDocument<T>(Document doc)
        {
            var type = Common.GetTypeByName<T>(doc[Common.TYPE]);
            var ctor = type.GetConstructor(Type.EmptyTypes);
            var serializable = (T)ctor.Invoke(null);
            var fields = Common.GetFields(serializable.GetType());

            foreach (var field in fields)
            {
                var entry = doc[field.Name];
                field.SetValue(serializable, Common.InvokeGeneric<DocumentSerializer>("Deserialize", field.FieldType, entry));
            }
            return serializable;
        }

        private static T DeserializeList<T>(DynamoDBList dbList) where T : IList
        {
            var type = typeof(T);
            var listCtor = type.GetConstructor(new Type[] { typeof(int) });
            var list = (IList)listCtor.Invoke(new object[] { dbList.Entries.Count });
            var elementType = Common.GetElementType(type);
            foreach (var entry in dbList.Entries)
            {
                list.Add(Common.InvokeGeneric<DocumentSerializer>("Deserialize", elementType, entry));
            }
            return (T)list;
        }
    }
}
