using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using nine3q.Tools;

namespace nine3q.Web
{
    public class Commandline : ICommandlineSingletonInstance
    {
        public class CommandDetail
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Template { get; set; }
            public string Arguments { get; set; }
        }

        public Guid CommandlineId { get; private set; }
        private static readonly object Mutex = new object();

        public interface ICommandlineUser
        {
            IEnumerable<string> Roles { get; }
        }

        public class User : ICommandlineUser
        {
            private IEnumerable<Claim> claims;

            public IEnumerable<string> Roles { get; }
            //public User(IEnumerable<string> roles) { Roles = roles; }
            public User(IEnumerable<Claim> claims)
            {
                this.claims = claims;
            }
        }

        public class Arglist : List<string>
        {
            public string Source = "";

            public string Get(string name, string defaultValue = null)
            {
                var prefix = name + "=";
                var arg = this.SingleOrDefault(item => item.StartsWith(prefix));
                if (arg != null) {
                    var value = arg.Substring(prefix.Length);
                    if (!string.IsNullOrEmpty(value)) {
                        return value;
                    }
                }

                return DefaultOrException(name, defaultValue);
            }

            public string Get(int pos, string name = "", string defaultValue = null)
            {
                if (pos < Count) {
                    return this[pos];
                }

                return DefaultOrException(name, defaultValue);
            }

            public string Next(string name = "", string defaultValue = null)
            {
                if (Count > 0) {
                    var arg = this[0];
                    RemoveAt(0);
                    return arg;
                }

                return DefaultOrException(name, defaultValue);
            }

            public string DefaultOrException(string name, string defaultValue)
            {
                if (defaultValue != null) {
                    return defaultValue;
                } else {
                    throw new Exception("Missing argument: " + name);
                }
            }
        }
        public enum ArgumentListType { KeyValue, Tokens }
        public class ArgumentDescriptionList : Dictionary<string, string> { }

        public enum Role { Public, Admin, Developer }

        public delegate object HandlerFunction(Arglist args);

        public class Handler
        {
            public string Name { get; set; }
            public HandlerFunction Function { get; set; }
            public string Role { get; set; }
            public string Description { get; set; }
            public ArgumentDescriptionList Arguments { get; set; }
            public ArgumentListType ArgumentList { get; set; } = ArgumentListType.KeyValue;
        }

        public class HandlerMap : Dictionary<string, Handler> { public HandlerMap(IEqualityComparer<string> comparer) : base(comparer) { } }

        public delegate string FormatterFunction(object o);
        public class FormatterList : List<FormatterFunction> { }
        public FormatterList GetFormatters()
        {
            lock (Mutex) {
                if (_formatters == null) {
                    _formatters = InitializeFormatters();
                }
            }
            return _formatters;
        }

        public delegate string ArgumentFormatterFunction(object o);
        public class ArgumentFormatterList : List<ArgumentFormatterFunction> { }
        public ArgumentFormatterList GetArgumentFormatters()
        {
            lock (Mutex) {
                if (_argumentFormatters == null) {
                    _argumentFormatters = InitializeArgumentFormatters();
                }
            }
            return _argumentFormatters;
        }

        public class Table
        {
            public enum Option
            {
                TableClass, // default: cl-table
                OddRowClass, // default: cl-odd
                TableHeader, // default: false
                LinkClass, // default: cl-link
            }
            public class OptionList : Dictionary<Option, string>
            {
                public string Get(Option o, string defaultValue)
                {
                    return ContainsKey(o) ? this[o] : defaultValue;
                }

                public bool Get(Option o, bool defaultValue)
                {
                    return ContainsKey(o) ? this[o].ToLower() == "true" || this[o].ToLower() == "yes" || this[o] == "1" : defaultValue;
                }
            }
            private OptionList _options = null;
            public OptionList Options { get { return _options ?? (_options = new OptionList()); } }

            public class Row : List<string> { }
            public class RowList : List<Row> { }
            private RowList _grid = null;
            public RowList Grid { get { return _grid ?? (_grid = new RowList()); } }
        }

        HandlerMap _handlers;
        private FormatterList _formatters;
        private ArgumentFormatterList _argumentFormatters;
        Dictionary<string, string> _vars;

        public Commandline() : this(Guid.NewGuid())
        {
        }

        public Commandline(Guid id)
        {
            CommandlineId = id;
        }

        public HandlerMap GetHandlers()
        {
            lock (Mutex) {
                if (_handlers == null) {
                    _handlers = InitializeHandlers();
                }
            }
            return _handlers;
        }


        public class VariableAssignment
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        #region Infrastructure

