using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orleans;
using n3q.GrainInterfaces;
using n3q.Items;
using n3q.Tools;
using n3q.Frontend;

namespace n3q.Web
{
    public class ItemCommandline: Commandline, ICommandline
    {
        public IClusterClient GrainClient { get; set; }

        public enum ItemRole { Content, LeadContent, SecurityAdmin }

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
            Inventory_WriteStorage,
            Inventory_ReadStorage,
            Inventory_DeleteStorage,
            Inventory_Deactivate,

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

            Content_Groups,
            Content_Templates,
            Content_ShowTemplates,
            Content_CreateTemplates,

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

            Handlers.Add("Dev_Item", new Handler { Name = "Dev_Item", Function = Dev_Item, Role = nameof(Role.Developer), Arguments = new ArgumentDescriptionList { ["ID"] = "Item ID" } });

            //Handlers.Add(nameof(Fn.Admin_TokenLogon), new Handler { Name = nameof(Fn.Admin_TokenLogon), Function = Admin_TokenLogon, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Log in as admin with token (for system bootstrap)", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Token"] = "Secret", } });
            //Handlers.Add(nameof(Fn.Admin_TokenLogoff), new Handler { Name = nameof(Fn.Admin_TokenLogoff), Function = Admin_TokenLogoff, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Log out as admin", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { } });
            //Handlers.Add(nameof(Fn.Admin_CreateRole), new Handler { Name = nameof(Fn.Admin_CreateRole), Function = Admin_CreateRole, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Create role item for accessing user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Secret", } });
            Handlers.Add(nameof(Fn.Admin_Environment), new Handler { Name = nameof(Fn.Admin_Environment), Function = Admin_Environment, Role = nameof(Role.Public), ImmediateExecute = true, Description = "Show environment variables", });
            Handlers.Add(nameof(Fn.Admin_Process), new Handler { Name = nameof(Fn.Admin_Process), Function = Admin_Process, Role = nameof(Role.Public), ImmediateExecute = true, Description = "Show process info", });
            //Handlers.Add(nameof(Fn.Admin_Request), new Handler { Name = nameof(Fn.Admin_Request), Function = Admin_Request, Role = nameof(Role.Public), ImmediateExecute = false, Description = "Show HTTPrequest info", });

