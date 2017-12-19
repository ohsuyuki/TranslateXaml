using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using IBM.WatsonDeveloperCloud.LanguageTranslator.v2;
using IBM.WatsonDeveloperCloud.LanguageTranslator.v2.Model;

namespace TranslateXaml
{
    public class Translate
    {
        public Action<int> OnFileLoaded { private get; set; }
        public Action<int> OnProgress { private get; set; }
        public Action OnCompleted { private get; set; }
        public Action<string, Exception> OnError { private get; set; }

        public const string From = "ja";
        public const string To = "en";

        private LanguageTranslatorService LanguageTranslator { get; set; }

        public Translate()
        {
            LanguageTranslator = new LanguageTranslatorService();
            LanguageTranslator.SetCredential("6d64d7e5-bea7-4e65-9768-604b0c31830b", "SYdQXJHzMhDF");
        }

        public Task RunAsync(string srcFilePath, string dstFilePath)
        {
            return Task.Run(() =>
            {
                Run(srcFilePath, dstFilePath);
            });
        }

        public void Run(string srcFilePath, string dstFilePath)
        {
            XDocument src = XDocument.Load(srcFilePath);
            OnFileLoaded?.Invoke(src.Root.Descendants().Count());
            XDocument dst = Run(src);
            var xamlString = dst.ToString().ToXamlString();
            File.WriteAllText(dstFilePath, xamlString);
            OnCompleted?.Invoke();
        }

        public XDocument Run(XDocument src)
        {
            var dst = new XDocument(src);

            var nsSys = XNamespace.Get(@"http://schemas.microsoft.com/winfx/2006/xaml");

            int count = 0;
            foreach (XElement element in dst.Root.Descendants())
            {
                OnProgress?.Invoke(count);
                count++;

                XAttribute attrKey = element.Attributes().FirstOrDefault(attr => attr.Name == nsSys.GetName("Key"));
                if (attrKey == null)
                {
                    continue;
                }

                if (attrKey.Value.Contains("ResourceString") == false)
                {
                    continue;
                }

                string translated = TranslateMsg(element.Value);
                element.Value = translated.EscapeXaml();
            }

            OnProgress?.Invoke(count);

            return dst;
        }

        public string TranslateMsg(string msg)
        {
            if (msg.ContainsUnicode() == false)
            {
                return msg;
            }

            try
            {
                TranslateResponse translateResponse = LanguageTranslator.Translate(From, To, msg);
                return translateResponse?.Translations?.First()?.Translation;
            }
            catch(Exception exp)
            {
                OnError?.Invoke(msg, exp);
                return null;
            }
        }
    }

    public static class StringExtensions
    {
        public static bool ContainsUnicode(this string str)
        {
            const int MaxAnsiCode = 255;
            return str.Any(c => c > MaxAnsiCode);
        }

        public static readonly Tuple<string, string>[] SpecialChars = new Tuple<string, string>[]
        {
            //new Tuple<string, string>("&", "&amp;"),
            //new Tuple<string, string>("<", "&lt;"),
            //new Tuple<string, string>(">", "&gt;"),
            //new Tuple<string, string>("\"", "&quot;"),
            new Tuple<string, string>("\r\n", "&#x0a;"),
            new Tuple<string, string>("\n", "&#x0a;"),
        };

        public static string EscapeXaml(this string str)
        {
            foreach (Tuple<string, string> specialCh in SpecialChars)
            {
                str = str.Replace(specialCh.Item1, specialCh.Item2);
            }

            return str;
        }

        public static string ToXamlString(this string str)
        {
            return str.Replace("&amp;#", "&#");
        }
    }
}
