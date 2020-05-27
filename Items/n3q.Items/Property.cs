using System;
using System.Collections.Generic;

namespace n3q.Items
{
    public static class Property
    {
        public enum Type
        {
            Unknown = 0,
            Int,
            String,
            Float,
            Bool,
            Item,
            ItemSet,
        }

        public enum Access
        {
            Unknown = 0,
            System = 1, // System sees all
            Owner = 2, // Owner sees all props except the internal props
            Public = 3, // Everyone else sees only the public props
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
            Level,
            App,
            Attribute, // User attributes
            //Competition,
        }

        public class Definition
        {
            public Definition(Pid pid, Type type, Use usage, Group group, Access access, Persistence persistence, string example, string description)
            {
                Id = pid;
                Type = type;
                Use = usage;
                Group = group;
                Access = access;
                Persistence = persistence;
                Example = example;
                Description = description;
            }

            public Pid Id { get; set; }
            public Type Type { get; set; }
            public Use Use { get; set; }
            public Group Group { get; set; }
            public Access Access { get; set; }
            public Persistence Persistence { get; set; }
            public string Example { get; set; }
            public string Description { get; set; }
        }

        public static Definition GetDefinition(Pid pid)
        {
            if (Definitions.ContainsKey(pid)) {
                return Definitions[pid];
            }
            throw new NotImplementedException($"Property {pid} not implemented.");
        }

