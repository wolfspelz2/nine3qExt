using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nine3q.Items.Test
{
    [TestClass]
    public class PropertySetTest
    {
        [TestMethod]
        public void Normalize_does_not_break_correct_input()
        {
            // Arrange
            var props = new PropertySet {
                { Pid.TestInt, (long)42 },
                { Pid.TestFloat, (double)3.14 },
                { Pid.TestString, "fourtytwo" },
                { Pid.TestBool, true },
                { Pid.TestItem, new long(10000000001) },
                { Pid.TestItemSet, new longSet { new long(42) ,new long(10000000001) } },
                { Pid.TestEnum, PropertyValue.TestEnum.Value1.ToString() },
            };

            // Act
            props.Normalize();

            // Assert
            Assert.AreEqual((long)42, props.GetInt(Pid.TestInt));
            Assert.AreEqual((double)3.14, props.GetFloat(Pid.TestFloat), 0.01);
            Assert.AreEqual("fourtytwo", props.GetString(Pid.TestString));
            Assert.AreEqual(true, props.GetBool(Pid.TestBool));
            Assert.AreEqual(new long(10000000001), props.GetItem(Pid.TestItem));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new longSet { new long(42), new long(10000000001) }, props.GetItemSet(Pid.TestItemSet)));
            Assert.AreEqual(PropertyValue.TestEnum.Value1, props.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));
        }

        [TestMethod]
        public void Normalize_normalizes_weird_input()
        {
            // Arrange
            var props = new PropertySet {
                { Pid.TestInt, (int)42 },
                { Pid.TestInt2, "42" },
                { Pid.TestInt3, 42.0 },
                { Pid.TestFloat, (float)3.14 },
                { Pid.TestFloat2, (int)314 },
                { Pid.TestFloat3, (long)314 },
                { Pid.TestFloat4, "3.14" },
                { Pid.TestString, "fourtytwo" },
                { Pid.TestString2, Pid.TestString2 },
                { Pid.TestBool, true },
                { Pid.TestBool2, "true" },
                { Pid.TestBool3, (int)1 },
                { Pid.TestBool4, (long)1 },
                { Pid.TestItem, new long((int)42) },
                { Pid.TestItem2, (long)42 },
                { Pid.TestItem3, "42" },
                { Pid.TestItemSet, "42 10000000001" },
                { Pid.TestEnum, PropertyValue.TestEnum.Value1 },
            };

            // Act
            props.Normalize();

            // Assert
            Assert.AreEqual((long)42, props.GetInt(Pid.TestInt), Pid.TestInt.ToString());
            Assert.AreEqual((long)42, props.GetInt(Pid.TestInt2), Pid.TestInt2.ToString());
            Assert.AreEqual((long)42, props.GetInt(Pid.TestInt3), Pid.TestInt3.ToString());
            Assert.AreEqual((double)3.14, props.GetFloat(Pid.TestFloat), 0.01, Pid.TestFloat.ToString());
            Assert.AreEqual((double)314, props.GetFloat(Pid.TestFloat2), 0.01, Pid.TestFloat2.ToString());
            Assert.AreEqual((double)314, props.GetFloat(Pid.TestFloat3), 0.01, Pid.TestFloat3.ToString());
            Assert.AreEqual((double)3.14, props.GetFloat(Pid.TestFloat4), 0.01, Pid.TestFloat4.ToString());
            Assert.AreEqual("fourtytwo", props.GetString(Pid.TestString), Pid.TestString.ToString());
            Assert.AreEqual("TestString2", props.GetString(Pid.TestString2), Pid.TestString2.ToString());
            Assert.AreEqual(true, props.GetBool(Pid.TestBool), Pid.TestBool.ToString());
            Assert.AreEqual(true, props.GetBool(Pid.TestBool2), Pid.TestBool2.ToString());
            Assert.AreEqual(true, props.GetBool(Pid.TestBool3), Pid.TestBool3.ToString());
            Assert.AreEqual(new long((long)42), props.GetItem(Pid.TestItem), Pid.TestItem.ToString());
            Assert.AreEqual(new long((long)42), props.GetItem(Pid.TestItem2), Pid.TestItem2.ToString());
            Assert.AreEqual(new long((long)42), props.GetItem(Pid.TestItem3), Pid.TestItem3.ToString());
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new longSet { new long((long)42), new long((long)10000000001) }, props.GetItemSet(Pid.TestItemSet)), Pid.TestItemSet.ToString());
            Assert.AreEqual(PropertyValue.TestEnum.Value1, props.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));
            Assert.AreEqual(PropertyValue.TestEnum.Value1.ToString(), props.GetString(Pid.TestEnum));
        }

    }
}
