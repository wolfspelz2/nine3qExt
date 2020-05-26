using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace n3q.Items.Test
{
    [TestClass]
    public class PropertySetTest
    {
        [TestMethod]
        public void Delete()
        {
            // Arrange
            var props = new PropertySet { [Pid.TestString] = 41, [Pid.TestInt] = 42 };

            // Act
            props.Delete(Pid.TestInt);

            // Assert
            Assert.AreEqual(1, props.Count);
            Assert.AreEqual("41", (string)props.Get(Pid.TestString));
        }

    }
}
