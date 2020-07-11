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
                    GetTemplate(nameof(DevSpec.Template.Developer), templates, text);
                    break;

                case nameof(DevSpec.Group.User):
                    //Don.t = () => {
                    //    GetTemplate(nameof(DevSpec.Template.Attributes), templates, text);
                    //    GetTemplate(nameof(DevSpec.Template.Backpack), templates, text);
                    //    GetTemplate(nameof(DevSpec.Template.Nickname), templates, text);
                    //    GetTemplate(nameof(DevSpec.Template.Avatar), templates, text);
                    //GetTemplate(nameof(DevSpec.Template.Trashcan), templates, text);
                    //};
                    GetTemplate(nameof(DevSpec.Template.Inventory), templates, text);
                    GetTemplate(nameof(DevSpec.Template.Recycler), templates, text);
                    GetTemplate(nameof(DevSpec.Template.Settings), templates, text);
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
                    GetTemplate(nameof(DevSpec.Template.TheatreScreenplayDispenser), templates, text);
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

                case nameof(DevSpec.Group.FridaysForFuture):
                    GetTemplate(nameof(DevSpec.Template.PosterHowDareYou), templates, text);
                    GetTemplate(nameof(DevSpec.Template.PosterThereIsNoPlanetB), templates, text);
                    GetTemplate(nameof(DevSpec.Template.PosterWirStreikenBisIhrHandelt), templates, text);
                    GetTemplate(nameof(DevSpec.Template.RallySpeaker), templates, text);
                    GetTemplate(nameof(DevSpec.Template.FieldMapleTree), templates, text);
                    GetTemplate(nameof(DevSpec.Template.MapleTree), templates, text);
                    GetTemplate(nameof(DevSpec.Template.PlatanusOccidentalis), templates, text);
                    GetTemplate(nameof(DevSpec.Template.SmallMapleTree), templates, text);
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
                        [Pid.Width] = 100,
                        [Pid.Height] = 100,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "Default/image.png",
                    };
                    break;

                case nameof(DevSpec.Template.Admin): {
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Admin",
                        [Pid.Width] = 50,
                        [Pid.Height] = 50,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "System/Admin.png",
                        [Pid.DeletableAspect] = false,
                        [Pid.RezableAspect] = false,
                        [Pid.RoleAspect] = true,
                        [Pid.RoleUserRoles] = ValueList.From(EnumUtil.GetEnumValues<Property.Value.UserRoles>().Where(role => role <= Property.Value.UserRoles.Admin).Select(role => role.ToString())),
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin";
                    break;
                }

                case nameof(DevSpec.Template.Developer): {
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Developer",
                        [Pid.DeletableAspect] = false,
                        [Pid.DeveloperAspect] = true,
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Developer";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Developer";
                    break;
                }

                case nameof(DevSpec.Template.CodeReviewer): {
                    Don.t = () => {
                        props = new PropertySet {
                            [Pid.Name] = name,
                            [Pid.Label] = "CodeReviewer",
                            [Pid.Width] = 75,
                            [Pid.Height] = 75,
                            [Pid.ImageUrl] = ItemService.ItemBaseVar + "System/Admin.png",
                            [Pid.DeletableAspect] = false,
                            [Pid.RezableAspect] = false,
                            [Pid.RoleAspect] = true,
                            [Pid.RoleUserRoles] = ValueList.From(EnumUtil.GetEnumValues<Property.Value.UserRoles>().Where(role => role <= Property.Value.UserRoles.CodeReview).Select(role => role.ToString())),
                        };
                        text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin mit Rechten zur Softwareprüfung";
                        text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Admin with code review privilege";
                    };
                    break;
                }

                case nameof(DevSpec.Template.Inventory):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Inventory",
                        [Pid.InventoryAspect] = true,
                        [Pid.ContainerAspect] = true,
                        [Pid.RezableAspect] = false,
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Inventar";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Inventory";
                    break;

                case nameof(DevSpec.Template.Recycler):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Recycler",
                        [Pid.Width] = 32,
                        [Pid.Height] = 32,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "User/Recycler.png",
                        [Pid.DeleterAspect] = true,
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Einstellungen";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Settings";
                    break;

                case nameof(DevSpec.Template.Settings):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "Settings",
                        [Pid.Width] = 32,
                        [Pid.Height] = 32,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "User/Settings.png",
                        [Pid.SettingsAspect] = true,
                        [Pid.RezableAspect] = false,
                        //[Pid.InventoryLeft] = -1,
                        //[Pid.InventoryBottom] = 250,
                        //[Pid.InventoryWidth] = 400,
                        //[Pid.InventoryHeight] = 300,
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Einstellungen";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Settings";
                    break;

                case nameof(DevSpec.Template.PirateFlag):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "PirateFlag",
                        [Pid.Width] = 43,
                        [Pid.Height] = 65,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "PirateFlag/image.png",
                        [Pid.AnimationsUrl] = ItemService.ItemBaseVar + "PirateFlag/animations.xml",
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
                        [Pid.Width] = 40,
                        [Pid.Height] = 65,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "PageProxy/image.png",
                        [Pid.DeletableAspect] = false,
                        [Pid.RezableAspect] = false,
                        [Pid.RezableProxyAspect] = true,
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Webseitenbesitz";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Page Claim";
                    break;

                case nameof(DevSpec.Template.WaterBottle):
                    props = new PropertySet {
                        [Pid.Name] = name,
                        [Pid.Label] = "WaterBottle",
                        [Pid.Width] = 18,
                        [Pid.Height] = 60,
                        [Pid.ImageUrl] = ItemService.ItemBaseVar + "WaterBottle/image.png",
                        [Pid.SourceAspect] = true,
                        [Pid.SourceResource] = Pid.WaterLevel.ToString(),
                        [Pid.SinkAspect] = true,
                        [Pid.SinkResource] = Pid.WaterLevel.ToString(),
                        [Pid.WaterLevel] = 0,
                        [Pid.WaterLevelMax] = 330,
                        [Pid.ExtractorAspect] = true,
                        [Pid.InjectorAspect] = true,
                        [Pid.ApplierAspect] = true,
                        [Pid.Actions] = ValueMap.From(new Dictionary<string, string> { ["ApplyTo"] = nameof(Applier.Apply), ["GetWater"] = nameof(Extractor.Extract), ["PutWater"] = nameof(Injector.Inject) }),
                        [Pid.Stats] = ValueList.From(new[] { Pid.WaterLevel.ToString() }),
                    };
                    text[DevSpec.de][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Wasserflasche";
                    text[DevSpec.en][$"ItemValue{Pid.Label}.{props[Pid.Label]}"] = "Water Bottle";
                    break;

                case nameof(DevSpec.Template.PosterHowDareYou): props = GetImageTemplate(name, text, name, "How Dare You", "How Dare You", 123, 226, "FridaysForFuture/PosterHowDareYou.png"); break;
                case nameof(DevSpec.Template.PosterThereIsNoPlanetB): props = GetImageTemplate(name, text, name, "There is no Planet B", "There is no Planet B", 158, 161, "FridaysForFuture/PosterThereIsNoPlanetB.png"); break;
                case nameof(DevSpec.Template.PosterWirStreikenBisIhrHandelt): props = GetImageTemplate(name, text, name, "Wir streiken bis ihr handelt", "Wir streiken bis ihr handelt", 255, 204, "FridaysForFuture/PosterWirStreikenBisIhrHandelt.png"); break;
                case nameof(DevSpec.Template.FieldMapleTree): props = GetImageTemplate(name, text, name, "Maple tree", "Feldahorn", 156, 200, "Trees/FieldMapleTree.png"); break;
                case nameof(DevSpec.Template.MapleTree): props = GetImageTemplate(name, text, name, "Maple tree", "Ahorn", 174, 250, "Trees/MapleTree.png"); break;
                case nameof(DevSpec.Template.PlatanusOccidentalis): props = GetImageTemplate(name, text, name, "Platane", "Planetree", 136, 300, "Trees/PlatanusOccidentalis.png"); break;
                case nameof(DevSpec.Template.SmallMapleTree): props = GetImageTemplate(name, text, name, "Small maple tree", "kleiner Ahornbaum", 58, 80, "Trees/SmallMapleTree.png"); break;

                case nameof(DevSpec.Template.TheatreScreenplay): {
                    props = GetIframeTemplate(name, text, name, "Theater Drehbuch", "Theatre Screenplay", 44, 64, "TheatreScreenplay/image.png", "https://weblin-theatre.dev.sui.li/iframe.html?context={context}", 400, 500, nameof(Property.Value.IframeFrame.Window));
                    props[Pid.Developer] = "partner-suat-theatre";
                    props[Pid.DocumentAspect] = true;
                    props[Pid.DocumentMaxLength] = 10000;
                }
                break;
                case nameof(DevSpec.Template.TheatreScreenplayDispenser): {
                    props = GetIframeTemplate(name, text, name, "Theater Drehbuch Generator", "Theatre Screenplay Generator", 78, 84, "TheatreScreenplay/TheatreScreenplayDispenser.png", ItemService.ItemIframeVar + name + "?context={context}", 100, 100, nameof(Property.Value.IframeFrame.Popup));
                    props[Pid.DispenserAspect] = true;
                    props[Pid.DispenserTemplate] = nameof(DevSpec.Template.TheatreScreenplay);
                    props[Pid.DispenserMaxAvailable] = 1000L;
                    props[Pid.DispenserAvailable] = 1000L;
                    props[Pid.DispenserCooldownSec] = 10.0D;
                    props[Pid.DispenserLastTime] = 0L;
                }
                break;
                case nameof(DevSpec.Template.RallySpeaker): props = GetIframeTemplate(name, text, name, "Lautsprecher", "Speaker", 75, 80, "FridaysForFuture/RallySpeaker.png", "https://meet.jit.si/{room}", 600, 400, nameof(Property.Value.IframeFrame.Window)); break;

                default:
                    throw new Exception($"No template for name={name}");
            }

            templates.Add(name, props);
        }

        public static PropertySet GetIframeTemplate(string id, DevSpec.TextCollection text, string labelKey, string deLabelText, string enLabelText, int imgWidth, int imgHeight, string relativeImagePath, string iframeUrl, int iframeWidth, int iframeHeight, string frameStyle)
        {
            var props = GetImageTemplate(id, text, labelKey, deLabelText, enLabelText, imgWidth, imgHeight, relativeImagePath);
            props[Pid.IframeAspect] = true;
            props[Pid.IframeUrl] = iframeUrl;
            props[Pid.IframeWidth] = (long)iframeWidth;
            props[Pid.IframeHeight] = (long)iframeHeight;
            props[Pid.IframeResizeable] = true;
            props[Pid.IframeFrame] = frameStyle;
            return props;
        }

        public static PropertySet GetImageTemplate(string id, DevSpec.TextCollection text, string labelKey, string deLabelText, string enLabelText, int imgWidth, int imgHeight, string relativeImagePath)
        {
            text[DevSpec.de][$"ItemValue{Pid.Label}.{labelKey}"] = deLabelText;
            text[DevSpec.en][$"ItemValue{Pid.Label}.{labelKey}"] = enLabelText;

            return new PropertySet {
                [Pid.Name] = id.ToString(),
                [Pid.Label] = labelKey,
                [Pid.Width] = (long)imgWidth,
                [Pid.Height] = (long)imgHeight,
                [Pid.ImageUrl] = ItemService.ItemBaseVar + relativeImagePath,
            };
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
