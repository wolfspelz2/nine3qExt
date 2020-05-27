using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.Items.Test
{
    [TestClass]
    public class AspectTest
    {
        Item GetDummyItem(string id) { return new Item(null, id); }

        [TestMethod]
        public void AsAspect()
        {
            var itemId = $"{nameof(AspectTest)}-{nameof(AsAspect)}-{RandomString.Get(10)}";
            var item = GetDummyItem(itemId);
            var aspect = item.AsAspect(Pid.TestGreeterAspect);
            var aspectName = aspect.GetType().Name;
            Assert.AreEqual(nameof(TestGreeter), aspectName);
        }

        [TestMethod]
        public async Task Execute()
        {
            var greetUserId = $"{nameof(AspectTest)}-{nameof(AsAspect) + "_GREETUSER"}-{RandomString.Get(10)}";
            var greeterId = $"{nameof(AspectTest)}-{nameof(AsAspect) + "_GREETER"}-{RandomString.Get(10)}";
            var greetUser = GetDummyItem(greetUserId);
            var aspect = greetUser.AsAspect(Pid.TestGreetUserAspect);
            var greeting = await aspect.Run(nameof(TestGreetUser.UseGreeter), new PropertySet { [Pid.Item] = greeterId, [Pid.Name] = "World" });
            Assert.AreEqual("Hello World", (string)greeting);
        }

    }
}
