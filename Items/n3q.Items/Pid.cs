﻿namespace n3q.Items
{
    public enum Pid
    {
        Unknown = 0,

        // Test
        FirstTest,
        TestInt,
        TestInt1,
        TestInt2,
        TestInt3,
        TestIntDefault,
        TestString,
        TestString1,
        TestString2,
        TestString3,
        TestString4,
        TestString5,
        TestStringDefault,
        TestFloat,
        TestFloat1,
        TestFloat2,
        TestFloat3,
        TestFloat4,
        TestFloatDefault,
        TestBool,
        TestBool1,
        TestBool2,
        TestBool3,
        TestBool4,
        TestBoolDefault,
        TestItem,
        TestItem1,
        TestItem2,
        TestItem3,
        TestItemList,
        TestItemList1,
        TestItemList2,
        TestItemList3,
        TestEnum,
        TestEnum1,
        TestEnum2,
        TestPublic,
        TestOwner,
        TestInternal,

        // Operational, not real item properties
        FirstSystem = 1000000,
        Item,
        MetaPublicAccess,
        MetaOwnerAccess,
        MetaAspectGroup,
        XmppRoomList,

        // Generic
        FirstGeneric = 2000000,
        //Id,
        Name,
        Template,
        Label,
        Container,
        Contains,
        Stacksize,
        Left,
        Bottom,
        Width,
        Height,
        ImageUrl,
        AnimationsUrl,
        Actions,
        Stats,
        Developer,

        // Aspect
        FirstAspect = 3000000,
        TestGreetedAspect,
        TestGreeterAspect,
        DeletableAspect,
        ContainerAspect,
        DeveloperAspect,
        ItemCapacityLimitAspect,
        InventoryAspect,
        SettingsAspect,
        RezableAspect,
        MovableAspect,
        IframeAspect,
        DocumentAspect,
        PageClaimAspect,
        RezableProxyAspect,
        RoleAspect,
        SourceAspect,
        SinkAspect,
        ExtractorAspect,
        InjectorAspect,
        ApplierAspect,

        // Public aspect method parameters
        FirstParameter = 4000000,
        TestGreetedGetGreetingGreeter,
        TestGreetedGetGreetingName,
        PartnerGenerateTokenSecret,
        RezableRezTo,
        RezableRezX,
        RezableRezDestination,
        RezableDerezTo,
        RezableDerezX,
        RezableDerezY,
        MovableMoveToX,
        InventorySetItemCoordinatesItem,
        InventorySetItemCoordinatesX,
        InventorySetItemCoordinatesY,
        //SettingsSetInventoryCoordinatesLeft,
        //SettingsSetInventoryCoordinatesBottom,
        //SettingsSetInventoryCoordinatesWidth,
        //SettingsSetInventoryCoordinatesHeight,
        InjectorInjectTo,
        ExtractorExtractFrom,
        ApplierApplyTo,

        // App
        FirstApp = 5000000,
        TestGreeterPrefix,
        TestGreeterResult,
        TestGreetedResult,
        PartnerToken,
        ContainerItemLimit,
        InventoryX,
        InventoryY,
        //InventoryLeft,
        //InventoryBottom,
        //InventoryWidth,
        //InventoryHeight,
        RezableIsRezzing,
        RezableIsRezzed,
        RezableIsDerezzing,
        RezableOrigin,
        RezzedX,
        IframeUrl,
        IframeWidth,
        IframeHeight,
        IframeResizeable,
        DocumentText,
        DocumentMaxLength,
        RezzableProxyTemplate,
        RoleUserRoles,
        SourceResource,
        SinkResource,
        WaterLevel,
        WaterLevelMax,

        // User Attribute
        FirstUserAttribute = 6000000,

        LastProperty,
    }
}