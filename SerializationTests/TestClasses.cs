using System.Collections.Generic;

namespace cpGames.Serialization.Tests
{
    public class TestClassA
    {
        public void SetValues()
        {
            a = "this is a string";

            n = 3;

            listOfStrings = new List<string>() { "a", "bc", "def" };

            listOfFloats = new List<float>() { 0.1f, 0.2f, 32.2f };

            b = new TestClassB();
            b.SetValues();

            abstractClasses = new List<AbstractClass>();
            var da = new DerivedA();
            da.SetValues();
            abstractClasses.Add(da);
            var db = new DerivedB();
            db.SetValues();
            abstractClasses.Add(db);

            interfaces = new List<Interface>();
            var ia = new DerivedA();
            ia.SetValues();
            interfaces.Add(ia);
            var ib = new DerivedB();
            ib.SetValues();
            interfaces.Add(ib);

            listOfLists = new List<List<int>>() {
                new List<int>() { 1, 2, 3},
                new List<int>() { 5, 6, 7}
            };

            ignoreMe = "this should be ignored";

            onlyForPrivileged = "can you read that?";

            onlyForSuperPrivileged = "how about that?";

            forBoth = "or that?";
        }
        

        public string a;

        public int n;

        public List<string> listOfStrings;

        public List<float> listOfFloats;

        public TestClassB b;

        public List<AbstractClass> abstractClasses;

        public List<Interface> interfaces;

        public List<List<int>> listOfLists;

        [Common.Field(ignore = true)]
        public string ignoreMe;

        [Common.Field(mask = 1)]
        public string onlyForPrivileged;

        [Common.Field(mask = 2)]
        public string onlyForSuperPrivileged;

        [Common.Field(mask = 1 | 2)]
        public string forBoth;
    }

    public class TestClassB
    {
        public void SetValues()
        {
            b = "this is b";
            c = new TestClassC();
            c.SetValues();
        }

        public string b;

        public TestClassC c;
    }

    [Common.Class("C")]
    public class TestClassC
    {
        public void SetValues()
        {
            c = "I am nested all the way, but I have a pretty short name in serialization blob that consumes space, makes me readable, and kittens happy :)";
        }

        public string c;
    }

    public abstract class AbstractClass
    {
    }

    public interface Interface
    {

    }

    public class DerivedA : AbstractClass, Interface
    {
        public void SetValues()
        {
            a = "I am A";
        }

        public string a;
    }

    public class DerivedB : AbstractClass, Interface
    {
        public void SetValues()
        {
            b = "I am B";
        }

        public string b;
    }
}
