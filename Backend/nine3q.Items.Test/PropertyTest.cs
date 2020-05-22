using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nine3q.Items.Test
{
    [TestClass]
    public class PropertyTest
    {
        [TestMethod]
        public void All_properties_have_definitions()
        {
            foreach (var pid in Enum.GetValues(typeof(Pid)).Cast<Pid>()) {
                var prop = Property.Get(pid);
                Assert.AreEqual(pid, prop.Id, "Id of PropertyId." + pid.ToString());
                Assert.AreEqual(pid.ToString(), prop.Name, "Name of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Type, "Type of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Use, "Use of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Group, "Group of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Example, "Example of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Description, "Description of PropertyId." + pid.ToString());
            }
        }

        [TestMethod]
        public void AreEquivalent()
        {
            Assert.IsTrue(Property.AreEquivalent(Pid.TestInt, (long)42, (long)42));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestInt, 42, 42));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestInt, 42, (long)42));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestInt, 42, "42"));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestString, "fourtytwo", "fourtytwo"));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestString, "fourtytwo", (object)"fourtytwo"));
            //Assert.IsTrue(Property.AreEquivalent(Pid.TestString, "{TestString:'42'}", "{ TestString: '42' }"));
            //Assert.IsTrue(Property.AreEquivalent(Pid.TestString, "{TestString:'42'}", "{TestString:\"42\"}"));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestFloat, (double)3.14, (double)3.14));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestFloat, 3.14, (double)3.14));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestItem, 1, 1L));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestItem, 1L, 1));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet("1 2"), new ItemIdSet("1 2")));
            Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet("1 2"), new ItemIdSet("2 1")));
        }

        [TestMethod]
        public void AreEquivalent_not()
        {
            Assert.IsFalse(Property.AreEquivalent(Pid.TestInt, (long)42, (long)43));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestString, "fourtytwo", "fourtythree"));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestFloat, (double)3.14, (double)2.71));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestItem, (1), (2)));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestItem, (1), ItemId.NoItem));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet(), new ItemIdSet("1 2")));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet("1 2"), new ItemIdSet()));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet("1 2 3"), new ItemIdSet("1 2")));
            Assert.IsFalse(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet("1 2"), new ItemIdSet("1 2 3")));
        }

        [TestMethod]
        public void AreEquivalent_does_not_sort_ItemIdSet_inplace()
        {
            var ids1 = new ItemIdSet("1 2");
            var ids2 = new ItemIdSet("2 1");
            Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, ids1, ids2));
            Assert.AreEqual("1 2", Property.ToString(Pid.TestItemSet, ids1));
            Assert.AreEqual("2 1", Property.ToString(Pid.TestItemSet, ids2));
        }
    }
}
