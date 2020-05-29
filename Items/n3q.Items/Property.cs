using System;
using System.Collections.Generic;

namespace n3q.Items
{
    public static class Property
    {
        public enum Basic
        {
            Unknown = 0,
            Int,
            String,
            Float,
            Bool,
        }

        public enum Type
        {
            Unknown = 0,
            Int,
            String,
            Float,
            Bool,
            Item,
            ItemList,
        }

        public enum Use
        {
            Unknown = 0,
            Bool,
            Int,
            Float,
            Percent,
            //Skill,
            //Time, // as Long YYYYMMDDhhmmssiii
            Delay, // in float sec
            String,
            //StringList,
            Json,
            //KeyValueList,
            ImageUrl,
            Url,
            UserId,
            //Text,
            //Translated,
            Item,
            ItemList,
            Ccm,
            Gram,
        }

        public enum Access
        {
            Unknown = 0,
            System = 1, // System sees all
            Owner = 2, // Owner sees all props except the internal props
            Public = 3, // Everyone else sees only the public props
        }

        public enum Persistence
        {
            Unknown = 0,
            Fixed, // Will not change
            Persistent, // Write through
            Slow, // Safe about hourly
            Unload, // Save on unload
            Transient, // Never save
        }


        public enum Group
        {
            Unknown = 0,
            Operation,
            Test,
            Generic,
            Aspect,
            Parameter,
            App,
            Attribute, // User attributes
            //Competition,
        }

        public class Definition
        {
            public Pid Id { get; set; }
            public Basic Basic { get; set; }
            public Type Type { get; set; }
            public Use Use { get; set; }
            public Group Group { get; set; }
            public Access Access { get; set; }
            public Persistence Persistence { get; set; }
            public string Example { get; set; }
            public string Description { get; set; }

            public Definition(Pid pid, Basic basic, Type type, Use usage, Group group, Access access, Persistence persistence, string example, string description)
            {
                Id = pid;
                Basic = basic;
                Type = type;
                Use = usage;
                Group = group;
                Access = access;
                Persistence = persistence;
                Example = example;
                Description = description;
            }
        }

        public static Definition GetDefinition(Pid pid)
        {
            if (Definitions.ContainsKey(pid)) {
                return Definitions[pid];
            }
            throw new NotImplementedException($"Property {pid} not implemented.");
        }

