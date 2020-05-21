using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using nine3q.GrainInterfaces;
using nine3q.Items;
using nine3q.Tools;
using Orleans;

namespace nine3q.Web
{
    public class ItemCommandline: Commandline, ICommandline
    {
        public IClusterClient GrainClient { get; set; }

        public enum MyRole { Content, LeadContent, SecurityAdmin }

        enum Fn
        {
            //Admin_TokenLogon,
            //Admin_TokenLogoff,
            //Admin_CreateRole,
            Admin_Environment,
            Admin_Process,
            //Admin_Request,

            //Inventory_Statistics,
            Inventory_Items,
            Inventory_DeleteAll,
            Inventory_DeleteStorage,
            Inventory_Deactivate,
            Inventory_Reload,

            Item_Create,
            Item_Delete,
            Item_ByName,
            Item_SetProperties,
            Item_GetProperties,
            Item_DeleteProperties,
            //Item_Action,
            Item_AddToContainer,
            Item_RemoveFromContainer,
            Item_Transfer,
            //Translation_Set,
            //Translation_Get,
            //Translation_Unset,
            //Translation_de,
            //Translation_en,

            //Content_Groups,
            //Content_Templates,
            //Content_ShowTemplates,
            //Content_CreateTemplates,

            //User_Statistics,
            //User_SetCustomized,
            //User_ReceiveFromFrontend,
            //User_ReceiveFromRoom,
            //User_Deactivate,
            //User_DeleteStorage,
            //Room_Statistics,
            //Room_ReceiveFromUser,
            //Room_Deactivate,
            //Room_DeleteStorage,
        }

