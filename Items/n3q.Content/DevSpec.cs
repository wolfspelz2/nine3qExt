using System;
using System.Collections.Generic;
using System.Linq;
using n3q.Items;

namespace n3q.Content
{
    public static class DevSpec
    {
        public enum Group
        {
            System,
            User,
            Room,
            AvatarTheatre,
            WaterResourceTest,
            FridaysForFuture,
        }

        public enum Template
        {
            Dummy,

            Admin,
            CodeReviewer,

            // User
            //Attributes,
            //Backpack,
            //TrashCan,
            Settings,
            //Nickname,
            //Avatar,

            // Room
            PirateFlag,
            Landmark,
            PageProxy,

            // Theatre
            TheatreScreenplay,

            // WaterResourceTest
            WaterCan,
            WaterBottle,
            WaterSink,
            PottedPlant,
            BioWaste,

            // FridaysForFuture
            PosterHowDareYou,
            PosterThereIsNoPlanetB,
            PosterWirStreikenBisIhrHandelt,
            RallySpeaker,
            FieldMapleTree,
            MapleTree,
            PlatanusOccidentalis,
            SmallMapleTree,
        }

        public const string AllGroupsSpecialSelector = "ALL";
        public const string TemplateContainer = "DevTemplates";
        //public const string TemplateSuffix = "Template";
        public const string TemplateSuffix = "";

        public static List<string> AllGroups = typeof(Group).GetEnumNames().ToList();
        public static List<string> AllTemplates = typeof(Template).GetEnumNames().Select(name => name + TemplateSuffix).ToList();

        public static string de = "de-DE";
        public static string en = "en-US";
        public static HashSet<string> Languages = new HashSet<string> { de, en };

        public static List<string> GetGroups()
        {
            var result = new List<string>();

            foreach (var group in DevSpec.AllGroups) {
                result.Add(group);
            }

            return result;
        }

        public static string GetTranslationCacheKey(string key, string lang)
        {
            return "Text-" + lang + "-" + key;
        }

        //public static class ActionName
        //{
        //    public class Test1
        //    {
        //        public static string Nop = Test1Aspect.Action.Nop.ToString();
        //        public static string AddTestInt = Test1Aspect.Action.AddTestInt.ToString();
        //    }
        //    public class Container
        //    {
        //        public static string SetChild = ContainerAspect.Action.SetChild.ToString();
        //        public static string RemoveChild = ContainerAspect.Action.RemoveChild.ToString();
        //    }
        //}

        public class TemplateCollection : Dictionary<string, PropertySet>
        {
        }

        public class TextCollection : Dictionary<string, Dictionary<string, string>>
        {
            public TextCollection()
            {
                foreach (var lang in DevSpec.Languages) {
                    Add(lang, new Dictionary<string, string>());
                }
            }
        }

    }
}
