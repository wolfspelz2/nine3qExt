using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using n3q.Aspects;
using n3q.Tools;

namespace n3q.Items.Test
{
    [TestClass]
    public class AspectTest
    {
        [TestMethod]
        public void AsAspect()
        {
            var siloSimulator = new ItemSiloSimulator();
            Item GetItem(string id) { return new Item(siloSimulator, id, Guid.Empty); }

            var itemId = $"{nameof(AspectTest)}-{nameof(AsAspect)}-{RandomString.Get(10)}";
            var item = GetItem(itemId);
            var aspect = item.AsAspect(Pid.TestGreeterAspect);
            var aspectName = aspect.GetType().Name;
            Assert.AreEqual(nameof(TestGreeter), aspectName);
        }

        //[TestMethod]
        //public async Task Execute()
        //{
        //    var siloSimulator = new ItemSiloSimulator();
        //    Item GetItem(string id) { return new Item(siloSimulator, id); }

        //    var greetUserId = $"{nameof(AspectTest)}-{nameof(Execute) + "_GREETUSER"}-{RandomString.Get(10)}";
        //    var greeterId = $"{nameof(AspectTest)}-{nameof(Execute) + "_GREETER"}-{RandomString.Get(10)}";
        //    var greetUser = GetItem(greetUserId);
        //    var aspect = greetUser.AsAspect(Pid.TestGreetUserAspect);
        //    var greeting = await aspect.Run(nameof(TestGreetUser.UseGreeter), new PropertySet { [Pid.Item] = greeterId, [Pid.Name] = "World" });
        //    Assert.AreEqual("Hello World", (string)greeting);
        //}

    }
}
