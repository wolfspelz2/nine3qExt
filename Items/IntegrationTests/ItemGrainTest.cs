using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using System.Threading;
using n3q.Aspects;

namespace IntegrationTests
{
    [TestClass]
    public class ItemGrainTest
    {
        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                // Act
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestInt] = 42,
                        [Pid.TestString] = "42",
                    }, PidSet.Empty);
                });

                var props = await item.Get(PidSet.All);

                // Assert
                Assert.AreEqual(42, props.GetInt(Pid.TestInt));
                Assert.AreEqual("42", props.GetString(Pid.TestString));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_by_access_level()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestInternal] = 41,
                        [Pid.TestPublic] = 42,
                    }, PidSet.Empty);
                });

                // Act
                var props = await item.Get(PidSet.Public);

                // Assert
                Assert.AreEqual(1, props.Count);
                Assert.AreEqual(0, (long)props.Get(Pid.TestInternal));
                Assert.AreEqual(42, (long)props.Get(Pid.TestPublic));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_by_group_incl_template()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());
            var tmpl = GrainClient.GetItemStub(GrainClient.GetRandomItemId("TEMPLATE"));

            try {
                await tmpl.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestInt1] = 41,
                        [Pid.TestGreeterAspect] = true,
                    }, PidSet.Empty);
                });
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestInt2] = 42,
                        [Pid.TestGreetedAspect] = true,
                        [Pid.Template] = tmpl.Id,
                    }, PidSet.Empty);
                });

                // Act
                var props = await item.Get(PidSet.Aspects);

                // Assert
                Assert.AreEqual(2, props.Count);
                Assert.AreEqual(0, props.GetInt(Pid.TestInt1));
                Assert.AreEqual(0, props.GetInt(Pid.TestInt2));
                Assert.AreEqual(true, props.GetBool(Pid.TestGreeterAspect));
                Assert.AreEqual(true, props.GetBool(Pid.TestGreetedAspect));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
                await tmpl.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_by_PidSet()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestInt1] = 41,
                        [Pid.TestInt2] = 42,
                        [Pid.TestInt3] = 43,
                    }, PidSet.Empty);
                });

                // Act
                var props = await item.Get(new PidSet { Pid.TestInt2, Pid.TestInt3, });

                // Assert
                Assert.AreEqual(2, props.Count);
                Assert.AreEqual(42, (long)props.Get(Pid.TestInt2));
                Assert.AreEqual(43, (long)props.Get(Pid.TestInt3));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task SetGet_Modify_add_set_delete()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        // stay
                        [Pid.TestString] = "40",
                        [Pid.TestInt] = 40000000000,
                        [Pid.TestFloat] = 40.14159265358979323,
                        [Pid.TestItemList] = new ValueList { "4", "0" },

                        // change
                        [Pid.TestString1] = "41",
                        [Pid.TestInt1] = 41000000000,
                        [Pid.TestFloat1] = 41.14159265358979323,
                        [Pid.TestItemList1] = new ValueList { "4", "1" },

                        // delete
                        [Pid.TestString3] = "43",
                        [Pid.TestInt3] = 43000000000,
                        [Pid.TestFloat3] = 43.14159265358979323,
                        [Pid.TestBool3] = true,
                        [Pid.TestItemList3] = new ValueList { "4", "3" },
                    }, PidSet.Empty);
                });

                await item.WithTransaction(async self => {
                    // Act
                    await self.Modify(new PropertySet {
                        [Pid.TestString1] = "412",
                        [Pid.TestInt1] = 41200000000,
                        [Pid.TestFloat1] = 412.14159265358979323,
                        [Pid.TestBool1] = true,
                        [Pid.TestItemList1] = new ValueList { "4", "1", "2" },

                        // add
                        [Pid.TestString2] = "42",
                        [Pid.TestInt2] = 42000000000,
                        [Pid.TestFloat2] = 42.14159265358979323,
                        [Pid.TestBool2] = true,
                        [Pid.TestItemList2] = new ValueList { "4", "2" },
                    }, new PidSet {
                        Pid.TestString3,
                        Pid.TestInt3,
                        Pid.TestFloat3,
                        Pid.TestBool3,
                        Pid.TestItemList3,
                    });
                });

                // Assert
                var props = await item.Get(PidSet.All);

                // stay
                Assert.AreEqual("40", props.GetString(Pid.TestString));
                Assert.AreEqual(40000000000, props.GetInt(Pid.TestInt));
                Assert.AreEqual(40.14159265358979323, props.GetFloat(Pid.TestFloat), 0.01);
                Assert.AreEqual(false, props.GetBool(Pid.TestBool));
                Assert.AreEqual("4 0", props.GetItemIdSet(Pid.TestItemList).ToString());

                // change
                Assert.AreEqual("412", props.GetString(Pid.TestString1));
                Assert.AreEqual(41200000000, props.GetInt(Pid.TestInt1));
                Assert.AreEqual(412.14159265358979323, props.GetFloat(Pid.TestFloat1), 0.01);
                Assert.AreEqual(true, props.GetBool(Pid.TestBool1));
                Assert.AreEqual("4 1 2", props.GetItemIdSet(Pid.TestItemList1).ToString());

                // set
                Assert.AreEqual("42", props.GetString(Pid.TestString2));
                Assert.AreEqual(42000000000, props.GetInt(Pid.TestInt2));
                Assert.AreEqual(42.14159265358979323, props.GetFloat(Pid.TestFloat2), 0.01);
                Assert.AreEqual(true, props.GetBool(Pid.TestBool2));
                Assert.AreEqual("4 2", props.GetItemIdSet(Pid.TestItemList2).ToString());

                // delete
                Assert.AreEqual("", props.GetString(Pid.TestString3));
                Assert.AreEqual(0L, props.GetInt(Pid.TestInt3));
                Assert.AreEqual(0.0D, props.GetFloat(Pid.TestFloat3), 0.01);
                Assert.AreEqual(false, props.GetBool(Pid.TestBool3));
                Assert.AreEqual("", props.GetItemIdSet(Pid.TestItemList3).ToString());

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_with_template()
        {
            // Arrange
            var itemId = GrainClient.GetRandomItemId();
            var tmplId = GrainClient.GetRandomItemId("TEMPLATE");
            var item = GrainClient.GetItemStub(itemId);
            var tmpl = GrainClient.GetItemStub(tmplId);

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.Template] = tmplId,
                    }, PidSet.Empty);
                });
                await tmpl.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestInt] = 42,
                    }, PidSet.Empty);
                });

                // Act
                var props = await item.Get(PidSet.All);

                // Assert
                Assert.AreEqual(2, props.Count);
                Assert.AreEqual(tmplId, (string)props.Get(Pid.Template));
                Assert.AreEqual(42, (long)props.Get(Pid.TestInt));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
                await tmpl.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_PidSet_with_template()
        {
            // Arrange
            var itemId = GrainClient.GetRandomItemId();
            var tmplId = GrainClient.GetRandomItemId("TEMPLATE");
            var item = GrainClient.GetItemStub(itemId);
            var tmpl = GrainClient.GetItemStub(tmplId);

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.Template] = tmplId,
                        [Pid.TestInt] = 40,
                        [Pid.TestString] = "item.TestString",   // item
                        [Pid.TestString1] = "item.TestString1", // item get
                        [Pid.TestString2] = "item.TestString2", // both
                        [Pid.TestString3] = "item.TestString3", // both get
                    }, PidSet.Empty);
                });
                await tmpl.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestString2] = "tmpl.TestString2", // both
                        [Pid.TestString3] = "tmpl.TestString3", // both get
                        [Pid.TestString4] = "tmpl.TestString4", // tmpl
                        [Pid.TestString5] = "tmpl.TestString5", // tmpl get
                    }, PidSet.Empty);
                });

                // Act
                var props = await item.Get(new PidSet { Pid.TestString1, Pid.TestString3, Pid.TestString5 });

                // Assert
                Assert.AreEqual(3, props.Count);
                Assert.AreEqual("item.TestString1", (string)props.Get(Pid.TestString1));
                Assert.AreEqual("item.TestString3", (string)props.Get(Pid.TestString3));
                Assert.AreEqual("tmpl.TestString5", (string)props.Get(Pid.TestString5));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
                await tmpl.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_PidSet_with_template_and_native()
        {
            // Arrange
            var itemId = GrainClient.GetRandomItemId();
            var tmplId = GrainClient.GetRandomItemId("TEMPLATE");
            var item = GrainClient.GetItemStub(itemId);
            var tmpl = GrainClient.GetItemStub(tmplId);

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.Template] = tmplId,
                        [Pid.TestInt] = 40,
                        [Pid.TestString] = "item.TestString",   // item
                        [Pid.TestString1] = "item.TestString1", // item get
                        [Pid.TestString2] = "item.TestString2", // both
                        [Pid.TestString3] = "item.TestString3", // both get
                    }, PidSet.Empty);
                });
                await tmpl.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestString2] = "tmpl.TestString2", // both
                        [Pid.TestString3] = "tmpl.TestString3", // both get
                        [Pid.TestString4] = "tmpl.TestString4", // tmpl
                        [Pid.TestString5] = "tmpl.TestString5", // tmpl get
                    }, PidSet.Empty);
                });

                // Act
                var props = await item.Get(new PidSet { Pid.TestString1, Pid.TestString3, Pid.TestString5 }, native: true);

                // Assert
                Assert.AreEqual(2, props.Count);
                Assert.AreEqual("item.TestString1", (string)props.Get(Pid.TestString1));
                Assert.AreEqual("item.TestString3", (string)props.Get(Pid.TestString3));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
                await tmpl.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task AddToList_RemoveFromList()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.WithTransaction(async self => {
                    await self.Modify(new PropertySet {
                        [Pid.TestItemList] = ValueList.FromString("a b"),
                    }, PidSet.Empty);
                });

                // Act
                await item.WithTransaction(async self => {
                    await self.AddToList(Pid.TestItemList, "c");
                });
                // Assert
                Assert.AreEqual("a b c", (string)(await item.Get(PidSet.All))[Pid.TestItemList]);

                // Act
                await item.WithTransaction(async self => {
                    await self.RemoveFromList(Pid.TestItemList, "b");
                });
                // Assert
                Assert.AreEqual("a c", (string)(await item.Get(PidSet.All))[Pid.TestItemList]);

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task AddToList_RemoveFromList_empty()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                // Act
                await item.WithTransaction(async self => {
                    await self.AddToList(Pid.TestItemList, "a");
                });
                // Assert
                Assert.AreEqual("a", ((ValueList)(await item.Get(PidSet.All))[Pid.TestItemList])[0]);

                // Act
                await item.WithTransaction(async self => {
                    await self.RemoveFromList(Pid.TestItemList, "a");
                });
                // Assert
                Assert.AreEqual(0, ((ValueList)(await item.Get(PidSet.All))[Pid.TestItemList]).Count);

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_with_default_values()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                // Act
                var props = await item.Get(new PidSet {
                    Pid.TestInt,
                    Pid.TestIntDefault,
                    Pid.TestStringDefault,
                    Pid.TestFloatDefault,
                    Pid.TestBoolDefault,
                });

                // Assert
                Assert.AreEqual(0, props.GetInt(Pid.TestInt));
                Assert.AreEqual(42, props.GetInt(Pid.TestIntDefault));
                Assert.AreEqual("42", props.GetString(Pid.TestStringDefault));
                Assert.AreEqual(3.14D, props.GetFloat(Pid.TestFloatDefault));
                Assert.AreEqual(true, props.GetBool(Pid.TestBoolDefault));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Get_after_set_during_transaction_returns_changed_value()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.WithTransaction(async self => {
                    await self.Set(Pid.TestInt, 41);
                });

                // Act
                var intermediateValue = 0L;
                await item.WithTransaction(async self => {
                    await self.Set(Pid.TestInt, 42);
                    intermediateValue = await self.GetInt(Pid.TestInt);
                    await self.Set(Pid.TestInt, 43);
                });

                // Assert
                Assert.AreEqual(42L, intermediateValue);
                Assert.AreEqual(43L, await item.GetInt(Pid.TestInt));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Set_without_transaction_also_works()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.WithTransaction(async self => {
                    await self.Set(Pid.TestInt, 41);
                });
                Assert.AreEqual(41L, await item.GetInt(Pid.TestInt));

                // Act
                await item.Grain.ModifyProperties(new PropertySet(Pid.TestInt, 42), PidSet.Empty, ItemTransaction.WithoutTransaction);

                // Assert
                Assert.AreEqual(42L, await item.GetInt(Pid.TestInt));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task Delete()
        {
            // Arrange
            var item = GrainClient.GetItemStub(GrainClient.GetRandomItemId());

            try {
                await item.Grain.ModifyProperties(new PropertySet(Pid.TestInt, 42), PidSet.Empty, ItemTransaction.WithoutTransaction);
                Assert.AreEqual(42L, await item.GetInt(Pid.TestInt));

                // Act
                await item.Grain.Delete(ItemTransaction.WithoutTransaction);

                // Assert
                Assert.AreEqual(0L, await item.GetInt(Pid.TestInt));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

    }
}
