using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace n3q.Items.Test
{
    [TestClass]
    public class PidTest
    {
        [TestMethod]
        public void All_pids_have_definitions()
        {
            foreach (var pid in Enum.GetValues(typeof(Pid)).Cast<Pid>()) {
                var prop = Property.GetDefinition(pid);
                Assert.AreEqual(pid, prop.Id, "Id of PropertyId." + pid.ToString());
                //Assert.AreEqual(pid.ToString(), prop.Name, "Name of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Basic, "Type of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Use, "Use of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Group, "Group of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Access, "Access of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Persistence, "Persistence of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Example, "Example of PropertyId." + pid.ToString());
                Assert.IsNotNull(prop.Description, "Description of PropertyId." + pid.ToString());
            }
        }

    }
}
