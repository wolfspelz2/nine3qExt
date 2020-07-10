using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Tools;

namespace n3q.Items.Test
{
    [TestClass]
    public class PidTest
    {
        [TestMethod]
        public void All_pids_have_definitions()
        {
            var missing = new List<Pid>();
            var problem = new Dictionary<Pid, string>();
            foreach (var pid in Enum.GetValues(typeof(Pid)).Cast<Pid>()) {
                try {
                    var prop = Property.GetDefinition(pid);
                    if (prop.Group != Property.Group.Test && prop.Group != Property.Group.System) {
                        if (prop.Default == null) { problem.Add(pid, "Default"); };
                        if (!Has.Value(prop.Example)) { problem.Add(pid, "Example"); };
                        if (!Has.Value(prop.Description)) { problem.Add(pid, "Description"); };
                    }
                } catch (Exception) {
                    missing.Add(pid);
                }
            }
            Assert.AreEqual(0, missing.Count, "Missing: " + string.Join(" ", missing));
            Assert.AreEqual(0, problem.Count, "Incomplete: " + string.Join(" | ", problem.Select(kv => $"{kv.Key}:{kv.Value}")));
        }

    }
}
