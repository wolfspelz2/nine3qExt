using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace nine3q.Items.Test
{
    [TestClass]
    public class AspectTest
    {
        [TestMethod]
        public void Aspect_all_aspects_are_registered_at_AspectRegistry()
        {
            foreach (var pid in Enum.GetValues(typeof(Pid)).Cast<Pid>()) {
                var prop = Property.Get(pid);
                if (prop.Group == Property.Group.Aspect) {
                    Assert.IsTrue(AspectRegistry.Aspects.ContainsKey(pid), "" + pid);
                }
            }
        }

    }
}
