using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nine3q.Tools;

namespace nine3q.Web
{
    public class CommandlineModel : PageModel
    {
        public class CommandDetail
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Template { get; set; }
            public string Arguments { get; set; }
        }

        ICommandline _commandline;
        
        public readonly Dictionary<string, CommandDetail> Commands = new Dictionary<string, CommandDetail>();

        public CommandlineModel(ICommandlineSingletonInstance commandline)
        {
            _commandline = commandline;
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
                    });
                }
            }
        }

        public void OnPostRun(string cmd)
        {
            var user = new Commandline.User(User.Claims);

            try {
                var html = _commandline.Run(cmd, user);
                //return Content(html, "text/html");
            } catch (Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //return Content(string.Join(" | ", ex.GetMessages()), "text/plain");
            }
        }
    }
}