using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.GrainInterfaces;
using nine3q.Tools;
using nine3q.Items;

namespace IntegrationTests
{
    [TestClass]
    public class InventoryStorageTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Create_items_store_unload_reload_verify_properties()
        {
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>($"{nameof(InventoryStorageTest)}-{nameof(Create_items_store_unload_reload_verify_properties)}-{RandomString.Get(10)}");
            try {
                var set1 = new PropertySet {
                        { Pid.TestInt, (long)41 },
                        { Pid.TestString, "42" },
                        { Pid.TestFloat, 3.141592653589793238462643383270502 },
                        { Pid.TestBool, true },
                        { Pid.TestItem, 42L },
                        { Pid.TestItemSet, new ItemIdSet("42 10000000043") },
                        { Pid.TestEnum, PropertyValue.TestEnum.Value1 },
                    };
                var set2 = new PropertySet {
                        { Pid.TestInt, 42L },
                    };
                var id1 = inv.CreateItem(set1).Result;
                var id2 = inv.CreateItem(set2).Result;

                //inv.WritePersistentStorage().Wait();
                inv.Deactivate();

                var get1 = inv.GetItemProperties(id1, PidList.All).Result;
                var get2 = inv.GetItemProperties(id2, PidList.All).Result;

                // Assert
                Assert.AreEqual(set1.GetInt(Pid.TestInt), get1.GetInt(Pid.TestInt));
                Assert.AreEqual(set1.GetString(Pid.TestString), get1.GetString(Pid.TestString));
                Assert.AreEqual(set1.GetFloat(Pid.TestFloat), get1.GetFloat(Pid.TestFloat), 0.001);
                Assert.AreEqual(set1.GetBool(Pid.TestBool), get1.GetBool(Pid.TestBool));
                Assert.AreEqual(set1.GetItem(Pid.TestItem), get1.GetItem(Pid.TestItem));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, set1.GetItemSet(Pid.TestItemSet), get1.GetItemSet(Pid.TestItemSet)));
                Assert.AreEqual(set1.GetString(Pid.TestEnum), get1.GetString(Pid.TestEnum));
                Assert.AreEqual(set1.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown), get1.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));

                Assert.AreEqual(set2.GetInt(Pid.TestInt), get2.GetInt(Pid.TestInt));

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }
    }
}