        public string Run(string script, ICommandlineUser user)
        {
            var html = "";
            var lines = ParseScript(script);
            _vars = new Dictionary<string, string>();
            var lineCount = 0;
            foreach (var line in lines) {
                lineCount++;
                if (line.Count > 0) {
                    try {
                        var result = Run(line, user);
                        var formatted = FormatAsHtml(result);
                        html += (string.IsNullOrEmpty(html) ? "" : "<br />\n") + formatted;
                    } catch (Exception ex) {
                        throw new Exception("Script failed at line " + lineCount + ": " + line.Source + ": " + string.Join(" | ", ex.GetMessages()));
                    }
                }
            }
            return html;
        }

        public object Run(Arglist args, ICommandlineUser user)
        {
            var actualArgs = new Arglist();
            foreach (var arg in args) {
                var embeddedLine = GetEmbeddedLine(arg);
                if (!string.IsNullOrEmpty(embeddedLine)) {
                    var unescapedLine = Unescape(embeddedLine);
                    var subArgs = ParseLine(unescapedLine);
                    var subResult = Run(subArgs, user);
                    var subReplacement = FormatAsArgument(subResult);
                    var replacedArg = arg.Replace("`" + embeddedLine + "`", subReplacement);
                    actualArgs.Add(replacedArg);
                } else {
                    actualArgs.Add(arg);
                }
            }

            string method = actualArgs[0];
            if (!GetHandlers().ContainsKey(method)) {
                throw new Exception("Unknown command: " + method);
            }

            var handler = GetHandlers()[method];

            AssertRole(handler, user);

            var result = handler.Function(actualArgs);

            return result;
        }

        public List<Arglist> ParseScript(string script)
        {
            var lines = new List<Arglist>();

            script = script.Replace("\r\n", "\n");
            var aScript = script.Split('\n');
            foreach (var line in aScript) {
                var args = ParseLine(line);
                lines.Add(args);
            }

            return lines;
        }

        protected void AssertRole(Handler handler, ICommandlineUser user)
        {
            var result = CheckRole(handler, user);
            if (!string.IsNullOrEmpty(result)) {
                throw new Exception(result);
            }
        }

        public string CheckRole(Commandline.Handler handler, Commandline.ICommandlineUser user)
        {
            if (string.IsNullOrEmpty(handler.Role)) {
                return "Unauthorized: function role undefined";
            }

            if (handler.Role == Role.Public.ToString()) { return ""; }

            if (!user.Roles.Contains(handler.Role)) {
                return $"Unauthorized: function={handler.Name} needs role={handler.Role}";
            }
            return "";
        }

        public string GetEmbeddedLine(string line)
        {
            bool done = false;
            bool inString = false;
            bool hideQuote = false;
            string embdedded = "";
            int pos = 0;
            while (!done) {
                bool isData = false;
                var c = line[pos];
                switch (c) {
                    case '\\':
                        if (!hideQuote) {
                            hideQuote = true;
                            if (inString) {
                                isData = true;
                            }
                        } else {
                            isData = true;
                        }
                        break;
                    case '`':
                        if (hideQuote) {
                            isData = true;
                        } else {
                            inString = !inString;
                        }
                        hideQuote = false;
                        break;
                    case '\0':
                        done = true;
                        break;
                    default:
                        if (inString) {
                            isData = true;
                        }
                        break;
                }
                if (!done) {
                    if (isData) {
                        embdedded += c;
                    }
                    pos++;
                    done = (pos >= line.Length);
                }
            }
            return embdedded;
        }

        public string Unescape(string line)
        {
            bool done = false;
            bool inString = false;
            bool hideQuote = false;
            string unescaped = "";
            int pos = 0;
            while (!done) {
                bool isData = false;
                var c = line[pos];
                switch (c) {
                    case '\\':
                        if (!hideQuote) {
                            hideQuote = true;
                            if (inString) {
                                isData = true;
                            }
                        }
                        break;
                    case '`':
                        if (hideQuote) {
                            isData = true;
                        } else {
                            inString = !inString;
                        }
                        hideQuote = false;
                        break;
                    case '\0':
                        done = true;
                        break;
                    default:
                        isData = true;
                        break;
                }
                if (!done) {
                    if (isData) {
                        unescaped += c;
                    }
                    pos++;
                    done = (pos >= line.Length);
                }
            }
            return unescaped;
        }

        public Arglist ParseLine(string line)
        {
            var args = new Arglist() { Source = line };

            if (string.IsNullOrEmpty(line)) { return args; }

            bool done = false;
            bool inString = false;
            bool hideQuote = false;
            char quoteChar = '\0';
            string token = "";
            int pos = 0;
            while (!done) {
                bool isData = false;
                var c = line[pos];
                switch (c) {
                    case '\\':
                        if (!hideQuote) {
                            hideQuote = true;
                            if (inString) {
                                isData = true;
                            }
                        } else {
                            isData = true;
                        }
                        break;
                    case '"':
                    case '\'':
                    case '`':
                        if (hideQuote) {
                            isData = true;
                        } else {
                            if (!inString) {
                                inString = true;
                                quoteChar = c;
                                if (c == '`') { isData = true; }
                            } else {
                                if (c == quoteChar) {
                                    inString = false;
                                    if (c == '`') { isData = true; }
                                } else {
                                    isData = true;
                                }
                            }
                        }
                        hideQuote = false;
                        break;
                    case '\0':
                        done = true;
                        break;
                    case ' ':
                        if (inString) {
                            isData = true;
                        } else {
                            if (token != "") {
                                args.Add(token);
                                token = "";
                            }
                        }
                        hideQuote = false;
                        break;
                    default:
                        isData = true;
                        hideQuote = false;
                        break;
                }

                if (!done) {
                    if (isData) {
                        token += c;
                    }
                    pos++;
                    done = (pos >= line.Length);
                }
            }

            if (token != "") {
                args.Add(token);
                token = "";
            }

            return args;
        }

