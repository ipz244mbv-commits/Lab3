using System;
using System.Collections.Generic;
using System.Text;

namespace CompositePattern
{
    public abstract class LightNode
    {
        public abstract string OuterHtml { get; }
        public abstract string InnerHtml { get; }
    }

    public class LightTextNode : LightNode
    {
        private string _text;

        public LightTextNode(string text)
        {
            _text = text;
        }

        public override string InnerHtml => _text;
        public override string OuterHtml => _text;
    }

    public enum DisplayType { Block, Inline }
    public enum ClosingType { Single, Paired }

    public class LightElementNode : LightNode
    {
        public string TagName { get; set; }
        public DisplayType DisplayType { get; set; }
        public ClosingType ClosingType { get; set; }
        public List<string> CssClasses { get; set; }
        public List<LightNode> Children { get; set; } = new List<LightNode>();

        public LightElementNode(string tagName, DisplayType displayType, ClosingType closingType, List<string> cssClasses)
        {
            TagName = tagName;
            DisplayType = displayType;
            ClosingType = closingType;
            CssClasses = cssClasses ?? new List<string>();
        }

        public void Add(LightNode node)
        {
            Children.Add(node);
        }

        public override string InnerHtml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var child in Children)
                {
                    sb.Append(child.OuterHtml);
                }
                return sb.ToString();
            }
        }

        public override string OuterHtml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"<{TagName}");

                if (CssClasses.Count > 0)
                {
                    sb.Append($" class=\"{string.Join(" ", CssClasses)}\"");
                }

                if (ClosingType == ClosingType.Single)
                {
                    sb.Append(" />");
                }
                else
                {
                    sb.Append(">");
                    sb.Append(InnerHtml);
                    sb.Append($"</{TagName}>");
                }

                if (DisplayType == DisplayType.Block)
                {
                    sb.Append("\n");
                }

                return sb.ToString();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== ЗБІРКА HTML ДЕРЕВА ЗА ДОПОМОГОЮ КОМПОНУВАЛЬНИКА ===\n");

            LightElementNode ulNode = new LightElementNode("ul", DisplayType.Block, ClosingType.Paired, new List<string> { "menu", "dark-theme" });

            for (int i = 1; i <= 3; i++)
            {
                LightElementNode liNode = new LightElementNode("li", DisplayType.Block, ClosingType.Paired, new List<string> { "menu-item" });

                LightElementNode imgNode = new LightElementNode("img", DisplayType.Inline, ClosingType.Single, new List<string> { "icon" });

                LightTextNode textNode = new LightTextNode($" Елемент списку номер {i}");

                liNode.Add(imgNode);
                liNode.Add(textNode);

                ulNode.Add(liNode);
            }

            Console.WriteLine("Отриманий OuterHTML:");
            Console.WriteLine("-----------------------");
            Console.WriteLine(ulNode.OuterHtml);
            Console.WriteLine("-----------------------");

            Console.ReadLine();
        }
    }
}