using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;
using n3q.Items;
using n3q.GrainInterfaces;
using System.Threading;

namespace IntegrationTests
{
    [TestClass]
    public class ItemGrainTest
    {
        IItem GetItemGrain(string id) { return GrainClient.GrainFactory.GetGrain<IItem>(id); }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties()
        {
            // Arrange
            var item = GrainClient.GetItemStub($"{nameof(ItemGrainTest)}-{nameof(GetProperties)}-{RandomString.Get(10)}");

            try {
                // Act
                using (var t = await item.BeginTransaction()) {
                    ;
                    await item.ModifyProperties(new PropertySet {
                        [Pid.TestInt] = 42,
                        [Pid.TestString] = "42",
                    }, PidSet.Empty);
                }

                var props = await item.GetProperties(PidSet.All);

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
            var item = GrainClient.GetItemStub($"{nameof(ItemGrainTest)}-{nameof(GetProperties_by_access_level)}-{RandomString.Get(10)}");

            try {
                await item.ModifyProperties(new PropertySet {
                    [Pid.TestInternal] = 41,
                    [Pid.TestPublic] = 42,
                }, PidSet.Empty);

                // Act
                var props = await item.GetProperties(PidSet.Public);

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
        public async Task GetProperties_by_PidSet()
        {
            // Arrange
            var item = GrainClient.GetItemStub($"{nameof(ItemGrainTest)}-{nameof(GetProperties_by_PidSet)}-{RandomString.Get(10)}");

            try {
                await item.ModifyProperties(new PropertySet {
                    [Pid.TestInt1] = 41,
                    [Pid.TestInt2] = 42,
                    [Pid.TestInt3] = 43,
                }, PidSet.Empty);

                // Act
                var props = await item.GetProperties(new PidSet { Pid.TestInt2, Pid.TestInt3, });

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
            var item = GrainClient.GetItemStub($"{nameof(ItemGrainTest)}-{nameof(SetGet_Modify_add_set_delete)}-{RandomString.Get(10)}");

            try {
                await item.ModifyProperties(new PropertySet {
                    // stay
                    [Pid.TestString] = "40",
                    [Pid.TestInt] = 40000000000,
                    [Pid.TestFloat] = 40.14159265358979323,
                    [Pid.TestItemSet] = new ItemIdSet { "4", "0" },
                    // change
                    [Pid.TestString1] = "41",
                    [Pid.TestInt1] = 41000000000,
                    [Pid.TestFloat1] = 41.14159265358979323,
                    [Pid.TestItemSet1] = new ItemIdSet { "4", "1" },
                    // delete
                    [Pid.TestString3] = "43",
                    [Pid.TestInt3] = 43000000000,
                    [Pid.TestFloat3] = 43.14159265358979323,
                    [Pid.TestBool3] = true,
                    [Pid.TestItemSet3] = new ItemIdSet { "4", "3" },
                }, PidSet.Empty);

                // Act
                await item.ModifyProperties(new PropertySet {
                    [Pid.TestString1] = "412",
                    [Pid.TestInt1] = 41200000000,
                    [Pid.TestFloat1] = 412.14159265358979323,
                    [Pid.TestBool1] = true,
                    [Pid.TestItemSet1] = new ItemIdSet { "4", "1", "2" },

                    [Pid.TestString2] = "42",
                    [Pid.TestInt2] = 42000000000,
                    [Pid.TestFloat2] = 42.14159265358979323,
                    [Pid.TestBool2] = true,
                    [Pid.TestItemSet2] = new ItemIdSet { "4", "2" },
                }, new PidSet {
                    Pid.TestString3,
                    Pid.TestInt3,
                    Pid.TestFloat3,
                    Pid.TestBool3,
                    Pid.TestItemSet3,
                });

                // Assert
                var props = await item.GetProperties(PidSet.All);

                // stay
                Assert.AreEqual("40", props.GetString(Pid.TestString));
                Assert.AreEqual(40000000000, props.GetInt(Pid.TestInt));
                Assert.AreEqual(40.14159265358979323, props.GetFloat(Pid.TestFloat), 0.01);
                Assert.AreEqual(false, props.GetBool(Pid.TestBool));
                Assert.AreEqual("4 0", props.GetItemIdSet(Pid.TestItemSet).ToString());

                // change
                Assert.AreEqual("412", props.GetString(Pid.TestString1));
                Assert.AreEqual(41200000000, props.GetInt(Pid.TestInt1));
                Assert.AreEqual(412.14159265358979323, props.GetFloat(Pid.TestFloat1), 0.01);
                Assert.AreEqual(true, props.GetBool(Pid.TestBool1));
                Assert.AreEqual("4 1 2", props.GetItemIdSet(Pid.TestItemSet1).ToString());

                // set
                Assert.AreEqual("42", props.GetString(Pid.TestString2));
                Assert.AreEqual(42000000000, props.GetInt(Pid.TestInt2));
                Assert.AreEqual(42.14159265358979323, props.GetFloat(Pid.TestFloat2), 0.01);
                Assert.AreEqual(true, props.GetBool(Pid.TestBool2));
                Assert.AreEqual("4 2", props.GetItemIdSet(Pid.TestItemSet2).ToString());

                // delete
                Assert.AreEqual("", props.GetString(Pid.TestString3));
                Assert.AreEqual(0L, props.GetInt(Pid.TestInt3));
                Assert.AreEqual(0.0D, props.GetFloat(Pid.TestFloat3), 0.01);
                Assert.AreEqual(false, props.GetBool(Pid.TestBool3));
                Assert.AreEqual("", props.GetItemIdSet(Pid.TestItemSet3).ToString());

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
            var itemId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_with_template)}-{RandomString.Get(10)}";
            var tmplId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_with_template) + "_TEMPLATE"}-{RandomString.Get(10)}";
            var item = GrainClient.GetItemStub(itemId);
            var tmpl = GrainClient.GetItemStub(tmplId);

            try {
                await item.ModifyProperties(new PropertySet {
                    [Pid.TemplateId] = tmplId,
                    [Pid.TestInt] = 42,
                }, PidSet.Empty);

                // Act
                var props = await item.GetProperties(PidSet.All);

                // Assert
                Assert.AreEqual(2, props.Count);
                Assert.AreEqual(tmplId, (string)props.Get(Pid.TemplateId));
                Assert.AreEqual(42, (long)props.Get(Pid.TestInt));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_PidSet_with_template()
        {
            // Arrange
            var itemId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_PidSet_with_template)}-{RandomString.Get(10)}";
            var tmplId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_PidSet_with_template) + "_TEMPLATE"}-{RandomString.Get(10)}";
            var item = GrainClient.GetItemStub(itemId);
            var tmpl = GrainClient.GetItemStub(tmplId);

