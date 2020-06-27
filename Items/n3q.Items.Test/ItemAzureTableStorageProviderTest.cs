using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.StorageProviders;

namespace n3q.Items.Test
{
    [TestClass]
    public class ItemAzureTableStorageProviderTest
    {
        [TestMethod]
        public void Provider_Template_Pid_name_equals_Pid_Template_Id()
        {
            Assert.AreEqual(ItemAzureTableStorage.PidTemplate, Pid.Template.ToString());
        }

    }
}
