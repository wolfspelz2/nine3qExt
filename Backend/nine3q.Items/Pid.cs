﻿namespace nine3q.Items
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
        //StaleTemplate,
        PublicAccess,
        OwnerAccess,
        TransferState,
        TransferContainer,
        TransferSlot,

        // Generic
        FirstGeneric = 2000000,
        Id,
        Name,
        TemplateName,
        //Label,
        //Owner,
        Container,
        Contains,
        Slots,
        Slot,
        //GridWidth,
        Stacksize,
        //ImageUrl,
        //Icon16Url,
        //Icon32Url,
        //AvatarUrl,
        //Image100Url,
        //AnimationsUrl,
        //AnimationsMime,
        Actions,
        //PassiveActions,
        //IsRezable,
        //Rezzed,
        //RezzedX,
        //IsClaim,
        //Claimed,
        //IsProxy,
        //ProxyTemplate,
        //ProxyName,
        //ProxyDestination,
        //ProxyInventory,
        //IsRepository,
        //IsSettings,
        //IsAvatar,
        //IsNickname,
        //IsRole,
        //Roles,
        //Stats,
        //Condition,

        // Aspect
        FirstAspect = 3000000,
        IsTest1,
        IsTest2,
        IsContainer,
        //IsTrashCan,
        //IsSource,
        //IsSink,
        //IsExtractor,
        //IsInjector,
        //IsDeletee,
        //IsApplier,
        //IsCondition,

        // Level (*Max by convention)
        FirstLevel = 4000000,
        //WaterLevel,
        //WaterLevelMax,
        //CoffeeLevel,
        //CoffeeLevelMax,
        //SoilLevel,

        // App
        FirstApp = 5000000,
        ContainerCanExport,
        ContainerCanImport,
        //ContainerCanReplace,
        ContainerCanShuffle,
        //ContainerCanRez,
        //ContainerCanDerez,
        //Resource,
        //DeleteExtractedResource,
        //DeleteTime,
        //ConditionResource,
        //ConditionUse,
        //ConditionInterval,
        //ConditionTimer,
        //ConditionRecover,
        //ConditionStarve,
        //ConditionDeadTemplate,

        // User Attribute
        FirstAttribute = 6000000,
        //Nickname,
        //HideNickname,
        //DefaultNicknamePrefix,
        //AvatarSpeed,
        //DefaultAvatarBase,
        //DefaultAvatarImage,
        //DefaultAvatarAnimation,
        //DefaultAvatarList,
        //PasswordHash,
        //PasswordSalt,
        //PasswordAlgorithm,
        //UserCustomized,

        LastProperty,
    }
}
