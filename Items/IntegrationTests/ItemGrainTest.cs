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
        IItem GetItemGrain(string id)
        {
            return GrainClient.GrainFactory.GetGrain<IItem>(id);
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task SetGet_string()
        {
            // Arrange
            var item = GetItemGrain($"{nameof(ItemGrainTest)}-{nameof(SetGet_string)}-{RandomString.Get(10)}");

            try {
                // Act
                await item.Set(Pid.TestString, "42");
                var value = await item.GetString(Pid.TestString);

                // Assert
                Assert.AreEqual("42", value);

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task SetGet_PropertyValue()
        {
            // Arrange
            var item = GetItemGrain($"{nameof(ItemGrainTest)}-{nameof(SetGet_PropertyValue)}-{RandomString.Get(10)}");

            try {
                // Act
                await item.Set(Pid.TestString, "42");
                var value = await item.Get(Pid.TestString);

                // Assert
                Assert.AreEqual("42", (string)value);

            } finally {
                // Cleanup
                await item.DeletePersistentStorage();
            }
        }

        [TestMethod]
        [TestCategory(GrainClient.Category)]
        public async Task GetProperties()
        {
            // Arrange
            var item = GetItemGrain($"{nameof(ItemGrainTest)}-{nameof(GetProperties)}-{RandomString.Get(10)}");

            try {
                // Act
                await item.Set(Pid.TestInt, 42);
                await item.Set(Pid.TestString, "42");
                var props = await item.GetProperties(PidSet.All);

                // Assert
                Assert.AreEqual(42, (long)props.Get(Pid.TestInt));
                Assert.AreEqual(42, (int)props.Get(Pid.TestInt));
                Assert.AreEqual("42", (string)props.Get(Pid.TestString));

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
            var item = GetItemGrain($"{nameof(ItemGrainTest)}-{nameof(GetProperties_by_access_level)}-{RandomString.Get(10)}");

            try {
                await item.Set(Pid.TestInternal, 41);
                await item.Set(Pid.TestPublic, 42);

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
            var item = GetItemGrain($"{nameof(ItemGrainTest)}-{nameof(GetProperties_by_PidSet)}-{RandomString.Get(10)}");

            try {
                await item.Set(Pid.TestInt1, 41);
                await item.Set(Pid.TestInt2, 42);
                await item.Set(Pid.TestInt3, 43);

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
        public async Task Delete()
        {
            // Arrange
            var item = GetItemGrain($"{nameof(ItemGrainTest)}-{nameof(Delete)}-{RandomString.Get(10)}");

            try {
                await item.Set(Pid.TestInt, 42);
                await item.Set(Pid.TestString, "42");

                // Act
                await item.Delete(Pid.TestInt);

                // Assert
                var props = await item.GetProperties(PidSet.All);
                Assert.AreEqual(1, props.Count);
                Assert.AreEqual("42", (string)props.Get(Pid.TestString));

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
            var item = GetItemGrain(itemId);
            var tmpl = GetItemGrain(tmplId);

            try {
                await item.Set(Pid.TemplateId, tmplId);
                await tmpl.Set(Pid.TestInt, 42);

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
            var item = GetItemGrain(itemId);
            var tmpl = GetItemGrain(tmplId);

            try {
                await item.Set(Pid.TemplateId, tmplId);
                await item.Set(Pid.TestInt, 40);
                await item.Set(Pid.TestString, "item.TestString");   // item
                await item.Set(Pid.TestString1, "item.TestString1"); // item get
                await item.Set(Pid.TestString2, "item.TestString2"); // both
                await tmpl.Set(Pid.TestString2, "tmpl.TestString2"); // both
                await item.Set(Pid.TestString3, "item.TestString3"); // both get
                await tmpl.Set(Pid.TestString3, "tmpl.TestString3"); // both get
                await tmpl.Set(Pid.TestString4, "tmpl.TestString4"); // tmpl
                await tmpl.Set(Pid.TestString5, "tmpl.TestString5"); // tmpl get

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
        public async Task GetProperties_PidSet_with_template__and_native()
        {
            // Arrange
            var itemId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_PidSet_with_template__and_native)}-{RandomString.Get(10)}";
            var tmplId = $"{nameof(ItemGrainTest)}-{nameof(GetProperties_PidSet_with_template__and_native) + "_TEMPLATE"}-{RandomString.Get(10)}";
            var item = GetItemGrain(itemId);
            var tmpl = GetItemGrain(tmplId);

            try {
                await item.Set(Pid.TemplateId, tmplId);
                await item.Set(Pid.TestInt, 40);
                await item.Set(Pid.TestString, "item.TestString");   // item
                await item.Set(Pid.TestString1, "item.TestString1"); // item get
                await item.Set(Pid.TestString2, "item.TestString2"); // both
                await tmpl.Set(Pid.TestString2, "tmpl.TestString2"); // both
                await item.Set(Pid.TestString3, "item.TestString3"); // both get
                await tmpl.Set(Pid.TestString3, "tmpl.TestString3"); // both get
                await tmpl.Set(Pid.TestString4, "tmpl.TestString4"); // tmpl
                await tmpl.Set(Pid.TestString5, "tmpl.TestString5"); // tmpl get

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
