using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace n3q.Xmpp
{
    public class Sax
    {
        public class AttributeSet : Dictionary<string, string>
        {
            public string Get(string key, string defaultValue)
            {
                if (this.ContainsKey(key)) {
                    return this[key];
                }
                return defaultValue;
            }
        }

        public class PreambleArgs : EventArgs
        {
            public string Name { get; set; }
            public AttributeSet Attributes { get; set; }
        }
        public class StartElementArgs : EventArgs
        {
            public string Name { get; set; }
            public int Depth { get; set; }
            public AttributeSet Attributes { get; set; }
        }
        public class EndElementArgs : EventArgs
        {
            public string Name { get; set; }
            public int Depth { get; set; }
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
            public string Vicinity { get; set; }
        }

        public event EventHandler<PreambleArgs> Preamble;
        public event EventHandler<StartElementArgs> StartElement;
        public event EventHandler<EndElementArgs> EndElement;
        public event EventHandler<CharacterDataArgs> CharacterData;
        public event EventHandler<ParseErrorArgs> ParseError;

        public void Parse(byte[] bytes)
        {
            var s = NextString(bytes, Encoding.UTF8);
            Parse(s);
        }

        Decoder _decoder;
        public string NextString(byte[] bytes, Encoding encoding)
        {
            //return Encoding.UTF8.GetString(bytes);

            _decoder ??= encoding.GetDecoder();
            StringBuilder sb = new StringBuilder();

            char[] chars = new char[encoding.GetMaxCharCount(bytes.Length)];
            int readChars = _decoder.GetChars(bytes, 0, bytes.Length, chars, 0);
            if (readChars > 0) {
                sb.Append(chars, 0, readChars);
            }

            return sb.ToString();
        }

        public enum State { BeforeRoot, TagName, Attributes, Text, Tag, ClosingTag, }

        public int errorVicinity = 30;
        public int slashFlag = -1;
        public int openingFlag = -1;
        public Stack<string> tagStack = new Stack<string>();
        public string tagName = "";
        public string tagText = "";
        public string attributes = "";
        public string closingName = "";
        public int charIndex = 0;
        public int columnNumber = 0;
        public int lineNumber = 1;
        public string errorText = "";
        public bool isError;
        public State state = State.BeforeRoot;

        private bool JustHadSlash => slashFlag == 0;
        private bool JustHadOpeningBracket => openingFlag == 0;

        public void Parse(string data)
        {
            Log.Verbose($"-> {data}");

            charIndex = -1;

            foreach (var c in data) {

                Always(c);

                switch (state) {
                    case State.BeforeRoot:
                        switch (c) {
                            case '<': BeginTag(); break;
                            case '>': Error($"'{c}' in {state}"); break;
                            default: break;
                        }
                        break;

                    case State.TagName:
                        switch (c) {
                            case '<': Error($"'{c}' in {state}"); break;
                            case '>':
                                if (string.IsNullOrEmpty(tagName)) {
                                    Error($"'{c}' in {state}");
                                } else {
                                    StartElement?.Invoke(this, new StartElementArgs { Name = tagName, Depth = tagStack.Count, Attributes = GetAttributes(attributes), });
                                    if (JustHadSlash) {
                                        EndTag();
                                    } else {
                                        state = State.Text;
                                        tagText = "";
                                    }
                                }
                                break;
                            case ' ':
                                state = State.Attributes;
                                break;
                            case '/':
                                if (JustHadOpeningBracket) {
                                    state = State.ClosingTag;
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
                            case '<': Error($"'{c}' in {state}"); break;
                            case '/':
                                slashFlag = 1;
                                attributes += c;
                                break;
                            case '>':
                                if (tagName.StartsWith("?") && tagName.ToLower() == "?xml") {
                                    if (attributes.EndsWith("?")) { attributes = attributes.Substring(0, attributes.Length - 1); }
                                    Preamble?.Invoke(this, new PreambleArgs { Name = tagName, Attributes = GetAttributes(attributes) });
                                    state = State.BeforeRoot;
                                    tagName = "";
                                } else {
                                    if (attributes.EndsWith("/")) { attributes = attributes.Substring(0, attributes.Length - 1); }
                                    StartElement?.Invoke(this, new StartElementArgs { Name = tagName, Depth = tagStack.Count, Attributes = GetAttributes(attributes), });
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
                            case '>': Error($"'{c}' in {state}"); break;
                            default:
                                tagText += c;
                                break;
                        }
                        break;

                    case State.Tag:
                        switch (c) {
                            case '<': Error($"'{c}' in {state}"); break;
                            case '>': state = State.BeforeRoot; break;
                            case ' ': Error($"'{c}' in {state}"); break;
                            case '/':
                                state = State.ClosingTag;
                                closingName = "";
                                break;

                            default: break;
                        }
                        break;

                    case State.ClosingTag:
                        switch (c) {
                            case '<': Error($"'{c}' in {state}"); break;
                            case '/': Error($"'{c}' in {state}"); break;
                            case '>':
                                if (string.IsNullOrEmpty(tagName)) {
                                    Error("Tag name empty");
                                } else {
                                    if (tagName == closingName) {
                                        EndTag();
                                    } else {
                                        Error("Tag name mismatch {tagName} != {closingName}");
                                    }
                                }
                                break;
                            case ' ': Error($"'{c}' in {state}"); break;

                            default:
                                closingName += c;
                                break;
                        }
                        break;
                }

                if (isError) {
                    var leftVicinity = Math.Max(charIndex - errorVicinity, 0);
                    var rightVicinity = Math.Min(charIndex + errorVicinity, data.Length - 1);
                    ParseError?.Invoke(this, new ParseErrorArgs {
                        Column = columnNumber,
                        Line = lineNumber,
                        Message = errorText,
                        Vicinity = data.Substring(leftVicinity, rightVicinity - leftVicinity + 1),
                    });
                    return;
                }
            }
        }

        private void Always(char c)
        {
            slashFlag--;
            openingFlag--;
            charIndex++;
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
            EndElement?.Invoke(this, new EndElementArgs { Name = tagName, Depth = tagStack.Count });
            state = State.Text;
            tagName = "";
            closingName = "";
            attributes = "";
        }

        private string GetText(string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        private AttributeSet GetAttributes(string attributes)
        {
            var dict = new Sax.AttributeSet();

            var attribs = attributes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var attrib in attribs) {
                if (attrib.StartsWith("=")) {
                    Error($"Invalid attribute '{attrib}'");
                    break;
                }

                var kv = attrib.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length == 0 || string.IsNullOrEmpty(kv[0])) {
                    Error($"Invalid attribute '{attrib}'");
                    break;
                }

                if (kv.Length == 1) {
                    dict[kv[0]] = "";
                } else if (kv.Length == 2) {
                    dict[kv[0]] = HttpUtility.HtmlDecode(kv[1].Trim('"').Trim('\''));
                }
            }

            return dict;
        }

        private void Error(string text = null)
        {
            errorText = text ?? $"Unexpected character in {state}";
            isError = true;
        }
    }
}
