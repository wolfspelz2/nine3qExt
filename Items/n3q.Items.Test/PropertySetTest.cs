using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace n3q.Items.Test
{
    [TestClass]
    public class PropertySet
    {
        [TestMethod]
        public void Serializes_correctly()
        {
            // Arrange
            var props = new Dictionary<Pid, PropertyValue> { [Pid.TestInt] = 42 };

            // Act
            var json = JsonConvert.SerializeObject(props);

            var propsDeserialized = JsonConvert.DeserializeObject<PropertyValue>(json);

            // Assert
            //Assert.AreEqual("42", s);
        }

    }
}