            try {
                await item.ModifyProperties(new PropertySet {
                    [Pid.TemplateId] = tmplId,
                    [Pid.TestInt] = 40,
                    [Pid.TestString] = "item.TestString",   // item
                    [Pid.TestString1] = "item.TestString1", // item get
                    [Pid.TestString2] = "item.TestString2", // both
                    [Pid.TestString3] = "item.TestString3", // both get
                }, PidSet.Empty);
                await tmpl.ModifyProperties(new PropertySet {
                    [Pid.TestString2] = "tmpl.TestString2", // both
                    [Pid.TestString3] = "tmpl.TestString3", // both get
                    [Pid.TestString4] = "tmpl.TestString4", // tmpl
                    [Pid.TestString5] = "tmpl.TestString5", // tmpl get
                }, PidSet.Empty);

                // Act
                var props = await item.GetProperties(new PidSet { Pid.TestString1, Pid.TestString3, Pid.TestString5 });

                // Assert
                Assert.AreEqual(3, props.Count);
                Assert.AreEqual("item.TestString1", (string)props.Get(Pid.TestString1));
                Assert.AreEqual("item.TestString3", (string)props.Get(Pid.TestString3));
                Assert.AreEqual("tmpl.TestString5", (string)props.Get(Pid.TestString5));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties_PidSet_with_template_and_native()
        {
            // Arrange
            var itemId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_PidSet_with_template_and_native)}-{RandomString.Get(10)}";
            var tmplId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_PidSet_with_template_and_native) + "_TEMPLATE"}-{RandomString.Get(10)}";
            var item = GrainClient.GetItemStub(itemId);
            var tmpl = GrainClient.GetItemStub(tmplId);

            try {
                await item.ModifyProperties(new PropertySet {
                    [Pid.TemplateId] = tmplId,
                    [Pid.TestInt] = 40,
                    [Pid.TestString] = "item.TestString",   // item
                    [Pid.TestString1] = "item.TestString1", // item get
                    [Pid.TestString2] = "item.TestString2", // both
                    [Pid.TestString3] = "item.TestString3", // both get
                }, PidSet.Empty);
                await tmpl.ModifyProperties(new PropertySet {
                    [Pid.TestString2] = "tmpl.TestString2", // both
                    [Pid.TestString3] = "tmpl.TestString3", // both get
                    [Pid.TestString4] = "tmpl.TestString4", // tmpl
                    [Pid.TestString5] = "tmpl.TestString5", // tmpl get
                }, PidSet.Empty);

                // Act
                var props = await item.GetProperties(new PidSet { Pid.TestString1, Pid.TestString3, Pid.TestString5 }, native: true);

                // Assert
                Assert.AreEqual(2, props.Count);
                Assert.AreEqual("item.TestString1", (string)props.Get(Pid.TestString1));
                Assert.AreEqual("item.TestString3", (string)props.Get(Pid.TestString3));
                //Assert.AreEqual("tmpl.TestString5", (string)props.Get(Pid.TestString5));

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

    }
}
