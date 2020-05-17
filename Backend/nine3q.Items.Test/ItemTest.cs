using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nine3q.Items.Test
{
    [TestClass]
    public class ItemTest
    {
        [TestMethod]
        public void Executes_action_by_aspect()
        {
            // Arrange
            var inv = new Inventory();
            var item1 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 1, [Pid.IsTest1] = true, });
            var item2 = inv.CreateItem(new PropertySet { [Pid.TestInt] = 2, });
            var arguments = new PropertySet { [Pid.Item] = item2.Id, };

            // Act
            item1.ExecuteAction(Test1Aspect.Action.AddTestInt.ToString(), arguments);

            // Assert
            Assert.AreEqual(3, item1.GetInt(Pid.TestInt));
        }

        [TestMethod]
        public void Delete_returns_true_if_deleted()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            var deleted = item.Delete(Pid.TestInt);

            // Assert
            Assert.IsTrue(deleted);
        }

        [TestMethod]
        public void Delete_returns_false_if_not_deleted()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            var deleted = item.Delete(Pid.TestInt2);

            // Assert
            Assert.IsFalse(deleted);
        }

        [TestMethod]
        public void AddToItemSet()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestItemSet, "1 2" } });

            // Act
            item.AddToItemSet(Pid.TestItemSet, (3));

            // Assert
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet("1 2 3"), item.GetItemSet(Pid.TestItemSet)));
        }

        [TestMethod]
        public void AddToEmptyItemSet()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestItemSet, "" } });

            // Act
            item.AddToItemSet(Pid.TestItemSet, (3));

            // Assert
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet("3"), item.GetItemSet(Pid.TestItemSet)));
        }

        [TestMethod]
        public void Add_redundant_item_to_ItemSet()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestItemSet, "1 2" } });

            // Act
            item.AddToItemSet(Pid.TestItemSet, (2));

            // Assert
            Assert.IsTrue(Property.AreEquivalent(Property.Type.ItemSet, new ItemIdSet("1 2"), item.GetItemSet(Pid.TestItemSet)));
        }
    }
}
