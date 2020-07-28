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
            Time, // as Long YYYYMMDDhhmmssiii
            Delay, // in float sec
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
            public Storage Storage { get; set; }
            public Type Type { get; set; }
            public Use Use { get; set; }
            public Group Group { get; set; }
            public Access Access { get; set; }
            public Persistence Persistence { get; set; }
            public PropertyValue Default { get; set; }
            public string Example { get; set; }
            public string Description { get; set; }

            public Definition(Storage storage, Type type, Use usage, Group group, Access access, Persistence persistence, PropertyValue defaultValue, string example, string description)
            {
                Storage = storage;
                Type = type;
                Use = usage;
                Group = group;
                Access = access;
                Persistence = persistence;
                Default = defaultValue;
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
            return GetDefinition(pid).Default;
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

            public enum IframeFrame
            {
                Window,
                Popup,
            }
        }

#pragma warning disable format
    public static readonly Dictionary<Pid, Definition> Definitions = new Dictionary<Pid, Definition> {
        [Pid.Unknown                               ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "", ""),
        [Pid.FirstSystem                           ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Fixed      , PropertyValue.Empty , "", ""),
        [Pid.Item                                  ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.System    , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "1", "Passive item of item action."),
        [Pid.MetaPublicAccess                      ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.System    , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "", "Meta property for declaring a PidSet with only this Pid as indicator for GetItemProperties for all public properties."),
        [Pid.MetaOwnerAccess                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.System    , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "", "Meta property for declaring a PidSet with only this Pid as indicator for GetItemProperties for all owner properties."),
        [Pid.MetaAspectGroup                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.System    , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "", "Meta property for declaring a PidSet with only this Pid as indicator for GetItemProperties for all aspect properties."),
        [Pid.XmppRoomList                          ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.System    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "", "Room ids managed by XMPP component. Part of the component's persistent state."),
        [Pid.FirstTest                             ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Fixed      , PropertyValue.Empty , "", ""),
        [Pid.TestInt                               ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.TestInt1                              ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.TestInt2                              ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.TestInt3                              ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.TestIntDefault                        ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , 42L                 , "1", "Evaluates to 1 if not set"),
        [Pid.TestString                            ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "fourtytwo", ""),
        [Pid.TestString1                           ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "fourtytwo", ""),
        [Pid.TestString2                           ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "fourtytwo", ""),
        [Pid.TestString3                           ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "fourtytwo", ""),
        [Pid.TestString4                           ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "fourtytwo", ""),
        [Pid.TestString5                           ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "fourtytwo", ""),
        [Pid.TestStringDefault                     ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "42"                , "42", "Evaluates to '42' if not set"),
        [Pid.TestFloat                             ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat1                            ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat2                            ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat3                            ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat4                            ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloatDefault                      ] = new Definition(Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , 3.14D               , "3.141592653589793238462643383279502", "Evaluates to 3.14 if not set"),
        [Pid.TestBool                              ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", ""),
        [Pid.TestBool1                             ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", ""),
        [Pid.TestBool2                             ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", ""),
        [Pid.TestBool3                             ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", ""),
        [Pid.TestBool4                             ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", ""),
        [Pid.TestBoolDefault                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , true                , "true", "Evaluates to true if not set"),
        [Pid.TestItem                              ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001", ""),
        [Pid.TestItem1                             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001", ""),
        [Pid.TestItem2                             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001", ""),
        [Pid.TestItem3                             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001", ""),
        [Pid.TestItemList                          ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001 10000000002", ""),
        [Pid.TestItemList1                         ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001 10000000002", ""),
        [Pid.TestItemList2                         ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001 10000000002", ""),
        [Pid.TestItemList3                         ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "10000000001 10000000002", ""),
        [Pid.TestEnum                              ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty ,                                                   Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestEnum1                             ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty ,                                                  Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestEnum2                             ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty ,                                                  Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestPublic                            ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.TestOwner                             ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.Owner   , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.TestInternal                          ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , PropertyValue.Empty , "42", ""),
        [Pid.FirstGeneric                          ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Fixed      , PropertyValue.Empty , "------------------------------------------------------", ""),
        [Pid.Name                                  ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "Avatar", "Unique name"),
        [Pid.Template                              ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "WaterBottleTemplate", "Grain Id of the template item."),
        [Pid.Label                                 ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "WaterBottle", "Used in public displays as primary designation. Will be translated."),
        [Pid.Container                             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.Public   , Persistence.Persistent , PropertyValue.Empty , "10000000001", "Id of container item."),
        [Pid.Contains                              ] = new Definition(Storage.String  , Type.StringList      , Use.ItemList     , Group.Generic   , Access.Owner   , Persistence.Persistent , PropertyValue.Empty , "10000000001 10000000002", "Container: list of child items."),
        [Pid.Stacksize                             ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , 1L                  , "3", "Number of items item stacked in one place."),
        [Pid.Left                                  ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", "X position of display elements."),
        [Pid.Bottom                                ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", "Y position of display elements."),
        [Pid.Width                                 ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", "Width of display elements, e.g. Image/Animation display width."),
        [Pid.Height                                ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", "Height of display elements, e.g. Image/Animation display width."),
        [Pid.ImageUrl                              ] = new Definition(Storage.String  , Type.String          , Use.ImageUrl     , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "http://...", "Static image"),
        [Pid.AnimationsUrl                         ] = new Definition(Storage.String  , Type.String          , Use.Url          , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "http://...", "Animations definition"),
        [Pid.Actions                               ] = new Definition(Storage.String  , Type.StringStringMap , Use.KeyValueList , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "MakeCoffee=Produce GetCoffee=EjectItem", "Maps external (app specific) actions to internal (Aspect) actions/methods."),
        [Pid.Stats                                 ] = new Definition(Storage.String  , Type.StringList      , Use.StringList   , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "WaterLevel CoffeeLevel", "List of stats visible on rezzed item."),
        [Pid.Developer                             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "developer-id-t6t8ogzubh", "Id of the the external developer who manages the item."),
        [Pid.Creator                               ] = new Definition(Storage.String  , Type.String          , Use.UserId       , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "user-id-t6t8ogzubh", "Id of the the user who created the item."),
        [Pid.Owner                                 ] = new Definition(Storage.String  , Type.String          , Use.UserId       , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "user-id-t6t8ogzubh", "Id of the the user who owns the item."),
        [Pid.FirstAspect                           ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Fixed      , PropertyValue.Empty , "------------------------------------------------------", ""),
        [Pid.GreetedAspect                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item has Greeted test aspect"),
        [Pid.GreeterAspect                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item has Greeter test aspect"),
        [Pid.DeletableAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , true                , "true", "Item can be deleted, which is the case for most items,hence default true by Property.Default."),
        [Pid.DeleterAspect                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item can delete other items."),
        [Pid.TimedAspect                           ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item uses current time an can be made using fake time for tests."),
        [Pid.ContainerAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item is a container for other items."),
        [Pid.DeveloperAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item represents an external partner"),
        [Pid.ItemCapacityLimitedAspect             ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item is a container with capacity limit"),
        [Pid.InventoryAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item can have x/y coordinates in container"),
        [Pid.SettingsAspect                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item holds user settings"),
        [Pid.RezableAspect                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , true                , "true", "Item can be rezzed to room."),
        [Pid.MovableAspect                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , true                , "true", "Item can be moved in a room."),
        [Pid.IframeAspect                          ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item opens an iframe on click."),
        [Pid.DocumentAspect                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item has text content."),
        [Pid.PageClaimAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item claims room ownership."),
        [Pid.RezableProxyAspect                    ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item is a proxy for a rezzed item."),
        [Pid.RoleAspect                            ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item contains account roles."),
        [Pid.DispenserAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "Item can generate and eject an item."),
        [Pid.SourceAspect                          ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "IsSource provides a resource (a property which can be extracted by an Extractor)."),
        [Pid.SinkAspect                            ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "IsSink takes a resource (a property which can be filled by a Supplier)."),
        [Pid.ExtractorAspect                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item extracts a resource from IsSource items."),
        [Pid.InjectorAspect                        ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item injects a resource into IsSink items."),
        [Pid.ApplierAspect                         ] = new Definition(Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "Item will select appropriate action to be applied to a passive item."),
        [Pid.FirstParameter                        ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "------------------------------------------------------", ""),
        [Pid.GreetedGetGreetingGreeter             ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "1", "Greeter item id"),
        [Pid.GreetedGetGreetingName                ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "World", "Greeted name"),
        [Pid.DeleterDeleteVictim                   ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "ItemId", "Item to be deleted."),
        [Pid.TimedSetTimeTime                      ] = new Definition(Storage.Int     , Type.Int             , Use.Time         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "120010909014640000", "Time as LongDateTime."),
        [Pid.RezableRezTo                          ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "room@chat.server.org", "Room to rez to by Rezable.Rez."),
        [Pid.RezableRezX                           ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "100", "X position in room to rez to."),
        [Pid.RezableRezDestination                 ] = new Definition(Storage.String  , Type.String          , Use.Url          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "http://...", "Url of the room to rez to."),
        [Pid.RezableDerezTo                        ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "{8AC310B6-4903-4BE9-AE8C-F329CABA9468}", "User repository to derez to."),
        [Pid.RezableDerezX                         ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "100", "X position in repository to derez to."),
        [Pid.RezableDerezY                         ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "100", "Y position in repository to derez to."),
        [Pid.MovableMoveToX                        ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "100", "X position in room to move to."),
        [Pid.DocumentSetTextText                   ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "This is a text", "Document text"),
        [Pid.InventorySetItemCoordinatesItem       ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "ItemId", "Item in inventory to be positioned."),
        [Pid.InventorySetItemCoordinatesX          ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "100", "X position in repository."),
        [Pid.InventorySetItemCoordinatesY          ] = new Definition(Storage.String  , Type.Int             , Use.Int          , Group.Parameter , Access.System  , Persistence.Transient  , PropertyValue.Empty , "100", "Y position in repository."),
        [Pid.InjectorInjectTo                      ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "ItemId", "Passive item of item action."),
        [Pid.ExtractorExtractFrom                  ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "ItemId", "Passive item of item action."),
        [Pid.ApplierApplyPassive                   ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , PropertyValue.Empty , "ItemId", "Passive item of item action."),
        [Pid.FirstApp                              ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Fixed      , PropertyValue.Empty, "------------------------------------------------------", ""),
        [Pid.GreeterPrefix                         ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Persistent , PropertyValue.Empty , "Hello ", "Greeting prefix"),
        [Pid.GreeterResult                         ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Transient  , PropertyValue.Empty , "Hello World", "Greeting result stored by Greeter before return"),
        [Pid.GreetedResult                         ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Transient  , PropertyValue.Empty , "Hello World", "Greeting result stored by Greeted"),
        [Pid.DeveloperToken                        ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Persistent , PropertyValue.Empty , "<base64-string>", "The external developer's secret token"),
        [Pid.ContainerItemLimit                    ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "3", "Number of items in the container counting stacksize."),
        [Pid.InventoryX                            ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", "X position in inventory."),
        [Pid.InventoryY                            ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", "Y position in inventory."),
        [Pid.TimedTime                             ] = new Definition(Storage.Int     , Type.Int             , Use.Time         , Group.Generic   , Access.System  , Persistence.Transient  , PropertyValue.Empty , "120010909014640000", "Preset time as LongDateTime, used instead of real time."),
        [Pid.RezableIsRezzing                      ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "True while rez not confirmed by room."),
        [Pid.RezableIsRezzed                       ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "True if rezzed to room."),
        [Pid.RezableIsDerezzing                    ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "true", "True after derez sent to room."),
        [Pid.RezableOrigin                         ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.System  , Persistence.Persistent , PropertyValue.Empty , "ItemId", "original container used in case of rez failed to return the item."),
        [Pid.RezzedX                               ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "735", "Position of item in room if rezzed."),
        [Pid.IframeUrl                             ] = new Definition(Storage.String  , Type.String          , Use.Url          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "https://", "TODO."),
        [Pid.IframeWidth                           ] = new Definition(Storage.Int     , Type.Int             , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "400", "Initial width of iframe gui window."),
        [Pid.IframeHeight                          ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "400", "Initial height of iframe gui window."),
        [Pid.IframeResizeable                      ] = new Definition(Storage.Bool    , Type.Bool            , Use.Bool         , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "true", "IFrame gui window is resizable"),
        [Pid.IframeFrame                           ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , nameof(Property.Value.IframeFrame.Window), "Window", "Iframe frame style (Window|Popup)"),
        [Pid.DocumentText                          ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "This is a text", "Document text."),
        [Pid.DocumentMaxLength                     ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , 1000L , "1000", "Max length of DocumentText property to be stored"),
        [Pid.RezzableProxyTemplate                 ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.App       , Access.System  , Persistence.Persistent , PropertyValue.Empty , "RezzableProxyTemplate", "Id of proxy item template."),
        [Pid.RoleUserRoles                         ] = new Definition(Storage.String  , Type.StringList      , Use.EnumList     , Group.App       , Access.System  , Persistence.Persistent , PropertyValue.Empty , $"{Value.UserRoles.Public} {Value.UserRoles.User} {Value.UserRoles.PowerUser}", "List of user roles."),
        [Pid.DispenserTemplate                     ] = new Definition(Storage.String  , Type.String          , Use.Item         , Group.App       , Access.System  , Persistence.Persistent , PropertyValue.Empty , "RezzableProxyTemplate", "Id of proxy item template."),
        [Pid.DispenserMaxAvailable                 ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , 1000000000L , "3", "Max number of items left to generate and eject."),
        [Pid.DispenserAvailable                    ] = new Definition(Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty  , "3", "Number of items left to generate and eject."),
        [Pid.DispenserCooldownSec                  ] = new Definition(Storage.Float   , Type.Float           , Use.Delay        , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "10.0", "Delay between actions."),
        [Pid.DispenserLastTime                     ] = new Definition(Storage.Int     , Type.Int             , Use.Time         , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "3", "Time of last action (for cooldown)."),
        [Pid.SourceResource                        ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "WaterLevel", "The name of the resource provided by the source."),
        [Pid.SinkResource                          ] = new Definition(Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "WaterLevel", "The name of the resource extracted by the sink."),
        [Pid.WaterLevel                            ] = new Definition(Storage.Float   , Type.Float           , Use.Ccm          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "100", nameof(Pid.WaterLevel)),
        [Pid.WaterLevelMax                         ] = new Definition(Storage.Float   , Type.Float           , Use.Ccm          , Group.App       , Access.Public  , Persistence.Persistent , PropertyValue.Empty , "300", nameof(Pid.WaterLevel)),
        [Pid.LastProperty                          ] = new Definition(Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.System    , Access.System  , Persistence.Persistent , PropertyValue.Empty , "", ""),
#pragma warning restore format

        };
    }
}
