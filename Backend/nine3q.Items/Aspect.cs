using System;
using System.Collections.Generic;
using nine3q.Tools;

namespace nine3q.Items
{
    public class Aspect : Item
    {
        public delegate Aspect AspectSpecializer(Item item);
        public delegate void ActionHandler(PropertySet args);

        public class ActionDescription
        {
            public ActionHandler Handler { set; get; }
        }
        public class ActionList : Dictionary<string, ActionDescription> { }

        protected Aspect(Item self) : base(self.Inventory, self.Id)
        {
            // Set here instead of base() circumvents Properties.Normalize
            Properties = self.Properties;
        }

        public virtual ActionList GetActionList()
        {
            return null;
        }

        internal virtual void OnAspectCreate()
        {
        }

        internal virtual void OnAspectDelete()
        {
        }

        internal virtual void OnAspectActivate()
        {
        }

        internal virtual void OnAspectDeactivate()
        {
        }

        internal virtual void OnAspectPropertyChange(ItemChange change)
        {
        }

        internal virtual void OnAspectTimer(string name)
        {
        }

        public bool ExecuteAspectAction(string action, PropertySet arguments)
        {
            var actions = GetActionList();
            if (actions != null) {
                if (actions.ContainsKey(action)) {
                    actions[action].Handler(arguments);
                    return true;
                }
            }
            return false;
        }
    }

    #region Test Aspects

    public static class Test1AspectExtensions
    {
        public static Test1Aspect AsTest1(this Item self) { self.AssertAspect(Pid.IsTest1); return new Test1Aspect(self); }
        public static bool IsTest1(this Item self) { return self.IsAspect(Pid.IsTest1); }
    }

    public class Test1Aspect : Aspect
    {
        public Test1Aspect(Item self) : base(self) { }

        public enum Action { Nop, SetTestInt42, AddTestInt, AddTestIntChangeAndDeletePassive }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { Action.Nop.ToString(), new ActionDescription() { Handler = (args) => Nop() } },
                { Action.SetTestInt42.ToString(), new ActionDescription() { Handler = (args) => SetTestInt42() } },
                { Action.AddTestInt.ToString(), new ActionDescription() { Handler = (args) => AddTestInt(Inventory.Item(args.GetItem(Pid.Item))) } },
                { Action.AddTestIntChangeAndDeletePassive.ToString(), new ActionDescription() { Handler = (args) => {
                    var passiveId = args.GetItem(Pid.Item);
                    if (passiveId == long.NoItem) { throw new Exceptions.MissingActionArgumentException(Inventory.Name, Id,  Action.AddTestIntChangeAndDeletePassive.ToString(), Pid.Item); }
                    AddTestIntChangeAndDeletePassive(Inventory.Item(passiveId));
                }}},
            };
        }

        public void Nop() => Utils.Dont = () => { var x = Get(Pid.TestBool); };

        internal void SetTestInt42()
        {
            Set(Pid.TestInt, 42);
        }

        internal void AddTestInt(Item summand)
        {
            Set(Pid.TestInt, GetInt(Pid.TestInt) + summand.GetInt(Pid.TestInt));
        }

        internal void AddTestIntChangeAndDeletePassive(Item passive)
        {
            Set(Pid.TestInt, GetInt(Pid.TestInt) + passive.GetInt(Pid.TestInt));
            passive.Set(Pid.TestString, "TestInt changed");
            Inventory.DeleteItem(passive.Id);
        }

    }

    public static class Test2AspectExtensions
    {
        public static Test2Aspect AsTest2(this Item self) { self.AssertAspect(Pid.IsTest2); return new Test2Aspect(self); }
        public static bool IsTest2(this Item self) { return self.IsAspect(Pid.IsTest2); }
    }

    public class Test2Aspect : Aspect
    {
        public Test2Aspect(Item self) : base(self) { }

        public enum Action { Nop }
        public override ActionList GetActionList()
        {
            return new ActionList() {
                { Action.Nop.ToString(), new ActionDescription() { Handler = (args) => Nop() } },
            };
        }

        public void Nop() => Utils.Dont = () => { var x = Get(Pid.TestBool); };
    }

    #endregion
}
