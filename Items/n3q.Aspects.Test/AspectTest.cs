using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.Items.Test
{
    [TestClass]
    public class AspectTest
    {
        [TestMethod]
        public void All_aspects_are_registered_at_AspectRegistry()
        {
            foreach (var pid in Enum.GetValues(typeof(Pid)).Cast<Pid>()) {
                var prop = Property.GetDefinition(pid);
                if (prop.Group == Property.Group.Aspect) {
                    Assert.IsTrue(AspectRegistry.Aspects.ContainsKey(pid), "" + pid);
                }
            }
        }

        [TestMethod]
        public void AsAspect()
        {
            // Arrange
            var siloSimulator = new SiloSimulator();

            ItemStub GetItem(string id)
            {
                var simulatorClient = new SiloSimulatorItemClient(siloSimulator, id);
                return new ItemStub(simulatorClient, new ItemTransaction());
            }

            var itemId = RandomString.Get(10);
            var item = GetItem(itemId);

            // Act
            var aspect = item.AsAspect(Pid.TestGreeterAspect);

            // Assert
            Assert.AreEqual(nameof(TestGreeter), aspect.GetType().Name);
        }

        [TestMethod]
        public async Task Execute()
        {
            // Arrange
            var greetedId = "GREETED";
            var greeterId = "GREETER";

            var siloSimulator = new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [greetedId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.TestGreetedAspect] = true,
                        }
                    },
                    [greeterId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.TestGreeterAspect] = true,
                            [Pid.TestGreeterPrefix] = "a",
                        }
                    },
                }
            };
            var siloSimulatorClient = new SiloSimulatorClusterClient(siloSimulator);

            ItemStub GetItemStub(string id)
            {
                var siloSimulatorItemClient = siloSimulatorClient.GetItemClient(id);
                return new ItemStub(siloSimulatorItemClient, new VoidTransaction());
            }

            var greeted = GetItemStub(greetedId);
            var aspect = greeted.AsAspect(Pid.TestGreetedAspect);

            // Act
            await aspect.Execute(nameof(TestGreeted.GetGreeting), new PropertySet { [Pid.TestGreetedGetGreetingGreeter] = greeterId, [Pid.TestGreetedGetGreetingName] = "b" });

            // Assert
            Assert.AreEqual("ab", (string)siloSimulator.Items[greetedId].Properties[Pid.TestGreetedResult]);
        }

    }
}