        public ItemCommandline(string path): base(path)
        {

            Handlers.Add("Dev_Item", new Handler { Name = "Dev_Item", Function = Dev_Item, Role = Role.Developer.ToString(), Arguments = new ArgumentDescriptionList { ["ID"] = "Item ID" } });

            //Handlers.Add(Fn.Admin_TokenLogon.ToString(), new Handler { Name = Fn.Admin_TokenLogon.ToString(), Function = Admin_TokenLogon, Role = Role.Public.ToString(), Description = "Log in as admin with token (for system bootstrap)", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Token"] = "Secret", } });
            //Handlers.Add(Fn.Admin_TokenLogoff.ToString(), new Handler { Name = Fn.Admin_TokenLogoff.ToString(), Function = Admin_TokenLogoff, Role = Role.Public.ToString(), Description = "Log out as admin", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { } });
            //Handlers.Add(Fn.Admin_CreateRole.ToString(), new Handler { Name = Fn.Admin_CreateRole.ToString(), Function = Admin_CreateRole, Role = Role.Public.ToString(), Description = "Create role item for accessing user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Secret", } });
            Handlers.Add(Fn.Admin_Environment.ToString(), new Handler { Name = Fn.Admin_Environment.ToString(), Function = Admin_Environment, Role = Role.Public.ToString(), ImmediateExecute = true, Description = "Show environment variables", });
            Handlers.Add(Fn.Admin_Process.ToString(), new Handler { Name = Fn.Admin_Process.ToString(), Function = Admin_Process, Role = Role.Public.ToString(), ImmediateExecute = true, Description = "Show process info", });
            //Handlers.Add(Fn.Admin_Request.ToString(), new Handler { Name = Fn.Admin_Request.ToString(), Function = Admin_Request, Role = Role.Public.ToString(), Description = "Show HTTPrequest info", });

            //Handlers.Add(Fn.Inventory_Statistics.ToString(), new Handler { Name = Fn.Inventory_Statistics.ToString(), Function = Inventory_Statistics, Role = Role.Admin.ToString(), Description = "Show statistics", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(Fn.Inventory_Items.ToString(), new Handler { Name = Fn.Inventory_Items.ToString(), Function = Inventory_GetItems, Role = Role.Admin.ToString(), Description = "Get all item IDs of inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(Fn.Inventory_DeleteAll.ToString(), new Handler { Name = Fn.Inventory_DeleteAll.ToString(), Function = Inventory_DeleteAllItems, Role = Role.Admin.ToString(), Description = "Delete all item from inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(Fn.Inventory_DeleteStorage.ToString(), new Handler { Name = Fn.Inventory_DeleteStorage.ToString(), Function = Inventory_DeletePermanentStorage, Role = Role.Admin.ToString(), Description = "Delete permanent storage of inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(Fn.Inventory_Deactivate.ToString(), new Handler { Name = Fn.Inventory_Deactivate.ToString(), Function = Inventory_Deactivate, Role = Role.Admin.ToString(), Description = "Deactivate inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(Fn.Inventory_Reload.ToString(), new Handler { Name = Fn.Inventory_Reload.ToString(), Function = Inventory_Reload, Role = Role.Admin.ToString(), Description = "Reload inventory data", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
           
            Handlers.Add(Fn.Item_Create.ToString(), new Handler { Name = Fn.Item_Create.ToString(), Function = Inventory_CreateItem, Role = Role.Admin.ToString(), Description = "Add item to inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["Properties"] = "Item properties as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(Fn.Item_Delete.ToString(), new Handler { Name = Fn.Item_Delete.ToString(), Function = Inventory_DeleteItem, Role = Role.Admin.ToString(), Description = "Delete item", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", } });
            Handlers.Add(Fn.Item_ByName.ToString(), new Handler { Name = Fn.Item_ByName.ToString(), Function = Inventory_GetItemByName, Role = Role.Admin.ToString(), Description = "Get item with given name", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["Name"] = "Item name", } });
            Handlers.Add(Fn.Item_SetProperties.ToString(), new Handler { Name = Fn.Item_SetProperties.ToString(), Function = Inventory_SetItemProperties, Role = Role.Admin.ToString(), Description = "Set (some or all) item properties", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["Properties"] = "Item properties as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(Fn.Item_GetProperties.ToString(), new Handler { Name = Fn.Item_GetProperties.ToString(), Function = Inventory_GetItemProperties, Role = Role.Admin.ToString(), Description = "Show item", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["Format"] = "Output format [table|json] (optional, default:table)", } });
            Handlers.Add(Fn.Item_DeleteProperties.ToString(), new Handler { Name = Fn.Item_DeleteProperties.ToString(), Function = Inventory_DeleteItemProperties, Role = Role.Admin.ToString(), Description = "Delete one or more properties", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["PropertyName"] = "Property (on or more)", } });
            //Handlers.Add(Fn.Item_Action.ToString(), new Handler { Name = Fn.Item_Action.ToString(), Function = Inventory_ExecuteItemAction, Role = Role.Admin.ToString(), Description = "Execute item aspect action", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["Action"] = "Action verb", ["Properties"] = "Action arguments as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(Fn.Item_AddToContainer.ToString(), new Handler { Name = Fn.Item_AddToContainer.ToString(), Function = Inventory_AddChildToContainer, Role = Role.Admin.ToString(), Description = "Make item a child of the container", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["ContainerId"] = "Container item ID", } });
            Handlers.Add(Fn.Item_RemoveFromContainer.ToString(), new Handler { Name = Fn.Item_RemoveFromContainer.ToString(), Function = Inventory_RemoveChildFromContainer, Role = Role.Admin.ToString(), Description = "Remove item from container", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["ContainerId"] = "Container item ID", } });
            Handlers.Add(Fn.Item_Transfer.ToString(), new Handler { Name = Fn.Item_Transfer.ToString(), Function = Inventory_TransferItem, Role = Role.Admin.ToString(), Description = "Transfer item between inventories including children", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["ItemId"] = "Item ID", ["SourceInventory"] = "Source inventory name", ["DestinationInventory"] = "Destination inventory name", ["DestinationContainer"] = "Destination container name", } });

            //Handlers.Add(Fn.Translation_Set.ToString(), new Handler { Name = Fn.Translation_Set.ToString(), Function = Translation_Set, Role = Role.Admin.ToString(), Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", ["Translated"] = "Translated text (omitting context)", } });
            //Handlers.Add(Fn.Translation_Get.ToString(), new Handler { Name = Fn.Translation_Get.ToString(), Function = Translation_Get, Role = Role.Admin.ToString(), Description = "Show translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", } });
            //Handlers.Add(Fn.Translation_Unset.ToString(), new Handler { Name = Fn.Translation_Unset.ToString(), Function = Translation_Unset, Role = Role.Admin.ToString(), Description = "Delete translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", } });
            //Handlers.Add(Fn.Translation_de.ToString(), new Handler { Name = Fn.Translation_de.ToString(), Function = Translation_Set_de, Role = Role.Admin.ToString(), Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Translated"] = "Translated text (omitting context)", } });
            //Handlers.Add(Fn.Translation_en.ToString(), new Handler { Name = Fn.Translation_en.ToString(), Function = Translation_Set_en, Role = Role.Admin.ToString(), Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Translated"] = "Translated text (omitting context)", } });

            //Handlers.Add(Fn.Content_Groups.ToString(), new Handler { Name = Fn.Content_Groups.ToString(), Function = Content_Groups, Role = MyRole.Content.ToString(), Description = "List available template groups" });
            //Handlers.Add(Fn.Content_Templates.ToString(), new Handler { Name = Fn.Content_Templates.ToString(), Function = Content_Templates, Role = MyRole.Content.ToString(), Description = "List available templates in group", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Name"] = "Group name", } });
            //Handlers.Add(Fn.Content_CreateTemplates.ToString(), new Handler { Name = Fn.Content_CreateTemplates.ToString(), Function = Content_CreateTemplates, Role = MyRole.LeadContent.ToString(), Description = "Create or update template(s)" });
            //Handlers.Add(Fn.Content_ShowTemplates.ToString(), new Handler { Name = Fn.Content_ShowTemplates.ToString(), Function = Content_ShowTemplates, Role = MyRole.Content.ToString(), Description = "Get all item IDs of templates inventory" });

            //Handlers.Add(Fn.User_Statistics.ToString(), new Handler { Name = Fn.User_Statistics.ToString(), Function = User_Statistics, Role = Role.Admin.ToString(), Description = "Show user statistics", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", } });
            //Handlers.Add(Fn.User_SetCustomized.ToString(), new Handler { Name = Fn.User_SetCustomized.ToString(), Function = User_SetCustomized, Role = MyRole.SecurityAdmin.ToString(), Description = "Make user persistent", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", } });
            //Handlers.Add(Fn.User_ReceiveFromFrontend.ToString(), new Handler { Name = Fn.User_ReceiveFromFrontend.ToString(), Function = User_ReceiveFromFrontend, Role = MyRole.SecurityAdmin.ToString(), Description = "Send data from frontend to user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", ["Data"] = "Protocol data", } });
            //Handlers.Add(Fn.User_ReceiveFromRoom.ToString(), new Handler { Name = Fn.User_ReceiveFromRoom.ToString(), Function = User_ReceiveFromRoom, Role = MyRole.SecurityAdmin.ToString(), Description = "Send data from roomto user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", ["Data"] = "Protocol data", } });
            //Handlers.Add(Fn.User_Deactivate.ToString(), new Handler { Name = Fn.User_Deactivate.ToString(), Function = User_Deactivate, Role = MyRole.SecurityAdmin.ToString(), Description = "Deactivate user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", } });

            //Handlers.Add(Fn.Room_Statistics.ToString(), new Handler { Name = Fn.Room_Statistics.ToString(), Function = Room_Statistics, Role = Role.Admin.ToString(), Description = "Show room statistics", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", } });
            //Handlers.Add(Fn.Room_ReceiveFromUser.ToString(), new Handler { Name = Fn.Room_ReceiveFromUser.ToString(), Function = Room_ReceiveFromUser, Role = MyRole.SecurityAdmin.ToString(), Description = "Send data to room", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", ["Data"] = "Protocol data", } });
            //Handlers.Add(Fn.Room_Deactivate.ToString(), new Handler { Name = Fn.Room_Deactivate.ToString(), Function = Room_Deactivate, Role = MyRole.SecurityAdmin.ToString(), Description = "Deactivate room", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", } });
            //Handlers.Add(Fn.Room_DeleteStorage.ToString(), new Handler { Name = Fn.Room_DeleteStorage.ToString(), Function = Room_DeleteStorage, Role = MyRole.SecurityAdmin.ToString(), Description = "Delete permanent storage of room", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", } });

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
        //        tmplName = Content.TemplateName[Content.Template.GodMode];
        //    }
        //    if (string.IsNullOrEmpty(tmplName)) { return "Unknown key"; }
            
        //    var tmpl = GrainClient.GetGrain<IInventory>(InventoryService.TemplatesInventoryName);
        //    var tmplId = tmpl.GetItemByName(tmplName).Result;
        //    if (tmplId == ItemId.NoItem) { return $"Missing template={tmplName}"; }
        //    var inv = GrainClient.GetGrain<IInventory>(userName);
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    user.Prepare().Wait();
        //    var roleId = inv.CreateItem(new PropertySet { [Pid.TemplateName] = tmplName }).Result;
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

        #region Inventory

        class ItemReference : IComparable
        {
            public string Inventory { get; protected set; }
            public long Item { get; protected set; }

            public ItemReference(string inventoryName, long itemId)
            {
                Inventory = inventoryName;
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
            return ShowItemLink(itemRef.Inventory, itemRef.Item);
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
            if (itemRef.Item == null) { throw new Exception("returned ItemId is (null)"); }
            return itemRef.Item.ToString();
        }

        object Dev_Item(Arglist args)
        {
            return new ItemReference("Inventory", long.Parse(args.Get("ID")));
        }

        //object Inventory_Statistics(Arglist args)
        //{
        //    args.Next("cmd");
        //    var inventoryName = args.Next("Inventory");
            
        //    var inv = GrainClient.GetGrain<IInventory>(inventoryName);
        //    var stats = inv.GetStatistics().Result;
        //    var table = new Table();
        //    foreach (var pair in stats) {
        //        table.Grid.Add(new Table.Row() {
        //            pair.Key,
        //            pair.Value,
        //        });
        //    }
        //    return table;
        //}

        object Inventory_CreateItem(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var props = GetPropertySetFromNextArgs(args);
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var itemId = inv.CreateItem(props).Result;
            return new ItemReference(inventoryName, itemId);
        }

        object Inventory_DeleteItem(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var ids = inv.GetItemIds().Result;
            var deleted = inv.DeleteItem(itemId).Result;
            var newIds = inv.GetItemIds().Result;
            return $"Deleted {(ids.Count - newIds.Count)} from {ShowInventoryLink(inventoryName)}";
        }

        object Inventory_SetItemProperties(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            var props = GetPropertySetFromNextArgs(args);
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            inv.SetItemProperties(itemId, props).Wait();
            return new ItemReference(inventoryName, itemId);
        }

        enum Inventory_Result_format { table, json }

        string Inventory_GetItemProperties_FormatValue(string inventoryName, Pid pid, object value)
        {
            var type = Property.Get(pid).Type;
            var s = Property.ToString(type, value);
            s = System.Net.WebUtility.HtmlEncode(s);
            if (pid == Pid.TemplateName) {
                inventoryName = GrainInterfaces.InventoryService.TemplatesInventoryName;
            }
            switch (Property.Get(pid).Type) {
                case Property.Type.Item:
                    s = ShowItemLink(inventoryName, (long)value);
                    break;
                case Property.Type.ItemSet:
                    var idList = (value as ItemIdSet).ToList();
                    idList.Sort();
                    s = string.Join(" ", idList.ConvertAll(x => ShowItemLink(inventoryName, x)));
                    break;
            }
            switch (Property.Get(pid).Use) {
                case Property.Use.Url:
                    s = $"<a href='{PropertyFilter.Url(s)}' target='_new'>{System.Net.WebUtility.HtmlEncode(s)}</a>";
                    break;
                case Property.Use.ImageUrl:
                    s = $"{s} <img src='{PropertyFilter.Url(s)}' />";
                    break;
            }
            return s;
        }

        object Inventory_GetItemProperties(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            var format = args.Next("Format", Inventory_Result_format.table.ToString());
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var props = inv.GetItemProperties(itemId, PidList.All).Result;
            var nativeProps = inv.GetItemProperties(itemId, PidList.All, native: true).Result;
            var templateProps = new PropertySet();
            var templateName = props.GetString(Pid.TemplateName);
            var templateUnavailable = false;
            if (!string.IsNullOrEmpty(templateName)) {
                try {
                    var templateInv = GrainClient.GetGrain<IInventory>(GrainInterfaces.InventoryService.TemplatesInventoryName);
                    var templateId = templateInv.GetItemByName(templateName).Result;
                    templateProps = templateInv.GetItemProperties(templateId, PidList.All).Result;
                } catch (Exception) {
                    templateUnavailable = true;
                }
            }

            object result = null;
            if (format == Inventory_Result_format.table.ToString()) {
                var table = new Table();

                table.Grid.Add(new Table.Row() {
                    "Property",
                    "Value",
                    "Native",
                    "Template",
                    CommandLink(Fn.Item_Delete.ToString(), new[] { inventoryName, itemId.ToString() }, "DELETE-ITEM"),
                    CommandLink(Fn.Item_GetProperties.ToString(), new[] { inventoryName, itemId.ToString(), "json" }, "JSON")
                });

                foreach (var pair in props) {
                    var pid = pair.Key;
                    var type = Property.Get(pid).Type;
                    var value = pair.Value;

                    table.Grid.Add(new Table.Row() {
                        pid.ToString(),

                        Inventory_GetItemProperties_FormatValue(inventoryName, pid, value),

                        nativeProps.ContainsKey(pid) ? Inventory_GetItemProperties_FormatValue(inventoryName, pid, nativeProps[pid]) : "",

                        (pid == Pid.TemplateName && templateUnavailable) ? "(unavailable)" :  templateProps.ContainsKey(pid) ? Inventory_GetItemProperties_FormatValue(GrainInterfaces.InventoryService.TemplatesInventoryName, pid, templateProps[pid]) : "",

                        nativeProps.ContainsKey(pid) && pid != Pid.Id?
                            CommandLink(Fn.Item_DeleteProperties.ToString(), new[] { inventoryName, itemId.ToString(), pid.ToString() }, "Delete") : "" ,

                        pid != Pid.Id?
                            CommandPrepareLink(Fn.Item_SetProperties.ToString(), new[] { "\"" + inventoryName + "\"", itemId.ToString(), pid.ToString() + "=\"" + FormatAsArgument(value) + "\"" }, "Set") : "",
                    });
                }

                table.Grid.Add(new Table.Row() {
                    "",
                    "",
                    "",
                    "",
                    CommandPrepareLink(Fn.Item_SetProperties.ToString(), new[] { inventoryName, itemId.ToString(), "Property=Value" }, "Add"),
                    "",
                });

                table.Options[Table.Option.TableHeader] = "yes";
                result = table;
            } else {
                var node = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
                foreach (var pair in props) {
                    JsonPath.Node propNode = null;
                    switch (Property.Get(pair.Key).Type) {
                        case Property.Type.Int:
                            propNode = new JsonPath.Node(JsonPath.Node.Type.Int, (long)pair.Value);
                            break;
                        case Property.Type.Float:
                            propNode = new JsonPath.Node(JsonPath.Node.Type.Float, (double)pair.Value);
                            break;
                        case Property.Type.Bool:
                            propNode = new JsonPath.Node(JsonPath.Node.Type.Bool, (bool)pair.Value);
                            break;
                        case Property.Type.Item:
                            propNode = new JsonPath.Node(JsonPath.Node.Type.Int, (long)pair.Value);
                            break;
                        default:
                            propNode = new JsonPath.Node(JsonPath.Node.Type.String, pair.Value.ToString());
                            break;
                    }
                    node.AsDictionary.Add(pair.Key.ToString(), propNode);
                }
                result = node.ToString();
            }

            return result;
        }

        object Inventory_DeleteItemProperties(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));

            var pids = new PidList();
            var arg = "";
            do {
                arg = args.Next("PropertyName", "");
                if (arg != "") {
                    pids.Add(Property.Get(arg).Id);
                }
            } while (arg != "");

            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var deleted = inv.DeleteItemProperties(itemId, pids).Result;

            return "Deleted " + deleted + " properties from item " + ShowItemLink(inventoryName, itemId);
        }

        object Inventory_GetItems(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var ids = inv.GetItemIds().Result;
            return ids.ToList().ConvertAll(id => new ItemReference(inventoryName, id));
        }

        object Inventory_AddChildToContainer(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            var containerId = long.Parse(args.Next("Container ID"));
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            inv.AddChildToContainer(itemId, containerId, 0).Wait();
            return new ItemReference(inventoryName, itemId);
        }

        object Inventory_RemoveChildFromContainer(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            var containerId = long.Parse(args.Next("Container ID"));
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            inv.RemoveChildFromContainer(itemId, containerId).Wait();
            return new ItemReference(inventoryName, itemId);
        }

        object Inventory_TransferItem(Arglist args)
        {
            args.Next("cmd");
            var itemId = long.Parse(args.Next("item ID"));
            var sourceInventory = args.Next("source inventory");
            var destinationInventory = args.Next("destination inventory");
            var destinationContainer = args.Next("destination container name", "");
            

            var source = GrainClient.GetGrain<IInventory>(sourceInventory);
            var dest = GrainClient.GetGrain<IInventory>(destinationInventory);
            var destinationContainerId = ItemId.NoItem;
            if (destinationContainer != "") {
                destinationContainerId = dest.GetItemByName(destinationContainer).Result;
                if (destinationContainerId == ItemId.NoItem) {
                    throw new Exception("No container=" + destinationContainer);
                }
            }
            var sourceId = itemId;
            var destId = ItemId.NoItem;
            ItemIdMap map;
            try {
                var transfer = source.BeginItemTransfer(sourceId).Result;
                if (transfer.Count == 0) {
                    throw new Exception("BeginItemTransfer: no data");
                }
                map = dest.ReceiveItemTransfer(itemId, destinationContainerId, 0, transfer, new PropertySet(), new PidList()).Result;
                destId = map[itemId];
                dest.EndItemTransfer(destId).Wait();
                source.EndItemTransfer(sourceId).Wait();
            } catch (Exception ex) {
                dest.CancelItemTransfer(destId).Wait();
                source.CancelItemTransfer(sourceId).Wait();
                throw new Exception("Transfer failed: " + string.Join(" ", ex.GetMessages()));
            }

            return ""
                + "from="
                + ShowInventoryLink(sourceInventory)
                + " to="
                + ShowInventoryLink(destinationInventory)
                + " item="
                + ShowItemLink(destinationInventory, destId)
                + " with="
                + string.Join(" ", map.Values.ToList().ConvertAll(id => ShowItemLink(destinationInventory, id)));
        }

        object Inventory_DeleteAllItems(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var ids = inv.GetItemIds().Result;
            var deleted = 0;
            foreach (var id in ids) {
                deleted += inv.DeleteItem(id).Result ? 1 : 0;
            }
            var newIds = inv.GetItemIds().Result;
            return "Deleted " + (ids.Count - newIds.Count) + " from " + ShowInventoryLink(inventoryName);
        }

        object Inventory_GetItemByName(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemName = args.Next("item name");
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var itemId = inv.GetItemByName(itemName).Result;
            return new ItemReference(inventoryName, itemId);
        }

        //object Inventory_ExecuteItemAction(Arglist args)
        //{
        //    args.Next("cmd");
        //    var inventoryName = args.Next("Inventory");
        //    var itemId = long.Parse(args.Next("item ID"));
        //    var actionName = args.Next("Inventory");
        //    var actionArgs = GetPropertySetFromNextArgs(args);
            
        //    var inv = GrainClient.GetGrain<IInventory>(inventoryName);
        //    inv.ExecuteItemAction(itemId, actionName, actionArgs).Wait();
        //    return new ItemReference(inventoryName, itemId);
        //}

        object Inventory_DeletePermanentStorage(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            inv.DeletePersistentStorage().Wait();
            return "OK";
        }

        object Inventory_Deactivate(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            inv.Deactivate().Wait();
            return "OK";
        }

        object Inventory_Reload(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            inv.ReadPersistentStorage().Wait();
            return "OK";
        }

        PropertySet GetPropertySetFromNextArgs(Arglist args)
        {
            var props = new PropertySet();

            var arg = "";
            do {
                arg = args.Next("properties as JSON", "");
                if (arg != "") {
                    if (arg.StartsWith("{")) {
                        var jsonNode = new JsonPath.Node(arg);
                        foreach (var pair in jsonNode.AsDictionary) {
                            props.Add(Property.Get(pair.Key).Id, pair.Value.AsString);
                        }
                    } else {
                        var parts = arg.Split(new[] { '=', ':' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2) {
                            throw new Exception("Parameter needs 2 parts: Property=Value, got arg=" + arg);
                        }
                        props.Add(Property.Get(parts[0]).Id, parts[1]);
                    }
                }
            } while (arg != "");

            return props;
        }

        string ShowInventoryLink(string inventoryName, string text = null)
        {
            return CommandLink(Fn.Inventory_Items.ToString(), new[] { inventoryName }, string.IsNullOrEmpty(text) ? inventoryName : text);
        }

        string ShowItemLink(string inventoryName, long itemId)
        {
            var inv = GrainClient.GetGrain<IInventory>(inventoryName);
            var text = itemId.ToString();
            try {
                var props = inv.GetItemProperties(itemId, new PidList { Pid.Name, Pid.Label }).Result;
                var name = props.GetString(Pid.Name);
                var label = props.GetString(Pid.Label);
                var show = name;
                if (show == "") {
                    show = label;
                }
                text += string.IsNullOrEmpty(show) ? "" : ":" + show;
            } catch (Exception) {
                text += "<img width='14' height='12' title='' alt='' src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAMCAYAAABSgIzaAAAABGdBTUEAAK/INwWK6QAAABl0RVh0U29mdHdhcmUAQWRvYmUgSW1hZ2VSZWFkeXHJZTwAAAF2SURBVHjalJK/S0JRFMe/DxuUzIooh8rScmgInHw0FEQguFmBCE3S3uAgJLXUZBC2+BcELeLgHkLQDwgkEIyGlpbaTMtfJHr73qtPzFo6vA/vnnu/33fuOTxtCT+jTPxkB8i/AU8pIHjLfGRAp/1lnAc8N8CDzN30fACVX8bFgY1nUgeEORwGikW8ZjKYpm5u0LjelxSJD4icAKdVIWBibtY07PK610BqvN+40V18knsi5JNMQkungWYTws+OYzFo1C73l5who2SLXACX0ijD7fGICadTreVeEjhfo2azC1xkirCHISmonJ0pre71CpfDodb1REKZLdTJG26jY1DxAnwZ1WQEAgGh63ovl2d3HMMetZPSsECCnIk8qOVyPd1jPi+ustleXisUlJnXXZk1+lSVfD7RH6uRiIDRoxGhkDKrqR4Ah8fA0Xs8jmG7Ha0yf4F2GxabjacaGqUSWpyuyWpFo1rFWDSKfXo61f4BP1uS728BBgC9quKyFfQlhgAAAABJRU5ErkJggg==' />";
            }
            return CommandLink(Fn.Item_GetProperties.ToString(), new[] { inventoryName, itemId.ToString() }, text);
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
            
        //    var inventoryName = GrainInterfaces.InventoryService.TemplatesInventoryName;
        //    var inv = GrainClient.GetGrain<IContentGenerator>(inventoryName);
        //    var groups = inv.GetGroups().Result;
        //    var s = "";
        //    foreach (var group in groups) {
        //        s += CommandLink(Fn.Content_Templates.ToString(), new[] { group }, group) + " ";
        //    }
        //    return s;
        //}

        //object Content_Templates(Arglist args)
        //{
        //    args.Next("cmd");
        //    var name = args.Next("GroupName");
            
        //    var inventoryName = GrainInterfaces.InventoryService.TemplatesInventoryName;
        //    var inv = GrainClient.GetGrain<IContentGenerator>(inventoryName);
        //    var templates = inv.GetTemplates(name).Result;
        //    var s = CommandLink(Fn.Content_CreateTemplates.ToString(), new[] { name }, "[Create all]") + " ";
        //    foreach (var template in templates) {
        //        s += CommandLink(Fn.Content_CreateTemplates.ToString(), new[] { template }, template) + " ";
        //    }
        //    return s;
        //}

        //private object Content_CreateTemplates(Arglist args)
        //{
        //    args.Next("cmd");
        //    var name = args.Next("template or group name");
            
        //    var inventoryName = GrainInterfaces.InventoryService.TemplatesInventoryName;
        //    var ids = new ItemIdSet(GrainClient.GetGrain<IContentGenerator>(inventoryName).CreateTemplates(name).Result);
        //    return string.Join(" ", ids.ToList().ConvertAll(id => ShowItemLink(inventoryName, id)));
        //}

        //object Content_ShowTemplates(Arglist args)
        //{
            
        //    var inventoryName = GrainInterfaces.InventoryService.TemplatesInventoryName;
        //    var inv = GrainClient.GetGrain<IInventory>(inventoryName);
        //    var ids = inv.GetItemIds().Result;
        //    return string.Join(" ", ids.ToList().ConvertAll(id => ShowItemLink(inventoryName, id)));
        //}

        #endregion

        #region User

        //object User_Statistics(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("User");
            
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    var stats = user.GetStatistics().Result;
        //    var table = new Table();
        //    foreach (var pair in stats) {
        //        table.Grid.Add(new Table.Row() {
        //            pair.Key,
        //            pair.Value,
        //        });
        //    }
        //    return table;
        //}

        //object User_SetCustomized(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("User");
            
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    user.SetCustomized().Wait();
        //    return "ok";
        //}

        //object User_ReceiveFromFrontend(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("User");
        //    var data = args.Next("Data");
            
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    user.ReceiveFromFrontend(data).Wait();
        //    return "ok";
        //}

        //object User_ReceiveFromRoom(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("User");
        //    var data = args.Next("Data");
            
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    user.ReceiveFromRoom(data).Wait();
        //    return "ok";
        //}

        //object User_Deactivate(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("User");
            
        //    var user = GrainClient.GetGrain<IUser>(userName);
        //    user.Deactivate().Wait();
        //    return "ok";
        //}

        #endregion

        #region Room

        //object Room_Statistics(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("Room");
            
        //    var user = GrainClient.GetGrain<IRoom>(userName);
        //    var stats = user.GetStatistics().Result;
        //    var table = new Table();
        //    foreach (var pair in stats) {
        //        table.Grid.Add(new Table.Row() {
        //            pair.Key,
        //            pair.Value,
        //        });
        //    }
        //    return table;
        //}

        //object Room_ReceiveFromUser(Arglist args)
        //{
        //    args.Next("cmd");
        //    var roomName = args.Next("Room");
        //    var data = args.Next("Data");
            
        //    var room = GrainClient.GetGrain<IRoom>(roomName);
        //    room.ReceiveFromUser(data).Wait();
        //    return "ok";
        //}

        //object Room_Deactivate(Arglist args)
        //{
        //    args.Next("cmd");
        //    var userName = args.Next("Room");
            
        //    var user = GrainClient.GetGrain<IRoom>(userName);
        //    user.Deactivate().Wait();
        //    return "ok";
        //}

        //object Room_DeleteStorage(Arglist args)
        //{
        //    args.Next("cmd");
        //    var roomName = args.Next("Room");
            
        //    var room = GrainClient.GetGrain<IRoom>(roomName);
        //    room.DeletePermanentStorage().Wait();
        //    return "ok";
        //}

        #endregion

    }
}
