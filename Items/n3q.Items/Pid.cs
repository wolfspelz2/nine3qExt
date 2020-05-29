namespace n3q.Items
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
        FirstOperation = 1000000,
        Item,
        PublicAccess,
        OwnerAccess,

        // Generic
        FirstGeneric = 2000000,
        //Id,
        Name,
        TemplateId,
        Label,
        Container,
        Contains,
        Stacksize,
        Icon32Url,
        Image100Url,
        AnimationsUrl,

        // Aspect
        FirstAspect = 3000000,
        TestGreetedAspect,
        TestGreeterAspect,
        DeletableAspect,
        ContainerAspect,
        ItemCapacityLimitAspect,
        RezableAspect,
        IframeAspect,

        // Method parameters
        FirstParameter = 4000000,
        TestGreeted_Item,
        TestGreeted_Name,
        TestGreeter_Result,
        TestGreeted_Result,
        RezRoom,
        DerezUser,

        // App
        FirstApp = 5000000,
        TestGreeterPrefix,
        ContainerItemLimit,
        IsRezzing,
        IsRezzed,
        IsDerezzing,
        RezzedX,
        IframeUrl,
        IframeWidth,
        IframeHeight,
        IframeResizeable,
        //WaterLevel,
        //WaterLevelMax,

        // User Attribute
        FirstUserAttribute = 6000000,

        LastProperty,
    }
}