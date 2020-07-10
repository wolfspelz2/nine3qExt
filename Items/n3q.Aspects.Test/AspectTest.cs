using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Tools;
using n3q.WebIt;

namespace n3q.Items.Test
{
    [TestClass]
    public class AspectTest
    {
        [TestMethod]
        public void All_aspects_are_registered_at_AspectRegistry()
        {
            var missing = new List<Pid>();
            foreach (var pid in Enum.GetValues(typeof(Pid)).Cast<Pid>()) {
                var prop = Property.GetDefinition(pid);
                if (prop.Group == Property.Group.Aspect) {
                    if (!AspectRegistry.Aspects.ContainsKey(pid)) {
                        missing.Add(pid);
                    }
                }
            }
            Assert.AreEqual(0, missing.Count, "Missing: " + string.Join(" ", missing));
        }

        [TestMethod]
        public void AsAspect()
        {
            // Arrange
            var siloSimulator = new SiloSimulator();

            ItemWriter GetItemWriter(string id)
            {
                var simulatorClient = new SiloSimulatorItemClient(siloSimulator, id);
                return new ItemWriter(simulatorClient);
            }

            var itemId = RandomString.Get(10);
            var item = GetItemWriter(itemId);

            // Act
            var aspect = item.AsAspect(Pid.GreeterAspect);

            // Assert
            Assert.AreEqual(nameof(Greeter), aspect.GetType().Name);
        }

        [TestMethod]
        public void DeveloperAspect_config_names_identical_to_definition()
        {
            Assert.AreEqual(nameof(WebItConfigDefinition.PayloadHashSecret), Aspects.Developer.PayloadHashSecretConfigName);
            Assert.AreEqual(nameof(WebItConfigDefinition.ItemServiceWebApiUrl), Aspects.Developer.ItemServiceWebApiUrlConfigName);
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
                            [Pid.GreetedAspect] = true,
                        }
                    },
                    [greeterId] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.GreeterAspect] = true,
                            [Pid.GreeterPrefix] = "a",
                        }
                    },
                }
            };
            var siloSimulatorClient = new SiloSimulatorClusterClient(siloSimulator);

            ItemWriter GetItemWriter(string id)
            {
                var siloSimulatorItemClient = siloSimulatorClient.ItemClient(id);
                return new ItemWriter(siloSimulatorItemClient);
            }

            var greeted = GetItemWriter(greetedId);

            // Act
            await greeted.WithTransaction(async self => {
                await self.AsGreeted().Execute(nameof(Greeted.GetGreeting), new PropertySet { [Pid.GreetedGetGreetingGreeter] = greeterId, [Pid.GreetedGetGreetingName] = "b" });
            });

            // Assert
            Assert.AreEqual("ab", (string)siloSimulator.Items[greetedId].Properties[Pid.GreetedResult]);
        }

    }
}
