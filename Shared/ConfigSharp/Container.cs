using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConfigSharp
{
    public class Container
    {
        public int Get(string key, int defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public string Get(string key, string defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public long Get(string key, long defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public double Get(string key, double defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public bool Get(string key, bool defaultValue) { return this.GetMemberValue(key, defaultValue); }
        public T Get<T>(string key) { return (T)this.GetMemberValue(key); }

        public ILoader Loader { get; set; } // public so that CopyValues copies it
        public Container Use(ILoader loader) { Loader = loader; return this; }

        public string BaseFolder { get; set; }
        public string CurrentFile { get; protected set; }
        public const string AnyPublicMember = "_public";
        public const string Not = "!";
        public List<string> Functions = new List<string> { "Load" }; // "_public"

        public static void Log(string message)
        {
            ConfigSharp.Log.Info(message);
        }

        public Container Include(string fileName)
        {
            string code = Loader == null ? Load(fileName) : Loader.Load(fileName);
            if (string.IsNullOrEmpty(code)) { throw new Exception($"No code: {fileName} cwd={Directory.GetCurrentDirectory()}"); }

            var references = GetReferences(code);

            CurrentFile = fileName;
            Execute(code, references);
            CurrentFile = "";

            return this;
        }

        public string Load(string fileName)
        {
            string code;

            if (fileName.StartsWith("http://") || fileName.StartsWith("https://")) {
                code = LoadUri(fileName);
            } else {
                code = LoadFile(fileName);
            }

            return code;
        }

        public string LoadFile(string fileName)
        {
            string code = "";

            try {
                var pathPart = Path.GetDirectoryName(fileName);
                if (pathPart == null) {
                    throw new Exception("File name has no path");
                }
                var filePart = Path.GetFileName(fileName);

                if (string.IsNullOrEmpty(BaseFolder)) {
                    if (Path.IsPathRooted(fileName)) {
                        BaseFolder = pathPart;
                    } else {
                        BaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathPart);
                    }
                    BaseFolder = Path.GetFullPath(BaseFolder);
                    ConfigSharp.Log.Info("Base folder: " + BaseFolder);
                }

                var filePath = Path.Combine(BaseFolder, filePart);

                ConfigSharp.Log.Info(filePath);
                code = File.ReadAllText(filePath);
            } catch (Exception ex) {
                ConfigSharp.Log.Error(ex.Message + "(" + fileName + ")");
            }

            return code;
        }

        public static string LoadUri(string url)
        {
            string code = "";

            try {
                ConfigSharp.Log.Info(url);
                var req = (HttpWebRequest)WebRequest.Create(url);
                var resp = (HttpWebResponse)req.GetResponse();
                var stream = resp.GetResponseStream();
                if (stream == null) {
                    throw new Exception("No response stream");
                }
                var sr = new StreamReader(stream, encoding: Encoding.UTF8);
                code = sr.ReadToEnd();
            } catch (Exception ex) {
                ConfigSharp.Log.Error(ex.Message + "(" + url + ")");
            }

            return code;
        }

        public void Execute(string code, IEnumerable<string> customReferences = null)
        {
            customReferences ??= new List<string>();
            var references = customReferences.ToList();

            // Netstandard & runtime
            var aTypicalCoreAssembly = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreFolder = Directory.GetParent(aTypicalCoreAssembly);
            references.Add(coreFolder.FullName + Path.DirectorySeparatorChar + "netstandard.dll");
            references.Add(coreFolder.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll");

            // Basics & config lib
            references.Add(typeof(Object).Assembly.Location);
            references.Add(typeof(Uri).Assembly.Location);
            references.Add(typeof(Container).Assembly.Location);

            // Application dll
            var baseType = GetType().BaseType;
            var typeAssembly = GetType().Assembly;
            var appLocation = typeAssembly.Location;
            if (string.IsNullOrEmpty(appLocation) && baseType != null) {
                var baseTypeAssembly = baseType.Assembly;
                appLocation = baseTypeAssembly.Location;
            }
            references.Add(appLocation);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(
                "ConfigSnippet",
                new[] { syntaxTree },
                references.Select(r => MetadataReference.CreateFromFile(r)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var assemblyStream = new MemoryStream();
            var compilationResult = compilation.Emit(assemblyStream);
            if (compilationResult.Success) {
                var compiledAssembly = assemblyStream.ToArray();
                var loadedAssembly = Assembly.Load(compiledAssembly);
                var assemblyTypes = loadedAssembly.GetTypes();
                foreach (var type in assemblyTypes) {
                    try {

                        var loadMethods = type
                            .GetMembers(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance)
                            .Where(m => {
                                if (m is ConstructorInfo) { return false; }
                                if (Functions.Contains(Not + m.Name)) { return false; }
                                if (Functions.Contains(m.Name)) { return true; }
                                if (Functions.Contains(AnyPublicMember)) { return true; }
                                return false;
                            });

                        if (loadMethods == null || loadMethods.Count() == 0) { throw new Exception("No Load method"); }

                        foreach (var loadMethod in loadMethods) {
                            var cfgObject = Activator.CreateInstance(type);
                            CopyValues(this, cfgObject);
                            type.InvokeMember(loadMethod.Name, BindingFlags.InvokeMethod, null, cfgObject, Array.Empty<object>());
                            CopyValues(cfgObject, this);
                        }

                    } catch (Exception ex) {
                        ConfigSharp.Log.Error(CurrentFile + " Exception: " + ex.Message);
                    }
                }
            } else {
                ConfigSharp.Log.Error(CurrentFile + " Diagnostics:");
                foreach (var diagnostic in compilationResult.Diagnostics) {
                    ConfigSharp.Log.Error(diagnostic.ToString());
                }
            }
        }

        void CopyValues(object fromObj, object toObj)
        {
            var fromType = fromObj.GetType();
            var toType = toObj.GetType();

            foreach (var propertyInfo in fromType.GetProperties()) {
                var targetProp = toType.GetProperty(propertyInfo.Name);
                if (targetProp == null) { continue; }

                targetProp.SetValue(toObj, propertyInfo.GetValue(fromObj));
            }

            foreach (var fieldInfo in fromType.GetFields()) {
                var targetField = toType.GetField(fieldInfo.Name);
                if (targetField == null) { continue; }

                targetField.SetValue(toObj, fieldInfo.GetValue(fromObj));
            }
        }

        public static IEnumerable<string> GetReferences(string code)
        {
            var references = new List<string>();

            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines) {
                if (line.StartsWith("//reference ")) {
                    var tokens = ParseReference(line);
                    if (tokens.Count != 2) {
                        throw new Exception("//reference needs an argument (absolute path or AQN)");
                    }
                    var reference = tokens[1].Trim(new[] { '"' });
                    if (Path.IsPathRooted(reference)) {
                        // Absolute Path: use as given
                        references.Add(reference);
                    } else {
                        // AssemblyQualifiedName
                        var type = Type.GetType(reference);
                        if (type == null) {
                            throw new Exception("No type for " + reference);
                        }
                        references.Add(type.Assembly.Location);
                    }
                }
            }

            return references;
        }

        public static List<string> ParseReference(string s)
        {
            var args = new List<string>();

            bool done = false;
            bool inString = false;
            string token = "";
            int pos = 0;
            while (!done) {
                bool isData = false;
                switch (s[pos]) {
                    case '"':
                        if (!inString) {
                            inString = true;
                        } else {
                            inString = false;
                        }
                        break;
                    case '\0':
                        done = true;
                        break;
                    case ' ':
                        if (inString) {
                            isData = true;
                        } else {
                            if (!string.IsNullOrEmpty(token)) {
                                args.Add(token);
                                token = "";
                            }
                        }
                        break;
                    default:
                        isData = true;
                        break;
                }

                if (!done) {
                    if (isData) {
                        token += s[pos];
                    }
                    pos++;
                    done = (pos >= s.Length);
                }
            }

            if (!string.IsNullOrEmpty(token)) {
                args.Add(token);
            }

            return args;
        }
    }
}
