using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace nine3q.Items.Test
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        public void CreateItem_adds_to_Changed_and_Added()
        {
            // Arrange
            var inv = new Inventory();

            // Act
            Item item = null;
            inv.Transaction(() => {
                item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.ChangedItems.Contains(item.Id));
            Assert.IsTrue(summary.AddedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.ChangedItems.Count);
            Assert.AreEqual(1, summary.AddedItems.Count);
        }

        [TestMethod]
        public void Create_second_item_adds_one_to_Changed_and_Added()
        {
            // Arrange
            var inv = new Inventory();
            inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            Item item = null;
            inv.Transaction(() => {
                item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.ChangedItems.Contains(item.Id));
            Assert.IsTrue(summary.AddedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.ChangedItems.Count);
            Assert.AreEqual(1, summary.AddedItems.Count);
        }

        [TestMethod]
        public void DeleteItem_adds_to_Deleted()
        {
            // Arrange
            var inv = new Inventory();
            inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt2, 2 } });

            // Act
            inv.Transaction(() => {
                inv.DeleteItem(item.Id);
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.DeletedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.DeletedItems.Count);
            Assert.AreEqual(0, summary.ChangedItems.Count);
        }

        [TestMethod]
        public void SetItemProperties_adds_to_Changed()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            inv.Transaction(() => {
                inv.SetItemProperties(item.Id, new PropertySet { { Pid.TestInt, 2 } });
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.ChangedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.ChangedItems.Count);
        }

        [TestMethod]
        public void SetItemProperties_and_DeleteItem_just_adds_to_Deleted()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            inv.Transaction(() => {
                inv.SetItemProperties(item.Id, new PropertySet { { Pid.TestInt, 2 } });
                inv.DeleteItem(item.Id);
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.DeletedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.DeletedItems.Count);
            Assert.AreEqual(0, summary.ChangedItems.Count);
        }

        [TestMethod]
        public void AddToItemSet_adds_to_Changed()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestItemSet, "1 2" } });

            // Act
            inv.Transaction(() => {
                item.AddToItemSet(Pid.TestItemSet, new long(3));
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.ChangedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.ChangedItems.Count);
        }

        [TestMethod]
        public void RemoveFromItemSet_adds_to_Changed()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestItemSet, "1 2" } });

            // Act
            inv.Transaction(() => {
                item.AddToItemSet(Pid.TestItemSet, new long(2));
            });

            // Assert
            var summary = new ItemSummaryRecorder(inv);
            Assert.IsTrue(summary.ChangedItems.Contains(item.Id));
            Assert.AreEqual(1, summary.ChangedItems.Count);
        }

        [TestMethod]
        public void CancelTransaction_restores_changed_property()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            Assert.ThrowsException<Exception>(() =>
                inv.Transaction(() => {
                    inv.SetItemProperties(item.Id, new PropertySet { { Pid.TestInt, 2 } });
                    throw new Exception("Check CancelTransaction");
                })
            );

            // Assert
            Assert.AreEqual(0, inv.Changes.Count);
            Assert.AreEqual(1, item.GetInt(Pid.TestInt));
        }

        [TestMethod]
        public void CancelTransaction_restores_deleted_property()
        {
            // Arrange
            var inv = new Inventory();
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt, 1 }, { Pid.TestInt2, 2 } });

            // Act
            Assert.ThrowsException<Exception>(() =>
                inv.Transaction(() => {
                    inv.DeleteItemProperties(item.Id, new PidList { Pid.TestInt2 });
                    throw new Exception("Check CancelTransaction");
                })
            );

            // Assert
            Assert.AreEqual(0, inv.Changes.Count);
            Assert.AreEqual(2, item.GetInt(Pid.TestInt2));
        }

        [TestMethod]
        public void CancelTransaction_restores_deleted_item()
        {
            // Arrange
            var inv = new Inventory();
            inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });
            var item = inv.CreateItem(new PropertySet { { Pid.TestInt2, 2 } });

            // Act
            Assert.ThrowsException<Exception>(() =>
                inv.Transaction(() => {
                    inv.DeleteItem(item.Id);
                    throw new Exception("Check CancelTransaction");
                })
            );

            // Assert
            Assert.AreEqual(0, inv.Changes.Count);
            Assert.AreEqual(2, inv.Item(item.Id).GetInt(Pid.TestInt2));
        }

        [TestMethod]
        public void CancelTransaction_deletes_created_item()
        {
            // Arrange
            var inv = new Inventory();
            inv.CreateItem(new PropertySet { { Pid.TestInt, 1 } });

            // Act
            Item item = null;
            Assert.ThrowsException<Exception>(() =>
                inv.Transaction(() => {
                    item = inv.CreateItem(new PropertySet { { Pid.TestInt2, 2 } });
                    throw new Exception("Check CancelTransaction");
                })
            );

            // Assert
            Assert.AreEqual(0, inv.Changes.Count);
            Assert.AreEqual(1, inv.Items.Count);
        }

    }
}
