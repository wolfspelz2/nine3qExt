﻿using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.Items.Aspects;

namespace nine3q.Items.Test
{
    [TestClass]
    public class ContainerAspectTest
    {
        [TestMethod]
        public void Adds_child()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var child = inv.CreateItem(new PropertySet { });

            // Act
            container.AsContainer().AddChild(child);

            // Assert
            Assert.AreEqual(1, container.GetItemSet(Pid.Contains).Count);
            Assert.IsTrue(container.GetItemSet(Pid.Contains).Contains(child.Id));
            Assert.AreEqual(container.Id, child.GetItem(Pid.Container));
        }

        [TestMethod]
        public void AddChild_assigns_desired_slot_to_existing_child()
        {
            // Arrange
            var inv = new Inventory();
            // Create configuration
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 4 });
            var child1 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child1, 1);
            var child2 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child2, 2);
            var child3 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child3, 3);
            var child4 = inv.CreateItem(new PropertySet { }); // <- desired slot

            // Act
            container.AsContainer().AddChild(child2, 4);

            // Assert
            Assert.AreEqual(4, child2.GetInt(Pid.Slot));
        }

        [TestMethod]
        public void SetChild_needs_shuffle_to_assign_desired_slot_to_existing_child()
        {
            // Arrange
            var inv = new Inventory();
            // Create configuration
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 4 });
            var child1 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child1, 1);
            var child2 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child2, 2);
            var child3 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child3, 3);
            var child4 = inv.CreateItem(new PropertySet { }); // <- desired slot

            // Act
            Assert.ThrowsException<Exceptions.MissingItemPropertyException>(() => container.AsContainer().SetChild(child2, 4));
            container.SetBool(Pid.ContainerCanShuffle, true);
            container.AsContainer().SetChild(child2, 4);

            // Assert
            Assert.AreEqual(4, child2.GetInt(Pid.Slot));
        }

        [DebuggerNonUserCode]
        [TestMethod]
        public void AddChild_throws_when_desired_slot_is_occupied_for_existing_child()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 4 });
            var child1 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child1, 1);
            var child2 = inv.CreateItem(new PropertySet { }); // <- free slot
            var child3 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child3, 3); // <- desired slot
            var child4 = inv.CreateItem(new PropertySet { });

            // Act
            // Assert
            Assert.ThrowsException<Exceptions.SlotAvailabilityException>(() => container.AsContainer().AddChild(child4, 3));
        }

        [TestMethod]
        public void AddChild_assigns_desired_slot_for_new_child()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 4 });
            var child1 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child1, 1);
            var child2 = inv.CreateItem(new PropertySet { }); // <- free slot
            var child3 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child3, 3);
            var child4 = inv.CreateItem(new PropertySet { }); // <- desired slot

            // Act
            container.AsContainer().AddChild(child4, 4);

            // Assert
            Assert.AreEqual(4, child4.GetInt(Pid.Slot));
        }

        [TestMethod]
        public void AddChild_throws_when_desired_slot_is_occupied_for_new_child()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 4 });
            var child1 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child1, 1);
            var child2 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child2, 2);
            var child3 = inv.CreateItem(new PropertySet { });  // <- free slot
            var child4 = inv.CreateItem(new PropertySet { }); container.AsContainer().AddChild(child4, 4);

            // Act
            // Assert
            Assert.ThrowsException<Exceptions.SlotAvailabilityException>(() => container.AsContainer().AddChild(child3, 1));
        }

        [TestMethod]
        public void SetChild_throws_when_not_ContainerCanImport()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, [Pid.Slots] = 4 });
            var child1 = inv.CreateItem(new PropertySet { [Pid.Slot] = 1 }); container.AsContainer().AddChild(child1);
            var child2 = inv.CreateItem(new PropertySet { [Pid.Slot] = 2 }); container.AsContainer().AddChild(child2);
            var child3 = inv.CreateItem(new PropertySet { }); // <- free slot
            var child4 = inv.CreateItem(new PropertySet { [Pid.Slot] = 4 }); container.AsContainer().AddChild(child4);

            // Act
            // Assert
            Assert.ThrowsException<Exceptions.MissingItemPropertyException>(() => container.AsContainer().SetChild(child3, 3));
        }

        [TestMethod]
        public void Delete_container_removes_child()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var child = inv.CreateItem(new PropertySet { });
            container.AsContainer().AddChild(child);
            Assert.AreEqual(2, inv.GetItemIds().Count);

            // Act
            inv.DeleteItem(container.Id);

            // Assert
            Assert.AreEqual(0, inv.GetItemIds().Count);
        }

        [TestMethod]
        public void Delete_child_removes_child_from_container()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var child = inv.CreateItem(new PropertySet { });
            container.AsContainer().AddChild(child);
            Assert.AreEqual(2, inv.GetItemIds().Count);

            // Act
            inv.DeleteItem(child.Id);

            // Assert
            Assert.AreEqual(1, inv.GetItemIds().Count);
            Assert.AreEqual(0, container.GetItemSet(Pid.Contains).Count);
            Assert.AreEqual(0, container.AsContainer().GetChildren().Count);
        }

    }
}
