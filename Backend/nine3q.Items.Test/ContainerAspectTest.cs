using Microsoft.VisualStudio.TestTools.UnitTesting;
using nine3q.Items.Aspects;
using System;

namespace nine3q.Items.Test
{
    [TestClass]
    public class ContainerAspectTest
    {
        [TestMethod]
        public void Container_adds_child()
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
        public void Container_AddChild_assigns_desired_slot_to_existing_child()
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
        public void Container_SetChild_needs_shuffle_to_assign_desired_slot_to_existing_child()
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

        [TestMethod]
        public void Container_AddChild_throws_when_desired_slot_is_occupied_for_existing_child()
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
        public void Container_AddChild_assigns_desired_slot_for_new_child()
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
        public void Container_AddChild_throws_when_desired_slot_is_occupied_for_new_child()
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
        public void Container_SetChild_throws_when_not_ContainerCanImport()
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
        public void Container_delete_container_removes_child()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var child = inv.CreateItem(new PropertySet { });
            container.AsContainer().AddChild(child);
            Assert.AreEqual(2, inv.GetItems().Count);

            // Act
            inv.DeleteItem(container.Id);

            // Assert
            Assert.AreEqual(0, inv.GetItems().Count);
        }

        [TestMethod]
        public void Container_delete_child_removes_child_from_container()
        {
            // Arrange
            var inv = new Inventory();
            var container = inv.CreateItem(new PropertySet { [Pid.IsContainer] = true, });
            var child = inv.CreateItem(new PropertySet { });
            container.AsContainer().AddChild(child);
            Assert.AreEqual(2, inv.GetItems().Count);

            // Act
            inv.DeleteItem(child.Id);

            // Assert
            Assert.AreEqual(1, inv.GetItems().Count);
            Assert.AreEqual(0, container.GetItemSet(Pid.Contains).Count);
            Assert.AreEqual(0, container.AsContainer().GetChildren().Count);
        }

    }
}

