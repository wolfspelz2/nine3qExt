using System;
using System.Collections.Generic;
using System.Linq;
using n3q.Common;
using n3q.Tools;
using n3q.Items;
using n3q.Aspects;

namespace n3q.Content
{
    public static class DevData
    {
        public static void GetTemplates(string name, Dev.NamePropertiesCollection templates, Dev.TextSet text)
        {
            switch (name) {
                case nameof(Dev.GroupName.Admin):
                    GetTemplate(Dev.TemplateName[Dev.Template.Admin], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.GodMode], templates, text);
                    break;

                case nameof(Dev.GroupName.User):
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Attributes], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Backpack], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.TrashCan], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Settings], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Nickname], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Avatar], templates, text);
                    GetTemplate(Dev.TemplateName[Dev.Template.Admin], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.GodMode], templates, text);
                    break;

                case nameof(Dev.GroupName.Room):
                    GetTemplate(Dev.TemplateName[Dev.Template.PirateFlag], templates, text);
                    //GetTemplate(BasicDefinition.TemplateName[BasicDefinition.Template.Landmark], templates, text);
                    GetTemplate(Dev.TemplateName[Dev.Template.PageProxy], templates, text);
                    break;

                case nameof(Dev.GroupName.AvatarTheatre):
                    GetTemplate(Dev.TemplateName[Dev.Template.TheatreScreenplay], templates, text);
                    break;

                case nameof(Dev.GroupName.WaterResourceTest):
                    GetTemplate(Dev.TemplateName[Dev.Template.WaterBottle], templates, text);
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

        public static void GetTemplate(string name, Dev.NamePropertiesCollection templates, Dev.TextSet text)
        {
            text[Dev.de][$"ItemProperty.{Pid.Label}"] = "Name";
            text[Dev.en][$"ItemProperty.{Pid.Label}"] = "Name";

            text[Dev.de][$"ItemProperty.{Pid.RezableAspect}"] = "Ablegbar";
            text[Dev.en][$"ItemProperty.{Pid.RezableAspect}"] = "Droppable";

            PropertySet props = null;

            if (name == Dev.TemplateName[Dev.Template.Dummy]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "Dummy",
                    [Pid.Icon32Url] = PropertyFilter.ItemBase + "Default/icon32.png",
                    [Pid.Image100Url] = PropertyFilter.ItemBase + "Default/image100.png",
                };
            }

            if (name == Dev.TemplateName[Dev.Template.Admin]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "Admin",
                    [Pid.Icon32Url] = PropertyFilter.ItemBase + "Admin/icon32.png",
                    [Pid.Image100Url] = PropertyFilter.ItemBase + "Admin/image100.jpg",
                    [Pid.RoleAspect] = true,
                    [Pid.RoleUserRoles] = ValueList.From(EnumUtil.GetEnumValues<Property.Value.UserRoles>().Where(role => role <= Property.Value.UserRoles.Admin).Select(role => role.ToString())),
                };
                text[Dev.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                text[Dev.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
            }

            //if (name == BasicDefinition.TemplateName[BasicDefinition.Template.GodMode]) {
            //    props = new PropertySet {
            //        [Pid.Name] = name,
            //        [Pid.Label] = "GodMode",
            //        [Pid.Icon32Url] = PropertyFilter.ItemImageVar + "Admin/icon32.png",
            //        [Pid.Image100Url] = PropertyFilter.ItemImageVar + "Admin/image100.jpg",
            //        [Pid.IsRole] = true,
            //        [Pid.Roles] = new JsonPath.Node(typeof(PropertyValue.Roles).GetEnumNames().ToList()).ToJson(bFormatted: true),
            //    };
            //    text[BasicDefinition.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin mit allen Rechten";
            //    text[BasicDefinition.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "All Access Admin";
            //}

            if (name == Dev.TemplateName[Dev.Template.PirateFlag]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "PirateFlag",
                    [Pid.Icon32Url] = PropertyFilter.ItemBase + "PirateFlag/icon32.png",
                    [Pid.Image100Url] = PropertyFilter.ItemBase + "PirateFlag/image100.png",
                    [Pid.AnimationsUrl] = PropertyFilter.ItemBase + "PirateFlag/animations.xml",
                    [Pid.PageClaimAspect] = true,
                    [Pid.RezableAspect] = true,
                    [Pid.RezzableProxyTemplate] = Dev.TemplateName[Dev.Template.PageProxy],
                };
                text[Dev.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Piratenflagge";
                text[Dev.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Pirate Flag";
            }

            if (name == Dev.TemplateName[Dev.Template.PageProxy]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "PageProxy",
                    [Pid.Icon32Url] = PropertyFilter.ItemBase + "PageProxy/icon32.png",
                    [Pid.Image100Url] = PropertyFilter.ItemBase + "PageProxy/image100.png",
                    [Pid.RezzableProxyAspect] = true,
                };
                text[Dev.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Webseitenbesitz";
                text[Dev.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Page Claim";
            }

            if (name == Dev.TemplateName[Dev.Template.TheatreScreenplay]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "TheatreScreenplay",
                    [Pid.Icon32Url] = PropertyFilter.ItemBase + "PageProxy/icon32.png",
                    [Pid.Image100Url] = PropertyFilter.ItemBase + "PageProxy/image100.png",
                    [Pid.RezableAspect] = true,
                    [Pid.IframeAspect] = true,
                    [Pid.IframeUrl] = "https://example.com",
                    [Pid.IframeWidth] = 400,
                    [Pid.IframeHeight] = 400,
                    [Pid.IframeResizeable] = true,
                };
                text[Dev.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Theater Drehbuch";
                text[Dev.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Theatre Screenplay";
            }

            if (name == Dev.TemplateName[Dev.Template.WaterBottle]) {
                props = new PropertySet {
                    [Pid.Name] = name,
                    [Pid.Label] = "WaterBottle",
                    [Pid.Icon32Url] = "{item.nine3q}WaterBottle/icon32.png",
                    [Pid.Image100Url] = "{item.nine3q}WaterBottle/image100.png",
                    [Pid.RezableAspect] = true,
                    [Pid.SourceAspect] = true,
                    [Pid.SourceResource] = Pid.WaterLevel.ToString(),
                    [Pid.SinkAspect] = true,
                    [Pid.SinkResource] = Pid.WaterLevel.ToString(),
                    [Pid.WaterLevel] = 0,
                    [Pid.WaterLevelMax] = 330,
                    [Pid.ExtractorAspect] = true,
                    [Pid.InjectorAspect] = true,
                    [Pid.ApplierAspect] = true,
                    [Pid.Actions] = ValueMap.From(new Dictionary<string, string> { ["ApplyTo"] = nameof(Applier.Action.ApplyTo), ["GetWater"] = nameof(Extractor.Action.Extract), ["PutWater"] = nameof(Injector.Action.Inject) }),
                    [Pid.Stats] = ValueList.From(new[] { Pid.WaterLevel.ToString() }),
                };
                text[Dev.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Wasserflasche";
                text[Dev.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Water Bottle";
            }

            if (props == null) {
                throw new Exception($"No template for name={name}");
            }

            templates.Add(name, props);
        }

        public static List<string> GetTemplates(string name)
        {
            var result = new List<string>();

            var translations = new Dev.TextSet();
            var templates = new Dev.NamePropertiesCollection();

            GetTemplates(name, templates, translations);

            foreach (var pair in templates) {
                result.Add(pair.Key);
            }

            return result;
        }

    }
}
