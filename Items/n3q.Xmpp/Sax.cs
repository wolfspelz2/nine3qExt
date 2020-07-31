using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace n3q.Xmpp
{
    public class Sax
    {
        public class PreambleArgs : EventArgs
        {
            public string Name { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }
        public class StartElementArgs : EventArgs
        {
            public string Name { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }
        public class EndElementArgs : EventArgs
        {
            public string Name { get; set; }
        }
        public class CharacterDataArgs : EventArgs
        {
            public string Text { get; set; }
        }
        public class ParseErrorArgs : EventArgs
        {
            public int Column { get; set; }
            public int Line { get; set; }
            public string Message { get; set; }
        }

        public event EventHandler<PreambleArgs> Preamble;
        public event EventHandler<StartElementArgs> StartElement;
        public event EventHandler<EndElementArgs> EndElement;
        public event EventHandler<CharacterDataArgs> CharacterData;
        public event EventHandler<ParseErrorArgs> ParseError;

        public void Parse(byte[] bytes)
        {
            var s = NextString(bytes);
        }

        public string NextString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public enum State { None, TagName, Attributes, Text, Tag, ClosingName, Error }

        public int slashFlag = -1;
        public int openingFlag = -1;
        public Stack<string> tagStack = new Stack<string>();
        public string tagName = "";
        public string tagText = "";
        public string attributes = "";
        public string closingName = "";
        public int columnNumber = 0;
        public int lineNumber = 1;
        public string errorText = "";
        public State state = State.None;

        private bool JustHadSlash => slashFlag == 0;
        private bool JustHadOpeningBracket => openingFlag == 0;

        public void Parse(string s)
        {
            foreach (var c in s) {

                Always(c);

                switch (state) {
                    case State.None:
                        switch (c) {
                            case '<': BeginTag(); break;
                            case '>': Error("> before <"); break;
                            default: break;
                        }
                        break;

                    case State.TagName:
                        switch (c) {
                            case '<': Error("< in tag name"); break;
                            case '>':
                                StartElement?.Invoke(this, new StartElementArgs { Name = tagName, Attributes = GetAttributes(attributes), });
                                if (JustHadSlash) {
                                    EndTag();
                                } else {
                                    state = State.Text;
                                    tagText = "";
                                }
                                break;
                            case ' ':
                                state = State.Attributes;
                                break;
                            case '/':
                                if (JustHadOpeningBracket) {
                                    state = State.ClosingName;
                                    tagName = tagStack.Pop();
                                } else {
                                    slashFlag = 1;
                                }
                                break;
                            default:
                                tagName += c;
                                break;
                        }
                        break;

                    case State.Attributes:
                        switch (c) {
                            case '<': Error("< in attributes"); break;
                            case '/':
                                slashFlag = 1;
                                attributes += c;
                                break;
                            case '>':
                                if (tagName.StartsWith("?") && tagName.ToLower() == "?xml") {
                                    if (attributes.EndsWith("?")) { attributes = attributes.Substring(0, attributes.Length - 1); }
                                    Preamble?.Invoke(this, new PreambleArgs { Name = tagName, Attributes = GetAttributes(attributes) });
                                    state = State.None;
                                } else {
                                    if (attributes.EndsWith("/")) { attributes = attributes.Substring(0, attributes.Length - 1); }
                                    StartElement?.Invoke(this, new StartElementArgs { Name = tagName, Attributes = GetAttributes(attributes), });
                                    if (JustHadSlash) {
                                        EndTag();
                                    }
                                    state = State.Text;
                                    tagText = "";
                                }
                                break;
                            default:
                                attributes += c;
                                break;
                        }
                        break;

                    case State.Text:
                        switch (c) {
                            case '<':
                                BeginTag();
                                openingFlag = 1;
                                break;
                            case '>': Error("> in node text before <"); break;
                            default:
                                tagText += c;
                                break;
                        }
                        break;

                    case State.Tag:
                        switch (c) {
                            case '<': Error("< in closing tag"); break;
                            case '>': state = State.None; break;
                            case ' ': Error("Space in closing tag"); break;
                            case '/':
                                state = State.ClosingName;
                                closingName = "";
                                break;

                            default: break;
                        }
                        break;

                    case State.ClosingName:
                        switch (c) {
                            case '<': Error("< in closing tag"); break;
                            case '>':
                                if (string.IsNullOrEmpty(tagName)) {
                                    Error("Tag name empty");
                                } else {
                                    if (tagName == closingName) {
                                        EndTag();
                                    } else {
                                        Error("Tag name mismatch");
                                    }
                                }
                                break;
                            case ' ': Error("Space in closing tag name"); break;
                            case '/': slashFlag = 1; break;

                            default:
                                closingName += c;
                                break;
                        }
                        break;
                }

                if (state == State.Error) {
                    ParseError?.Invoke(this, new ParseErrorArgs { Column = columnNumber, Line = lineNumber, Message = errorText, });
                    return;
                }
            }
        }

        private void Always(char c)
        {
            slashFlag--;
            openingFlag--;
            columnNumber++;
            if (c == '\n') {
                lineNumber++;
                columnNumber = 0;
            }
        }

        private void BeginTag()
        {
            if (!string.IsNullOrEmpty(tagName)) {
                tagStack.Push(tagName);
                tagName = "";
            }
            if (!string.IsNullOrEmpty(tagText)) {
                CharacterData?.Invoke(this, new CharacterDataArgs { Text = GetText(tagText), });
                tagText = "";
            }
            attributes = "";
            state = State.TagName;
        }

        private void EndTag()
        {
            EndElement?.Invoke(this, new EndElementArgs { Name = tagName, });
            state = State.Text;
            tagName = "";
            closingName = "";
            attributes = "";
        }

        private string GetText(string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        private Dictionary<string, string> GetAttributes(string attributes)
        {
            var dict = new Dictionary<string, string>();

            var attribs = attributes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var attrib in attribs) {
                var kv = attrib.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 1) {
                    dict[kv[0]] = "";
                } else if (kv.Length == 2) {
                    dict[kv[0]] = HttpUtility.HtmlDecode(kv[1].Trim('"').Trim('\''));
                } else {
                    Error("Invalid attribute");
                }
            }

            return dict;
        }

        private void Error(string text = null)
        {
            state = State.Error;
            errorText = text ?? $"Unexpected character in {state}";
        }
    }
}
