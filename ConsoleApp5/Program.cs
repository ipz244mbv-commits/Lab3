using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MKR1
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Single, Paired }

    // --- БАЗОВІ КЛАСИ (Template Method + Iterator всередині) ---
    public abstract class LightNode
    {
        public abstract string OuterHtml { get; }
        public abstract string InnerHtml { get; }

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

    public class LightElementNode : LightNode, IEnumerable<LightNode>
    {
        public string TagName { get; set; }
        public DisplayType DisplayType { get; set; }
        public ClosingType ClosingType { get; set; }
        public List<string> CssClasses { get; set; } = new List<string>();
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

        public IEnumerator<LightNode> GetEnumerator()
        {
            foreach (var child in Children)
            {
                yield return child;
                if (child is LightElementNode element)
                    foreach (var subChild in element) yield return subChild;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

    // === ПАТЕРН 3: КОМАНДА (COMMAND) ===
    public interface ICommand
    {
        void Execute();
        void Undo(); // Додамо можливість скасування для солідності
    }

    public class AddClassCommand : ICommand
    {
        private LightElementNode _node;
        private string _className;

        public AddClassCommand(LightElementNode node, string className)
        {
            _node = node;
            _className = className;
        }

        public void Execute()
        {
            if (!_node.CssClasses.Contains(_className))
            {
                _node.CssClasses.Add(_className);
                Console.WriteLine($"[Command]: Клас '{_className}' додано до <{_node.TagName}>.");
            }
        }

        public void Undo()
        {
            if (_node.CssClasses.Contains(_className))
            {
                _node.CssClasses.Remove(_className);
                Console.WriteLine($"[Command]: Клас '{_className}' видалено (Undo) з <{_node.TagName}>.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== ТЕСТ ПАТЕРНУ КОМАНДА ===\n");

            var btn = new LightElementNode("button", DisplayType.Inline, ClosingType.Paired, new List<string> { "btn" });
            Console.WriteLine("Початковий стан: " + btn.OuterHtml.Trim());

            // Створюємо та виконуємо команду
            ICommand addActive = new AddClassCommand(btn, "btn-active");
            ICommand addPrimary = new AddClassCommand(btn, "btn-primary");

            addActive.Execute();
            addPrimary.Execute();
            Console.WriteLine("Після виконання команд: " + btn.OuterHtml.Trim());

            // Скасовуємо останню дію
            addPrimary.Undo();
            Console.WriteLine("Після скасування: " + btn.OuterHtml.Trim());

            Console.ReadLine();
        }
    }
}