        public static readonly Dictionary<Pid, Definition> Definitions = new Dictionary<Pid, Definition> {
            [Pid.Unknown] = new Definition(Pid.Unknown, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Unknown, "", ""),
            [Pid.FirstOperation] = new Definition(Pid.FirstOperation, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.Item] = new Definition(Pid.Item, Basic.String, Type.Item, Use.Item, Group.Operation, Access.System, Persistence.Unknown, "1", "Passive item of item action."),
            [Pid.PublicAccess] = new Definition(Pid.PublicAccess, Basic.Bool, Type.Bool, Use.Bool, Group.Operation, Access.System, Persistence.Unknown, "", "Dummy property for declaring a PropertyIdList with only this Pid as indicator for GetItemProperties."),
            [Pid.OwnerAccess] = new Definition(Pid.OwnerAccess, Basic.Bool, Type.Bool, Use.Bool, Group.Operation, Access.System, Persistence.Unknown, "", "Dummy property for declaring a PropertyIdList with only this Pid as indicator for GetItemProperties."),
            [Pid.FirstTest] = new Definition(Pid.FirstTest, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.TestInt] = new Definition(Pid.TestInt, Basic.Int, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestInt1] = new Definition(Pid.TestInt1, Basic.Int, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestInt2] = new Definition(Pid.TestInt2, Basic.Int, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestInt3] = new Definition(Pid.TestInt3, Basic.Int, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestString] = new Definition(Pid.TestString, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString1] = new Definition(Pid.TestString1, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString2] = new Definition(Pid.TestString2, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString3] = new Definition(Pid.TestString3, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString4] = new Definition(Pid.TestString4, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString5] = new Definition(Pid.TestString5, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestFloat] = new Definition(Pid.TestFloat, Basic.Float, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat1] = new Definition(Pid.TestFloat1, Basic.Float, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat2] = new Definition(Pid.TestFloat2, Basic.Float, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat3] = new Definition(Pid.TestFloat3, Basic.Float, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat4] = new Definition(Pid.TestFloat4, Basic.Float, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestBool] = new Definition(Pid.TestBool, Basic.Bool, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool1] = new Definition(Pid.TestBool1, Basic.Bool, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool2] = new Definition(Pid.TestBool2, Basic.Bool, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool3] = new Definition(Pid.TestBool3, Basic.Bool, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool4] = new Definition(Pid.TestBool4, Basic.Bool, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestItem] = new Definition(Pid.TestItem, Basic.String, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItem1] = new Definition(Pid.TestItem1, Basic.String, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItem2] = new Definition(Pid.TestItem2, Basic.String, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItem3] = new Definition(Pid.TestItem3, Basic.String, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItemList] = new Definition(Pid.TestItemList, Basic.String, Type.ItemList, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestItemList1] = new Definition(Pid.TestItemList1, Basic.String, Type.ItemList, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestItemList2] = new Definition(Pid.TestItemList2, Basic.String, Type.ItemList, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestItemList3] = new Definition(Pid.TestItemList3, Basic.String, Type.ItemList, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestEnum] = new Definition(Pid.TestEnum, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), ""),
            [Pid.TestEnum1] = new Definition(Pid.TestEnum1, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), ""),
            [Pid.TestEnum2] = new Definition(Pid.TestEnum2, Basic.String, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), ""),
            [Pid.TestPublic] = new Definition(Pid.TestPublic, Basic.Int, Type.Int, Use.Int, Group.Test, Access.Public, Persistence.Persistent, "42", ""),
            [Pid.TestOwner] = new Definition(Pid.TestOwner, Basic.Int, Type.Int, Use.Int, Group.Test, Access.Owner, Persistence.Persistent, "42", ""),
            [Pid.TestInternal] = new Definition(Pid.TestInternal, Basic.Int, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.FirstGeneric] = new Definition(Pid.FirstGeneric, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.Name] = new Definition(Pid.Name, Basic.String, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "Avatar", ""),
            [Pid.TemplateId] = new Definition(Pid.TemplateId, Basic.String, Type.Item, Use.Item, Group.Generic, Access.System, Persistence.Persistent, "WaterBottleTemplate", "Grain Id of the template item."),
            [Pid.Label] = new Definition(Pid.Label, Basic.String, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "WaterBottle", "Used in public displays as primary designation. Will be translated."),
            [Pid.Container] = new Definition(Pid.Container, Basic.String, Type.Item, Use.Item, Group.Generic, Access.Owner, Persistence.Persistent, "10000000001", "Id of container item."),
            [Pid.Contains] = new Definition(Pid.Contains, Basic.String, Type.ItemList, Use.ItemList, Group.Generic, Access.Owner, Persistence.Persistent, "10000000001 10000000002", "Container: list of child items."),
            [Pid.Stacksize] = new Definition(Pid.Stacksize, Basic.Int, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "3", "Number of items item stacked in one place."),
            [Pid.Icon32Url] = new Definition(Pid.Icon32Url, Basic.String, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "Medium images"),
            [Pid.Image100Url] = new Definition(Pid.Image100Url, Basic.String, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", ""),
            [Pid.AnimationsUrl] = new Definition(Pid.AnimationsUrl, Basic.String, Type.String, Use.Url, Group.Generic, Access.Public, Persistence.Persistent, "http://...", ""),
            [Pid.IsRezzing] = new Definition(Pid.IsRezzing, Basic.Bool, Type.Bool, Use.Bool, Group.Generic, Access.System, Persistence.Persistent, "true", "True while rez not confirmed by room."),
            [Pid.IsRezzed] = new Definition(Pid.IsRezzed, Basic.Bool, Type.Bool, Use.Bool, Group.Generic, Access.System, Persistence.Persistent, "true", "True if rezzed to room."),
            [Pid.IsDerezzing] = new Definition(Pid.IsDerezzing, Basic.Bool, Type.Bool, Use.Bool, Group.Generic, Access.System, Persistence.Persistent, "true", "True after derez sent to room."),
            [Pid.RezzedX] = new Definition(Pid.RezzedX, Basic.Int, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "735", "Position of item in room if rezzed."),
            [Pid.FirstAspect] = new Definition(Pid.FirstAspect, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.TestGreetedAspect] = new Definition(Pid.TestGreetedAspect, Basic.Bool, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Test aspect flag"),
            [Pid.TestGreeterAspect] = new Definition(Pid.TestGreeterAspect, Basic.Bool, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Test aspect flag"),
            [Pid.ContainerAspect] = new Definition(Pid.ContainerAspect, Basic.Bool, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Item is a container for other items."),
            [Pid.ItemCapacityLimitAspect] = new Definition(Pid.ItemCapacityLimitAspect, Basic.Bool, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Item is a container with capacity limit"),
            [Pid.RezableAspect] = new Definition(Pid.RezableAspect, Basic.Bool, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "True if item can be rezzed to room."),
            [Pid.IframeAspect] = new Definition(Pid.IframeAspect, Basic.Bool, Type.Bool, Use.Bool, Group.Aspect, Access.Public, Persistence.Fixed, "true", "True if item opens an iframe on click."),
            [Pid.FirstParameter] = new Definition(Pid.FirstParameter, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.TestGreeted_Item] = new Definition(Pid.TestGreeted_Item, Basic.String, Type.Item, Use.Item, Group.Parameter, Access.System, Persistence.Transient, "1", "Greeter item id"),
            [Pid.TestGreeted_Name] = new Definition(Pid.TestGreeted_Name, Basic.String, Type.String, Use.String, Group.Parameter, Access.System, Persistence.Transient, "World", "Greeted name"),
            [Pid.TestGreeter_Result] = new Definition(Pid.TestGreeter_Result, Basic.String, Type.String, Use.String, Group.Parameter, Access.System, Persistence.Persistent, "Hello World", "Greeting result stored by Greeter before return"),
            [Pid.TestGreeted_Result] = new Definition(Pid.TestGreeted_Result, Basic.String, Type.String, Use.String, Group.Parameter, Access.System, Persistence.Persistent, "Hello World", "Greeting result stored by Greeted"),
            [Pid.RezRoom] = new Definition(Pid.RezRoom, Basic.String, Type.Item, Use.Item, Group.Parameter, Access.System, Persistence.Transient, "1", "Room to rez to by Rezable.Rez."),
            [Pid.DerezUser] = new Definition(Pid.DerezUser, Basic.String, Type.Item, Use.Item, Group.Parameter, Access.System, Persistence.Transient, "1", "User to derez to by Rezable.Derez."),
            [Pid.FirstApp] = new Definition(Pid.FirstApp, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.TestGreeterPrefix] = new Definition(Pid.TestGreeterPrefix, Basic.String, Type.String, Use.String, Group.Parameter, Access.System, Persistence.Persistent, "Hello ", "Greeting prefix"),
            [Pid.ContainerItemLimit] = new Definition(Pid.ContainerItemLimit, Basic.Int, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "3", "Number of items in the container counting stacksize."),
            [Pid.IframeUrl] = new Definition(Pid.IframeUrl, Basic.String, Type.String, Use.Url, Group.App, Access.Public, Persistence.Persistent, "IFrameUrl", "TODO."),
            [Pid.IframeWidth] = new Definition(Pid.IframeWidth, Basic.Int, Type.Int, Use.String, Group.App, Access.Public, Persistence.Persistent, "IFrameWidth", "TODO."),
            [Pid.IframeHeight] = new Definition(Pid.IframeHeight, Basic.Int, Type.Int, Use.Int, Group.App, Access.Public, Persistence.Persistent, "IFrameHeight", "TODO."),
            [Pid.IframeResizeable] = new Definition(Pid.IframeResizeable, Basic.Bool, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Persistent, "IFrameResizable", "TODO."),
            [Pid.FirstUserAttribute] = new Definition(Pid.FirstUserAttribute, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.LastProperty] = new Definition(Pid.LastProperty, Basic.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),

        };
    }
}