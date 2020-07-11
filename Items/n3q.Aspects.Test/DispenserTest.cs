using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Common;
using n3q.Tools;
using n3q.WebIt;

namespace n3q.Items.Test
{
    [TestClass]
    public class DispenserTest
    {
        [TestMethod]
        public async Task GetItem()
        {
            // Arrange
            var DISPENSERID = "dispenser1";
            var GREETERTPL = "greeterTpl";

            var siloSimulator = new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [DISPENSERID] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DispenserAspect] = true,
                            [Pid.DispenserTemplate] = GREETERTPL,
                            [Pid.DispenserMaxAvailable] = 3,
                            [Pid.DispenserAvailable] = 2,
                            [Pid.DispenserCooldownSec] = 10.0D,
                            [Pid.DispenserLastTime] = 0L,
                        }
                    },
                    [GREETERTPL] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.GreeterAspect] = true,
                            [Pid.GreeterPrefix] = "a",
                        }
                    },
                }
            };
            var siloSimulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            Assert.AreEqual(2, siloSimulator.Items.Count);

            // Act
            await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                await self.AsDispenser().GetItem();
            });

            // Assert
            Assert.AreEqual(3, siloSimulator.Items.Count);
        }

        [TestMethod]
        public async Task GetItem_depletes()
        {
            // Arrange
            var DISPENSERID = "dispenser1";
            var GREETERTPL = "greeterTpl";

            var siloSimulator = new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [DISPENSERID] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DispenserAspect] = true,
                            [Pid.DispenserTemplate] = GREETERTPL,
                            [Pid.DispenserMaxAvailable] = 3,
                            [Pid.DispenserAvailable] = 1,
                            [Pid.DispenserCooldownSec] = 10.0D,
                            [Pid.DispenserLastTime] = 0L,
                        }
                    },
                    [GREETERTPL] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.GreeterAspect] = true,
                            [Pid.GreeterPrefix] = "a",
                        }
                    },
                }
            };
            var siloSimulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            Assert.AreEqual(2, siloSimulator.Items.Count);

            await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                await self.AsDispenser().GetItem();
            });

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ItemException>(async () => {
                await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                    await self.AsDispenser().GetItem();
                });
            });
        }

        [TestMethod]
        public async Task GetItem_in_cooldown()
        {
            // Arrange
            var DISPENSERID = "dispenser1";
            var GREETERTPL = "greeterTpl";

            var siloSimulator = new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [DISPENSERID] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DispenserAspect] = true,
                            [Pid.DispenserTemplate] = GREETERTPL,
                            [Pid.DispenserMaxAvailable] = 3,
                            [Pid.DispenserAvailable] = 2,
                            [Pid.DispenserCooldownSec] = 10.0D,
                            [Pid.DispenserLastTime] = 0L,
                        }
                    },
                    [GREETERTPL] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.GreeterAspect] = true,
                            [Pid.GreeterPrefix] = "a",
                        }
                    },
                }
            };
            var siloSimulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            Assert.AreEqual(2, siloSimulator.Items.Count);

            var time = DateTime.UtcNow;

            await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                await self.AsDispenser().GetItem();
            });

            // Act
            // Assert
            await Assert.ThrowsExceptionAsync<ItemException>(async () => {
                await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                    await self.AsDispenser().GetItem();
                });
            });
        }

        [TestMethod]
        public async Task GetItem_after_cooldown()
        {
            // Arrange
            var DISPENSERID = "dispenser1";
            var GREETERTPL = "greeterTpl";

            var siloSimulator = new SiloSimulator() {
                Items = new Dictionary<string, SiloSimulatorItem> {
                    [DISPENSERID] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.DispenserAspect] = true,
                            [Pid.DispenserTemplate] = GREETERTPL,
                            [Pid.DispenserMaxAvailable] = 3,
                            [Pid.DispenserAvailable] = 2,
                            [Pid.DispenserCooldownSec] = 10.0D,
                            [Pid.DispenserLastTime] = 0L,
                        }
                    },
                    [GREETERTPL] = new SiloSimulatorItem {
                        Properties = new PropertySet {
                            [Pid.GreeterAspect] = true,
                            [Pid.GreeterPrefix] = "a",
                        }
                    },
                }
            };
            var siloSimulatorClient = new SiloSimulatorClusterClient(siloSimulator);
            Assert.AreEqual(2, siloSimulator.Items.Count);

            var time = new DateTime(1969, 7, 20, 20, 17, 0);
            await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                await self.AsTimed().SetCurrentTime(time);
                await self.AsDispenser().GetItem();
                time = time.AddSeconds(11);
                await self.AsTimed().SetCurrentTime(time);
            });

            // Act
            await siloSimulatorClient.Transaction(DISPENSERID, async self => {
                await self.AsDispenser().GetItem();
            });

            // Assert
            Assert.AreEqual(4, siloSimulator.Items.Count);
        }

    }
}
