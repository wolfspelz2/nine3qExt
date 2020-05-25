namespace nine3q.Items
{
    public enum Pid
    {
        NoProperty = 0,

        // Test
        FirstTest,
        TestInt,
        TestInt1,
        TestInt2,
        TestInt3,
        TestString,
        TestString1,
        TestString2,
        TestString3,
        TestFloat,
        TestFloat1,
        TestFloat2,
        TestFloat3,
        TestFloat4,
        TestBool,
        TestBool1,
        TestBool2,
        TestBool3,
        TestBool4,
        TestItem,
        TestItem1,
        TestItem2,
        TestItem3,
        TestItemSet,
        TestItemSet1,
        TestItemSet2,
        TestItemSet3,
        TestEnum,
        TestEnum1,
        TestEnum2,
        TestPublic,
        TestOwner,
        TestInternal,

        // Operational, not real item properties
        FirstOperation = 1000000,
        Item,

        // Generic
        FirstGeneric = 2000000,
        Id,
        Name,
        TemplateId,
        Label,

        // Aspect
        FirstAspect = 3000000,
        Test1Aspect,
        Test2Aspect,
        RezableAspect,
        IframeAspect,

        // Level (*Max by convention)
        FirstLevel = 4000000,
        //WaterLevel,
        //WaterLevelMax,

        // App
        FirstApp = 5000000,
        IframeUrl,
        IframeWidth,
        IframeHeight,
        IframeResizeable,

        // User Attribute
        FirstAttribute = 6000000,

        LastProperty,
    }
}