        public static readonly Dictionary<Pid, Definition> Definitions = new Dictionary<Pid, Definition> {
            [Pid.Unknown] = new Definition(Pid.Unknown, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Unknown, "", ""),
            [Pid.FirstOperation] = new Definition(Pid.FirstOperation, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.Item] = new Definition(Pid.Item, Type.Item, Use.Item, Group.Operation, Access.System, Persistence.Unknown, "1", "Passive item of item action."),
            [Pid.PublicAccess] = new Definition(Pid.PublicAccess, Type.Bool, Use.Bool, Group.Operation, Access.System, Persistence.Unknown, "", "Dummy property for declaring a PropertyIdList with only this Pid as indicator for GetItemProperties."),
            [Pid.OwnerAccess] = new Definition(Pid.OwnerAccess, Type.Bool, Use.Bool, Group.Operation, Access.System, Persistence.Unknown, "", "Dummy property for declaring a PropertyIdList with only this Pid as indicator for GetItemProperties."),
            [Pid.FirstTest] = new Definition(Pid.FirstTest, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.TestInt] = new Definition(Pid.TestInt, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestInt1] = new Definition(Pid.TestInt1, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestInt2] = new Definition(Pid.TestInt2, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestInt3] = new Definition(Pid.TestInt3, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.TestString] = new Definition(Pid.TestString, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString1] = new Definition(Pid.TestString1, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString2] = new Definition(Pid.TestString2, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString3] = new Definition(Pid.TestString3, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString4] = new Definition(Pid.TestString4, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestString5] = new Definition(Pid.TestString5, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, "fourtytwo", ""),
            [Pid.TestFloat] = new Definition(Pid.TestFloat, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat1] = new Definition(Pid.TestFloat1, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat2] = new Definition(Pid.TestFloat2, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat3] = new Definition(Pid.TestFloat3, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestFloat4] = new Definition(Pid.TestFloat4, Type.Float, Use.Float, Group.Test, Access.System, Persistence.Persistent, "3.141592653589793238462643383279502", ""),
            [Pid.TestBool] = new Definition(Pid.TestBool, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool1] = new Definition(Pid.TestBool1, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool2] = new Definition(Pid.TestBool2, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool3] = new Definition(Pid.TestBool3, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestBool4] = new Definition(Pid.TestBool4, Type.Bool, Use.Bool, Group.Test, Access.System, Persistence.Persistent, "true", ""),
            [Pid.TestItem] = new Definition(Pid.TestItem, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItem1] = new Definition(Pid.TestItem1, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItem2] = new Definition(Pid.TestItem2, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItem3] = new Definition(Pid.TestItem3, Type.Item, Use.Item, Group.Test, Access.System, Persistence.Persistent, "10000000001", ""),
            [Pid.TestItemSet] = new Definition(Pid.TestItemSet, Type.ItemSet, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestItemSet1] = new Definition(Pid.TestItemSet1, Type.ItemSet, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestItemSet2] = new Definition(Pid.TestItemSet2, Type.ItemSet, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestItemSet3] = new Definition(Pid.TestItemSet3, Type.ItemSet, Use.ItemList, Group.Test, Access.System, Persistence.Persistent, "10000000001 10000000002", ""),
            [Pid.TestEnum] = new Definition(Pid.TestEnum, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), ""),
            [Pid.TestEnum1] = new Definition(Pid.TestEnum1, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), ""),
            [Pid.TestEnum2] = new Definition(Pid.TestEnum2, Type.String, Use.String, Group.Test, Access.System, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), ""),
            [Pid.TestPublic] = new Definition(Pid.TestPublic, Type.Int, Use.Int, Group.Test, Access.Public, Persistence.Persistent, "42", ""),
            [Pid.TestOwner] = new Definition(Pid.TestOwner, Type.Int, Use.Int, Group.Test, Access.Owner, Persistence.Persistent, "42", ""),
            [Pid.TestInternal] = new Definition(Pid.TestInternal, Type.Int, Use.Int, Group.Test, Access.System, Persistence.Persistent, "42", ""),
            [Pid.FirstGeneric] = new Definition(Pid.FirstGeneric, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            //[Pid.Id] = new Definition(Pid.Id, Type.Item, Use.Item, Group.Generic, Access.Public, Persistence.Persistent, "10000000001", ""),
            [Pid.Name] = new Definition(Pid.Name, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "Avatar", ""),
            [Pid.TemplateId] = new Definition(Pid.TemplateId, Type.String, Use.String, Group.Generic, Access.System, Persistence.Persistent, "WaterBottleTemplate", "Grain Id of the template item."),
            [Pid.Label] = new Definition(Pid.Label, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "WaterBottle", "Used in public displays as primary designation. Will be translated."),
            [Pid.Container] = new Definition(Pid.Container, Type.Item, Use.Item, Group.Generic, Access.Owner, Persistence.Persistent, "10000000001", "Id of container item."),
            [Pid.Contains] = new Definition(Pid.Contains, Type.ItemSet, Use.ItemList, Group.Generic, Access.Owner, Persistence.Persistent, "10000000001 10000000002", "Container: list of child items."),
            [Pid.Stacksize] = new Definition(Pid.Stacksize, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "3", "Number of items item stacked in one place."),
            [Pid.Icon32Url] = new Definition(Pid.Icon32Url, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "Medium images"),
            [Pid.Image100Url] = new Definition(Pid.Image100Url, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", ""),
            [Pid.AnimationsUrl] = new Definition(Pid.AnimationsUrl, Type.String, Use.Url, Group.Generic, Access.Public, Persistence.Persistent, "http://...", ""),
            [Pid.IsRezzing] = new Definition(Pid.IsRezzing, Type.Bool, Use.Bool, Group.Generic, Access.System, Persistence.Persistent, "true", "True while rez not confirmed by room."),
            [Pid.IsRezzed] = new Definition(Pid.IsRezzed, Type.Bool, Use.Bool, Group.Generic, Access.System, Persistence.Persistent, "true", "True if rezzed to room."),
            [Pid.IsDerezzing] = new Definition(Pid.IsDerezzing, Type.Bool, Use.Bool, Group.Generic, Access.System, Persistence.Persistent, "true", "True after derez sent to room."),
            [Pid.RezzedX] = new Definition(Pid.RezzedX, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "735", "Position of item in room if rezzed."),
            [Pid.FirstAspect] = new Definition(Pid.FirstAspect, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.TestGreetUserAspect] = new Definition(Pid.TestGreetUserAspect, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Test aspect flag"),
            [Pid.TestGreeterAspect] = new Definition(Pid.TestGreeterAspect, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Test aspect flag"),
            [Pid.ContainerAspect] = new Definition(Pid.ContainerAspect, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Item is a container for other items."),
            [Pid.ItemCapacityLimitAspect] = new Definition(Pid.ItemCapacityLimitAspect, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "Item is a container with capacity limit" ),
            [Pid.RezableAspect] = new Definition(Pid.RezableAspect, Type.Bool, Use.Bool, Group.Aspect, Access.System, Persistence.Fixed, "true", "True if item can be rezzed to room."),
            [Pid.IframeAspect] = new Definition(Pid.IframeAspect, Type.Bool, Use.Bool, Group.Aspect, Access.Public, Persistence.Fixed, "true", "True if item opens an iframe on click."),
            [Pid.FirstApp] = new Definition(Pid.FirstApp, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.ContainerItemLimit] = new Definition(Pid.ContainerItemLimit, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "3", "Number of items in the container counting stacksize."),
            [Pid.IframeUrl] = new Definition(Pid.IframeUrl, Type.String, Use.Url, Group.App, Access.Public, Persistence.Fixed, "IFrameUrl", "TODO."),
            [Pid.IframeWidth] = new Definition(Pid.IframeWidth, Type.Int, Use.String, Group.App, Access.Public, Persistence.Fixed, "IFrameWidth", "TODO."),
            [Pid.IframeHeight] = new Definition(Pid.IframeHeight, Type.Int, Use.Int, Group.App, Access.Public, Persistence.Fixed, "IFrameHeight", "TODO."),
            [Pid.IframeResizeable] = new Definition(Pid.IframeResizeable, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "IFrameResizable", "TODO."),
            [Pid.FirstAttribute] = new Definition(Pid.FirstAttribute, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),
            [Pid.LastProperty] = new Definition(Pid.LastProperty, Type.Unknown, Use.Unknown, Group.Unknown, Access.System, Persistence.Fixed, "", ""),

        };

    }
}