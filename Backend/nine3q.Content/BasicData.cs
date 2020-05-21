using System;
using System.Collections.Generic;
using System.Linq;
using nine3q.Items;

namespace nine3q.Content
{
    public static class BasicData
    {
        public static void GetTemplates(string name, NamePropertiesCollection templates, TextSet text)
        {
            switch (name) {
                case nameof(BasicDefinition.GroupName.Admin):
                    GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Admin], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.GodMode], templates, text);
                    break;

                case nameof(BasicDefinition.GroupName.User):
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Attributes], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Backpack], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.TrashCan], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Settings], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Nickname], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Avatar], templates, text);
                    GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Admin], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.GodMode], templates, text);
                    break;

                case nameof(BasicDefinition.GroupName.Room):
                    GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.PirateFlag], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Landmark], templates, text);
                    GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.PageProxy], templates, text);
                    break;

                case nameof(BasicDefinition.GroupName.WaterResourceTest):
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.WaterBottle], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.WaterCan], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.WaterSink], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.PottedPlant], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.BioWaste], templates, text);
                    break;

                default:
                    GetTemplate(name, templates, text);
                    break;
            }
        }

        //public static string GetTemplateJson(string name)
        //{
        //    var translations = new TextSet();
        //    var templates = new NamePropertiesCollection();
        //    GetTemplate(name, templates, translations);
        //    var props = templates.First().Value;
        //    var dict = props.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
        //    var node = new JsonPath.Node(dict);
        //    var json = node.ToJson();
        //    return json;
        //}

        public static void GetTemplate(string name, NamePropertiesCollection templates, TextSet text)
        {
            text[BasicDefinition.de][$"ItemProperty.{Pid.Label}"] = "Name";
            text[BasicDefinition.en][$"ItemProperty.{Pid.Label}"] = "Name";

            text[BasicDefinition.de][$"ItemProperty.{Pid.RezableAspect}"] = "Installierbar";
            text[BasicDefinition.en][$"ItemProperty.{Pid.RezableAspect}"] = "Installable";

            PropertySet props = null;

            if (name == BasicDefinition.TemplateName[BasicDefinition.Template.Dummy]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "Dummy",
                    [Pid.Icon32Url] = "{item.nine3q}Default/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}Default/image100.png",
                };
            }

            if (name == BasicDefinition.TemplateName[BasicDefinition.Template.Admin]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "Admin",
                    [Pid.Icon32Url] = "{item.nine3q}Admin/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}Admin/image100.jpg",
                    [Pid.IsRole] = true,
                    [Pid.Roles] = new JsonPath.Node(new List<string> { "Public", "User", "PowerUser", "Janitor", "Moderator", "LeadModerator", "Content", "Admin" }).ToJson(bFormatted: true),
                };
                text[BasicDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                text[BasicDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
            }

            //if (name == BasicDefinition.TemplateName[BasicDefinition.Template.GodMode]) {
            //    props = new PropertySet {
            //        [Pid.Name] = name,
            //        [Pid.Label] = "GodMode",
            //        [Pid.Icon32Url] = "{item.nine3q}Admin/icon32.png",
            //        [Pid.Image100Url] = "{item.nine3q}Admin/image100.jpg",
            //        [Pid.IsRole] = true,
            //        [Pid.Roles] = new JsonPath.Node(typeof(PropertyValue.Roles).GetEnumNames().ToList()).ToJson(bFormatted: true),
            //    };
            //    text[BasicDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin mit allen Rechten";
            //    text[BasicDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "All Access Admin";
            //}

            if (name == BasicDefinition.TemplateName[BasicDefinition.Template.PirateFlag]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "PirateFlag",
                    [Pid.Icon32Url] = "{item.nine3q}PirateFlag/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}PirateFlag/image100.png",
                    [Pid.AnimationsUrl] = "{item.nine3q}PirateFlag/animations.xml",
                    [Pid.IsClaim] = true,
                    [Pid.RezableAspect] = true,
                    [Pid.ProxyTemplate] = BasicDefinition.TemplateName[BasicDefinition.Template.PageProxy],
                };
                text[BasicDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Piratenflagge";
                text[BasicDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Pirate Flag";
            }

            if (name == BasicDefinition.TemplateName[BasicDefinition.Template.PageProxy]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "PageProxy",
                    [Pid.Icon32Url] = "{item.nine3q}PageProxy/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}PageProxy/image100.png",
                    [Pid.IsProxy] = true,
                };
                text[BasicDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Webseitenbesitz";
                text[BasicDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Page Claim";
            }

            if (props == null) {
                throw new Exception($"No template for name={name}");
            }

            templates.Add(name, props);
        }

        public static List<string> GetGroups()
        {
            var result = new List<string>();

            foreach (var group in BasicDefinition.Group) {
                result.Add(group);
            }

            return result;
        }

        public static List<string> GetTemplates(string name)
        {
            var result = new List<string>();

            var translations = new TextSet();
            var templates = new NamePropertiesCollection();

            GetTemplates(name, templates, translations);

            foreach (var pair in templates) {
                result.Add(pair.Key);
            }

            return result;
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
