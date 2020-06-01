using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Orleans;
using n3q.GrainInterfaces;
using n3q.Tools;
using JsonPath;

namespace n3q.Web
{
    public class CommandSymbols
    {
        public string Delete { get; set; } = "&#10006;";
        public string Up { get; set; } = "&uArr;";
        public string Insert { get; set; } = "&#8628;"; // &#10095; // &#8690;
        public string Execute { get; set; } = "&#10097;&#10097;"; // &#10097;&#10097; // &larrhk;
        public string Save { get; set; } = "&#9733;"; // &#9733; // &#10029;
    }

    public class CommandDetail
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Template { get; set; }
        public string Arguments { get; set; }
        public bool ImmediateExecute { get; set; }
    }

    public class CommandResult
    {
        public string Content { get; set; }
        public string ContentType { get; set; }

        public CommandResult(string content, string contentType)
        {
            Content = content;
            ContentType = contentType;
        }
    }

    public class CommandlineFavorites
    {
        public List<KeyValuePair<string, string>> Favorites { get; set; }
        public CommandSymbols Symbols = new CommandSymbols();

        public CommandlineFavorites(List<KeyValuePair<string, string>> favorites)
        {
            Favorites = favorites;
        }
    }

    public class CommandlineModel : PageModel
    {
        readonly ICommandline _commandline;
        readonly IClusterClient _clusterClient;

        public readonly Dictionary<string, CommandDetail> Commands = new Dictionary<string, CommandDetail>();
        public CommandSymbols Symbols = new CommandSymbols();

        public CommandlineModel(ICommandlineSingletonInstance commandline, IClusterClient clusterClient)
        {
            _commandline = commandline;
            _clusterClient = clusterClient;

            if (_commandline is ItemCommandline itemCommandline) {
                if (itemCommandline.GrainClient == null) {
                    itemCommandline.GrainClient = clusterClient;
                }
            }
        }

        public void OnGet()
        {
            var user = new Commandline.User(User.Claims);

            foreach (var pair in _commandline.GetHandlers()) {
                var handler = pair.Value;
                if (string.IsNullOrEmpty(_commandline.CheckRole(handler, user))) {
                    Commands.Add(pair.Key, new CommandDetail {
                        Name = pair.Key,
                        Description = handler.Description,
                        Template = pair.Key + (pair.Value.Arguments == null ? "" :
                            pair.Value.ArgumentList == Commandline.ArgumentListType.KeyValue ?
                            pair.Value.Arguments.Aggregate(new StringBuilder(), (sb, x) => sb.Append(" " + x.Key + "="), sb => sb.ToString()) :
                            pair.Value.Arguments.Aggregate(new StringBuilder(), (sb, x) => sb.Append(" " + x.Key), sb => sb.ToString())
                            ),
                        Arguments = (pair.Value.Arguments == null ? "" : pair.Value.Arguments.Aggregate(new StringBuilder(), (sb, x) => sb.Append("[" + x.Key + ": " + x.Value + "] "), sb => sb.ToString())),
                        ImmediateExecute = pair.Value.ImmediateExecute,
                    });
                }
            }
        }

        public PartialViewResult OnPostRun(string arg)
        {
            var cmd = arg;
            var user = new Commandline.User(User.Claims);

            try {
                var html = string.IsNullOrEmpty(cmd) ? "" : _commandline.Run(cmd, user);
                return Partial("_CommandlineResult", new CommandResult(html, "text/html"));
            } catch (Exception ex) {
                return Partial("_CommandlineResult", new CommandResult("<pre>" + string.Join(" | ", ex.GetMessages()) + "</pre>", "text/html"));
            }
        }

        public async Task<PartialViewResult> OnPostSaveFavorite(string arg)
        {
            var cmd = arg;
            try {
                var favoritesNode = await ReadFavorites();

                var key = RandomString.Get(10);
                var newNode = new JsonPath.Node(JsonPath.Node.Type.Dictionary);
                newNode.AsDictionary.Add(key, cmd);
                favoritesNode.AsList.Add(newNode);

                var model = await WriteFavorites(favoritesNode);
                return Partial("_CommandlineFavorites", model);
            } catch (Exception ex) {
                return Partial("_CommandlineResult", new CommandResult("<pre>" + string.Join(" | ", ex.GetMessages()) + "</pre>", "text/html"));
            }
        }

        public async Task<PartialViewResult> OnPostDeleteFavorite(string arg)
        {
            var key = arg;
            try {
                var favoritesNode = await ReadFavorites();

                var fav = favoritesNode.AsList.Where(node => node.AsDictionary.First().Key == key).FirstOrDefault();
                if (fav != null) {
                    favoritesNode.AsList.Remove(fav);
                }

                var model = await WriteFavorites(favoritesNode);
                return Partial("_CommandlineFavorites", model);
            } catch (Exception ex) {
                return Partial("_CommandlineResult", new CommandResult("<pre>" + string.Join(" | ", ex.GetMessages()) + "</pre>", "text/html"));
            }
        }

        public async Task<PartialViewResult> OnPostUpFavorite(string arg)
        {
            var key = arg;
            try {
                var favoritesNode = await ReadFavorites();

                var fav = favoritesNode.AsList.Where(node => node.AsDictionary.First().Key == key).FirstOrDefault();
                if (fav != null) {
                    var idx = favoritesNode.AsList.FindIndex(node => node == fav);
                    if (idx > 0) {
                        favoritesNode.AsList.Remove(fav);
                        favoritesNode.AsList.Insert(idx - 1, fav);
                    }
                }


                var model = await WriteFavorites(favoritesNode);
                return Partial("_CommandlineFavorites", model);
            } catch (Exception ex) {
                return Partial("_CommandlineResult", new CommandResult("<pre>" + string.Join(" | ", ex.GetMessages()) + "</pre>", "text/html"));
            }
        }

        public async Task<JsonPath.Node> ReadFavorites()
        {
            var favoritesJson = await _clusterClient.GetGrain<ICachedString>("Web.Favorites").Get();
            if (string.IsNullOrEmpty(favoritesJson)) {
                favoritesJson = "{}";
            }
            var favoritesNode = new JsonPath.Node(favoritesJson);
            return favoritesNode;
        }

        public async Task<CommandlineFavorites> WriteFavorites(JsonPath.Node favoritesNode)
        {
            var favoritesJson = favoritesNode.ToJson(bFormatted: true, bWrapped: true);
            await _clusterClient.GetGrain<ICachedString>("Web.Favorites").Set(favoritesJson, CachedStringOptions.Timeout.Infinite, CachedStringOptions.Persistence.Persistent);
            return new CommandlineFavorites(
                favoritesNode.AsList
                .Select(node => {
                    var first = node.AsDictionary.First();
                    return new KeyValuePair<string, string>(first.Key, first.Value.AsString);
                })
                .ToList()
            );
        }
    }
}