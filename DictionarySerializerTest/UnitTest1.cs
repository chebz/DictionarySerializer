using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cpGames.Serialization;

namespace DictionarySerializerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DictionarySerializerTest()
        {
            var a = new TestClassA();
            a.SetValues();

            var serializedData_noMask = DictionarySerializer.Serialize(a);
            var a_NoMask = DictionarySerializer.Deserialize<TestClassA>(serializedData_noMask);

            var serializedData_Mask_1 = DictionarySerializer.Serialize(a, 1);
            var a_Mask_1 = DictionarySerializer.Deserialize<TestClassA>(serializedData_Mask_1);

            var serializedData_Mask_2 = DictionarySerializer.Serialize(a, 2);
            var a_Mask_2 = DictionarySerializer.Deserialize<TestClassA>(serializedData_Mask_2);

            var serializedData_Mask_1_2 = DictionarySerializer.Serialize(a, 1 | 2);
            var a_Mask_1_2 = DictionarySerializer.Deserialize<TestClassA>(serializedData_Mask_1_2);
        }
    }
}
