using Amazon.DynamoDBv2.DocumentModel;
using cpGames.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace cpGames.Serialization.Tests
{
    [TestClass]
    public class DocumentSerializerTests
    {
        [TestMethod]
        public void DocumentTest1()
        {
            var a = new TestClassA();
            a.SetValues();

            var doc = DocumentSerializer.Serialize(a);

            var b = DocumentSerializer.Deserialize<TestClassA>(doc);
        }

        [TestMethod]
        public void DocumentTest2()
        {
            var a = new DerivedA();
            a.SetValues();

            var doc = DocumentSerializer.Serialize(a);

            var b = DocumentSerializer.Deserialize<Interface>(doc);
        }
    }
}