            //Handlers.Add(nameof(Fn.Inventory_Statistics), new Handler { Name = nameof(Fn.Inventory_Statistics), Function = Inventory_Statistics, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show statistics", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(nameof(Fn.Inventory_Items), new Handler { Name = nameof(Fn.Inventory_Items), Function = Inventory_GetItems, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Get all item IDs of inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(nameof(Fn.Inventory_DeleteAll), new Handler { Name = nameof(Fn.Inventory_DeleteAll), Function = Inventory_DeleteAllItems, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete all item from inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(nameof(Fn.Inventory_WriteStorage), new Handler { Name = nameof(Fn.Inventory_WriteStorage), Function = Inventory_WritePersistentStorage, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete permanent storage of inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(nameof(Fn.Inventory_ReadStorage), new Handler { Name = nameof(Fn.Inventory_ReadStorage), Function = Inventory_ReadStorage, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Reload inventory data", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(nameof(Fn.Inventory_DeleteStorage), new Handler { Name = nameof(Fn.Inventory_DeleteStorage), Function = Inventory_DeletePersistentStorage, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete permanent storage of inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
            Handlers.Add(nameof(Fn.Inventory_Deactivate), new Handler { Name = nameof(Fn.Inventory_Deactivate), Function = Inventory_Deactivate, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Deactivate inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", } });
           
            Handlers.Add(nameof(Fn.Item_Create), new Handler { Name = nameof(Fn.Item_Create), Function = Inventory_CreateItem, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add item to inventory", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["Properties"] = "Item properties as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(nameof(Fn.Item_Delete), new Handler { Name = nameof(Fn.Item_Delete), Function = Inventory_DeleteItem, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete item", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", } });
            Handlers.Add(nameof(Fn.Item_ByName), new Handler { Name = nameof(Fn.Item_ByName), Function = Inventory_GetItemByName, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Get item with given name", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["Name"] = "Item name", } });
            Handlers.Add(nameof(Fn.Item_SetProperties), new Handler { Name = nameof(Fn.Item_SetProperties), Function = Inventory_SetItemProperties, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Set (some or all) item properties", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["Properties"] = "Item properties as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(nameof(Fn.Item_GetProperties), new Handler { Name = nameof(Fn.Item_GetProperties), Function = Inventory_GetItemProperties, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show item", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["Format"] = "Output format [table|json] (optional, default:table)", } });
            Handlers.Add(nameof(Fn.Item_DeleteProperties), new Handler { Name = nameof(Fn.Item_DeleteProperties), Function = Inventory_DeleteItemProperties, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete one or more properties", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["PropertyName"] = "Property (on or more)", } });
            //Handlers.Add(nameof(Fn.Item_Action), new Handler { Name = nameof(Fn.Item_Action), Function = Inventory_ExecuteItemAction, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Execute item aspect action", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["Action"] = "Action verb", ["Properties"] = "Action arguments as JSON dictionary or as PropertyName=Value pairs", } });
            Handlers.Add(nameof(Fn.Item_AddToContainer), new Handler { Name = nameof(Fn.Item_AddToContainer), Function = Inventory_AddChildToContainer, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Make item a child of the container", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["ContainerId"] = "Container item ID", } });
            Handlers.Add(nameof(Fn.Item_RemoveFromContainer), new Handler { Name = nameof(Fn.Item_RemoveFromContainer), Function = Inventory_RemoveChildFromContainer, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Remove item from container", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Inventory"] = "Inventory name", ["ItemId"] = "Item ID", ["ContainerId"] = "Container item ID", } });
            Handlers.Add(nameof(Fn.Item_Transfer), new Handler { Name = nameof(Fn.Item_Transfer), Function = Inventory_TransferItem, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Transfer item between inventories including children", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["ItemId"] = "Item ID", ["SourceInventory"] = "Source inventory name", ["DestinationInventory"] = "Destination inventory name", ["DestinationContainer"] = "Destination container name", } });

            //Handlers.Add(nameof(Fn.Translation_Set), new Handler { Name = nameof(Fn.Translation_Set), Function = Translation_Set, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", ["Translated"] = "Translated text (omitting context)", } });
            //Handlers.Add(nameof(Fn.Translation_Get), new Handler { Name = nameof(Fn.Translation_Get), Function = Translation_Get, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", } });
            //Handlers.Add(nameof(Fn.Translation_Unset), new Handler { Name = nameof(Fn.Translation_Unset), Function = Translation_Unset, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Delete translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Language"] = "Language [de_DE|en_US|...]", } });
            //Handlers.Add(nameof(Fn.Translation_de), new Handler { Name = nameof(Fn.Translation_de), Function = Translation_Set_de, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Translated"] = "Translated text (omitting context)", } });
            //Handlers.Add(nameof(Fn.Translation_en), new Handler { Name = nameof(Fn.Translation_en), Function = Translation_Set_en, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Add translation", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Key"] = "Text to translate, format: context.text", ["Translated"] = "Translated text (omitting context)", } });

            Handlers.Add(nameof(Fn.Content_Groups), new Handler { Name = nameof(Fn.Content_Groups), Function = Content_Groups, Role = nameof(ItemRole.Content), ImmediateExecute = true, Description = "List available template groups" });
            Handlers.Add(nameof(Fn.Content_Templates), new Handler { Name = nameof(Fn.Content_Templates), Function = Content_Templates, Role = nameof(ItemRole.Content), ImmediateExecute = false, Description = "List available templates in group", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Name"] = "Group name", } });
            Handlers.Add(nameof(Fn.Content_CreateTemplates), new Handler { Name = nameof(Fn.Content_CreateTemplates), Function = Content_CreateTemplates, Role = nameof(ItemRole.LeadContent), ImmediateExecute = false, Description = "Create or update template(s)", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Name"] = "[template or group name]", } });
            Handlers.Add(nameof(Fn.Content_ShowTemplates), new Handler { Name = nameof(Fn.Content_ShowTemplates), Function = Content_ShowTemplates, Role = nameof(ItemRole.Content), ImmediateExecute = true, Description = "Get all item IDs of templates inventory" });

            //Handlers.Add(nameof(Fn.User_Statistics), new Handler { Name = nameof(Fn.User_Statistics), Function = User_Statistics, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show user statistics", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", } });
            //Handlers.Add(nameof(Fn.User_SetCustomized), new Handler { Name = nameof(Fn.User_SetCustomized), Function = User_SetCustomized, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Make user persistent", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", } });
            //Handlers.Add(nameof(Fn.User_ReceiveFromFrontend), new Handler { Name = nameof(Fn.User_ReceiveFromFrontend), Function = User_ReceiveFromFrontend, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Send data from frontend to user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", ["Data"] = "Protocol data", } });
            //Handlers.Add(nameof(Fn.User_ReceiveFromRoom), new Handler { Name = nameof(Fn.User_ReceiveFromRoom), Function = User_ReceiveFromRoom, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Send data from roomto user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", ["Data"] = "Protocol data", } });
            //Handlers.Add(nameof(Fn.User_Deactivate), new Handler { Name = nameof(Fn.User_Deactivate), Function = User_Deactivate, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Deactivate user", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["User"] = "User name", } });

            //Handlers.Add(nameof(Fn.Room_Statistics), new Handler { Name = nameof(Fn.Room_Statistics), Function = Room_Statistics, Role = nameof(Role.Admin), ImmediateExecute = false, Description = "Show room statistics", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", } });
            //Handlers.Add(nameof(Fn.Room_ReceiveFromUser), new Handler { Name = nameof(Fn.Room_ReceiveFromUser), Function = Room_ReceiveFromUser, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Send data to room", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", ["Data"] = "Protocol data", } });
            //Handlers.Add(nameof(Fn.Room_Deactivate), new Handler { Name = nameof(Fn.Room_Deactivate), Function = Room_Deactivate, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Deactivate room", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", } });
            //Handlers.Add(nameof(Fn.Room_DeleteStorage), new Handler { Name = nameof(Fn.Room_DeleteStorage), Function = Room_DeleteStorage, Role = nameof(MyRoleSecurityAdmin), ImmediateExecute = false, Description = "Delete permanent storage of room", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Room"] = "Room name", } });

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
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (itemRef.Item == null) { throw new Exception("returned ItemId is (null)"); }
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
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
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            var itemId = inv.CreateItem(props).Result;
            return new ItemReference(inventoryName, itemId);
        }

        object Inventory_DeleteItem(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
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
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            inv.SetItemProperties(itemId, props).Wait();
            return new ItemReference(inventoryName, itemId);
        }

        enum Inventory_Result_format { table, json }

        string Inventory_GetItemProperties_FormatValue(string inventoryName, Pid pid, object value)
        {
            var s = Property.ToString(pid, value);
            s = System.Net.WebUtility.HtmlEncode(s);
            if (pid == Pid.TemplateName) {
                inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
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
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            var props = inv.GetItemProperties(itemId, PidSet.All).Result;
            var nativeProps = inv.GetItemProperties(itemId, PidSet.All, native: true).Result;
            var templateProps = new PropertySet();
            var templateName = props.GetString(Pid.TemplateName);
            var templateUnavailable = false;
            if (!string.IsNullOrEmpty(templateName)) {
                try {
                    var templateInv = GrainClient.GetGrain<IItem>(GrainInterfaces.ItemService.TemplatesInventoryName);
                    var templateId = templateInv.GetItemByName(templateName).Result;
                    templateProps = templateInv.GetItemProperties(templateId, PidSet.All).Result;
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
                    CommandExecuteLink(Fn.Item_Delete.ToString(), new[] { inventoryName, itemId.ToString() }, "DELETE-ITEM"),
                    CommandExecuteLink(Fn.Item_GetProperties.ToString(), new[] { inventoryName, itemId.ToString(), "json" }, "JSON")
                });

                foreach (var pair in props) {
                    var pid = pair.Key;
                    var value = Property.Normalize(pid, pair.Value);

                    table.Grid.Add(new Table.Row() {
                        pid.ToString(),

                        Inventory_GetItemProperties_FormatValue(inventoryName, pid, value),

                        nativeProps.ContainsKey(pid) ? Inventory_GetItemProperties_FormatValue(inventoryName, pid, nativeProps[pid]) : "",

                        (pid == Pid.TemplateName && templateUnavailable) ? "(unavailable)" :  templateProps.ContainsKey(pid) ? Inventory_GetItemProperties_FormatValue(GrainInterfaces.ItemService.TemplatesInventoryName, pid, templateProps[pid]) : "",

                        nativeProps.ContainsKey(pid) && pid != Pid.Id?
                            CommandExecuteLink(Fn.Item_DeleteProperties.ToString(), new[] { inventoryName, itemId.ToString(), pid.ToString() }, "Delete") : "" ,

                        pid != Pid.Id?
                            CommandInsertLink(Fn.Item_SetProperties.ToString(), new[] { "\"" + inventoryName + "\"", itemId.ToString(), pid.ToString() + "=\"" + FormatAsArgument(value) + "\"" }, "Set") : "",
                    });
                }

                table.Grid.Add(new Table.Row() {
                    "",
                    "",
                    "",
                    "",
                    CommandInsertLink(Fn.Item_SetProperties.ToString(), new[] { inventoryName, itemId.ToString(), "Property=Value" }, "Add"),
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

            var pids = new PidSet();
            var arg = "";
            do {
                arg = args.Next("PropertyName", "");
                if (!string.IsNullOrEmpty(arg)) {
                    pids.Add(Property.Get(arg).Id);
                }
            } while (!string.IsNullOrEmpty(arg));

            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            var deleted = inv.DeleteItemProperties(itemId, pids).Result;

            return "Deleted " + deleted + " properties from item " + ShowItemLink(inventoryName, itemId);
        }

        object Inventory_GetItems(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            var ids = inv.GetItemIds().Result;
            return ids.ToList().ConvertAll(id => new ItemReference(inventoryName, id));
        }

        object Inventory_AddChildToContainer(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            var containerId = long.Parse(args.Next("Container ID"));
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            inv.AddChildToContainer(itemId, containerId, 0).Wait();
            return new ItemReference(inventoryName, itemId);
        }

        object Inventory_RemoveChildFromContainer(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            var itemId = long.Parse(args.Next("item ID"));
            var containerId = long.Parse(args.Next("Container ID"));
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
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
            

            var source = GrainClient.GetGrain<IItem>(sourceInventory);
            var dest = GrainClient.GetGrain<IItem>(destinationInventory);
            var destinationContainerId = ItemId.NoItem;
            if (!string.IsNullOrEmpty(destinationContainer)) {
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
                map = dest.ReceiveItemTransfer(itemId, destinationContainerId, 0, transfer, new PropertySet(), new PidSet()).Result;
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
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
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
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
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

        object Inventory_WritePersistentStorage(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");

            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            inv.WritePersistentStorage().Wait();
            return "OK";
        }

        object Inventory_DeletePersistentStorage(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");

            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            inv.DeletePersistentStorage().Wait();
            return "OK";
        }

        object Inventory_Deactivate(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            inv.Deactivate().Wait();
            return "OK";
        }

        object Inventory_ReadStorage(Arglist args)
        {
            args.Next("cmd");
            var inventoryName = args.Next("Inventory");
            
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            inv.ReadPersistentStorage().Wait();
            return "OK";
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
            } while (!string.IsNullOrEmpty(arg));

            return props;
        }

        string ShowInventoryLink(string inventoryName, string text = null)
        {
            return CommandExecuteLink(Fn.Inventory_Items.ToString(), new[] { inventoryName }, string.IsNullOrEmpty(text) ? inventoryName : text);
        }

        string ShowItemLink(string inventoryName, long itemId)
        {
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            var text = itemId.ToString();
            try {
                var props = inv.GetItemProperties(itemId, new PidSet { Pid.Name, Pid.Label }).Result;
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
            return CommandExecuteLink(Fn.Item_GetProperties.ToString(), new[] { inventoryName, itemId.ToString() }, text);
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

        object Content_Groups(Arglist args)
        {
            args.Next("cmd");

            var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
            var inv = GrainClient.GetGrain<IContentGenerator>(inventoryName);
            var groups = inv.GetGroups().Result;
            var s = "";
            foreach (var group in groups) {
                s += CommandExecuteLink(Fn.Content_Templates.ToString(), new[] { group }, group) + " ";
            }
            return s;
        }

        object Content_Templates(Arglist args)
        {
            args.Next("cmd");
            var name = args.Next("GroupName");

            var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
            var inv = GrainClient.GetGrain<IContentGenerator>(inventoryName);
            var templates = inv.GetTemplates(name).Result;
            var s = CommandExecuteLink(Fn.Content_CreateTemplates.ToString(), new[] { name }, "[Create all]") + " Create: ";
            foreach (var template in templates) {
                s += CommandExecuteLink(Fn.Content_CreateTemplates.ToString(), new[] { template }, template) + " ";
            }
            return s;
        }

        private object Content_CreateTemplates(Arglist args)
        {
            args.Next("cmd");
            var name = args.Next("template or group name");

            var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
            var ids = new ItemIdSet(GrainClient.GetGrain<IContentGenerator>(inventoryName).CreateTemplates(name).Result);
            return string.Join(" ", ids.ToList().ConvertAll(id => ShowItemLink(inventoryName, id)));
        }

        object Content_ShowTemplates(Arglist args)
        {

            var inventoryName = GrainInterfaces.ItemService.TemplatesInventoryName;
            var inv = GrainClient.GetGrain<IItem>(inventoryName);
            var ids = inv.GetItemIds().Result;
            return string.Join(" ", ids.ToList().ConvertAll(id => ShowItemLink(inventoryName, id)));
        }

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
