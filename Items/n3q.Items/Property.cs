using System;
using System.Collections.Generic;
using n3q.Tools;

namespace n3q.Items
{
    public static class Property
    {
        public enum Storage
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
            StringList,
            StringStringMap,
        }

        public enum Use
        {
            Unknown = 0,
            Bool,
            Int,
            Float,
            Percent,
            Aspect,
            //Skill,
            //Time, // as Long YYYYMMDDhhmmssiii
            //Delay, // in float sec
            String,
            StringList,
            //Json,
            KeyValueList,
            ImageUrl,
            Url,
            UserId,
            //Text,
            //Translated,
            Item,
            ItemList,
            EnumList,
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
            System,
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
            public Storage Basic { get; set; }
            public Type Type { get; set; }
            public Use Use { get; set; }
            public Group Group { get; set; }
            public Access Access { get; set; }
            public Persistence Persistence { get; set; }
            public string Example { get; set; }
            public string Description { get; set; }

            public Definition(Storage basic, Type type, Use usage, Group group, Access access, Persistence persistence, string example, string description)
            {
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
            throw new NotImplementedException($"Property {pid} unknown.");
        }

        public static PropertyValue DefaultValue(Pid pid)
        {
            return pid switch
            {
                Pid.TestIntDefault => 42L,
                Pid.TestStringDefault => "42",
                Pid.TestFloatDefault => 3.14D,
                Pid.TestBoolDefault => true,

                Pid.DeletableAspect => true,
                Pid.Stacksize => 1,

                _ => PropertyValue.Empty,
            };
        }

        public static bool HasDefaultValue(Pid pid)
        {
            return DefaultValue(pid) != PropertyValue.Empty;
        }

        public static bool IsEmpty(Pid key, PropertyValue value)
        {
            return !Has.Value((string)value);
        }

        public static class Value
        {
            public enum TestEnum
            {
                Unknown,
                Value1,
                Value2,
            }

