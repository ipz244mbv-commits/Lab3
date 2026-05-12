using System;
using System.Collections.Generic;
using System.Text;

namespace FlyweightPattern
{

    public class ElementConfig
    {
        public string TagName { get; }
        public bool IsBlock { get; }
        public bool IsPaired { get; }

        public ElementConfig(string tagName, bool isBlock, bool isPaired)
        {
            TagName = tagName;
            IsBlock = isBlock;
            IsPaired = isPaired;
        }
    }


    public static class ElementFactory
    {
        private static Dictionary<string, ElementConfig> _cache = new Dictionary<string, ElementConfig>();

        public static ElementConfig GetConfig(string tagName, bool isBlock, bool isPaired)
        {
            if (!_cache.ContainsKey(tagName))
            {
                _cache[tagName] = new ElementConfig(tagName, isBlock, isPaired);
            }
            return _cache[tagName];
        }

        public static int CacheSize => _cache.Count;
    }

    public abstract class LightNode
    {
        public abstract string OuterHtml { get; }
        public abstract string InnerHtml { get; }
    }

    public class LightTextNode : LightNode
    {
        private string _text;
        public LightTextNode(string text) { _text = text; }
        public override string InnerHtml => _text;
        public override string OuterHtml => _text;
    }

    public class LightElementNode : LightNode
    {
        private ElementConfig _config;
        public List<string> CssClasses { get; set; } = new List<string>();
        public List<LightNode> Children { get; set; } = new List<LightNode>();

        public LightElementNode(string tagName, bool isBlock, bool isPaired)
        {
            _config = ElementFactory.GetConfig(tagName, isBlock, isPaired);
        }

        public void Add(LightNode node) => Children.Add(node);

        public override string InnerHtml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var child in Children) sb.Append(child.OuterHtml);
                return sb.ToString();
            }
        }

        public override string OuterHtml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{_config.TagName}");
                if (CssClasses.Count > 0) sb.Append($" class=\"{string.Join(" ", CssClasses)}\"");

                if (!_config.IsPaired) sb.Append(" />");
                else
                {
                    sb.Append(">");
                    sb.Append(InnerHtml);
                    sb.Append($"</{_config.TagName}>");
                }
                if (_config.IsBlock) sb.Append("\n");

                return sb.ToString();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string[] lines = {
                "ACT V",
                "Scene I. Mantua. A Street.",
                "Scene II. Friar Lawrence's Cell.",
                "Scene III. A churchyard; in it a Monument belonging to the Capulets",
                "Dramatis Personæ",
                "ESCALUS, Prince of Verona.",
                "MERCUTIO, kinsman to the Prince, and friend to Romeo.",
                " PARIS, a young Nobleman, kinsman to the Prince.",
                " Page to Paris."
            };

            List<string> hugeBook = new List<string>();
            for (int i = 0; i < 100000; i++)
            {
                hugeBook.AddRange(lines);
            }

            long memoryBefore = GC.GetTotalMemory(true);

            LightElementNode root = new LightElementNode("div", true, true);
            bool isFirstLine = true;

            foreach (var line in hugeBook)
            {
                LightElementNode node;

                if (isFirstLine)
                {
                    node = new LightElementNode("h1", true, true);
                    isFirstLine = false;
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else if (char.IsWhiteSpace(line[0]))
                {
                    node = new LightElementNode("blockquote", true, true);
                }
                else if (line.Length < 20)
                {
                    node = new LightElementNode("h2", true, true);
                }
                else
                {
                    node = new LightElementNode("p", true, true);
                }

                node.Add(new LightTextNode(line.TrimStart()));
                root.Add(node);
            }

            long memoryAfter = GC.GetTotalMemory(true);

            Console.WriteLine("=== ДЕМОНСТРАЦІЯ ШАБЛОНУ ЛЕГКОВАГОВИК ===");
            Console.WriteLine($"Загальна кількість згенерованих вузлів: {root.Children.Count}");

            Console.WriteLine($"Кількість унікальних тегів у кеші Легковаговика: {ElementFactory.CacheSize}");

            double memoryUsedMb = (memoryAfter - memoryBefore) / (1024.0 * 1024.0);
            Console.WriteLine($"Споживання пам'яті деревом: ~{memoryUsedMb:F2} MB");

            Console.WriteLine("\n=== Фрагмент згенерованого HTML (перші 9 рядків) ===");

            for (int i = 0; i < 9; i++)
            {
                Console.Write(root.Children[i].OuterHtml);
            }

            Console.ReadLine();
        }
    }
}