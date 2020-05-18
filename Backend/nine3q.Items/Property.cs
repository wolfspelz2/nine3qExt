using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using nine3q.Tools;

namespace nine3q.Items
{
    public static class PropertyName
    {
        public enum Modifier
        {
            Max,
            Remaining,
        }
    }

    public static class Property
    {
        static readonly object _mutex = new object();
        private static Dictionary<Pid, Definition> _definitions = null;
        public static Dictionary<Pid, Definition> Definitions
        {
            private set { }
            get {
                lock (_mutex) {
                    if (_definitions == null) {
                        _definitions = new Dictionary<Pid, Definition>();
                        InitDefinitions();
                    }
                }
                return _definitions;
            }
        }
        public static void Add(Pid pid, Type type, Use use, Group group, Access access, Persistence persistence, string example, string description)
        {
            var def = new Definition {
                Id = pid,
                Name = pid.ToString(),
                Type = type,
                Use = use,
                Group = group,
                Access = access,
                Persistence = persistence,
                Example = example,
                Description = description,
            };
            _definitions.Add(pid, def);
        }
        public static void InitDefinitions()
        {
            Add(Pid.NoProperty, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Unknown, "", "");

            Add(Pid.FirstOperation, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            Add(Pid.Item, Type.Item, Use.Item, Group.Operation, Access.Internal, Persistence.Unknown, "1", "Passive item of item action.");
            //Add(Pid.StaleTemplate, Type.Bool, Use.Bool, Group.Operation, Access.Internal, Persistence.Unknown, "false", "Tells if this is a cached template, which is missing its original in the templates inventory.");
            Add(Pid.PublicAccess, Type.Bool, Use.Bool, Group.Operation, Access.Internal, Persistence.Unknown, "", "Dummy property for declaring a PropertyIdList with only this Pid as indicator for GetItemProperties.");
            Add(Pid.OwnerAccess, Type.Bool, Use.Bool, Group.Operation, Access.Internal, Persistence.Unknown, "", "Dummy property for declaring a PropertyIdList with only this Pid as indicator for GetItemProperties.");
            Add(Pid.TransferState, Type.String, Use.String, Group.Operation, Access.Internal, Persistence.Transient, PropertyValue.TransferState.Source.ToString(), "Indicates, that item is in transfer, either at source (Begin) oder at dest (Received).");
            Add(Pid.TransferContainer, Type.Item, Use.Item, Group.Operation, Access.Internal, Persistence.Transient, "1", "Container of item before removed from container for transfer to other inventory.");
            Add(Pid.TransferSlot, Type.Int, Use.Int, Group.Operation, Access.Internal, Persistence.Transient, "1", "Slot of item before removed from container for transfer to other inventory.");

            Add(Pid.FirstTest, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            Add(Pid.TestInt, Type.Int, Use.Int, Group.Test, Access.Internal, Persistence.Persistent, "42", "");
            Add(Pid.TestInt1, Type.Int, Use.Int, Group.Test, Access.Internal, Persistence.Persistent, "42", "");
            Add(Pid.TestInt2, Type.Int, Use.Int, Group.Test, Access.Internal, Persistence.Persistent, "42", "");
            Add(Pid.TestInt3, Type.Int, Use.Int, Group.Test, Access.Internal, Persistence.Persistent, "42", "");
            Add(Pid.TestString, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, "fourtytwo", "");
            Add(Pid.TestString1, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, "fourtytwo", "");
            Add(Pid.TestString2, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, "fourtytwo", "");
            Add(Pid.TestString3, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, "fourtytwo", "");
            Add(Pid.TestFloat, Type.Float, Use.Float, Group.Test, Access.Internal, Persistence.Persistent, "3.141592653589793238462643383279502", "");
            Add(Pid.TestFloat1, Type.Float, Use.Float, Group.Test, Access.Internal, Persistence.Persistent, "3.141592653589793238462643383279502", "");
            Add(Pid.TestFloat2, Type.Float, Use.Float, Group.Test, Access.Internal, Persistence.Persistent, "3.141592653589793238462643383279502", "");
            Add(Pid.TestFloat3, Type.Float, Use.Float, Group.Test, Access.Internal, Persistence.Persistent, "3.141592653589793238462643383279502", "");
            Add(Pid.TestFloat4, Type.Float, Use.Float, Group.Test, Access.Internal, Persistence.Persistent, "3.141592653589793238462643383279502", "");
            Add(Pid.TestBool, Type.Bool, Use.Bool, Group.Test, Access.Internal, Persistence.Persistent, "true", "");
            Add(Pid.TestBool1, Type.Bool, Use.Bool, Group.Test, Access.Internal, Persistence.Persistent, "true", "");
            Add(Pid.TestBool2, Type.Bool, Use.Bool, Group.Test, Access.Internal, Persistence.Persistent, "true", "");
            Add(Pid.TestBool3, Type.Bool, Use.Bool, Group.Test, Access.Internal, Persistence.Persistent, "true", "");
            Add(Pid.TestBool4, Type.Bool, Use.Bool, Group.Test, Access.Internal, Persistence.Persistent, "true", "");
            Add(Pid.TestItem, Type.Item, Use.Item, Group.Test, Access.Internal, Persistence.Persistent, "10000000001", "");
            Add(Pid.TestItem1, Type.Item, Use.Item, Group.Test, Access.Internal, Persistence.Persistent, "10000000001", "");
            Add(Pid.TestItem2, Type.Item, Use.Item, Group.Test, Access.Internal, Persistence.Persistent, "10000000001", "");
            Add(Pid.TestItem3, Type.Item, Use.Item, Group.Test, Access.Internal, Persistence.Persistent, "10000000001", "");
            Add(Pid.TestItemSet, Type.ItemSet, Use.ItemList, Group.Test, Access.Internal, Persistence.Persistent, "10000000001 10000000002", "");
            Add(Pid.TestItemSet1, Type.ItemSet, Use.ItemList, Group.Test, Access.Internal, Persistence.Persistent, "10000000001 10000000002", "");
            Add(Pid.TestItemSet2, Type.ItemSet, Use.ItemList, Group.Test, Access.Internal, Persistence.Persistent, "10000000001 10000000002", "");
            Add(Pid.TestItemSet3, Type.ItemSet, Use.ItemList, Group.Test, Access.Internal, Persistence.Persistent, "10000000001 10000000002", "");
            Add(Pid.TestEnum, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), "");
            Add(Pid.TestEnum1, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), "");
            Add(Pid.TestEnum2, Type.String, Use.String, Group.Test, Access.Internal, Persistence.Persistent, PropertyValue.TestEnum.Value1.ToString(), "");
            Add(Pid.TestPublic, Type.Int, Use.Int, Group.Test, Access.Public, Persistence.Persistent, "42", "");
            Add(Pid.TestOwner, Type.Int, Use.Int, Group.Test, Access.Owner, Persistence.Persistent, "42", "");
            Add(Pid.TestInternal, Type.Int, Use.Int, Group.Test, Access.Internal, Persistence.Persistent, "42", "");

            Add(Pid.FirstGeneric, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            Add(Pid.Id, Type.Item, Use.Item, Group.Generic, Access.Public, Persistence.Persistent, "10000000001", "");
            Add(Pid.Name, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "Avatar", "");
            Add(Pid.TemplateName, Type.String, Use.String, Group.Generic, Access.Internal, Persistence.Persistent, "WaterBottleTemplate", "Template provides additional properties.");
            //Add(Pid.Label, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "WaterBottle", "Used in public displays as primary designation. Will be translated.");
            Add(Pid.Owner, Type.String, Use.UserId, Group.Generic, Access.Public, Persistence.Persistent, "9c0a5e6d-1278-4e14-8981-e3909d9e7a4b", "User Id of the item owner (when rezzed in room).");
            Add(Pid.Container, Type.Item, Use.Item, Group.Generic, Access.Owner, Persistence.Persistent, "10000000001", "Id of container item.");
            Add(Pid.Contains, Type.ItemSet, Use.ItemList, Group.Generic, Access.Owner, Persistence.Persistent, "10000000001 10000000002", "Container: list of child items.");
            Add(Pid.Slots, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "3", "Number of solts in container.");
            Add(Pid.Slot, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "0", "Slot of item in slotted container.");
            //Add(Pid.GridWidth, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "5", "Width of repository container.");
            Add(Pid.Stacksize, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "3", "Number of items item stacked in one place.");
            //Add(Pid.ImageUrl, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "");
            //Add(Pid.Icon16Url, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "Small representations");
            //Add(Pid.Icon32Url, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "Medium images");
            //Add(Pid.AvatarUrl, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "Live display, minimum transparent area");
            Add(Pid.Image100Url, Type.String, Use.ImageUrl, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "");
            Add(Pid.AnimationsUrl, Type.String, Use.Url, Group.Generic, Access.Public, Persistence.Persistent, "http://...", "");
            //Add(Pid.AnimationsMime, Type.String, Use.String, Group.Generic, Access.Public, Persistence.Persistent, "text/xml", "");
            Add(Pid.Actions, Type.String, Use.Json, Group.Generic, Access.Public, Persistence.Fixed, "{MakeCoffee:'Produce',GetCoffee:'EjectItem'}", "Maps external (app specific) actions to internal (Aspect) actions/methods.");
            //Add(Pid.PassiveActions, Type.String, Use.Json, Group.Generic, Access.Public, Persistence.Fixed, "['PutWater']", $"Supports these actions as passive item. You may 'PutWater' into a {Pid.IsSink}=true/{Pid.Resource}={Pid.WaterLevel} item");
            Add(Pid.Room, Type.String, Use.String, Group.Generic, Access.Internal, Persistence.Persistent, "d954c536629c2d729c65630963af57c119e24836@muc4.virtual-presence.org", "Room Address.");
            Add(Pid.IsRezzing, Type.Bool, Use.Int, Group.Generic, Access.Internal, Persistence.Persistent, "true", "True while in the process of being rezzed.");
            Add(Pid.IsRezzed, Type.Bool, Use.Bool, Group.Generic, Access.Internal, Persistence.Persistent, "true", "True if rezzed to room.");
            Add(Pid.RezzedX, Type.Int, Use.Int, Group.Generic, Access.Public, Persistence.Persistent, "735", "Position of item in room if rezzed.");
            //Add(Pid.IsClaim, Type.Bool, Use.Bool, Group.Generic, Access.Public, Persistence.Fixed, "true", "Item claims room ownership.");
            //Add(Pid.Claimed, Type.Bool, Use.Bool, Group.Generic, Access.Public, Persistence.Persistent, "true", "True if IsClaim-item rezzed to room.");
            //Add(Pid.IsProxy, Type.Bool, Use.Bool, Group.Generic, Access.Owner, Persistence.Fixed, "true", "Item is a proxy for a rezzed item.");
            //Add(Pid.ProxyTemplate, Type.String, Use.String, Group.Generic, Access.Owner, Persistence.Persistent, "PageProxyTemplate", "Name of proxy item template.");
            //Add(Pid.ProxyName, Type.String, Use.String, Group.Generic, Access.Owner, Persistence.Persistent, "83726273-2344-8765-1234-936591538251", "Unique name of the rezzed item / proxy item relationship. Used by derez to find associated proxy.");
            //Add(Pid.ProxyDestination, Type.String, Use.Url, Group.Generic, Access.Owner, Persistence.Persistent, "http://www.heise.de", "The page URL where the proxied item was rezzed.");
            //Add(Pid.ProxyInventory, Type.String, Use.String, Group.Generic, Access.Owner, Persistence.Persistent, "xmpp:0caaf24ab1a0c33440c06afe99df986365b0781f@muc4.virtual-presence.org", "Room ID where the proxied item was rezzed.");
            //Add(Pid.IsRepository, Type.Bool, Use.Bool, Group.Generic, Access.Owner, Persistence.Fixed, "true", "Item is a user repository, a visible 'bag/grid' in the user inventory.");
            //Add(Pid.IsSettings, Type.Bool, Use.Bool, Group.Generic, Access.Public, Persistence.Fixed, "true", "Item contains user settings.");
            //Add(Pid.IsAvatar, Type.Bool, Use.Bool, Group.Generic, Access.Public, Persistence.Fixed, "true", "Item contains avatar data.");
            //Add(Pid.IsNickname, Type.Bool, Use.Bool, Group.Generic, Access.Public, Persistence.Fixed, "true", "Item contains a nickname for the avatar.");
            //Add(Pid.IsRole, Type.Bool, Use.Bool, Group.Generic, Access.Public, Persistence.Fixed, "true", "Item contains account roles.");
            //Add(Pid.Roles, Type.String, Use.Json, Group.Generic, Access.Public, Persistence.Persistent, "['Public', 'Admin']", "List of user roles.");
            //Add(Pid.Stats, Type.String, Use.Json, Group.Generic, Access.Public, Persistence.Persistent, "['WaterLevel']", "List of stats visible on rezzed item.");
            //Add(Pid.Condition, Type.Float, Use.Percent, Group.Generic, Access.Public, Persistence.Persistent, "0.5", "Item condition between 0.0 and 1.0.");

            Add(Pid.FirstAspect, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            Add(Pid.IsTest1, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "For unit testing of aspects.");
            Add(Pid.IsTest2, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "For unit testing of aspects.");
            Add(Pid.IsContainer, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "Item is a container for other items.");
            Add(Pid.RezableAspect, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "True if item can be rezzed to room.");
            //Add(Pid.IsTrashCan, Type.Bool, Use.Bool, Group.Aspect, Access.Public, Persistence.Fixed, "true", "Item is a trash repository, can be emptied.");
            //Add(Pid.IsSource, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "IsSource provides a resource (a property which can be extracted by an Extractor).");
            //Add(Pid.IsSink, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "IsSink takes a resource (a property which can be filled by a Supplier).");
            //Add(Pid.IsExtractor, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "Item extracts a resource from IsSource items.");
            //Add(Pid.IsInjector, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "Item injects a resource into IsSink items.");
            //Add(Pid.IsDeletee, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "Item is subject to timed deletion.");
            //Add(Pid.IsApplier, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "Item will select appropriate action to be applied to a passive item.");
            //Add(Pid.IsCondition, Type.Bool, Use.Bool, Group.Aspect, Access.Internal, Persistence.Fixed, "true", "Item will continually loose a resource to support living, starve, starve to death, and recover depending on the availability of the resource.");

            Add(Pid.FirstLevel, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            //Add(Pid.WaterLevel, Type.Float, Use.Ccm, Group.Level, Access.Public, Persistence.Persistent, "100", "Water level.");
            //Add(Pid.WaterLevelMax, Type.Float, Use.Float, Group.Level, Access.Public, Persistence.Fixed, "300", "Water level max.");
            //Add(Pid.CoffeeLevel, Type.Float, Use.Gram, Group.Level, Access.Public, Persistence.Persistent, "15", "Coffee level.");
            //Add(Pid.CoffeeLevelMax, Type.Float, Use.Float, Group.Level, Access.Public, Persistence.Fixed, "100", "Coffee level max.");
            //Add(Pid.SoilLevel, Type.Float, Use.Gram, Group.Level, Access.Public, Persistence.Persistent, "2", "Amount of Soil, dirt.");

            Add(Pid.FirstApp, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            Add(Pid.ContainerCanExport, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Container can export items via drag/drop.");
            Add(Pid.ContainerCanImport, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Container can import items via drag/drop.");
            //Add(Pid.ContainerCanReplace, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Container can replace items via drag/drop with items from other repository.");
            Add(Pid.ContainerCanShuffle, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Container can move items around inside via drag/drop.");
            //Add(Pid.ContainerCanRez, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Container can rez items to room via drag/drop.");
            //Add(Pid.ContainerCanDerez, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Container can derez items from room.");
            //Add(Pid.Resource, Type.String, Use.String, Group.App, Access.Public, Persistence.Fixed, "WaterLevel", "The name of the resource provided by the source.");
            //Add(Pid.DeleteExtractedResource, Type.Bool, Use.Bool, Group.App, Access.Public, Persistence.Fixed, "true", "Delete item if extractor changed resource level to 0.");
            //Add(Pid.DeleteTime, Type.Float, Use.Delay, Group.App, Access.Internal, Persistence.Fixed, "true", "Delete IsDeletee after seconds.");

            //Add(Pid.ConditionResource, Type.String, Use.String, Group.App, Access.Internal, Persistence.Fixed, "WaterLevel", "The name of the resource which is used up over time.");
            //Add(Pid.ConditionUse, Type.Float, Use.Float, Group.App, Access.Internal, Persistence.Fixed, "1.0", "Amount of used resource.");
            //Add(Pid.ConditionInterval, Type.Float, Use.Float, Group.App, Access.Internal, Persistence.Fixed, "300", "Interval in seconds.");
            //Add(Pid.ConditionTimer, Type.String, Use.String, Group.App, Access.Internal, Persistence.Persistent, "Leaker.Leak", "The name of the timer, if set.");
            //Add(Pid.ConditionRecover, Type.Float, Use.Float, Group.App, Access.Internal, Persistence.Fixed, "0.0.5", "Amount of condition gained.");
            //Add(Pid.ConditionStarve, Type.Float, Use.Float, Group.App, Access.Internal, Persistence.Fixed, "0.1", "Amount of condition lost.");
            //Add(Pid.ConditionDeadTemplate, Type.String, Use.String, Group.App, Access.Internal, Persistence.Fixed, "BioWasteTemplate", "Name of the template for an item which replaces the item.");

            Add(Pid.FirstAttribute, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
            //Add(Pid.Nickname, Type.String, Use.String, Group.Attribute, Access.Public, Persistence.Persistent, "Wolfspelz", "User nickname.");
            //Add(Pid.HideNickname, Type.Bool, Use.Bool, Group.Attribute, Access.Public, Persistence.Fixed, "true", "Hide nickname at user avatar, default: false.");
            //Add(Pid.DefaultNicknamePrefix, Type.String, Use.String, Group.Attribute, Access.Internal, Persistence.Persistent, "zweitgeist-", "Prefix of automatically assigned nickname.");
            //Add(Pid.AvatarSpeed, Type.Int, Use.Int, Group.Attribute, Access.Public, Persistence.Fixed, "75", "Walk speed of user avatar in pixels per second.");
            //Add(Pid.DefaultAvatarBase, Type.String, Use.Url, Group.Attribute, Access.Internal, Persistence.Persistent, "http://avatar.zweitgeist.com/gif", "Base URL for automatically assigned avatar.");
            //Add(Pid.DefaultAvatarImage, Type.String, Use.String, Group.Attribute, Access.Internal, Persistence.Persistent, "idle.gif", "Still image file name for automatically assigned avatar.");
            //Add(Pid.DefaultAvatarAnimation, Type.String, Use.String, Group.Attribute, Access.Internal, Persistence.Persistent, "config.xml", "Animations config xml file name for automatically assigned avatar.");
            //Add(Pid.DefaultAvatarList, Type.String, Use.Json, Group.Attribute, Access.Internal, Persistence.Persistent, "['002/sportive03_f', '002/business03_f']", "List of default avatars for automatically assigned avatar.");
            //Add(Pid.PasswordHash, Type.String, Use.String, Group.Attribute, Access.Internal, Persistence.Persistent, "cYUFrhbMCz8+LUNsdfs3426zIZK63QWgo=", "User's salted and hashed password.");
            //Add(Pid.PasswordSalt, Type.String, Use.String, Group.Attribute, Access.Internal, Persistence.Persistent, "PQkZF8rMo8e7qGCxjlG34frsgsfgE5gA==", "User's password individual salt.");
            //Add(Pid.PasswordAlgorithm, Type.String, Use.String, Group.Attribute, Access.Internal, Persistence.Persistent, PropertyValue.PasswordAlgorithm.SaltedPasswordSha256.ToString(), "User's password algorithm.");
            //Add(Pid.UserCustomized, Type.Bool, Use.Bool, Group.Attribute, Access.Internal, Persistence.Fixed, "true", "Indicates, that user did active nickname/avatar/inventory changes.");

            Add(Pid.LastProperty, Type.Unknown, Use.Unknown, Group.Unknown, Access.Internal, Persistence.Fixed, "", "");
        }

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
            Internal, // Internal sees all
            Owner, // Owner sees all props except the internal props
            Public, // Everyone else sees only the public props
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
            public Pid Id { get; set; }
            public string Name { get; set; }
            public Type Type { get; set; }
            public Use Use { get; set; }
            public Group Group { get; set; }
            public Access Access { get; set; }
            public Persistence Persistence { get; set; }
            public string Example { get; set; }
            public string Description { get; set; }
        }

        public static object Normalize(Property.Type type, object value)
        {
            if (value == null) { return value; }
            switch (type) {
                case Property.Type.Unknown: throw new InvalidOperationException("Property type=" + type.ToString() + " should not never surface.");
                case Property.Type.Int:
                    if (value is long) {
                        return (long)value;
                    } else if (value is int) {
                        return (long)(int)value;
                    } else if (value is Enum) {
                        return (long)(int)value;
                    } else if (value is string) {
                        return Convert.ToInt64((string)value, CultureInfo.InvariantCulture);
                    } else if (value is double) {
                        return (long)(double)value;
                    }
                    break;
                case Property.Type.String:
                    if (value is string) {
                        return (string)value;
                    } else if (value is Enum) {
                        return value.ToString();
                    } else if (value is object) {
                        return value.ToString();
                    }
                    break;
                case Property.Type.Float:
                    if (value is double) {
                        return (double)value;
                    } else if (value is int) {
                        return (double)(int)value;
                    } else if (value is long) {
                        return (double)(long)value;
                    } else if (value is float) {
                        return (double)(float)value;
                    } else if (value is string) {
                        return Convert.ToDouble((string)value, CultureInfo.InvariantCulture);
                    }
                    break;
                case Property.Type.Bool:
                    if (value is bool) {
                        return (bool)value;
                    } else if (value is int) {
                        return ((int)value) > 0;
                    } else if (value is long) {
                        return ((long)value) > 0;
                    } else if (value is string) {
                        return ((string)value).IsTrue();
                    }
                    break;
                case Property.Type.Item:
                    if (value is long) {
                        return value;
                    } else if (value is int) {
                        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
                    } else if (value is string) {
                        return (string)value;
                    }
                    break;
                case Property.Type.ItemSet:
                    if (value is ItemIdSet) {
                        return value as ItemIdSet;
                    } else if (value is string) {
                        return new ItemIdSet((string)value);
                    }
                    break;
                default: throw new NotImplementedException("Property type=" + type.ToString() + " not yet implemented.");
            }
            return Property.FromString(type, Property.ToString(type, value));
        }

        public static string ToString(Property.Type type, object value)
        {
            if (value == null) return "";
            if (value is string) {
                return (string)value;
            } else if (value is int) {
                return value.ToString();
            } else if (value is long) {
                return value.ToString();
            } else if (value is float) {
                return ((float)value).ToString(CultureInfo.InvariantCulture);
            } else if (value is double) {
                return ((double)value).ToString(CultureInfo.InvariantCulture);
            } else if (value is bool) {
                return (bool)value ? "true" : "false";
            } else if (value is Enum) {
                return value.ToString();
            } else if (value is ItemIdSet) {
                return value.ToString();
            }
            return value.ToString();
        }

        public static object FromString(Property.Type type, string s)
        {
            return type switch
            {
                Property.Type.Unknown => throw new InvalidOperationException("Property type=" + type.ToString() + " should not never surface."),
                Property.Type.Int => Convert.ToInt64(s, CultureInfo.InvariantCulture),
                Property.Type.String => s,
                Property.Type.Float => Convert.ToDouble(s, CultureInfo.InvariantCulture),
                Property.Type.Bool => s.IsTrue(),
                Property.Type.Item => s,
                Property.Type.ItemSet => new ItemIdSet(s),
                _ => throw new NotImplementedException("Property type=" + type.ToString() + " not yet implemented."),
            };
        }

        public static object Clone(Property.Type type, object value)
        {
            if (value == null) return null;
            return type switch
            {
                Property.Type.Unknown => throw new InvalidOperationException("Property type=" + type.ToString() + " should not never surface."),
                Property.Type.Int => (long)value,
                Property.Type.String => (string)value,
                Property.Type.Float => (double)value,
                Property.Type.Bool => (bool)value,
                Property.Type.Item => (long)value,
                Property.Type.ItemSet => ((ItemIdSet)value).Clone(),
                _ => throw new NotImplementedException("Property type=" + type.ToString() + " not yet implemented."),
            };
        }

        public static object Default(Property.Type type)
        {
            return type switch
            {
                Property.Type.Unknown => throw new InvalidOperationException("Property type=" + type.ToString() + " should not never surface."),
                Property.Type.Int => 0L,
                Property.Type.String => "",
                Property.Type.Float => 0.0D,
                Property.Type.Bool => false,
                Property.Type.Item => ItemId.NoItem,
                Property.Type.ItemSet => new ItemIdSet(),
                _ => throw new NotImplementedException("Property type=" + type.ToString() + " not yet implemented."),
            };
        }

        public static bool AreEquivalent(Property.Type type, object value1, object value2)
        {
            if (value1 == null && value2 != null || value1 != null && value2 == null) { return false; }
            switch (type) {
                case Property.Type.Unknown: throw new InvalidOperationException("Property type=" + type.ToString() + " should not never surface.");
                case Property.Type.Int: return (long)Normalize(type, value1) == (long)Normalize(type, value2);
                case Property.Type.String: return (string)Normalize(type, value1) == (string)Normalize(type, value2);
                case Property.Type.Float: return (double)Normalize(type, value1) == (double)Normalize(type, value2);
                case Property.Type.Bool: return (bool)Normalize(type, value1) == (bool)Normalize(type, value2);
                case Property.Type.Item: return (long)Normalize(type, value1) == (long)Normalize(type, value2);
                case Property.Type.ItemSet: {
                    var set1 = Normalize(type, value1) as ItemIdSet;
                    var set2 = Normalize(type, value2) as ItemIdSet;
                    if (set1.Count != set1.Count) { return false; }
                    var union = set1.Union(set2);
                    if (union.Count() != set1.Count) { return false; }
                    var intersection = set1.Intersect(set2);
                    if (intersection.Count() != set1.Count) { return false; }
                    return true;
                }
                default: throw new NotImplementedException("Property type=" + type.ToString() + " not yet implemented.");
            }
        }

        static readonly object _nameIndexMutex = new object();
        static Dictionary<string, Definition> NameIndex = null;
        public static Definition Get(string name)
        {
            lock (_nameIndexMutex) {
                if (NameIndex == null) {
                    NameIndex = new Dictionary<string, Definition>();
                    foreach (var pair in Definitions) {
                        NameIndex.Add(pair.Value.Name, pair.Value);
                    }
                }
            }

            if (NameIndex.ContainsKey(name)) {
                return NameIndex[name];
            }

            throw new NotImplementedException("Property name=" + name + " not implemented.");
        }

        public static Definition Get(Pid pid)
        {
            if (Definitions.ContainsKey(pid)) {
                return Definitions[pid];
            }
            throw new NotImplementedException("Property=" + pid.ToString() + " has no definition.");
        }

        //public static Pid GetAssociatedProperty(Pid pid, PropertyName.Modifier suffix)
        //{
        //    string name = Property.Get(pid).Name + suffix.ToString();
        //    var id = Property.Get(name).Id;
        //    if (id == Pid.NoProperty) { throw new Exceptions.MissingAssociatedPropertyException(pid, name); }
        //    return id;
        //}
    }
}
