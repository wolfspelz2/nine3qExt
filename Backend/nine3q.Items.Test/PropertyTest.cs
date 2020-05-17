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
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Int, (long)42, (long)42));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Int, 42, 42));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Int, 42, (long)42));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Int, 42, "42"));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.String, "fourtytwo", "fourtytwo"));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Float, (double)3.14, (double)3.14));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Float, 3.14, (double)3.14));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Item, new long(1), new long(1)));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Item, 1, new long(1)));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.Item, (long)1, new long(1)));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new longSet("1 2"), new longSet("1 2")));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new longSet("1 2"), new longSet("2 1")));
        }

        [TestMethod]
        public void AreEquivalent_not()
        {
            Assert.IsFalse(Property.AreEquivalent(Property.Type.Int, (long)42, (long)43));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.String, "fourtytwo", "fourtythree"));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.Float, (double)3.14, (double)2.71));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.Item, new long(1), new long(2)));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.Item, new long(1), long.NoItem));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.ItemSet, new longSet(), new longSet("1 2")));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.ItemSet, new longSet("1 2"), new longSet()));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.ItemSet, new longSet("1 2 3"), new longSet("1 2")));
            Assert.IsFalse(Property.AreEquivalent(Property.Type.ItemSet, new longSet("1 2"), new longSet("1 2 3")));
        }

        [TestMethod]
        public void AreEquivalent_does_not_sort_longSet_inplace()
        {
            var ids1 = new longSet("1 2");
            var ids2 = new longSet("2 1");
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, ids1, ids2));
            Assert.AreEqual("1 2", Property.ToString(Property.Type.ItemSet, ids1));
            Assert.AreEqual("2 1", Property.ToString(Property.Type.ItemSet, ids2));
        }
    }
}