        #endregion

        #region Handlers

        protected virtual HandlerMap InitializeHandlers()
        {
            return new HandlerMap(StringComparer.OrdinalIgnoreCase) {
                { "Echo", new Handler { Name = "Echo", Function = Echo, Role = Role.Public.ToString(), Description = "Return all arguments", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { { "arg1", "first argument" }, { "...", "more arguments" } } } },
                //{ "Dev_TestTable", new Handler { Name = "Dev_TestTable", Function = Dev_TestTable, Role = Role.Developer.ToString(), Description = "Full table example" } },
                //{ "Dev_Exception", new Handler { Name = "Dev_Exception", Function = Dev_Exception, Role = Role.Developer.ToString(), Description = "Throw exception" } },
                //{ "Dev_null", new Handler { Name = "Dev_null", Function = Dev_null, Role = Role.Developer.ToString(), Description = "Do nothing, return null" } },
                //{ "var", new Handler { Name = "var", Function = GetSetVar, Role = Role.Public.ToString(), Description = "Assign or use variable", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Name[=Value]"] = "Name=Value assigns variable value to name, Name only returns variable value",  } } },
                //{ "//", new Handler { Name = "//", Function = Comment, Role = Role.Public.ToString(), Description = "Ignored and copied to output", ArgumentList = ArgumentListType.Tokens, Arguments = new ArgumentDescriptionList { ["Comment"] = "Comment line",  } } },
            };
        }

        private object Echo(Arglist args)
        {
            args.RemoveAt(0);
            return string.Join(" ", args);
        }

        #endregion

        #region Formatters

        protected virtual FormatterList InitializeFormatters()
        {
            return new FormatterList {
                FormatString,
                FormatTable,
                FormatVariableAssignment,
                FormatNull,
            };
        }

        protected virtual ArgumentFormatterList InitializeArgumentFormatters()
        {
            return new ArgumentFormatterList {
                FormatStringAsArgument,
            };
        }

        public string FormatStringAsArgument(object o)
        {
            return FormatString(o);
        }

        public string FormatString(object o)
        {
            if (!(o is string)) { return null; }
            return o as string;
        }

        public string FormatTable(object o)
        {
            if (!(o is Table)) { return null; }
            var table = o as Table;

            var sb = new StringBuilder();

            if (table.Grid != null) {
                sb.Append("<table class=\"" + table.Options.Get(Table.Option.TableClass, "cl-table") + "\">");
                var rowCount = 0;
                foreach (var row in table.Grid) {
                    var rowClass = "";
                    if (rowCount % 2 == (table.Options.Get(Table.Option.TableHeader, false) ? 0 : 1)) {
                        rowClass += (string.IsNullOrEmpty(rowClass) ? "" : " ") + table.Options.Get(Table.Option.OddRowClass, "cl-odd");
                    }
                    sb.Append("<tr" + (string.IsNullOrEmpty(rowClass) ? "" : " class=\"" + rowClass) + "\">");
                    foreach (var cell in row) {
                        var tagName = "td";
                        if (rowCount == 0 && table.Options.Get(Table.Option.TableHeader, false)) {
                            tagName = "th";
                        }
                        sb.Append("<" + tagName + ">");
                        sb.Append(FormatAsHtml(cell));
                        sb.Append("</" + tagName + ">");
                    }
                    sb.Append("</tr>");
                    rowCount++;
                }
                sb.Append("</table>");
            }

            return sb.ToString();
        }

        public string FormatVariableAssignment(object o)
        {
            if (!(o is VariableAssignment)) { return null; }
            var assigmnment = o as VariableAssignment;
            return assigmnment.Name + "=" + assigmnment.Value;
        }

        public string FormatNull(object o)
        {
            if (o != null) { return null; }
            return "(null)";
        }

        public string FormatAsHtml(object data)
        {
            foreach (var formatter in GetFormatters()) {
                var result = formatter(data);
                if (result != null) {
                    return result;
                }
            }
            return data.ToString();
        }

        public string FormatAsArgument(object data)
        {
            foreach (var formatter in GetArgumentFormatters()) {
                var result = formatter(data);
                if (result != null) {
                    return result;
                }
            }
            return data.ToString();
        }
        #endregion

    }
}