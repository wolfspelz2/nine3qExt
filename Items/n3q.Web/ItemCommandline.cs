﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orleans;
using n3q.GrainInterfaces;
using n3q.Items;
using n3q.Tools;
using n3q.Frontend;
using n3q.Aspects;

namespace n3q.Web
{
    public class ItemCommandline : Commandline, ICommandline
    {
        public IClusterClient GrainClient { get; set; }

        public ItemStub GetItemStub(string id)
        {
            return new ItemStub(GrainClient, id);
        }

        public enum ItemRole { Content, LeadContent, SecurityAdmin }

        enum Fn
        {
            //Admin_TokenLogon,
            //Admin_TokenLogoff,
            //Admin_CreateRole,
            Admin_Environment,
            Admin_Process,
            //Admin_Request,

            Item_SetProperties,
            Item_GetProperties,
            Item_DeleteProperties,
            Item_Delete,
            Item_AddToContainer,
            Item_RemoveFromContainer,

            //Translation_Set,
            //Translation_Get,
            //Translation_Unset,
            //Translation_de,
            //Translation_en,

            //Content_Groups,
            //Content_Templates,
            //Content_ShowTemplates,
            //Content_CreateTemplates,
        }

        public ItemCommandline(string path) : base(path)
        {

            Handlers.Add("Dev_Item", new Handler { Name = "Dev_Item", Function = Dev_Item, Role = nameof(Role.Developer), Arguments = new ArgumentDescriptionList { ["ID"] = "Item-ID" } });

            //Handlers.Add(nameof(Fn.Admin_TokenLogon), new Handler { Name = nameof(Fn.Admin_TokenLogon), Function = Admin_TokenLogon, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Log in as admin with token (for system bootstrap)", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Token"] = "Secret", } });
            //Handlers.Add(nameof(Fn.Admin_TokenLogoff), new Handler { Name = nameof(Fn.Admin_TokenLogoff), Function = Admin_TokenLogoff, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Log out as admin", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { } });
            //Handlers.Add(nameof(Fn.Admin_CreateRole), new Handler { Name = nameof(Fn.Admin_CreateRole), Function = Admin_CreateRole, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Create role item for accessing user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Secret", } });
            Handlers.Add(nameof(Fn.Admin_Environment), new Handler { Name = nameof(Fn.Admin_Environment), Function = Admin_Environment, Role = nameof(Role.Public), ImmediateExecute = true, Description = "Show environment variables", });
            Handlers.Add(nameof(Fn.Admin_Process), new Handler { Name = nameof(Fn.Admin_Process), Function = Admin_Process, Role = nameof(Role.Public), ImmediateExecute = true, Description = "Show process info", });
            //Handlers.Add(nameof(Fn.Admin_Request), new Handler { Name = nameof(Fn.Admin_Request), Function = Admin_Request, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Show HTTPrequest info", });

            Handlers.Add(nameof(Fn.Item_SetProperties), new Handler { Name = nameof(Fn.Item_SetProperties), Function = Item_SetProperties, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Set (some or all) item properties", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Item"] = "Item-ID", ["Properties"] = "Item properties as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(nameof(Fn.Item_GetProperties), new Handler { Name = nameof(Fn.Item_GetProperties), Function = Item_GetProperties, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show item", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Item"] = "Item-ID", ["Format"] = "Output format [table|json] (optional, default:table)", } });
            Handlers.Add(nameof(Fn.Item_DeleteProperties), new Handler { Name = nameof(Fn.Item_DeleteProperties), Function = Item_DeleteProperties, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete one or more properties", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Item"] = "Item-ID", ["PropertyName"] = "Property (on or more)", } });
            Handlers.Add(nameof(Fn.Item_AddToContainer), new Handler { Name = nameof(Fn.Item_AddToContainer), Function = Item_AddChildToContainer, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Make item a child of the container", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Item"] = "Item-ID", ["Container"] = "Container-ID", } });
            Handlers.Add(nameof(Fn.Item_RemoveFromContainer), new Handler { Name = nameof(Fn.Item_RemoveFromContainer), Function = Item_RemoveChildFromContainer, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Remove item from container", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Item"] = "Item-ID", ["Container"] = "Container-ID", } });
            Handlers.Add(nameof(Fn.Item_Delete), new Handler { Name = nameof(Fn.Item_Delete), Function = Item_Delete, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete item and storage", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Item"] = "Item-ID", ["PropertyName"] = "Property (on or more)", } });

            //Handlers.Add(nameof(Fn.Translation_Set), new Handler { Name = nameof(Fn.Translation_Set), Function = Translation_Set, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", ["Translated"] = "Translated text (omitting context)", } });
            //Handlers.Add(nameof(Fn.Translation_Get), new Handler { Name = nameof(Fn.Translation_Get), Function = Translation_Get, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", } });
            //Handlers.Add(nameof(Fn.Translation_Unset), new Handler { Name = nameof(Fn.Translation_Unset), Function = Translation_Unset, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", } });
            //Handlers.Add(nameof(Fn.Translation_de), new Handler { Name = nameof(Fn.Translation_de), Function = Translation_Set_de, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Translated"] = "Translated text (omitting context)", } });
            //Handlers.Add(nameof(Fn.Translation_en), new Handler { Name = nameof(Fn.Translation_en), Function = Translation_Set_en, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Translated"] = "Translated text (omitting context)", } });

            //Handlers.Add(nameof(Fn.Content_Groups), new Handler { Name = nameof(Fn.Content_Groups), Function = Content_Groups, Role = nameof(ItemRole.Content), ImmediateExecute = true, Description = "List available template groups" });
            //Handlers.Add(nameof(Fn.Content_Templates), new Handler { Name = nameof(Fn.Content_Templates), Function = Content_Templates, Role = nameof(ItemRole.Content), ImmediateExecute = false, Description = "List available templates in group", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Name"] = "Group name", } });
            //Handlers.Add(nameof(Fn.Content_CreateTemplates), new Handler { Name = nameof(Fn.Content_CreateTemplates), Function = Content_CreateTemplates, Role = nameof(ItemRole.LeadContent), ImmediateExecute = false, Description = "Create or update template(s)", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Name"] = "[template or group name]", } });
            //Handlers.Add(nameof(Fn.Content_ShowTemplates), new Handler { Name = nameof(Fn.Content_ShowTemplates), Function = Content_ShowTemplates, Role = nameof(ItemRole.Content), ImmediateExecute = true, Description = "Get all item-IDs of templates inventory" });

            Formatters.Add(FormatItem);
            Formatters.Add(FormatItemList);

            ArgumentFormatters.Add(FormatItemAsArgument);
        }

        #region Admin

        //object Admin_CreateRole(Arglist args)
        //{
        //    args.Next("cmd");
        //    var key = args.Next("Key");
        //    var userName = new MyHttpContextSettings().Get(ContextAccessor.HttpContext)[MyHttpContextSettings.UserIdKey];
        //    if (string.IsNullOrEmpty(userName)) { return "No user"; }
        //    var tmplName = "";
        //    if (key == "jhzui765rdi76tzrdfjgkhiz7645erdfg8754e") {
        //        tmplName = Content.TemplateId[Content.Template.GodMode];
        //    }
        //    if (string.IsNullOrEmpty(tmplName)) { return "Unknown key"; }

        //    var tmpl = GrainClient.GetGrain<IInventory>(InventoryService.TemplatesInventoryName);
        //    var tmplId = tmpl.GetItemByName(tmplName).Result;
        //    if (tmplId == ItemId.NoItem) { return $"Missing template={tmplName}"; }
        //    var inv = GrainClient.GetGrain<IInventory>(userName);
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    user.Prepare().Wait();
        //    var roleId = inv.CreateItem(new PropertySet { [Pid.TemplateId] = tmplName }).Result;
        //    var backpackId = inv.GetItemByName(GrainInterfaces.User.ItemPath.Backpack).Result;
        //    inv.AddChildToContainer(roleId, backpackId, 0).Wait();
        //    user.SetCustomized().Wait();
        //    var roles = user.GetRoles().Result;
        //    return new ItemReference(userName, roleId);
        //}

        //object Admin_TokenLogon(Arglist args)
        //{
        //    args.Next("cmd");
        //    var token = args.Next("Token");
        //    if (token != MyAuthentication.AdminToken) { return "unauthorized"; }
        //    MyAuthentication.SetCookieToken(ContextAccessor.HttpContext, MyAuthentication.AdminToken);
        //    return "Token added";
        //}

        //object Admin_TokenLogoff(Arglist args)
        //{
        //    args.Next("cmd");
        //    MyAuthentication.DeleteCookieToken(ContextAccessor.HttpContext);
        //    return "Token removed";
        //}

        private object Admin_Environment(Commandline.Arglist args)
        {
            var table = new Commandline.Table();

            var env = Environment.GetEnvironmentVariables();
            foreach (var key in env.Keys) {
                table.Grid.Add(new Table.Row { key.ToString(), env[key].ToString() });
            }

            return table;
        }

        //private object Admin_Request(Commandline.Arglist args)
        //{
        //    var table = new Commandline.Table();

        //    table.Grid.Add(new Table.Row { "IsLocal", ContextAccessor.HttpContext.Connection.IsLocal.ToString() });
        //    table.Grid.Add(new Table.Row { "LocalIpAddress", ContextAccessor.HttpContext.Connection.LocalIpAddress?.ToString() });
        //    table.Grid.Add(new Table.Row { "LocalPort", ContextAccessor.HttpContext.Connection.LocalPort.ToString() });
        //    table.Grid.Add(new Table.Row { "RemoteIpAddress", ContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() });
        //    table.Grid.Add(new Table.Row { "RemotePort", ContextAccessor.HttpContext.Connection.RemotePort.ToString() });

        //    foreach (var header in ContextAccessor.HttpContext.Request.Headers) {
        //        table.Grid.Add(new Table.Row { header.Key, header.Value.ToString() });
        //    }
        //    table.Grid.Add(new Table.Row { "IsHttps", ContextAccessor.HttpContext.Request.IsHttps.ToString() });
        //    table.Grid.Add(new Table.Row { "Method", ContextAccessor.HttpContext.Request.Method?.ToString() });
        //    table.Grid.Add(new Table.Row { "ContentLength", ContextAccessor.HttpContext.Request.ContentLength?.ToString() });
        //    table.Grid.Add(new Table.Row { "ContentType", ContextAccessor.HttpContext.Request.ContentType?.ToString() });
        //    foreach (var cookie in ContextAccessor.HttpContext.Request.Cookies) {
        //        table.Grid.Add(new Table.Row { "Cookie-" + cookie.Key, cookie.Value });
        //    }
        //    table.Grid.Add(new Table.Row { "Host", ContextAccessor.HttpContext.Request.Host.ToString() });
        //    table.Grid.Add(new Table.Row { "Path", ContextAccessor.HttpContext.Request.Path.ToString() });
        //    table.Grid.Add(new Table.Row { "Protocol", ContextAccessor.HttpContext.Request.Protocol?.ToString() });
        //    table.Grid.Add(new Table.Row { "Protocol", ContextAccessor.HttpContext.Request.QueryString.ToString() });
        //    table.Grid.Add(new Table.Row { "Scheme", ContextAccessor.HttpContext.Request.Scheme?.ToString() });

        //    return table;
        //}

        private object Admin_Process(Commandline.Arglist args)
        {
            var table = new Commandline.Table();

            table.Grid.Add(new Table.Row { "Commandline", Environment.CommandLine.ToString() });
            table.Grid.Add(new Table.Row { "CurrentDirectory", Environment.CurrentDirectory.ToString() });
            table.Grid.Add(new Table.Row { "CurrentManagedThreadId", Environment.CurrentManagedThreadId.ToString() });
            table.Grid.Add(new Table.Row { "CommandLineArgs", string.Join(" ", Environment.GetCommandLineArgs()) });
            table.Grid.Add(new Table.Row { "Is64BitOperatingSystem", Environment.Is64BitOperatingSystem.ToString() });
            table.Grid.Add(new Table.Row { "Is64BitProcess", Environment.Is64BitProcess.ToString() });
            table.Grid.Add(new Table.Row { "MachineName", Environment.MachineName.ToString() });
            table.Grid.Add(new Table.Row { "OSVersion", Environment.OSVersion.ToString() });
            table.Grid.Add(new Table.Row { "ProcessorCount", Environment.ProcessorCount.ToString() });
            table.Grid.Add(new Table.Row { "UserDomainName", Environment.UserDomainName.ToString() });
            table.Grid.Add(new Table.Row { "UserName", Environment.UserName.ToString() });
            table.Grid.Add(new Table.Row { "Version", Environment.Version.ToString() });
            table.Grid.Add(new Table.Row { "Culture", Thread.CurrentThread.CurrentCulture.Name });
            table.Grid.Add(new Table.Row { "UICulture", Thread.CurrentThread.CurrentUICulture.Name });

            return table;
        }

        #endregion

        #region Item

        class ItemReference : IComparable
        {
            public string Item { get; protected set; }

            public ItemReference(string itemId)
            {
                Item = itemId;
            }

            public int CompareTo(object other)
            {
                if (other is ItemReference) {
                    return Item.CompareTo((other as ItemReference).Item);
                }
                return 0;
            }
        }
        string FormatItem(object o)
        {
            if (!(o is ItemReference)) { return null; }
            var itemRef = o as ItemReference;
            return ShowItemLink(itemRef.Item);
        }

        string FormatItemList(object o)
        {
            if (!(o is List<ItemReference>)) { return null; }
            var itemList = o as List<ItemReference>;
            itemList.Sort();
            return string.Join(" ", itemList.ConvertAll(x => FormatItem(x)));
        }

        string FormatItemAsArgument(object o)
        {
            if (!(o is ItemReference)) { return null; }
            var itemRef = o as ItemReference;
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (itemRef.Item == null) { throw new Exception("returned ItemId is (null)"); }
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            return itemRef.Item.ToString();
        }

        object Dev_Item(Arglist args)
        {
            return new ItemReference(args.Get("ID"));
        }

        object Item_SetProperties(Arglist args)
        {
            args.Next("cmd");
            var itemId = args.Next("Item");
            var props = GetPropertySetFromNextArgs(args);

            var item = GetItemStub(itemId);
            item.WithTransaction(async self => {
                await self.ModifyProperties(props, PidSet.Empty);
            }).Wait();

            return new ItemReference(itemId);
        }

        object Item_GetProperties(Arglist args)
        {
            args.Next("cmd");
            var itemId = args.Next("Item");
            var format = args.Next("Format", Item_Result_format.table.ToString());

            var item = GetItemStub(itemId);
            var props = item.GetProperties(PidSet.All).Result;
            var nativeProps = item.GetProperties(PidSet.All, native: true).Result;
            var templateProps = new PropertySet();
            var templateId = nativeProps.GetString(Pid.TemplateId);
            var templateUnavailable = false;

            if (Has.Value(templateId)) {
                var template = GetItemStub(templateId);
                templateProps = template.GetProperties(PidSet.All).Result;
            }

            object result = null;
            if (format == Item_Result_format.table.ToString()) {
                var table = new Table();

                table.Grid.Add(new Table.Row() {
                    "Property",
                    "Value",
                    "Native",
                    "Template",
                    CommandExecuteLink(Fn.Item_Delete.ToString(), new[] { itemId.ToString() }, "DELETE-ITEM"),
                    CommandExecuteLink(Fn.Item_GetProperties.ToString(), new[] { itemId.ToString(), "json" }, "JSON")
                });

                foreach (var pair in props) {
                    var pid = pair.Key;
                    var value = pair.Value;

                    table.Grid.Add(new Table.Row() {
                        pid.ToString(),

                        Item_GetProperties_FormatValue(pid, value),

                        nativeProps.ContainsKey(pid) ? 
                            Item_GetProperties_FormatValue(pid, nativeProps[pid]) : "",

                        (pid == Pid.TemplateId && templateUnavailable) ?
                            "(unavailable)" :  templateProps.ContainsKey(pid) ?
                                Item_GetProperties_FormatValue(pid, templateProps[pid]) : "",

                        nativeProps.ContainsKey(pid) ?
                            CommandExecuteLink(Fn.Item_DeleteProperties.ToString(), new[] { itemId.ToString(), pid.ToString() }, "Delete") : "" ,

                        CommandInsertLink(Fn.Item_SetProperties.ToString(), new[] { itemId.ToString(), pid.ToString() + "=\"" + FormatAsArgument(value) + "\"" }, "Set"),
                    });
                }

                table.Grid.Add(new Table.Row() {
                    "",
                    "",
                    "",
                    "",
                    CommandInsertLink(Fn.Item_SetProperties.ToString(), new[] { itemId.ToString(), "Property=Value" }, "Add"),
                    "",
                });

                table.Options[Table.Option.TableHeader] = "yes";
                result = table;
            } else {
                var node = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
                foreach (var pair in props) {
                    JsonPath.Node propNode = null;
                    propNode = Property.GetDefinition(pair.Key).Type switch
                    {
                        Property.Type.Int => new JsonPath.Node(JsonPath.Node.Type.Int, (long)pair.Value),
                        Property.Type.Float => new JsonPath.Node(JsonPath.Node.Type.Float, (double)pair.Value),
                        Property.Type.Bool => new JsonPath.Node(JsonPath.Node.Type.Bool, (bool)pair.Value),
                        _ => new JsonPath.Node(JsonPath.Node.Type.String, pair.Value.ToString()),
                    };
                    node.AsDictionary.Add(pair.Key.ToString(), propNode);
                }
                result = node.ToString();
            }

            return result;
        }

        enum Item_Result_format { table, json }

        string Item_GetProperties_FormatValue(Pid pid, PropertyValue value)
        {
            var s = (string)value;
            s = System.Net.WebUtility.HtmlEncode(s);
            switch (Property.GetDefinition(pid).Use) {
                case Property.Use.Item:
                    s = ShowItemLink(value);
                    break;
                case Property.Use.ItemList:
                    var idList = ItemIdList.FromString(value).ToList();
                    idList.Sort();
                    s = string.Join(" ", idList.ConvertAll(x => ShowItemLink(x)));
                    break;
            }
            switch (Property.GetDefinition(pid).Use) {
                case Property.Use.Url:
                    s = $"<a href='{PropertyFilter.Url(s)}' target='_new'>{System.Net.WebUtility.HtmlEncode(s)}</a>";
                    break;
                case Property.Use.ImageUrl:
                    s = $"{s} <img src='{PropertyFilter.Url(s)}' />";
                    break;
            }
            return s;
        }

        object Item_DeleteProperties(Arglist args)
        {
            args.Next("cmd");
            var itemId = args.Next("item-ID");

            var pids = new PidSet();
            var arg = "";
            do {
                arg = args.Next("PropertyName", "");
                if (Has.Value(arg)) {
                    var pid = arg.ToEnum(Pid.Unknown);
                    pids.Add(Property.GetDefinition(pid).Id);
                }
            } while (!string.IsNullOrEmpty(arg));

            var item = GetItemStub(itemId);
            item.WithTransaction(async self => {
                await self.ModifyProperties(PropertySet.Empty, pids);
            }).Wait();

            return "Deleted from " + ShowItemLink(itemId);
        }

        object Item_Delete(Arglist args)
        {
            args.Next("cmd");
            var itemId = args.Next("item-ID");

            var item = GrainClient.GetGrain<IItem>(itemId);
            item.DeletePersistentStorage();
            item.Deactivate();

            return $"Deleted";
        }

        object Item_AddChildToContainer(Arglist args)
        {
            args.Next("cmd");
            var itemId = args.Next("item-ID");
            var containerId = args.Next("container-ID");

            var container = GetItemStub(containerId);
            container.WithTransaction(async self => {
                await self.AsContainer().AddChild(await self.Item(itemId));
            }).Wait();

            return new ItemReference(itemId);
        }

        object Item_RemoveChildFromContainer(Arglist args)
        {
            args.Next("cmd");
            var itemId = args.Next("item-ID");
            var containerId = args.Next("container-ID");

            var container = GetItemStub(containerId);
            container.WithTransaction(async self => {
                await self.AsContainer().RemoveChild(await self.Item(itemId));
            }).Wait();

            return new ItemReference(itemId);
        }

        PropertySet GetPropertySetFromNextArgs(Arglist args)
        {
            var props = new PropertySet();

            var arg = "";
            do {
                arg = args.Next("properties as JSON", "");
                if (!string.IsNullOrEmpty(arg)) {
                    if (arg.StartsWith("{")) {
                        var jsonNode = new JsonPath.Node(arg);
                        foreach (var pair in jsonNode.AsDictionary) {
                            props.Add(Property.GetDefinition(pair.Key.ToEnum(Pid.Unknown)).Id, pair.Value.AsString);
                        }
                    } else {
                        var parts = arg.Split(new[] { '=', ':' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2) {
                            throw new Exception("Parameter needs 2 parts: Property=Value, got arg=" + arg);
                        }
                        props.Add(Property.GetDefinition(parts[0].ToEnum(Pid.Unknown)).Id, parts[1]);
                    }
                }
            } while (!string.IsNullOrEmpty(arg));

            return props;
        }

        string ShowItemLink(string itemId)
        {
            var item = GrainClient.GetGrain<IItem>(itemId);
            var text = itemId.ToString();
            try {
                var props = item.GetProperties(new PidSet { Pid.Name, Pid.Label }).Result;
                var name = props.GetString(Pid.Name);
                var label = props.GetString(Pid.Label);
                var show = name;
                if (string.IsNullOrEmpty(show)) {
                    show = label;
                }
                text += string.IsNullOrEmpty(show) ? "" : ":" + show;
            } catch (Exception) {
                text += "<img width='14' height='12' title='' alt='' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAMCAYAAABSgIzaAAAABGdBTUEAAK/INwWK6QAAABl0RVh0U29mdHdhcmUAQWRvYmUgSW1hZ2VSZWFkeXHJZTwAAAF2SURBVHjalJK/S0JRFMe/DxuUzIooh8rScmgInHw0FEQguFmBCE3S3uAgJLXUZBC2+BcELeLgHkLQDwgkEIyGlpbaTMtfJHr73qtPzFo6vA/vnnu/33fuOTxtCT+jTPxkB8i/AU8pIHjLfGRAp/1lnAc8N8CDzN30fACVX8bFgY1nUgeEORwGikW8ZjKYpm5u0LjelxSJD4icAKdVIWBibtY07PK610BqvN+40V18knsi5JNMQkungWYTws+OYzFo1C73l5who2SLXACX0ijD7fGICadTreVeEjhfo2azC1xkirCHISmonJ0pre71CpfDodb1REKZLdTJG26jY1DxAnwZ1WQEAgGh63ovl2d3HMMetZPSsECCnIk8qOVyPd1jPi+ustleXisUlJnXXZk1+lSVfD7RH6uRiIDRoxGhkDKrqR4Ah8fA0Xs8jmG7Ha0yf4F2GxabjacaGqUSWpyuyWpFo1rFWDSKfXo61f4BP1uS728BBgC9quKyFfQlhgAAAABJRU5ErkJggg==' />";
            }
            return CommandExecuteLink(Fn.Item_GetProperties.ToString(), new[] { itemId }, text);
        }

        #endregion

        #region Translation

        //private object Translation_Set_de(Arglist args)
        //{
        //    return Translation_Set(new Arglist { args.Next("cmd"), args.Next("key"), "de-DE", args.Next("translated") });
        //}

        //private object Translation_Set_en(Arglist args)
        //{
        //    return Translation_Set(new Arglist { args.Next("cmd"), args.Next("key"), "en-US", args.Next("translated") });
        //}

        //private object Translation_Set(Arglist args)
        //{
        //    args.Next("cmd");
        //    var key = args.Next("key");
        //    var lang = args.Next("language");
        //    var translated = args.Next("translated");

        //    var cacheKey = GrainInterfaces.Content.GetTranslationCacheKey(key, lang);
        //    GrainClient.GetGrain<ICachedString>(cacheKey).Set(translated, CachedStringOptions.Infinite, CachedStringOptions.Persistent).Wait();
        //    return GrainClient.GetGrain<ICachedString>(cacheKey).Get().Result;
        //}

        //private object Translation_Get(Arglist args)
        //{
        //    args.Next("cmd");
        //    var key = args.Next("key");
        //    var lang = args.Next("language");

        //    var cacheKey = GrainInterfaces.Content.GetTranslationCacheKey(key, lang);
        //    var translated = GrainClient.GetGrain<ICachedString>(cacheKey).Get().Result;
        //    return translated;
        //}

        //private object Translation_Unset(Arglist args)
        //{
        //    args.Next("cmd");
        //    var key = args.Next("key");
        //    var lang = args.Next("language");

        //    var cacheKey = GrainInterfaces.Content.GetTranslationCacheKey(key, lang);
        //    GrainClient.GetGrain<ICachedString>(cacheKey).Unset().Wait();
        //    return "(ok)";
        //}

        #endregion

        #region Content

        //object Content_Groups(Arglist args)
        //{
        //    args.Next("cmd");

        //    var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
        //    var inv = GrainClient.GetGrain<IContentGenerator>(inventoryName);
        //    var groups = inv.GetGroups().Result;
        //    var s = "";
        //    foreach (var group in groups) {
        //        s += CommandExecuteLink(Fn.Content_Templates.ToString(), new[] { group }, group) + " ";
        //    }
        //    return s;
        //}

        //object Content_Templates(Arglist args)
        //{
        //    args.Next("cmd");
        //    var name = args.Next("GroupName");

        //    var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
        //    var inv = GrainClient.GetGrain<IContentGenerator>(inventoryName);
        //    var templates = inv.GetTemplates(name).Result;
        //    var s = CommandExecuteLink(Fn.Content_CreateTemplates.ToString(), new[] { name }, "[Create all]") + " Create: ";
        //    foreach (var template in templates) {
        //        s += CommandExecuteLink(Fn.Content_CreateTemplates.ToString(), new[] { template }, template) + " ";
        //    }
        //    return s;
        //}

        //private object Content_CreateTemplates(Arglist args)
        //{
        //    args.Next("cmd");
        //    var name = args.Next("template or group name");

        //    var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
        //    var ids = new ItemIdSet(GrainClient.GetGrain<IContentGenerator>(inventoryName).CreateTemplates(name).Result);
        //    return string.Join(" ", ids.ToList().ConvertAll(id => ShowItemLink(inventoryName, id)));
        //}

        //object Content_ShowTemplates(Arglist args)
        //{

        //    var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
        //    var item = GrainClient.GetGrain<IItem>(itemId);
        //    var ids = inv.GetItemIds().Result;
        //    return string.Join(" ", ids.ToList().ConvertAll(id => ShowItemLink(inventoryName, id)));
        //}

        #endregion

    }
}