            public enum UserRoles
            {
                Public,
                User,
                PowerUser,
                Moderator,
                LeadModerator,
                Janitor,
                LeadJanitor,
                Content,
                LeadContent,
                Developer,
                Admin,
                CodeReview,
                SecurityAdmin
            }
        }

#pragma warning disable format
    public static readonly Dictionary<Pid, Definition> Definitions = new Dictionary<Pid, Definition> {
        [Pid.Unknown                          ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Unknown    , "", ""),
        [Pid.FirstSystem                      ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "", ""),
        [Pid.Item                             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.System    , Access.System  , Persistence.Unknown    , "1", "Passive item of item action."),
        [Pid.MetaPublicAccess                 ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.System    , Access.System  , Persistence.Unknown    , "", "Meta property for declaring a PidSet with only this Pid as indicator for GetItemProperties for all public properties."),
        [Pid.MetaOwnerAccess                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.System    , Access.System  , Persistence.Unknown    , "", "Meta property for declaring a PidSet with only this Pid as indicator for GetItemProperties for all owner properties."),
        [Pid.MetaAspectGroup                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.System    , Access.System  , Persistence.Unknown    , "", "Meta property for declaring a PidSet with only this Pid as indicator for GetItemProperties for all aspect properties."),
        [Pid.XmppRoomList                     ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.System    , Access.System  , Persistence.Persistent , "", "Room ids managed by XMPP component. Part of the component's persistent state."),
        [Pid.FirstTest                        ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "", ""),
        [Pid.TestInt                          ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestInt1                         ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestInt2                         ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestInt3                         ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestIntDefault                   ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "1", "Evaluates to 1 if not set"),
        [Pid.TestString                       ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString1                      ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString2                      ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString3                      ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString4                      ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString5                      ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestStringDefault                ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "42", "Evaluates to '42' if not set"),
        [Pid.TestFloat                        ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat1                       ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat2                       ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat3                       ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat4                       ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloatDefault                 ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", "Evaluates to 3.14 if not set"),
        [Pid.TestBool                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool1                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool2                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool3                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool4                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBoolDefault                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", "Evaluates to true if not set"),
        [Pid.TestItem                         ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItem1                        ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItem2                        ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItem3                        ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItemList                     ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestItemList1                    ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestItemList2                    ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestItemList3                    ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestEnum                         ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent ,                                                   Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestEnum1                        ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent ,                                                  Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestEnum2                        ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent ,                                                  Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestPublic                       ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.Public  , Persistence.Persistent , "42", ""),
        [Pid.TestOwner                        ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.Owner   , Persistence.Persistent , "42", ""),
        [Pid.TestInternal                     ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.FirstGeneric                     ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "------------------------------------------------------", ""),
        [Pid.Name                             ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Generic   , Access.Public  , Persistence.Persistent , "Avatar", ""),
        [Pid.Template                         ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.System  , Persistence.Persistent , "WaterBottleTemplate", "Grain Id of the template item."),
        [Pid.Label                            ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Generic   , Access.Public  , Persistence.Persistent , "WaterBottle", "Used in public displays as primary designation. Will be translated."),
        [Pid.Container                        ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.Owner   , Persistence.Persistent , "10000000001", "Id of container item."),
        [Pid.Contains                         ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Generic   , Access.Owner   , Persistence.Persistent , "10000000001 10000000002", "Container: list of child items."),
        [Pid.Stacksize                        ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , "3", "Number of items item stacked in one place."),
        [Pid.Icon32Url                        ] = new Definition(Storage.String  , Type.String          , Use.ImageUrl     , Group.Generic   , Access.Public  , Persistence.Persistent , "http://...", "Medium images"),
        [Pid.Image100Url                      ] = new Definition(Storage.String  , Type.String          , Use.ImageUrl     , Group.Generic   , Access.Public  , Persistence.Persistent , "http://...", ""),
        [Pid.AnimationsUrl                    ] = new Definition(Storage.String  , Type.String          , Use.Url          , Group.Generic   , Access.Public  , Persistence.Persistent , "http://...", ""),
        [Pid.Actions                          ] = new Definition(Storage.String  , Type.StringStringMap , Use.KeyValueList , Group.Generic   , Access.Public  , Persistence.Persistent , "MakeCoffee=Produce GetCoffee=EjectItem", "Maps external (app specific) actions to internal (Aspect) actions/methods."),
        [Pid.Stats                            ] = new Definition(Storage.String  , Type.StringList      , Use.StringList   , Group.Generic   , Access.Public  , Persistence.Persistent , "WaterLevel CoffeeLevel", "List of stats visible on rezzed item."),
        [Pid.RezableIsRezzing                 ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , "true", "True while rez not confirmed by room."),
        [Pid.RezableIsRezzed                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , "true", "True if rezzed to room."),
        [Pid.RezableIsDerezzing               ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , "true", "True after derez sent to room."),
        [Pid.RezzedX                          ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , "735", "Position of item in room if rezzed."),
        [Pid.FirstAspect                      ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "------------------------------------------------------", ""),
        [Pid.TestGreetedAspect                ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item has Greeted test aspect"),
        [Pid.TestGreeterAspect                ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item has Greeter test aspect"),
        [Pid.DeletableAspect                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item can be deleted,which is the case for most items,hence default true by Property.Default."),
        [Pid.ContainerAspect                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item is a container for other items."),
        [Pid.ItemCapacityLimitAspect          ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item is a container with capacity limit"),
        [Pid.InventoryAspect                ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item can have x/y coordinates in container"),
        [Pid.RezableAspect                    ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item can be rezzed to room."),
        [Pid.IframeAspect                     ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item opens an iframe on click."),
        [Pid.PageClaimAspect                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item claims room ownership."),
        [Pid.RezzableProxyAspect              ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item is a proxy for a rezzed item."),
        [Pid.RoleAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item contains account roles."),
        [Pid.SourceAspect                     ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "IsSource provides a resource (a property which can be extracted by an Extractor)."),
        [Pid.SinkAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "IsSink takes a resource (a property which can be filled by a Supplier)."),
        [Pid.ExtractorAspect                  ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item extracts a resource from IsSource items."),
        [Pid.InjectorAspect                   ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item injects a resource into IsSink items."),
        [Pid.ApplierAspect                    ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item will select appropriate action to be applied to a passive item."),
        [Pid.FirstParameter                   ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Persistent , "------------------------------------------------------", ""),
        [Pid.TestGreetedGetGreetingGreeter    ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , "1", "Greeter item id"),
        [Pid.TestGreetedGetGreetingName       ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Parameter , Access.System  , Persistence.Transient  , "World", "Greeted name"),
        [Pid.RezableRezTo                     ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , "room@chat.server.org", "Room to rez to by Rezable.Rez."),
        [Pid.RezableRezX                      ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , "100", "X position in room to rez to."),
        [Pid.RezableRezDestination            ] = new Definition(Storage.String  , Type.String          , Use.Url          , Group.Parameter , Access.System  , Persistence.Transient  , "http://...", "Url of the room to rez to."),
        [Pid.RezableDerezTo                   ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , "{8AC310B6-4903-4BE9-AE8C-F329CABA9468}", "User repository to derez to."),
        [Pid.RezableDerezX                    ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , "100", "X position in repository to derez to."),
        [Pid.RezableDerezY                    ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , "100", "Y position in repository to derez to."),
        [Pid.InventorySetCoordinateItem       ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , "ItemId", "Item in inventory to be positioned."),
        [Pid.InventorySetCoordinateX          ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , "100", "X position in repository."),
        [Pid.InventorySetCoordinateY          ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , "100", "Y position in repository."),
        [Pid.InjectorInjectTo                 ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , "ItemId", "Passive item of item action."),
        [Pid.ExtractorExtractFrom             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , "ItemId", "Passive item of item action."),
        [Pid.ApplierApplyTo                   ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , "ItemId", "Passive item of item action."),        [Pid.FirstApp                       ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "------------------------------------------------------", ""),
        [Pid.TestGreeterPrefix                ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Persistent , "Hello ", "Greeting prefix"),
        [Pid.TestGreeterResult                ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Transient  , "Hello World", "Greeting result stored by Greeter before return"),
        [Pid.TestGreetedResult                ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Transient  , "Hello World", "Greeting result stored by Greeted"),
        [Pid.ContainerItemLimit               ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , "3", "Number of items in the container counting stacksize."),
        [Pid.InventoryX                       ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , "100", "X position in inventory."),
        [Pid.InventoryY                       ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , "100", "Y position in inventory."),
        [Pid.IframeUrl                        ] = new Definition(Storage.String  , Type.String          , Use.Url          , Group.App       , Access.Public  , Persistence.Persistent , "IFrameUrl", "TODO."),
        [Pid.IframeWidth                      ] = new Definition(Storage.Int     , Type.Int             , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , "IFrameWidth", "TODO."),
        [Pid.IframeHeight                     ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , "IFrameHeight", "TODO."),
        [Pid.IframeResizeable                 ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.App       , Access.Public  , Persistence.Persistent , "IFrameResizable", "TODO."),
        [Pid.RezzableProxyTemplate            ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.App       , Access.System  , Persistence.Persistent , "RezzableProxyTemplate", "Id of proxy item template."),
        [Pid.RoleUserRoles                    ] = new Definition(Storage.String  , Type.StringList      , Use.EnumList     , Group.App       , Access.System  , Persistence.Persistent , $"{Value.UserRoles.Public} {Value.UserRoles.User} {Value.UserRoles.PowerUser}", "List of user roles."),
        [Pid.SourceResource                   ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , "WaterLevel", "The name of the resource provided by the source."),
        [Pid.SinkResource                     ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , "WaterLevel", "The name of the resource provided by the source."),
        [Pid.WaterLevel                       ] = new Definition(Storage.Float   , Type.Float           , Use.Ccm          , Group.App       , Access.Public  , Persistence.Persistent , "100", "Water level."),
        [Pid.WaterLevelMax                    ] = new Definition(Storage.Float   , Type.Float           , Use.Ccm          , Group.App       , Access.Public  , Persistence.Persistent , "300", "Water level max."),
        [Pid.FirstUserAttribute               ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Persistent , "", ""),
        [Pid.LastProperty                     ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Persistent , "", ""),
#pragma warning restore format

        };
    }
}
