using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MKR1
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Single, Paired }

    // Базовий клас
    public abstract class LightNode
    {
        public abstract string OuterHtml { get; }
        public abstract string InnerHtml { get; }

        // Шаблонний метод (з попереднього кроку)
        public string Render()
        {
            OnCreated();
            OnStylesApplied();
            OnClassListApplied();
            string output = OuterHtml;
            OnTextRendered();
            return output;
        }

        public virtual void OnCreated() { }
        public virtual void OnInserted() { }
        public virtual void OnStylesApplied() { }
        public virtual void OnClassListApplied() { }
        public virtual void OnTextRendered() { }
    }

    public class LightTextNode : LightNode
    {
        private string _text;
        public LightTextNode(string text) { _text = text; }
        public override string InnerHtml => _text;
        public override string OuterHtml => _text;
    }

    // === ПАТЕРН 2: ІТЕРАТОР (ITERATOR) ===
    public class LightElementNode : LightNode, IEnumerable<LightNode>
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
            node.OnInserted();
        }

        // Ітератор для обходу в глибину (DFS) - за замовчуванням
        public IEnumerator<LightNode> GetEnumerator()
        {
            foreach (var child in Children)
            {
                yield return child; // Повертаємо сам вузол
                if (child is LightElementNode element)
                {
                    foreach (var subChild in element) // Рекурсивно йдемо в глибину
                    {
                        yield return subChild;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Ітератор для обходу в ширину (BFS)
        public IEnumerable<LightNode> BreadthFirstSearch()
        {
            Queue<LightNode> queue = new Queue<LightNode>();
            foreach (var child in Children) queue.Enqueue(child);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;

                if (node is LightElementNode element)
                {
                    foreach (var child in element.Children) queue.Enqueue(child);
                }
            }
        }

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
                sb.Append($"<{TagName}");
                if (CssClasses.Count > 0) sb.Append($" class=\"{string.Join(" ", CssClasses)}\"");
                if (ClosingType == ClosingType.Single) sb.Append(" />");
                else
                {
                    sb.Append(">");
                    sb.Append(InnerHtml);
                    sb.Append($"</{TagName}>");
                }
                if (DisplayType == DisplayType.Block) sb.Append("\n");
                return sb.ToString();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Створюємо дерево: div -> (p, span -> b)
            var div = new LightElementNode("div", DisplayType.Block, ClosingType.Paired, new List<string> { "main" });
            var p = new LightElementNode("p", DisplayType.Block, ClosingType.Paired, null);
            var span = new LightElementNode("span", DisplayType.Inline, ClosingType.Paired, null);
            var b = new LightElementNode("b", DisplayType.Inline, ClosingType.Paired, null);

            div.Add(p);
            div.Add(span);
            span.Add(b);

            Console.WriteLine("=== ТЕСТ ПАТЕРНУ ІТЕРАТОР (ОБХІД ДОКУМЕНТА) ===\n");

            Console.WriteLine("--- Обхід в глибину (DFS): ---");
            foreach (var node in div)
            {
                string type = node is LightElementNode el ? $"<{el.TagName}>" : "Текст";
                Console.WriteLine($"Знайдено елемент: {type}");
            }

            Console.WriteLine("\n--- Обхід в ширину (BFS): ---");
            foreach (var node in div.BreadthFirstSearch())
            {
                string type = node is LightElementNode el ? $"<{el.TagName}>" : "Текст";
                Console.WriteLine($"Знайдено елемент: {type}");
            }

            Console.ReadLine();
        }
    }
}
