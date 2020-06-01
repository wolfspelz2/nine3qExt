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
        public static void GetTemplates(string name, DevSpec.TemplateCollection templates, DevSpec.TextCollection text)
        {
            switch (name) {
                case nameof(DevSpec.Group.System):
                    GetTemplate(nameof(DevSpec.Template.Admin), templates, text);
                    break;

                case nameof(DevSpec.Group.User):
                    Don.t = () => {
                        GetTemplate(nameof(DevSpec.Template.Attributes), templates, text);
                        GetTemplate(nameof(DevSpec.Template.Backpack), templates, text);
                        GetTemplate(nameof(DevSpec.Template.TrashCan), templates, text);
                        GetTemplate(nameof(DevSpec.Template.Settings), templates, text);
                        GetTemplate(nameof(DevSpec.Template.Nickname), templates, text);
                        GetTemplate(nameof(DevSpec.Template.Avatar), templates, text);
                    };
                    break;

                case nameof(DevSpec.Group.Room):
                    GetTemplate(nameof(DevSpec.Template.PirateFlag), templates, text);
                    GetTemplate(nameof(DevSpec.Template.PageProxy), templates, text);
                    Don.t = () => {
                        GetTemplate(nameof(DevSpec.Template.Landmark), templates, text);
                    };
                    break;

                case nameof(DevSpec.Group.AvatarTheatre):
                    GetTemplate(nameof(DevSpec.Template.TheatreScreenplay), templates, text);
                    break;

                case nameof(DevSpec.Group.WaterResourceTest):
                    GetTemplate(nameof(DevSpec.Template.WaterBottle), templates, text);
                    Don.t = () => {
                        GetTemplate(nameof(DevSpec.Template.WaterCan), templates, text);
                        GetTemplate(nameof(DevSpec.Template.WaterSink), templates, text);
                        GetTemplate(nameof(DevSpec.Template.PottedPlant), templates, text);
                        GetTemplate(nameof(DevSpec.Template.BioWaste), templates, text);
                    };
                    break;

                default:
                    GetTemplate(name, templates, text);
                    break;
            }
        }

        public static void GetTemplate(string name, DevSpec.TemplateCollection templates, DevSpec.TextCollection text)
        {
            text[DevSpec.de][$"ItemProperty.{Pid.Label}"] = "Name";
            text[DevSpec.en][$"ItemProperty.{Pid.Label}"] = "Name";

            text[DevSpec.de][$"ItemProperty.{Pid.RezableAspect}"] = "Ablegbar";
            text[DevSpec.en][$"ItemProperty.{Pid.RezableAspect}"] = "Droppable";

            PropertySet props = null;

            switch (name) {
                //case nameof(DevSpec.Group.ALL):
                //    break;

                case nameof(DevSpec.Template.Dummy):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Dummy",
                        [Pid.Icon32Url] = PropertyFilter.ItemBase + "Default/icon32.png",
                        [Pid.Image100Url] = PropertyFilter.ItemBase + "Default/image100.png",
                    };
                    break;

                case nameof(DevSpec.Template.Admin): {
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Admin",
                        [Pid.Icon32Url] = PropertyFilter.ItemBase + "Admin/icon32.png",
                        [Pid.Image100Url] = PropertyFilter.ItemBase + "Admin/image100.jpg",
                        [Pid.RoleAspect] = true,
                        [Pid.RoleUserRoles] = ValueList.From(EnumUtil.GetEnumValues<Property.Value.UserRoles>().Where(role => role <= Property.Value.UserRoles.Admin).Select(role => role.ToString())),
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                    break;
                }

                case nameof(DevSpec.Template.CodeReviewer): {
                    Don.t = () => {
                        props = new PropertySet {
                            [Pid.Name] = name,
                            [Pid.Label] = "CodeReviewer",
                            [Pid.Icon32Url] = PropertyFilter.ItemBase + "Admin/icon32.png",
                            [Pid.Image100Url] = PropertyFilter.ItemBase + "Admin/image100.jpg",
                            [Pid.RoleAspect] = true,
                            [Pid.RoleUserRoles] = ValueList.From(EnumUtil.GetEnumValues<Property.Value.UserRoles>().Where(role => role <= Property.Value.UserRoles.CodeReview).Select(role => role.ToString())),
                        };
                        text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin mit Rechten zur Softwareprüfung";
                        text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin with code review access";
                    };
                    break;
                }

                case nameof(DevSpec.Template.PirateFlag):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "PirateFlag",
                        [Pid.Icon32Url] = PropertyFilter.ItemBase + "PirateFlag/icon32.png",
                        [Pid.Image100Url] = PropertyFilter.ItemBase + "PirateFlag/image100.png",
                        [Pid.AnimationsUrl] = PropertyFilter.ItemBase + "PirateFlag/animations.xml",
                        [Pid.PageClaimAspect] = true,
                        [Pid.RezableAspect] = true,
                        [Pid.RezzableProxyTemplate] = nameof(DevSpec.Template.PageProxy),
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Piratenflagge";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Pirate Flag";
                    break;

                case nameof(DevSpec.Template.PageProxy):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "PageProxy",
                        [Pid.Icon32Url] = PropertyFilter.ItemBase + "PageProxy/icon32.png",
                        [Pid.Image100Url] = PropertyFilter.ItemBase + "PageProxy/image100.png",
                        [Pid.RezzableProxyAspect] = true,
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Webseitenbesitz";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Page Claim";
                    break;

                case nameof(DevSpec.Template.TheatreScreenplay):
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
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Theater Drehbuch";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Theatre Screenplay";
                    break;

                case nameof(DevSpec.Template.WaterBottle):
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
                        [Pid.Actions] = ValueMap.From(new Dictionary<string, string> { ["ApplyTo"] = nameof(Applier.Action.Apply), ["GetWater"] = nameof(Extractor.Action.Extract), ["PutWater"] = nameof(Injector.Action.Inject) }),
                        [Pid.Stats] = ValueList.From(new[] { Pid.WaterLevel.ToString() }),
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Wasserflasche";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Water Bottle";
                    break;

                default:
                    throw new Exception($"No template for name={name}");
            }

            templates.Add(name, props);
        }

        public static List<string> GetTemplateNames(string name)
        {
            var result = new List<string>();

            var translations = new DevSpec.TextCollection();
            var templates = new DevSpec.TemplateCollection();

            GetTemplates(name, templates, translations);

            foreach (var pair in templates) {
                if (pair.Value != null) {
                    result.Add(pair.Key);
                }
            }

            return result;
        }

    }
}
