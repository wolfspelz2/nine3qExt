using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.GrainInterfaces;
using nine3q.Tools;
using nine3q.Items;
using System;

namespace IntegrationTests
{
    [TestClass]
    public class InventoryGrainTest
    {
        private string GetRandomInventoryName()
        {
            return "Test-" + Stack.GetMethodName(1) + "-" + RandomString.Get(10);
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void CreateItem()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {

                // Act
                var itemId = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)42 } }).Result;

                // Assert
                Assert.IsTrue(itemId != ItemId.NoItem);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void DeleteItem()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                var itemId = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)42 } }).Result;

                // Act
                var isDeleted = inv.DeleteItem(itemId).Result;

                // Assert
                Assert.IsTrue(isDeleted);
                Assert.AreEqual(0, inv.GetItemIds().Result.Count);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void Sent_and_received_properties_are_identical()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {

                // Act
                var props = new PropertySet {
                        { Pid.TestInt, (long)41 },
                        { Pid.TestString, "42" },
                        { Pid.TestFloat, 3.141592653589793238462643383270502 },
                        { Pid.TestBool, true },
                        { Pid.TestItem, 42L },
                        { Pid.TestItemSet, new ItemIdSet("42 10000000043") },
                        { Pid.TestEnum, PropertyValue.TestEnum.Value1 },
                    };
                var itemId = inv.CreateItem(props).Result;
                var receivedProps = inv.GetItemProperties(itemId, PidList.All).Result;

                // Assert
                Assert.AreEqual(props.GetInt(Pid.TestInt), receivedProps.GetInt(Pid.TestInt));
                Assert.AreEqual(props.GetString(Pid.TestString), receivedProps.GetString(Pid.TestString));
                Assert.AreEqual(props.GetFloat(Pid.TestFloat), receivedProps.GetFloat(Pid.TestFloat), 0.001);
                Assert.AreEqual(props.GetBool(Pid.TestBool), receivedProps.GetBool(Pid.TestBool));
                Assert.AreEqual(props.GetItem(Pid.TestItem), receivedProps.GetItem(Pid.TestItem));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, props.GetItemSet(Pid.TestItemSet), receivedProps.GetItemSet(Pid.TestItemSet)));
                Assert.AreEqual(props.GetString(Pid.TestEnum), receivedProps.GetString(Pid.TestEnum));
                Assert.AreEqual(props.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown), receivedProps.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void TransferItem()
        {
            // Arrange
            var source = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName() + "-source");
            var dest = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName() + "-dest");
            try {
                // Arrange
                var item100Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 100, [Pid.IsContainer] = true, }).Result;
                var item110Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 110, [Pid.IsContainer] = true, }).Result;
                var item120Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 120, [Pid.IsContainer] = true, }).Result;
                var item130Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 130, [Pid.IsContainer] = true, }).Result; // empty container
                var item111Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 111, }).Result;
                var item112Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 112, }).Result;
                var item113Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 113, }).Result;
                var item121Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 121, }).Result;
                var item200Id = source.CreateItem(new PropertySet { [Pid.TestInt1] = 200, }).Result;
                source.AddChildToContainer(item111Id, item110Id, 0);
                source.AddChildToContainer(item112Id, item110Id, 0);
                source.AddChildToContainer(item113Id, item110Id, 0);
                source.AddChildToContainer(item121Id, item120Id, 0);
                source.AddChildToContainer(item110Id, item100Id, 0);
                source.AddChildToContainer(item120Id, item100Id, 0);
                source.AddChildToContainer(item130Id, item100Id, 0);
                var destContainerId = dest.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 10, }).Result;

                // Act
                var transfer = source.BeginItemTransfer(item100Id).Result;
                var map = dest.ReceiveItemTransfer(item100Id, destContainerId, 0, transfer, new PropertySet(), new PidList()).Result;
                source.EndItemTransfer(item100Id).Wait();
                dest.EndItemTransfer(map[item100Id]).Wait();

                // Assert
                var sourceProps = source.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All).Result;
                var destProps = dest.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All).Result;
                Assert.AreEqual(1, sourceProps.Count);
                Assert.AreEqual(9, destProps.Count);
                Assert.IsTrue(destProps[destContainerId].GetItemSet(Pid.Contains).Contains(map[item100Id]));
                Assert.AreEqual(destContainerId, destProps[map[item100Id]].GetItem(Pid.Container));
                Assert.AreEqual(100, destProps[map[item100Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(110, destProps[map[item110Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(120, destProps[map[item120Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(130, destProps[map[item130Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(111, destProps[map[item111Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(112, destProps[map[item112Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(113, destProps[map[item113Id]].GetInt(Pid.TestInt1));
                Assert.AreEqual(121, destProps[map[item121Id]].GetInt(Pid.TestInt1));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet { map[item110Id], map[item120Id], map[item130Id] }, destProps[map[item100Id]].GetItemSet(Pid.Contains)));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet { map[item111Id], map[item112Id], map[item113Id] }, destProps[map[item110Id]].GetItemSet(Pid.Contains)));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet { map[item121Id] }, destProps[map[item120Id]].GetItemSet(Pid.Contains)));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, new ItemIdSet { }, destProps[map[item130Id]].GetItemSet(Pid.Contains)));
                Assert.AreEqual(map[item100Id], destProps[map[item110Id]].GetItem(Pid.Container));
                Assert.AreEqual(map[item100Id], destProps[map[item120Id]].GetItem(Pid.Container));
                Assert.AreEqual(map[item100Id], destProps[map[item130Id]].GetItem(Pid.Container));
                Assert.AreEqual(map[item110Id], destProps[map[item111Id]].GetItem(Pid.Container));
                Assert.AreEqual(map[item110Id], destProps[map[item112Id]].GetItem(Pid.Container));
                Assert.AreEqual(map[item110Id], destProps[map[item113Id]].GetItem(Pid.Container));
                Assert.AreEqual(map[item120Id], destProps[map[item121Id]].GetItem(Pid.Container));

            } finally {
                source.DeletePersistentStorage().Wait();
                dest.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void TransferItem_with_weird_properties()
        {
            // Arrange
            var source = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName() + "-source");
            var dest = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName() + "-dest");
            try {
                // Arrange
                var itemId = source.CreateItem(new PropertySet {
                    [Pid.TestInt] = 40,
                    [Pid.TestInt1] = 41000000000,
                    [Pid.TestInt2] = "42",
                    [Pid.TestInt3] = 43.0,
                    [Pid.TestFloat] = 3.14D,
                    [Pid.TestFloat1] = (float)3.14,
                    [Pid.TestFloat2] = (int)314,
                    [Pid.TestFloat3] = (long)314,
                    [Pid.TestFloat4] = "3.14",
                    [Pid.TestString] = "fourtytwo",
                    [Pid.TestString2] = Pid.TestString2,
                    [Pid.TestBool] = true,
                    [Pid.TestBool1] = false,
                    [Pid.TestBool2] = "true",
                    [Pid.TestBool3] = (int)1,
                    [Pid.TestBool4] = (long)1,
                    [Pid.TestItem] = (int)42,
                    [Pid.TestItem2] = (long)42,
                    [Pid.TestItem3] = "42",
                    [Pid.TestItemSet] = "42 10000000001",
                    [Pid.TestEnum] = PropertyValue.TestEnum.Value1,
                    [Pid.TestEnum1] = PropertyValue.TestEnum.Value1,
                    [Pid.TestEnum2] = PropertyValue.TestEnum.Value1.ToString(),
                }).Result;

                // Act
                var transfer = source.BeginItemTransfer(itemId).Result;
                var map = dest.ReceiveItemTransfer(itemId, ItemId.NoItem, 0, transfer, new PropertySet(), new PidList()).Result;
                source.EndItemTransfer(itemId).Wait();
                dest.EndItemTransfer(map[itemId]).Wait();

                // Assert
                var sourceProps = source.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All).Result;
                var destProps = dest.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All).Result;
                Assert.AreEqual(0, sourceProps.Count);
                Assert.AreEqual(1, destProps.Count);
                Assert.AreEqual(40, destProps[map[itemId]].GetInt(Pid.TestInt));
                Assert.AreEqual(41000000000, destProps[map[itemId]].GetInt(Pid.TestInt1));
                Assert.AreEqual(42, destProps[map[itemId]].GetInt(Pid.TestInt2));
                Assert.AreEqual(43, destProps[map[itemId]].GetInt(Pid.TestInt3));
                Assert.AreEqual(3.14, destProps[map[itemId]].GetFloat(Pid.TestFloat), 0.001);
                Assert.AreEqual(3.14, destProps[map[itemId]].GetFloat(Pid.TestFloat1), 0.001);
                Assert.AreEqual(314, destProps[map[itemId]].GetFloat(Pid.TestFloat2), 0.001);
                Assert.AreEqual(314, destProps[map[itemId]].GetFloat(Pid.TestFloat3), 0.001);
                Assert.AreEqual(3.14, destProps[map[itemId]].GetFloat(Pid.TestFloat4), 0.001);
                Assert.AreEqual("fourtytwo", destProps[map[itemId]].GetString(Pid.TestString));
                Assert.AreEqual(Pid.TestString2.ToString(), destProps[map[itemId]].GetString(Pid.TestString2));
                Assert.AreEqual(true, destProps[map[itemId]].GetBool(Pid.TestBool));
                Assert.AreEqual(false, destProps[map[itemId]].GetBool(Pid.TestBool1));
                Assert.AreEqual(true, destProps[map[itemId]].GetBool(Pid.TestBool2));
                Assert.AreEqual(true, destProps[map[itemId]].GetBool(Pid.TestBool3));
                Assert.AreEqual(true, destProps[map[itemId]].GetBool(Pid.TestBool4));
                Assert.AreEqual(42.ToString(), destProps[map[itemId]].GetItem(Pid.TestItem).ToString());
                Assert.AreEqual(42.ToString(), destProps[map[itemId]].GetItem(Pid.TestItem2).ToString());
                Assert.AreEqual(42.ToString(), destProps[map[itemId]].GetItem(Pid.TestItem3).ToString());
                Assert.AreEqual(new ItemIdSet("42 10000000001").ToString(), destProps[map[itemId]].GetItemSet(Pid.TestItemSet).ToString());
                Assert.AreEqual(PropertyValue.TestEnum.Value1, destProps[map[itemId]].GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));
                Assert.AreEqual(PropertyValue.TestEnum.Value1.ToString(), destProps[map[itemId]].GetString(Pid.TestEnum1));
                Assert.AreEqual(PropertyValue.TestEnum.Value1, destProps[map[itemId]].GetEnum(Pid.TestEnum2, PropertyValue.TestEnum.Unknown));

            } finally {
                source.DeletePersistentStorage().Wait();
                dest.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void CancelItemTransfer_deletes_received_items_and_resets_source()
        {
            // Arrange
            var source = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName() + "-source");
            var dest = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName() + "-dest");
            try {
                // Arrange
                var containerId = source.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.ContainerCanImport] = true }).Result;
                var itemId = source.CreateItem(new PropertySet { [Pid.TestInt1] = 10, [Pid.IsContainer] = true, [Pid.ContainerCanImport] = true }).Result;
                var childId = source.CreateItem(new PropertySet { [Pid.TestInt1] = 11, }).Result;
                source.AddChildToContainer(childId, itemId, 0).Wait();
                source.AddChildToContainer(itemId, containerId, 0).Wait();

                // Act
                var transfer = source.BeginItemTransfer(itemId).Result;
                var map = dest.ReceiveItemTransfer(itemId, ItemId.NoItem, 0, transfer, new PropertySet(), new PidList()).Result;
                source.CancelItemTransfer(itemId).Wait();
                dest.CancelItemTransfer(map[itemId]).Wait();

                // Assert
                Assert.AreEqual(containerId, source.GetItemProperties(itemId, PidList.All).Result.GetItem(Pid.Container));
                Assert.AreEqual(3, source.GetItemIds().Result.Count);
                Assert.AreEqual(0, dest.GetItemIds().Result.Count);

            } finally {
                source.DeletePersistentStorage().Wait();
                dest.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void persists_properties()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {

                var props = new PropertySet {
                        { Pid.TestInt, (long)41 },
                        { Pid.TestString, "42" },
                        { Pid.TestFloat, 3.141592653589793238462643383270502 },
                        { Pid.TestBool, true },
                        { Pid.TestItem, 42 },
                        { Pid.TestItemSet, new ItemIdSet("42 10000000043") },
                        { Pid.TestEnum, PropertyValue.TestEnum.Value1 },
                    };
                var itemId = inv.CreateItem(props).Result;

                // Act
                inv.Deactivate();
                var receivedProps = inv.GetItemProperties(itemId, PidList.All).Result;

                // Assert
                Assert.AreEqual(props.GetInt(Pid.TestInt), receivedProps.GetInt(Pid.TestInt));
                Assert.AreEqual(props.GetString(Pid.TestString), receivedProps.GetString(Pid.TestString));
                Assert.AreEqual(props.GetFloat(Pid.TestFloat), receivedProps.GetFloat(Pid.TestFloat), 0.001);
                Assert.AreEqual(props.GetBool(Pid.TestBool), receivedProps.GetBool(Pid.TestBool));
                Assert.AreEqual(props.GetItem(Pid.TestItem), receivedProps.GetItem(Pid.TestItem));
                Assert.IsTrue(Property.AreEquivalent(Pid.TestItemSet, props.GetItemSet(Pid.TestItemSet), receivedProps.GetItemSet(Pid.TestItemSet)));
                Assert.AreEqual(props.GetString(Pid.TestEnum), receivedProps.GetString(Pid.TestEnum));
                Assert.AreEqual(props.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown), receivedProps.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }


        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void get_subset_of_item_properties()
        {
            // Arrange
            var invName = GetRandomInventoryName();
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                var props = new PropertySet {
                    { Pid.TestInt, (long)41 },
                    { Pid.TestString, "42" },
                    { Pid.TestFloat, 3.141592653589793238462643383270502 },
                    { Pid.TestItem, 42 },
                    { Pid.TestItemSet, new ItemIdSet("42 10000000043") },
                    { Pid.TestBool, true },
                };
                var itemId = inv.CreateItem(props).Result;

                // Act
                var pids = new PidList { Pid.TestInt, Pid.TestBool };
                var receivedProps = inv.GetItemProperties(itemId, pids).Result;

                // Assert
                Assert.AreEqual(pids.Count, receivedProps.Count);
                Assert.AreEqual(props[Pid.TestInt], receivedProps.GetInt(Pid.TestInt));
                Assert.AreEqual(props[Pid.TestBool], receivedProps.GetBool(Pid.TestBool));

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void GetItemByName()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                var itemId = inv.CreateItem(new PropertySet { { Pid.Name, "TestItem" } }).Result;

                // Act
                var namedId = inv.GetItemByName("TestItem").Result;

                // Assert
                Assert.AreEqual(itemId, namedId);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void GetIdsAndValues()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                var itemId1 = inv.CreateItem(new PropertySet { { Pid.TestString, "one" }, { Pid.TestInt1, (long)11 }, { Pid.TestInt2, (long)12 }, }).Result;
                var itemId2 = inv.CreateItem(new PropertySet { { Pid.TestString, "two" }, { Pid.TestInt1, (long)21 }, { Pid.TestInt2, (long)22 }, }).Result;
                var itemId3 = inv.CreateItem(new PropertySet { { Pid.TestString, "three" }, { Pid.TestInt2, (long)32 }, }).Result;

                // Act
                var idValues = inv.GetItemIdsAndValuesByProperty(Pid.TestInt1, new PidList { Pid.TestInt1, Pid.TestString, Pid.TestInt2, }).Result;

                // Assert
                Assert.AreEqual(2, idValues.Count);

                Assert.AreEqual(11, idValues[itemId1].GetInt(Pid.TestInt1));
                Assert.AreEqual(12, idValues[itemId1].GetInt(Pid.TestInt2));
                Assert.AreEqual("one", idValues[itemId1].GetString(Pid.TestString));

                Assert.AreEqual(21, idValues[itemId2].GetInt(Pid.TestInt1));
                Assert.AreEqual(22, idValues[itemId2].GetInt(Pid.TestInt2));
                Assert.AreEqual("two", idValues[itemId2].GetString(Pid.TestString));

            } catch (Exception ex) {
                throw ex;
            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void GetIdsAndValues_also_useful_to_get_some_properties_of_all_items()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                var itemId1 = inv.CreateItem(new PropertySet { { Pid.TestString, "one" }, { Pid.TestInt1, (long)11 }, { Pid.TestInt2, (long)12 }, }).Result;
                var itemId2 = inv.CreateItem(new PropertySet { { Pid.TestString, "two" }, { Pid.TestInt1, (long)21 }, { Pid.TestInt2, (long)22 }, }).Result;
                var itemId3 = inv.CreateItem(new PropertySet { { Pid.TestString, "three" }, { Pid.TestInt2, (long)32 }, }).Result;

                // Act
                var idValues = inv.GetItemIdsAndValuesByProperty(Pid.Id, new PidList { Pid.TestInt1, Pid.TestString, Pid.TestInt2, }).Result;

                // Assert
                Assert.AreEqual(3, idValues.Count);

                Assert.AreEqual("one", idValues[itemId1].GetString(Pid.TestString));
                Assert.AreEqual(11, idValues[itemId1].GetInt(Pid.TestInt1));
                Assert.AreEqual(12, idValues[itemId1].GetInt(Pid.TestInt2));

                Assert.AreEqual("two", idValues[itemId2].GetString(Pid.TestString));
                Assert.AreEqual(21, idValues[itemId2].GetInt(Pid.TestInt1));
                Assert.AreEqual(22, idValues[itemId2].GetInt(Pid.TestInt2));

                Assert.AreEqual("three", idValues[itemId3].GetString(Pid.TestString));
                Assert.AreEqual(32, idValues[itemId3].GetInt(Pid.TestInt2));

            } catch (Exception ex) {
                throw ex;
            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void CreateItem_with_slot_and_container_places_item_inside()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                // Arrange
                var containerId = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.ContainerCanImport] = true, [Pid.Slots] = 3 }).Result;

                // Act
                var itemId = inv.CreateItem(new PropertySet {
                    [Pid.TestInt] = 42,
                    [Pid.Container] = containerId,
                    [Pid.Slot] = 2,
                }).Result;

                // Assert
                Assert.AreEqual(42, inv.GetItemProperties(itemId, new PidList { Pid.TestInt }).Result.GetInt(Pid.TestInt));
                Assert.AreEqual(2, inv.GetItemProperties(itemId, new PidList { Pid.Slot }).Result.GetInt(Pid.Slot));
                Assert.AreEqual(containerId, inv.GetItemProperties(itemId, new PidList { Pid.Container }).Result.GetItem(Pid.Container));
                Assert.IsTrue(inv.GetItemProperties(containerId, new PidList { Pid.Contains }).Result.GetItemSet(Pid.Contains).Contains(itemId));

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void activate_big_inventory()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                const int inventorySize = 200;
                var ids = new ItemIdSet();
                for (int i = 1; i <= inventorySize; i++) {
                    ids.Add(inv.CreateItem(new PropertySet { { Pid.TestInt, (long)i } }).Result);
                }
                inv.Deactivate();

                // Act
                // Assert
                Assert.AreEqual(inventorySize, inv.GetItemIds().Result.Count);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void transient_inventory_does_not_persist_changes()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                inv.SetPersistent(false);

                // Act
                var itemId1 = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)41 } }).Result;
                var itemId2 = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)42 } }).Result;
                Assert.AreEqual(2, inv.GetItemIds().Result.Count);
                inv.Deactivate().Wait();

                // Assert
                Assert.AreEqual(0, inv.GetItemIds().Result.Count);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void default_persistent_inventory_does_persist()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                // Act
                var itemId1 = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)41 } }).Result;
                var itemId2 = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)42 } }).Result;
                Assert.AreEqual(2, inv.GetItemIds().Result.Count);
                inv.Deactivate().Wait();

                // Assert
                Assert.AreEqual(2, inv.GetItemIds().Result.Count);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void transient_inventory_persist_completely_on_WritePermanentStorage()
        {
            // Arrange
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
            try {
                inv.SetPersistent(false);

                // Act
                var itemId1 = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)41 } }).Result;
                var itemId2 = inv.CreateItem(new PropertySet { { Pid.TestInt, (long)42 } }).Result;
                inv.WritePersistentStorage().Wait();
                inv.Deactivate().Wait();

                // Assert
                Assert.AreEqual(2, inv.GetItemIds().Result.Count);

            } finally {
                inv.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void just_created_item_gets_updates_of_template_changes()
        {
            // Arrange
            var uniqueName = GetRandomInventoryName();
            var inventoryName = uniqueName + "-Inventory";
            var templatesInventoryName = uniqueName + "-TemplateInventory";
            var templateName = uniqueName + "-Template";
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(inventoryName);
            var templates = GrainClient.GrainFactory.GetGrain<IInventory>(templatesInventoryName);
            GrainClient.GrainFactory.GetGrain<IInventory>(templatesInventoryName).SetStreamNamespace(templateName).Wait();
            try {
                inv.SetTemplateInventoryName(templatesInventoryName);
                var templateId = templates.CreateItem(new PropertySet { [Pid.Name] = templateName, [Pid.TestInt] = (long)41 }).Result;
                var itemId = inv.CreateItem(new PropertySet { [Pid.TemplateName] = templateName }).Result;

                // Act
                templates.SetItemProperties(templateId, new PropertySet { [Pid.TestInt] = 42 }).Wait();

                // Assert
                Assert.AreEqual((long)42, inv.GetItemProperties(itemId, PidList.All).Result.GetInt(Pid.TestInt));

            } finally {
                inv.DeletePersistentStorage().Wait();
                templates.DeletePersistentStorage().Wait();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public void item_with_later_assigned_template_gets_updates_of_template_changes()
        {
            // Arrange
            var uniqueName = GetRandomInventoryName();
            var inventoryName = uniqueName + "-Inventory";
            var templatesInventoryName = uniqueName + "-TemplateInventory";
            var templateName = uniqueName + "-Template";
            var inv = GrainClient.GrainFactory.GetGrain<IInventory>(inventoryName);
            var templates = GrainClient.GrainFactory.GetGrain<IInventory>(templatesInventoryName);
            GrainClient.GrainFactory.GetGrain<IInventory>(templatesInventoryName).SetStreamNamespace(templateName).Wait();
            try {
                inv.SetTemplateInventoryName(templatesInventoryName);
                var templateId = templates.CreateItem(new PropertySet { [Pid.Name] = templateName, [Pid.TestInt] = (long)41 }).Result;
                var itemId = inv.CreateItem(new PropertySet { [Pid.TestString] = "fourtytwo" }).Result;
                inv.SetItemProperties(itemId, new PropertySet { [Pid.TemplateName] = templateName }).Wait();

                // Act
                templates.SetItemProperties(templateId, new PropertySet { [Pid.TestInt] = 42 }).Wait();

                // Assert
                Assert.AreEqual((long)42, inv.GetItemProperties(itemId, PidList.All).Result.GetInt(Pid.TestInt));

            } finally {
                inv.DeletePersistentStorage().Wait();
                templates.DeletePersistentStorage().Wait();
            }
        }

        /*
            [TestMethod][TestCategory(GrainClient.Category)]
            public void check_write_ops_of_multiple_transactions()
            {
                // Arrange
                var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());

                try {
                    // Act
                    var itemId1 = inv.CreateItem(new PropertySet { { Pid.TestInt, 101 }, { Pid.IsTest1, true }, { Pid.Actions, "{'just do it':'AddTestIntChangeAndDeletePassive'}" } }).Result;
                    var itemId2 = inv.CreateItem(new PropertySet { { Pid.TestInt, 201 } }).Result;
                    var itemId3 = inv.CreateItem(new PropertySet { { Pid.TestInt, 301 } }).Result;
                    inv.SetItemProperties(itemId1, new PropertySet { { Pid.TestInt, 102 } }).Wait();
                    inv.DeleteItem(itemId3).Wait();
                    //inv.ExecuteItemAction(itemId1, new PropertySet { { Pid.Action, "just do it" } });
                    inv.ExecuteItemAction(itemId1, "just do it", new PropertySet { { Pid.Item, itemId2 } }).Wait();

                    // Assert
                    var props = inv.GetItemProperties(itemId1, PidList.All).Result;
                    Assert.AreEqual((long)303, props.GetInt(Pid.TestInt));

                    try {
                        var props2 = inv.GetItemProperties(itemId2, PidList.All).Result;
                        Assert.IsTrue(false, "Expected exception");
                    } catch (Exception ex) {
                        Assert.IsTrue(true, ex.Message);
                    }
                } finally {
                    inv.DeletePersistentStorage().Wait();
                }
            }

            [TestMethod][TestCategory(GrainClient.Category)]
            public void ExecuteAction_with_action_map()
            {
                var inv = GrainClient.GrainFactory.GetGrain<IInventory>(GetRandomInventoryName());
                try {
                    // Arrange
                    var itemId = inv.CreateItem(new PropertySet {
                        [Pid.IsTest1] = true,
                        [Pid.Actions] = new JsonNode(new Dictionary<string, string> {
                            ["DoNothing"] = Content.ActionName.Test1.Nop,
                            ["Add"] = Content.ActionName.Test1.AddTestInt,
                        }).ToJson(),
                        [Pid.TestInt] = 42,
                    }).Result;
                    var summandId = inv.CreateItem(new PropertySet {
                        [Pid.IsTest1] = true,
                        [Pid.TestInt] = 1,
                    }).Result;

                    // Act
                    inv.ExecuteItemAction(itemId, "Add", new PropertySet { [Pid.Item] = summandId }).Wait();

                    // Assert
                    Assert.AreEqual(43, inv.GetItemProperties(itemId, PidList.All).Result.GetInt(Pid.TestInt));
                } finally {
                    inv.DeletePersistentStorage().Wait();
                }
            }


        */
    }
}
