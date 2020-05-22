using System;
using System.Collections.Generic;
using System.Linq;
using nine3q.Items;
using nine3q.Items.Aspects;

namespace nine3q.Content
{
    public class BasicDefinition
    {
        public enum GroupName
        {
            Admin,
            User,
            Room,
            WaterResourceTest,
        }

        public static List<string> Groups = typeof(GroupName).GetEnumNames().ToList();

        public enum Template
        {
            Dummy,

            Admin,

            // User
            //Attributes,
            //Backpack,
            //TrashCan,
            //Settings,
            //Nickname,
            //Avatar,

            // Room
            PirateFlag,
            //Landmark,
            PageProxy,

            // WaterResourceTest
            //WaterCan,
            //WaterBottle,
            //WaterSink,
            //PottedPlant,
            //BioWaste,
        }

        public const string TemplateSuffix = "Template";

        public static Dictionary<Template, string> TemplateName = typeof(Template).GetEnumNames().ToDictionary(key => (Template)Enum.Parse(typeof(Template), key), key => key + TemplateSuffix);

        public const string de = "de-DE";
        public const string en = "en-US";
        public static HashSet<string> Languages = new HashSet<string> { de, en };

        public static string GetTranslationCacheKey(string key, string lang)
        {
            return "Text-" + lang + "-" + key;
        }

        public class ActionName
        {
            public class Test1
            {
                public static string Nop = Test1Aspect.Action.Nop.ToString();
                public static string AddTestInt = Test1Aspect.Action.AddTestInt.ToString();
            }
            public class Container
            {
                public static string SetChild = ContainerAspect.Action.SetChild.ToString();
                public static string RemoveChild = ContainerAspect.Action.RemoveChild.ToString();
            }
        }

        public class TextSet : Dictionary<string, Dictionary<string, string>>
        {
            public TextSet()
            {
                foreach (var lang in BasicDefinition.Languages) {
                    Add(lang, new Dictionary<string, string>());
                }
            }
        }

    }
}
