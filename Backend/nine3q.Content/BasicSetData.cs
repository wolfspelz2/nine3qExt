using System;
using System.Collections.Generic;
using System.Linq;
using nine3q.Items;

namespace nine3q.Content
{
    public static class BasicSetData
    {
        public static void GetTemplates(string name, NamePropertiesCollection templates, TextSet text)
        {
            if (name == BasicSetDefinition.Group[BasicSetDefinition.GroupName.GenericUser]) {
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.Attributes], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.Backpack], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.TrashCan], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.Settings], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.Nickname], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.Avatar], templates, text);
                GetTemplate(BasicSetDefinition.TemplateName[BasicSetDefinition.Template.Admin], templates, text);
                GetTemplate(BasicSetDefinition.TemplateName[BasicSetDefinition.Template.GodMode], templates, text);
            } else if (name == BasicSetDefinition.Group[BasicSetDefinition.GroupName.GenericRoom]) {
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.PirateFlag], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.Landmark], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.PageProxy], templates, text);
            } else if (name == BasicSetDefinition.Group[BasicSetDefinition.GroupName.WaterResourceTest]) {
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.WaterBottle], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.WaterCan], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.WaterSink], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.PottedPlant], templates, text);
                //GetTemplate(BasicSpec.TemplateName[BasicSpec.Template.BioWaste], templates, text);
            } else {
                GetTemplate(name, templates, text);
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
            text[BasicSetDefinition.de][$"ItemProperty.{Pid.Label}"] = "Name";
            text[BasicSetDefinition.en][$"ItemProperty.{Pid.Label}"] = "Name";

            PropertySet props = null;

            if (name == BasicSetDefinition.TemplateName[BasicSetDefinition.Template.Dummy]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "Dummy",
                    [Pid.Icon32Url] = "{item.nine3q}Default/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}Default/image100.png",
                };
            }

            if (name == BasicSetDefinition.TemplateName[BasicSetDefinition.Template.Admin]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "Admin",
                    [Pid.Icon32Url] = "{item.nine3q}Admin/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}Admin/image100.jpg",
                    [Pid.IsRole] = true,
                    [Pid.Roles] = new JsonPath.Node(new List<string> { "Public", "User", "Janitor", "Moderator", "LeadModerator", "Content", "Admin" }).ToJson(bFormatted: true),
                };
                text[BasicSetDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                text[BasicSetDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
            }

            if (name == BasicSetDefinition.TemplateName[BasicSetDefinition.Template.GodMode]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "GodMode",
                    [Pid.Icon32Url] = "{item.nine3q}Admin/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}Admin/image100.jpg",
                    [Pid.IsRole] = true,
                    [Pid.Roles] = new JsonPath.Node(typeof(PropertyValue.Roles).GetEnumNames().ToList()).ToJson(bFormatted: true),
                };
                text[BasicSetDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin mit allen Rechten";
                text[BasicSetDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "All Access Admin";
            }

            if (props == null) {
                throw new Exception($"No template for name={name}");
            }

            templates.Add(name, props);
        }

        public static List<string> GetGroups()
        {
            var result = new List<string>();

            foreach (var pair in BasicSetDefinition.Group) {
                result.Add(pair.Value);
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
                foreach (var lang in BasicSetDefinition.Languages) {
                    Add(lang, new Dictionary<string, string>());
                }
            }
        }

    }
}
