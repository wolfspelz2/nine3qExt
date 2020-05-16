using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.Items.Aspects;

namespace nine3q.Items.Test
{
    [TestClass]
    public class InventoryTest
    {
        [TestMethod]
        public void Inventory_GetItemByName()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.Name, "TestItem" } });

            // Act
            var namedId = inv.GetItemByName("TestItem");

            // Assert
            Assert.AreEqual(item.Id, namedId);
        }

        [TestMethod]
        public void Inventory_GetItemByName_also_takes_key_value_pairs()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet {
                [Pid.Name] = "Item1",
                [Pid.TestInt] = 41,
                [Pid.TestString] = "fourtyone",
                [Pid.TestBool] = true,
                [Pid.IsContainer] = true,
            });
            var item2 = inv.CreateItem(new PropertySet {
                [Pid.Name] = "Item2",
                [Pid.TestInt] = 42,
                [Pid.TestString] = "fourtytwo",
                [Pid.TestBool] = false,
            });
            item1.AsContainer().AddChild(item2);

            // Act
            // Assert
            Assert.AreEqual(item1.Id, inv.GetItemByName("Item1"));
            Assert.AreEqual(item1.Id, inv.GetItemByName("TestInt=41"));
            Assert.AreEqual(item1.Id, inv.GetItemByName("TestString=fourtyone"));
            Assert.AreEqual(item1.Id, inv.GetItemByName("TestBool=true"));
            Assert.AreEqual(item1.Id, inv.GetItemByName("IsContainer=true"));

            Assert.AreEqual(item2.Id, inv.GetItemByName("Item2"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestInt=42"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestString=fourtytwo"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestBool=false"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("IsContainer=false"));

            Assert.AreEqual(item2.Id, inv.GetItemByName("Item1/Item2"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("Item1/TestInt=42"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("Item1/TestString=fourtytwo"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("Item1/TestBool=false"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("Item1/IsContainer=false"));

            Assert.AreEqual(item2.Id, inv.GetItemByName("TestInt=41/Item2"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestInt=41/TestInt=42"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestInt=41/TestString=fourtytwo"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestInt=41/TestBool=false"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestInt=41/IsContainer=false"));

            Assert.AreEqual(item2.Id, inv.GetItemByName("TestString=fourtyone/Item2"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestString=fourtyone/TestInt=42"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestString=fourtyone/TestString=fourtytwo"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestString=fourtyone/TestBool=false"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestString=fourtyone/IsContainer=false"));

            Assert.AreEqual(item2.Id, inv.GetItemByName("TestBool=true/Item2"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestBool=true/TestInt=42"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestBool=true/TestString=fourtytwo"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestBool=true/TestBool=false"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("TestBool=true/IsContainer=false"));

            Assert.AreEqual(item2.Id, inv.GetItemByName("IsContainer=true/Item2"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("IsContainer=true/TestInt=42"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("IsContainer=true/TestString=fourtytwo"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("IsContainer=true/TestBool=false"));
            Assert.AreEqual(item2.Id, inv.GetItemByName("IsContainer=true/IsContainer=false"));
        }

        [TestMethod]
        public void Inventory_GetItemByName_with_path_loops_through_containers()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 1 });
            var item2 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 2 });
            var item3 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 3, [Pid.Name] = "NamedItem" });
            var item4 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 4 });
            var container1 = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Name] = "Container1" });
            var container2 = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Name] = "Container2" });
            container1.AsContainer().AddChild(container2);
            container1.AsContainer().AddChild(item2);
            container2.AsContainer().AddChild(item3);
            container2.AsContainer().AddChild(item4);

            // Act
            var namedId = inv.GetItemByName("Container1/Container2/NamedItem");

            // Assert
            Assert.AreEqual(item3.Id, namedId);
        }

        [TestMethod]
        public void Inventory_DeleteItem()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.Name, "TestItem" } });

            // Act
            inv.DeleteItem(item.Id);

            // Assert
            Assert.IsFalse(inv.IsItem(item.Id));
        }

        [TestMethod]
        public void Inventory_DeleteItemProperties()
        {
            // Arrange
            var inv = new Inventory();
            var props = new PropertySet {
                { Pid.TestInt, (long)42 },
                { Pid.TestFloat, (double)3.14 },
                { Pid.TestString, "fourtytwo" },
                { Pid.TestBool, true },
                { Pid.TestItem, new ItemId(10000000001) },
                { Pid.TestItemSet, new ItemIdSet { new ItemId(42), new ItemId(10000000001) } },
            };
            var item = inv.CreateItem(props);

            // Act
            inv.DeleteItemProperties(item.Id, new PidList { Pid.TestInt, Pid.TestBool });

            // Assert
            Assert.AreEqual(0, item.GetInt(Pid.TestInt));
            Assert.AreEqual(props[Pid.TestFloat], item.GetFloat(Pid.TestFloat));
            Assert.AreEqual(props[Pid.TestString], item.GetString(Pid.TestString));
            Assert.AreEqual(false, item.GetBool(Pid.TestBool));
            Assert.AreEqual(props[Pid.TestItem], item.GetItem(Pid.TestItem));
            Assert.AreEqual((ItemIdSet)props[Pid.TestItemSet], item.GetItemSet(Pid.TestItemSet));
        }

        [TestMethod]
        public void Inventory_get_all_item_properties()
        {
            // Arrange
            var inv = new Inventory();
            var props = new PropertySet {
                { Pid.TestInt, (long)42 },
                { Pid.TestFloat, (double)3.14 },
                { Pid.TestString, "fourtytwo" },
                { Pid.TestBool, true },
                { Pid.TestItem, new ItemId(10000000001) },
                { Pid.TestItemSet, new ItemIdSet { new ItemId(42), new ItemId(10000000001) } },
                { Pid.TestEnum, PropertyValue.TestEnum.Value1 },
            };
            var item = inv.CreateItem(props);

            // Act
            var receivedProps = inv.GetItemProperties(item.Id, PidList.All);

            // Assert
            Assert.AreEqual(item.Id, receivedProps.GetItem(Pid.Id));
            Assert.AreEqual(42, receivedProps.GetInt(Pid.TestInt));
            Assert.AreEqual(props[Pid.TestFloat], item.GetFloat(Pid.TestFloat));
            Assert.AreEqual(props[Pid.TestString], item.GetString(Pid.TestString));
            Assert.AreEqual(true, receivedProps.GetBool(Pid.TestBool));
            Assert.AreEqual(props[Pid.TestItem], item.GetItem(Pid.TestItem));
            Assert.AreEqual((ItemIdSet)props[Pid.TestItemSet], item.GetItemSet(Pid.TestItemSet));
            Assert.AreEqual(props[Pid.TestEnum], item.GetEnum(Pid.TestEnum, PropertyValue.TestEnum.Unknown));
            Assert.AreEqual(props[Pid.TestEnum].ToString(), item.GetString(Pid.TestEnum));
        }

        [TestMethod]
        public void Inventory_get_subset_of_item_properties()
        {
            // Arrange
            var inv = new Inventory();
            var props = new PropertySet {
                { Pid.TestInt, (long)42 },
                { Pid.TestFloat, (double)3.14 },
                { Pid.TestString, "fourtytwo" },
                { Pid.TestBool, true },
                { Pid.TestItem, new ItemId(10000000001) },
                { Pid.TestItemSet, new ItemIdSet { new ItemId(42), new ItemId(10000000001) } },
            };
            var item = inv.CreateItem(props);

            // Act
            var pids = new PidList { Pid.TestInt, Pid.TestBool };
            var receivedProps = inv.GetItemProperties(item.Id, pids);

            // Assert
            Assert.AreEqual(pids.Count, receivedProps.Count);
            Assert.AreEqual(42, receivedProps.GetInt(Pid.TestInt));
            Assert.AreEqual(true, receivedProps.GetBool(Pid.TestBool));
        }

        [TestMethod]
        public void Inventory_get_public_properties()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { [Pid.TestPublic] = 1, [Pid.TestOwner] = 2, [Pid.TestInternal] = 3 });

            // Act
            var receivedProps = inv.GetItemProperties(item.Id, PidList.Public);

            // Assert
            Assert.AreEqual(2, receivedProps.Count);
            Assert.AreEqual(item.Id, receivedProps.GetItem(Pid.Id));
            Assert.AreEqual(1, receivedProps.GetInt(Pid.TestPublic));
            Assert.AreEqual(0, receivedProps.GetInt(Pid.TestOwner));
            Assert.AreEqual(0, receivedProps.GetInt(Pid.TestInternal));
        }

        [TestMethod]
        public void Inventory_get_owner_properties()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { [Pid.TestPublic] = 1, [Pid.TestOwner] = 2, [Pid.TestInternal] = 3 });

            // Act
            var receivedProps = inv.GetItemProperties(item.Id, PidList.Owner);

            // Assert
            Assert.AreEqual(3, receivedProps.Count);
            Assert.AreEqual(item.Id, receivedProps.GetItem(Pid.Id));
            Assert.AreEqual(1, receivedProps.GetInt(Pid.TestPublic));
            Assert.AreEqual(2, receivedProps.GetInt(Pid.TestOwner));
            Assert.AreEqual(0, receivedProps.GetInt(Pid.TestInternal));
        }

        [TestMethod]
        public void Inventory_get_public_properties_with_template()
        {
            // Arrange
            var inv = new Inventory();
            inv.Templates = new Inventory();
            inv.Templates.CreateItem(new PropertySet { [Pid.Name] = "TestTemplate", [Pid.TestPublic] = 1, [Pid.TestOwner] = 2, [Pid.TestInternal] = 3 });
            var item = inv.CreateItem(new PropertySet { [Pid.TemplateName] = "TestTemplate" });

            // Act
            var receivedProps = inv.GetItemProperties(item.Id, PidList.Public);

            // Assert
            Assert.AreEqual(2, receivedProps.Count);
            Assert.AreEqual(item.Id, receivedProps.GetItem(Pid.Id));
            Assert.AreEqual(1, receivedProps.GetInt(Pid.TestPublic));
            Assert.AreEqual(0, receivedProps.GetInt(Pid.TestOwner));
            Assert.AreEqual(0, receivedProps.GetInt(Pid.TestInternal));
        }

        [TestMethod]
        public void Inventory_ExecuteAction()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet {
                { Pid.IsTest1, true },
                { Pid.Actions, "{DoNothing:'" + Test1Aspect.Action.Nop + "',Add:'" + Test1Aspect.Action.AddTestInt + "'}" },
                { Pid.TestInt, 42 },
            });
            var summand = inv.CreateItem(new PropertySet {
                { Pid.IsTest1, true },
                { Pid.TestInt, 1 },
            });

            // Act
            inv.ExecuteItemAction(item.Id, "Add", new PropertySet { { Pid.Item, summand.Id } });

            // Assert
            Assert.AreEqual(43, item.GetInt(Pid.TestInt));
        }

        [TestMethod]
        public void Inventory_GetItemIdsAndValuesByProperty()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 }, { Pid.TestInt2, 12 }, });
            var item2 = inv.CreateItem(new PropertySet { { Pid.TestInt, 21 }, });
            var item3 = inv.CreateItem(new PropertySet { { Pid.TestInt2, 32 }, { Pid.TestInt3, 33 }, });

            // Act
            var idValues = inv.GetItemIdsAndValuesByProperty(Pid.TestInt, new PidList { Pid.TestInt, Pid.TestInt2 });

            // Assert
            Assert.AreEqual(2, idValues.Count);
            Assert.AreEqual(2, idValues[item1.Id].Count);
            Assert.AreEqual(11, idValues[item1.Id].GetInt(Pid.TestInt));
            Assert.AreEqual(12, idValues[item1.Id].GetInt(Pid.TestInt2));
            Assert.AreEqual(1, idValues[item2.Id].Count);
            Assert.AreEqual(21, idValues[item2.Id].GetInt(Pid.TestInt));
        }

        [TestMethod]
        public void Inventory_GetItemIdsAndValuesByPropertyValue()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 }, { Pid.TestInt2, 12 }, });
            var item2 = inv.CreateItem(new PropertySet { { Pid.TestInt, 21 }, });
            var item3 = inv.CreateItem(new PropertySet { { Pid.TestInt2, 32 }, { Pid.TestInt3, 33 }, });

            // Act
            var idValues = inv.GetItemIdsAndValuesByPropertyValue(new PropertySet { [Pid.TestInt] = 11 }, new PidList { Pid.TestInt, Pid.TestInt2 });

            // Assert
            Assert.AreEqual(1, idValues.Count);
            Assert.AreEqual(2, idValues[item1.Id].Count);
            Assert.AreEqual(11, idValues[item1.Id].GetInt(Pid.TestInt));
            Assert.AreEqual(12, idValues[item1.Id].GetInt(Pid.TestInt2));
        }

        [TestMethod]
        public void Inventory_GetItemIdsAndValuesByPropertyValue_with_multiple_filter_pairs()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 }, { Pid.TestString, "s1" }, { Pid.TestInt2, 21 }, });
            var item2 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 }, { Pid.TestString, "s2" }, { Pid.TestInt2, 22 }, });
            var item3 = inv.CreateItem(new PropertySet { { Pid.TestInt, 13 }, { Pid.TestString, "s3" }, { Pid.TestInt2, 23 }, });
            var item4 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 }, { Pid.TestString, "s1" }, { Pid.TestInt2, 24 }, });

            // Act
            var idValues = inv.GetItemIdsAndValuesByPropertyValue(new PropertySet { [Pid.TestInt] = 11, [Pid.TestString] = "s1" }, new PidList { Pid.TestInt2 });

            // Assert
            Assert.AreEqual(2, idValues.Count);
            Assert.AreEqual(1, idValues[item1.Id].Count);
            Assert.AreEqual(21, idValues[item1.Id].GetInt(Pid.TestInt2));
            Assert.AreEqual(24, idValues[item4.Id].GetInt(Pid.TestInt2));
        }

        [TestMethod]
        public void Inventory_GetParentContainers_returns_parents()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 } });
            var item2 = inv.CreateItem(new PropertySet { { Pid.TestInt, 21 } });
            var item3 = inv.CreateItem(new PropertySet { { Pid.TestInt2, 32 } });
            item1.Set(Pid.Container, item2.Id);
            item2.Set(Pid.Container, item3.Id);

            // Act
            var parentList = inv.GetParentContainers(item1.Id);

            // Assert
            Assert.AreEqual(2, parentList.Count);
            Assert.AreEqual(item2.Id, parentList[0]);
            Assert.AreEqual(item3.Id, parentList[1]);
        }

        [TestMethod]
        public void Inventory_GetParentContainers_without_parents_returns_empty_list()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { { Pid.TestInt, 11 } });

            // Act
            var parentList = inv.GetParentContainers(item1.Id);

            // Assert
            Assert.AreEqual(0, parentList.Count);
        }

        [TestMethod]
        public void Inventory_GetParentContainers_of_a_3_deep_stack()
        {
            // Arrange
            var inv = new Inventory();
            var item100 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 100, [Pid.IsContainer] = true, });
            var item110 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 110, [Pid.IsContainer] = true, });
            var item120 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 120, [Pid.IsContainer] = true, });
            var item130 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 130, [Pid.IsContainer] = true, }); // empty container
            var item111 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 111, });
            var item112 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 112, });
            var item113 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 113, });
            var item121 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 121, });
            var item200 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 200, });
            item110.AsContainer().AddChild(item111);
            item110.AsContainer().AddChild(item112);
            item110.AsContainer().AddChild(item113);
            item120.AsContainer().AddChild(item121);
            item100.AsContainer().AddChild(item110);
            item100.AsContainer().AddChild(item120);
            item100.AsContainer().AddChild(item130);

            // Act
            // Assert
            Assert.AreEqual(0, inv.GetParentContainers(item100.Id).Count);
            Assert.AreEqual(1, inv.GetParentContainers(item110.Id).Count);
            Assert.AreEqual(1, inv.GetParentContainers(item120.Id).Count);
            Assert.AreEqual(1, inv.GetParentContainers(item130.Id).Count);
            Assert.AreEqual(2, inv.GetParentContainers(item111.Id).Count);
            Assert.AreEqual(2, inv.GetParentContainers(item112.Id).Count);
            Assert.AreEqual(2, inv.GetParentContainers(item113.Id).Count);
            Assert.AreEqual(2, inv.GetParentContainers(item121.Id).Count);
            Assert.AreEqual(0, inv.GetParentContainers(item200.Id).Count);
        }

        [TestMethod]
        public void Inventory_GetParentContainers_bails_out_of_ininite_loop()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { [Pid.TestInt] = 11 });
            item.Set(Pid.Container, item.Id);

            // Act
            var parentList = inv.GetParentContainers(item.Id);

            // Assert
            Assert.AreEqual(0, parentList.Count);
        }

        [TestMethod]
        public void Inventory_CreateItem_with_conflicting_name_throws()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { [Pid.Name] = "TestItem" });

            // Act
            // Assert
            Assert.ThrowsException<Exceptions.WrongItemPropertyException>(() => inv.CreateItem(new PropertySet { [Pid.Name] = "TestItem" }));
        }

        [TestMethod]
        public void Inventory_CreateItem_with_not_conflicting_name_does_not_throw()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { [Pid.Name] = "TestItem1" });

            // Act
            var item2 = inv.CreateItem(new PropertySet { [Pid.Name] = "TestItem2" });

            // Assert
            Assert.IsFalse(item2 == null);
            Assert.AreNotEqual(item1, item2);
        }

        [TestMethod]
        public void Inventory_SetItemProperties_with_conflicting_name_throws()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 41, [Pid.Name] = "TestItem" });
            var item2 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 42 });

            // Act
            // Assert
            Assert.ThrowsException<Exceptions.WrongItemPropertyException>(() => inv.SetItemProperties(item2.Id, new PropertySet { [Pid.Name] = "TestItem" }));
        }

        [TestMethod]
        public void Inventory_GetItemAndChildrenProperties_of_a_single_item()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 11 });

            // Act
            var idProps = inv.GetItemAndChildrenProperties(item1.Id);

            // Assert
            Assert.AreEqual(1, idProps.Count);
            Assert.AreEqual(11, idProps[item1.Id].GetInt(Pid.TestInt1));
        }

        [TestMethod]
        public void Inventory_GetItemAndChildrenProperties_only_native_of_a_single_item_with_template()
        {
            // Arrange
            var inv = new Inventory();
            inv.Templates = new Inventory();
            inv.Templates.CreateItem(new PropertySet { [Pid.Name] = "TestTemplate", [Pid.TestInt2] = 22 });
            var item1 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 11, [Pid.TemplateName] = "TestTemplate", });

            // Act
            var idProps = inv.GetItemAndChildrenProperties(item1.Id, native: true);

            // Assert
            Assert.AreEqual(1, idProps.Count);
            Assert.AreEqual(11, idProps[item1.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(0, idProps[item1.Id].GetInt(Pid.TestInt2));
        }

        [TestMethod]
        public void Inventory_GetItemAndChildrenProperties_of_a_3_deep_stack()
        {
            // Arrange
            var inv = new Inventory();
            var item100 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 100, [Pid.IsContainer] = true, });
            var item110 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 110, [Pid.IsContainer] = true, });
            var item120 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 120, [Pid.IsContainer] = true, });
            var item130 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 130, [Pid.IsContainer] = true, }); // empty container
            var item111 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 111, });
            var item112 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 112, });
            var item113 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 113, });
            var item121 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 121, });
            var item200 = inv.CreateItem(new PropertySet { [Pid.TestInt1] = 200, });
            item110.AsContainer().AddChild(item111);
            item110.AsContainer().AddChild(item112);
            item110.AsContainer().AddChild(item113);
            item120.AsContainer().AddChild(item121);
            item100.AsContainer().AddChild(item110);
            item100.AsContainer().AddChild(item120);
            item100.AsContainer().AddChild(item130);

            // Act
            var idProps = inv.GetItemAndChildrenProperties(item100.Id, native: true);

            // Assert
            Assert.AreEqual(8, idProps.Count);
            Assert.AreEqual(100, idProps[item100.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(110, idProps[item110.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(120, idProps[item120.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(130, idProps[item130.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(111, idProps[item111.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(112, idProps[item112.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(113, idProps[item113.Id].GetInt(Pid.TestInt1));
            Assert.AreEqual(121, idProps[item121.Id].GetInt(Pid.TestInt1));
        }

        [TestMethod]
        public void Inventory_transfer_of_a_3_deep_stack()
        {
            // Arrange
            var source = new Inventory();
            var dest = new Inventory();
            var item100 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 100, [Pid.IsContainer] = true, });
            var item110 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 110, [Pid.IsContainer] = true, });
            var item120 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 120, [Pid.IsContainer] = true, });
            var item130 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 130, [Pid.IsContainer] = true, }); // empty container
            var item111 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 111, });
            var item112 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 112, });
            var item113 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 113, });
            var item121 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 121, });
            var item200 = source.CreateItem(new PropertySet { [Pid.TestInt1] = 200, });
            item110.AsContainer().AddChild(item111);
            item110.AsContainer().AddChild(item112);
            item110.AsContainer().AddChild(item113);
            item120.AsContainer().AddChild(item121);
            item100.AsContainer().AddChild(item110);
            item100.AsContainer().AddChild(item120);
            item100.AsContainer().AddChild(item130);
            var item100PropCount = item100.GetProperties(PidList.All).Count;
            var item110PropCount = item110.GetProperties(PidList.All).Count;
            var item120PropCount = item120.GetProperties(PidList.All).Count;
            var item130PropCount = item130.GetProperties(PidList.All).Count;
            var item111PropCount = item111.GetProperties(PidList.All).Count;
            var item112PropCount = item112.GetProperties(PidList.All).Count;
            var item113PropCount = item113.GetProperties(PidList.All).Count;
            var item121PropCount = item121.GetProperties(PidList.All).Count;
            var item200PropCount = item200.GetProperties(PidList.All).Count;

            // Act
            var transfer = source.BeginItemTransfer(item100.Id);
            var map = dest.ReceiveItemTransfer(item100.Id, ItemId.NoItem, 0, transfer, new PropertySet(), new PidList());
            source.EndItemTransfer(item100.Id);
            dest.EndItemTransfer(map[item100.Id]);

            // Assert
            var sourceProps = source.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All);
            var destProps = dest.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All);
            Assert.AreEqual(1, sourceProps.Count);
            Assert.AreEqual(8, destProps.Count);
            Assert.AreEqual(item100PropCount, destProps[map[item100.Id]].Count);
            Assert.AreEqual(item110PropCount, destProps[map[item110.Id]].Count);
            Assert.AreEqual(item120PropCount, destProps[map[item120.Id]].Count);
            Assert.AreEqual(item130PropCount, destProps[map[item130.Id]].Count);
            Assert.AreEqual(item111PropCount, destProps[map[item111.Id]].Count);
            Assert.AreEqual(item112PropCount, destProps[map[item112.Id]].Count);
            Assert.AreEqual(item113PropCount, destProps[map[item113.Id]].Count);
            Assert.AreEqual(item121PropCount, destProps[map[item121.Id]].Count);
            Assert.AreEqual(item200PropCount, item200.GetProperties(PidList.All).Count);
            Assert.AreEqual(100, destProps[map[item100.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(110, destProps[map[item110.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(120, destProps[map[item120.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(130, destProps[map[item130.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(111, destProps[map[item111.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(112, destProps[map[item112.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(113, destProps[map[item113.Id]].GetInt(Pid.TestInt1));
            Assert.AreEqual(121, destProps[map[item121.Id]].GetInt(Pid.TestInt1));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet { map[item110.Id], map[item120.Id], map[item130.Id] }, destProps[map[item100.Id]].GetItemSet(Pid.Contains)));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet { map[item111.Id], map[item112.Id], map[item113.Id] }, destProps[map[item110.Id]].GetItemSet(Pid.Contains)));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet { map[item121.Id] }, destProps[map[item120.Id]].GetItemSet(Pid.Contains)));
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet { }, destProps[map[item130.Id]].GetItemSet(Pid.Contains)));
            Assert.AreEqual(ItemId.NoItem, destProps[map[item100.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item100.Id], destProps[map[item110.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item100.Id], destProps[map[item120.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item100.Id], destProps[map[item130.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item110.Id], destProps[map[item111.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item110.Id], destProps[map[item112.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item110.Id], destProps[map[item113.Id]].GetItem(Pid.Container));
            Assert.AreEqual(map[item120.Id], destProps[map[item121.Id]].GetItem(Pid.Container));
        }

        [TestMethod]
        public void Inventory_BeginItemTransfer_frees_item_from_container_CancelItemTransfer_restores()
        {
            // Arrange
            var source = new Inventory();
            var container = source.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var item = source.CreateItem(new PropertySet { [Pid.TestInt1] = 10, [Pid.IsContainer] = true, });
            var child = source.CreateItem(new PropertySet { [Pid.TestInt1] = 11, });
            item.AsContainer().AddChild(child);
            container.AsContainer().AddChild(item);

            // Act
            var transfer = source.BeginItemTransfer(item.Id);

            // Assert
            Assert.AreEqual(ItemId.NoItem, item.GetItem(Pid.Container));

            // Act
            source.CancelItemTransfer(item.Id);

            // Assert
            Assert.AreEqual(container.Id, item.GetItem(Pid.Container));
        }

        [TestMethod]
        public void Inventory_CancelItemTransfer_deletes_received_items_and_resets_source()
        {
            // Arrange
            var source = new Inventory();
            var dest = new Inventory();
            var container = source.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var item = source.CreateItem(new PropertySet { [Pid.TestInt1] = 10, [Pid.IsContainer] = true, });
            var child = source.CreateItem(new PropertySet { [Pid.TestInt1] = 11, });
            item.AsContainer().AddChild(child);
            container.AsContainer().AddChild(item);

            // Act
            var transfer = source.BeginItemTransfer(item.Id);
            var map = dest.ReceiveItemTransfer(item.Id, ItemId.NoItem, 0, transfer, new PropertySet(), new PidList());
            source.CancelItemTransfer(item.Id);
            dest.CancelItemTransfer(map[item.Id]);

            // Assert
            Assert.AreEqual(container.Id, source.Item(item.Id).GetItem(Pid.Container));
            Assert.AreEqual(3, source.GetItems().Count);
            Assert.AreEqual(0, dest.GetItems().Count);
        }

        [TestMethod]
        public void Inventory_transfer_into_destination_repository()
        {
            // Arrange
            var source = new Inventory();
            var dest = new Inventory();
            var sourceContainer = source.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var item = source.CreateItem(new PropertySet { [Pid.TestInt1] = 10, [Pid.IsContainer] = true, });
            var child = source.CreateItem(new PropertySet { [Pid.TestInt1] = 11, });
            item.AsContainer().AddChild(child);
            sourceContainer.AsContainer().AddChild(item);
            var destContainer = dest.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 2, });
            var destItem = dest.CreateItem(new PropertySet { [Pid.TestInt1] = 12, });
            destContainer.AsContainer().AddChild(destItem);

            // Act
            var transfer = source.BeginItemTransfer(item.Id);
            var map = dest.ReceiveItemTransfer(item.Id, destContainer.Id, 0, transfer, new PropertySet(), new PidList());
            source.EndItemTransfer(item.Id);
            dest.EndItemTransfer(map[item.Id]);

            // Assert
            Assert.AreEqual(1, source.GetItems().Count);
            Assert.AreEqual(4, dest.GetItems().Count);
            Assert.IsTrue(destContainer.GetItemSet(Pid.Contains).Contains(map[item.Id]));
        }

        [TestMethod]
        public void Inventory_transfer_into_destination_repository_throws_due_to_lack_of_space()
        {
            // Arrange
            var source = new Inventory();
            var dest = new Inventory();
            var sourceContainer = source.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var item = source.CreateItem(new PropertySet { [Pid.TestInt1] = 10, [Pid.IsContainer] = true, });
            var child = source.CreateItem(new PropertySet { [Pid.TestInt1] = 11, });
            item.AsContainer().AddChild(child);
            sourceContainer.AsContainer().AddChild(item);
            var destContainer = dest.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 1, });
            var destItem = dest.CreateItem(new PropertySet { [Pid.TestInt1] = 12, });
            destContainer.AsContainer().AddChild(destItem);

            var transfer = source.BeginItemTransfer(item.Id);
            // Act
            // Assert
            Assert.ThrowsException<Exceptions.SlotAvailabilityException>(() => dest.ReceiveItemTransfer(item.Id, destContainer.Id, 0, transfer, new PropertySet(), new PidList()));
        }

        [TestMethod]
        public void Inventory_transfer_with_set_and_delete_properties()
        {
            // Arrange
            var source = new Inventory();
            var dest = new Inventory();
            var item = source.CreateItem(new PropertySet { [Pid.TestInt1] = 41, [Pid.TestInt2] = 42 });

            // Act
            var transfer = source.BeginItemTransfer(item.Id);
            var map = dest.ReceiveItemTransfer(item.Id, ItemId.NoItem, 0, transfer, new PropertySet { [Pid.TestInt3] = 43 }, new PidList { Pid.TestInt2 });
            source.EndItemTransfer(item.Id);
            dest.EndItemTransfer(map[item.Id]);

            // Assert
            Assert.AreEqual(0, source.GetItems().Count);
            Assert.AreEqual(1, dest.GetItems().Count);
            Assert.AreEqual(41, dest.GetItemProperties(map[item.Id], new PidList { Pid.TestInt1 }).GetInt(Pid.TestInt1));
            Assert.AreEqual(0, dest.GetItemProperties(map[item.Id], new PidList { Pid.TestInt2 }).GetInt(Pid.TestInt2));
            Assert.AreEqual(43, dest.GetItemProperties(map[item.Id], new PidList { Pid.TestInt3 }).GetInt(Pid.TestInt3));
        }

        [TestMethod]
        public void Inventory_GetItemIdsAndValuesByProperty_returns_only_select_properties_of_containers()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 41, [Pid.TestString] = "one", });
            var item2 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 42, [Pid.TestString] = "two", [Pid.IsContainer] = true, });

            // Act
            var idValues = inv.GetItemIdsAndValuesByProperty(Pid.IsContainer, new PidList { Pid.TestInt });

            // Assert
            Assert.AreEqual(1, idValues.Count);
            Assert.AreEqual(1, idValues[item2.Id].Count);
            Assert.AreEqual(42, idValues[item2.Id].GetInt(Pid.TestInt));
        }

        [TestMethod]
        public void Inventory_GetItemIdsAndValuesByProperty_can_return_all_properties()
        {
            // Arrange
            var inv = new Inventory();
            var props = new PropertySet {
                { Pid.TestInt, (long)42 },
                { Pid.TestFloat, (double)3.14 },
                { Pid.TestString, "fourtytwo" },
                { Pid.TestBool, true },
                { Pid.TestItem, new ItemId(10000000001) },
                { Pid.TestItemSet, new ItemIdSet { new ItemId(42), new ItemId(10000000001) } },
            };
            var item = inv.CreateItem(props);

            // Act
            var idValues = inv.GetItemIdsAndValuesByProperty(Pid.Id, PidList.All);

            // Assert
            var receivedProps = idValues[item.Id];
            Assert.AreEqual(item.Id, receivedProps.GetItem(Pid.Id));
            Assert.AreEqual(42, receivedProps.GetInt(Pid.TestInt));
            Assert.AreEqual(props[Pid.TestFloat], item.GetFloat(Pid.TestFloat));
            Assert.AreEqual(props[Pid.TestString], item.GetString(Pid.TestString));
            Assert.AreEqual(true, receivedProps.GetBool(Pid.TestBool));
            Assert.AreEqual(props[Pid.TestItem], item.GetItem(Pid.TestItem));
            Assert.AreEqual((ItemIdSet)props[Pid.TestItemSet], item.GetItemSet(Pid.TestItemSet));
        }

        //[TestMethod]
        //public void Inventory_CreateItem_re_uses_lowest_possible_free_number()
        //{
        //    // Arrange
        //    var inv = new Inventory();
        //    inv.CreateItem(new PropertySet { [Pid.Id] = new ItemId(3) });

        //    // Act
        //    // Assert
        //    Assert.AreEqual(new ItemId(1), inv.CreateItem(new PropertySet { }).Id);

        //    // Act
        //    // Assert
        //    Assert.AreEqual(new ItemId(2), inv.CreateItem(new PropertySet { }).Id);

        //    // Act
        //    // Assert
        //    Assert.AreEqual(new ItemId(4), inv.CreateItem(new PropertySet { }).Id);
        //}

        [TestMethod]
        public void Inventory_CreateItem_dont_re_use_id()
        {
            // Arrange
            var inv = new Inventory();
            inv.CreateItem(new PropertySet { [Pid.Id] = new ItemId(1) });
            inv.CreateItem(new PropertySet { [Pid.Id] = new ItemId(2) });
            inv.CreateItem(new PropertySet { [Pid.Id] = new ItemId(3) });
            inv.DeleteItem(new ItemId(2));

            // Act
            // Assert
            Assert.AreNotEqual(new ItemId(2), inv.CreateItem(new PropertySet { }).Id);
        }

    }
}

