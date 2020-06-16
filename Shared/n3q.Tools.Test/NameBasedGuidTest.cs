using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace n3q.Tools.Test
{
    [TestClass]
    public class NameBasedGuidTest
    {
        [TestMethod]
        public void Different_names_different_guids()
        {
            // Arrange // Act // Assert
            Assert.IsTrue(NameBasedGuid.Create(NameBasedGuid.UrlNamespace, "http://www.example.com/1") != NameBasedGuid.Create(NameBasedGuid.UrlNamespace, "http://www.example.com/2"));
        }

        [TestMethod]
        public void Same_name_same_guid()
        {
            // Arrange // Act // Assert
            Assert.IsTrue(NameBasedGuid.Create(NameBasedGuid.UrlNamespace, "http://www.example.com/") == NameBasedGuid.Create(NameBasedGuid.UrlNamespace, "http://www.example.com/"));
        }

        [TestMethod]
        public void Is_not_empty()
        {
            // Arrange // Act // Assert
            Assert.AreNotEqual(Guid.Empty, NameBasedGuid.Create(NameBasedGuid.UrlNamespace, "http://www.example.com/"));
        }
    }
}
