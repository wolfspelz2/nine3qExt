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
            public Storage Basic { get; set; }
            public Type Type { get; set; }
            public Use Use { get; set; }
            public Group Group { get; set; }
            public Access Access { get; set; }
            public Persistence Persistence { get; set; }
            public string Example { get; set; }
            public string Description { get; set; }

            public Definition(Pid pid, Storage basic, Type type, Use usage, Group group, Access access, Persistence persistence, string example, string description)
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
        [Pid.Unknown                ] = new Definition(Pid.Unknown                 , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Unknown    , "", ""),
        [Pid.FirstOperation         ] = new Definition(Pid.FirstOperation          , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "", ""),
        [Pid.Item                   ] = new Definition(Pid.Item                    , Storage.String  , Type.String          , Use.Item         , Group.Operation , Access.System  , Persistence.Unknown    , "1", "Passive item of item action."),
        [Pid.PublicAccess           ] = new Definition(Pid.PublicAccess            , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Operation , Access.System  , Persistence.Unknown    , "", "Dummy property for declaring a PidSet with only this Pid as indicator for GetItemProperties."),
        [Pid.OwnerAccess            ] = new Definition(Pid.OwnerAccess             , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Operation , Access.System  , Persistence.Unknown    , "", "Dummy property for declaring a PidSet with only this Pid as indicator for GetItemProperties."),
        [Pid.FirstTest              ] = new Definition(Pid.FirstTest               , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "", ""),
        [Pid.TestInt                ] = new Definition(Pid.TestInt                 , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestInt1               ] = new Definition(Pid.TestInt1                , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestInt2               ] = new Definition(Pid.TestInt2                , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestInt3               ] = new Definition(Pid.TestInt3                , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.TestIntDefault         ] = new Definition(Pid.TestIntDefault          , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "1", "Evaluates to 1 if not set"),
        [Pid.TestString             ] = new Definition(Pid.TestString              , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString1            ] = new Definition(Pid.TestString1             , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString2            ] = new Definition(Pid.TestString2             , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString3            ] = new Definition(Pid.TestString3             , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString4            ] = new Definition(Pid.TestString4             , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestString5            ] = new Definition(Pid.TestString5             , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "fourtytwo", ""),
        [Pid.TestStringDefault      ] = new Definition(Pid.TestStringDefault       , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent , "42", "Evaluates to '42' if not set"),
        [Pid.TestFloat              ] = new Definition(Pid.TestFloat               , Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat1             ] = new Definition(Pid.TestFloat1              , Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat2             ] = new Definition(Pid.TestFloat2              , Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat3             ] = new Definition(Pid.TestFloat3              , Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloat4             ] = new Definition(Pid.TestFloat4              , Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", ""),
        [Pid.TestFloatDefault       ] = new Definition(Pid.TestFloatDefault        , Storage.Float   , Type.Float           , Use.Float        , Group.Test      , Access.System  , Persistence.Persistent , "3.141592653589793238462643383279502", "Evaluates to 3.14 if not set"),
        [Pid.TestBool               ] = new Definition(Pid.TestBool                , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool1              ] = new Definition(Pid.TestBool1               , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool2              ] = new Definition(Pid.TestBool2               , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool3              ] = new Definition(Pid.TestBool3               , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBool4              ] = new Definition(Pid.TestBool4               , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", ""),
        [Pid.TestBoolDefault        ] = new Definition(Pid.TestBoolDefault         , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Test      , Access.System  , Persistence.Persistent , "true", "Evaluates to true if not set"),
        [Pid.TestItem               ] = new Definition(Pid.TestItem                , Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItem1              ] = new Definition(Pid.TestItem1               , Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItem2              ] = new Definition(Pid.TestItem2               , Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItem3              ] = new Definition(Pid.TestItem3               , Storage.String  , Type.String          , Use.Item         , Group.Test      , Access.System  , Persistence.Persistent , "10000000001", ""),
        [Pid.TestItemList           ] = new Definition(Pid.TestItemList            , Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestItemList1          ] = new Definition(Pid.TestItemList1           , Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestItemList2          ] = new Definition(Pid.TestItemList2           , Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestItemList3          ] = new Definition(Pid.TestItemList3           , Storage.String  , Type.StringList      , Use.ItemList     , Group.Test      , Access.System  , Persistence.Persistent , "10000000001 10000000002", ""),
        [Pid.TestEnum               ] = new Definition(Pid.TestEnum                , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent ,                                                   Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestEnum1              ] = new Definition(Pid.TestEnum1               , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent ,                                                  Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestEnum2              ] = new Definition(Pid.TestEnum2               , Storage.String  , Type.String          , Use.String       , Group.Test      , Access.System  , Persistence.Persistent ,                                                  Property.Value.TestEnum.Value1.ToString(), ""),
        [Pid.TestPublic             ] = new Definition(Pid.TestPublic              , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.Public  , Persistence.Persistent , "42", ""),
        [Pid.TestOwner              ] = new Definition(Pid.TestOwner               , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.Owner   , Persistence.Persistent , "42", ""),
        [Pid.TestInternal           ] = new Definition(Pid.TestInternal            , Storage.Int     , Type.Int             , Use.Int          , Group.Test      , Access.System  , Persistence.Persistent , "42", ""),
        [Pid.FirstGeneric           ] = new Definition(Pid.FirstGeneric            , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "------------------------------------------------------", ""),
        [Pid.Name                   ] = new Definition(Pid.Name                    , Storage.String  , Type.String          , Use.String       , Group.Generic   , Access.Public  , Persistence.Persistent , "Avatar", ""),
        [Pid.Template               ] = new Definition(Pid.Template                , Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.System  , Persistence.Persistent , "WaterBottleTemplate", "Grain Id of the template item."),
        [Pid.Label                  ] = new Definition(Pid.Label                   , Storage.String  , Type.String          , Use.String       , Group.Generic   , Access.Public  , Persistence.Persistent , "WaterBottle", "Used in public displays as primary designation. Will be translated."),
        [Pid.Container              ] = new Definition(Pid.Container               , Storage.String  , Type.String          , Use.Item         , Group.Generic   , Access.Owner   , Persistence.Persistent , "10000000001", "Id of container item."),
        [Pid.Contains               ] = new Definition(Pid.Contains                , Storage.String  , Type.StringList      , Use.ItemList     , Group.Generic   , Access.Owner   , Persistence.Persistent , "10000000001 10000000002", "Container: list of child items."),
        [Pid.Stacksize              ] = new Definition(Pid.Stacksize               , Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , "3", "Number of items item stacked in one place."),
        [Pid.Icon32Url              ] = new Definition(Pid.Icon32Url               , Storage.String  , Type.String          , Use.ImageUrl     , Group.Generic   , Access.Public  , Persistence.Persistent , "http://...", "Medium images"),
        [Pid.Image100Url            ] = new Definition(Pid.Image100Url             , Storage.String  , Type.String          , Use.ImageUrl     , Group.Generic   , Access.Public  , Persistence.Persistent , "http://...", ""),
        [Pid.AnimationsUrl          ] = new Definition(Pid.AnimationsUrl           , Storage.String  , Type.String          , Use.Url          , Group.Generic   , Access.Public  , Persistence.Persistent , "http://...", ""),
        [Pid.Actions                ] = new Definition(Pid.Actions                 , Storage.String  , Type.StringStringMap , Use.KeyValueList , Group.Generic   , Access.Public  , Persistence.Persistent , "MakeCoffee=Produce GetCoffee=EjectItem", "Maps external (app specific) actions to internal (Aspect) actions/methods."),
        [Pid.Stats                  ] = new Definition(Pid.Stats                   , Storage.String  , Type.StringList      , Use.StringList   , Group.Generic   , Access.Public  , Persistence.Persistent , "WaterLevel CoffeeLevel", "List of stats visible on rezzed item."),
        [Pid.RezableIsRezzing       ] = new Definition(Pid.RezableIsRezzing        , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , "true", "True while rez not confirmed by room."),
        [Pid.RezableIsRezzed        ] = new Definition(Pid.RezableIsRezzed         , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , "true", "True if rezzed to room."),
        [Pid.RezableIsDerezzing     ] = new Definition(Pid.RezableIsDerezzing      , Storage.Bool    , Type.Bool            , Use.Bool         , Group.Generic   , Access.System  , Persistence.Persistent , "true", "True after derez sent to room."),
        [Pid.RezableX               ] = new Definition(Pid.RezableX                , Storage.Int     , Type.Int             , Use.Int          , Group.Generic   , Access.Public  , Persistence.Persistent , "735", "Position of item in room if rezzed."),
        [Pid.FirstAspect            ] = new Definition(Pid.FirstAspect             , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "------------------------------------------------------", ""),
        [Pid.TestGreetedAspect      ] = new Definition(Pid.TestGreetedAspect       , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item has Greeted test aspect"),
        [Pid.TestGreeterAspect      ] = new Definition(Pid.TestGreeterAspect       , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item has Greeter test aspect"),
        [Pid.DeletableAspect        ] = new Definition(Pid.DeletableAspect         , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item can be deleted,which is the case for most items,hence default true by Property.Default."),
        [Pid.ContainerAspect        ] = new Definition(Pid.ContainerAspect         , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item is a container for other items."),
        [Pid.ItemCapacityLimitAspect] = new Definition(Pid.ItemCapacityLimitAspect , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item is a container with capacity limit"),
        [Pid.RezableAspect          ] = new Definition(Pid.RezableAspect           , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item can be rezzed to room."),
        [Pid.IframeAspect           ] = new Definition(Pid.IframeAspect            , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item opens an iframe on click."),
        [Pid.PageClaimAspect        ] = new Definition(Pid.PageClaimAspect         , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item claims room ownership."),
        [Pid.RezzableProxyAspect    ] = new Definition(Pid.RezzableProxyAspect     , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item is a proxy for a rezzed item."),
        [Pid.RoleAspect             ] = new Definition(Pid.RoleAspect              , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.System  , Persistence.Persistent , "true", "Item contains account roles."),
        [Pid.SourceAspect           ] = new Definition(Pid.SourceAspect            , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "IsSource provides a resource (a property which can be extracted by an Extractor)."),
        [Pid.SinkAspect             ] = new Definition(Pid.SinkAspect              , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "IsSink takes a resource (a property which can be filled by a Supplier)."),
        [Pid.ExtractorAspect        ] = new Definition(Pid.ExtractorAspect         , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item extracts a resource from IsSource items."),
        [Pid.InjectorAspect         ] = new Definition(Pid.InjectorAspect          , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item injects a resource into IsSink items."),
        [Pid.ApplierAspect          ] = new Definition(Pid.ApplierAspect           , Storage.Bool    , Type.Bool            , Use.Aspect       , Group.Aspect    , Access.Public  , Persistence.Persistent , "true", "Item will select appropriate action to be applied to a passive item."),
        [Pid.FirstParameter         ] = new Definition(Pid.FirstParameter          , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Persistent , "------------------------------------------------------", ""),
        [Pid.TestGreeted_Item       ] = new Definition(Pid.TestGreeted_Item        , Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Transient  , "1", "Greeter item id"),
        [Pid.TestGreeted_Name       ] = new Definition(Pid.TestGreeted_Name        , Storage.String  , Type.String          , Use.String       , Group.Parameter , Access.System  , Persistence.Transient  , "World", "Greeted name"),
        [Pid.TestGreeter_Result     ] = new Definition(Pid.TestGreeter_Result      , Storage.String  , Type.String          , Use.String       , Group.Parameter , Access.System  , Persistence.Persistent , "Hello World", "Greeting result stored by Greeter before return"),
        [Pid.TestGreeted_Result     ] = new Definition(Pid.TestGreeted_Result      , Storage.String  , Type.String          , Use.String       , Group.Parameter , Access.System  , Persistence.Persistent , "Hello World", "Greeting result stored by Greeted"),
        [Pid.PassiveItem            ] = new Definition(Pid.PassiveItem             , Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Unknown    , "ItemId", "Passive item of item action."),
        [Pid.RezableRoom            ] = new Definition(Pid.RezableRoom             , Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Persistent , "1", "Room to rez to by Rezable.Rez."),
        [Pid.RezableUser            ] = new Definition(Pid.RezableUser             , Storage.String  , Type.String          , Use.Item         , Group.Parameter , Access.System  , Persistence.Persistent , "1", "User to derez to by Rezable.Derez."),
        [Pid.FirstApp               ] = new Definition(Pid.FirstApp                , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Fixed      , "------------------------------------------------------", ""),
        [Pid.TestGreeter_Prefix     ] = new Definition(Pid.TestGreeter_Prefix      , Storage.String  , Type.String          , Use.String       , Group.App       , Access.System  , Persistence.Persistent , "Hello ", "Greeting prefix"),
        [Pid.ContainerItemLimit     ] = new Definition(Pid.ContainerItemLimit      , Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , "3", "Number of items in the container counting stacksize."),
        [Pid.IframeUrl              ] = new Definition(Pid.IframeUrl               , Storage.String  , Type.String          , Use.Url          , Group.App       , Access.Public  , Persistence.Persistent , "IFrameUrl", "TODO."),
        [Pid.IframeWidth            ] = new Definition(Pid.IframeWidth             , Storage.Int     , Type.Int             , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , "IFrameWidth", "TODO."),
        [Pid.IframeHeight           ] = new Definition(Pid.IframeHeight            , Storage.Int     , Type.Int             , Use.Int          , Group.App       , Access.Public  , Persistence.Persistent , "IFrameHeight", "TODO."),
        [Pid.IframeResizeable       ] = new Definition(Pid.IframeResizeable        , Storage.Bool    , Type.Bool            , Use.Bool         , Group.App       , Access.Public  , Persistence.Persistent , "IFrameResizable", "TODO."),
        [Pid.RezzableProxyTemplate  ] = new Definition(Pid.RezzableProxyTemplate   , Storage.String  , Type.String          , Use.Item         , Group.App       , Access.System  , Persistence.Persistent , "RezzableProxyTemplate", "Id of proxy item template."),
        [Pid.RoleUserRoles          ] = new Definition(Pid.RoleUserRoles           , Storage.String  , Type.StringList      , Use.EnumList     , Group.App       , Access.System  , Persistence.Persistent ,                                                $"{Value.UserRoles.Public} {Value.UserRoles.User} {Value.UserRoles.PowerUser}", "List of user roles."),
        [Pid.SourceResource         ] = new Definition(Pid.SourceResource          , Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , "WaterLevel", "The name of the resource provided by the source."),
        [Pid.SinkResource           ] = new Definition(Pid.SinkResource            , Storage.String  , Type.String          , Use.String       , Group.App       , Access.Public  , Persistence.Persistent , "WaterLevel", "The name of the resource provided by the source."),
        [Pid.WaterLevel             ] = new Definition(Pid.WaterLevel              , Storage.Float   , Type.Float           , Use.Ccm          , Group.App       , Access.Public  , Persistence.Persistent , "100", "Water level."),
        [Pid.WaterLevelMax          ] = new Definition(Pid.WaterLevelMax           , Storage.Float   , Type.Float           , Use.Ccm          , Group.App       , Access.Public  , Persistence.Persistent , "300", "Water level max."),
        [Pid.FirstUserAttribute     ] = new Definition(Pid.FirstUserAttribute      , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Persistent , "", ""),
        [Pid.LastProperty           ] = new Definition(Pid.LastProperty            , Storage.Unknown , Type.Unknown         , Use.Unknown      , Group.Unknown   , Access.System  , Persistence.Persistent , "", ""),
#pragma warning restore format

        };
    }
}
