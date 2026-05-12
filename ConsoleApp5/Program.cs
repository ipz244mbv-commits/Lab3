using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MKR1
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Single, Paired }

    // === ПАТЕРН 4: СТЕЙТ (STATE) ===
    public interface INodeState
    {
        string Render(LightElementNode context);
    }

    public class VisibleState : INodeState
    {
        public string Render(LightElementNode context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<{context.TagName}");
            if (context.CssClasses.Count > 0) sb.Append($" class=\"{string.Join(" ", context.CssClasses)}\"");
            if (context.ClosingType == ClosingType.Single) sb.Append(" />");
            else
            {
                sb.Append(">");
                sb.Append(context.InnerHtml);
                sb.Append($"</{context.TagName}>");
            }
            if (context.DisplayType == DisplayType.Block) sb.Append("\n");
            return sb.ToString();
        }
    }

    public class HiddenState : INodeState
    {
        public string Render(LightElementNode context)
        {
            return $"\n";
        }
    }

    // --- БАЗОВІ КЛАСИ ---
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

        // Стан за замовчуванням - видимий
        private INodeState _state = new VisibleState();

        public LightElementNode(string tagName, DisplayType displayType, ClosingType closingType, List<string> cssClasses)
        {
            TagName = tagName;
            DisplayType = displayType;
            ClosingType = closingType;
            CssClasses = cssClasses ?? new List<string>();
        }

        public void SetState(INodeState state)
        {
            _state = state;
            Console.WriteLine($"[State]: Стан елемента <{TagName}> змінено на {state.GetType().Name}.");
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

        // Тепер OuterHtml залежить від стану!
        public override string OuterHtml => _state.Render(this);
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== ТЕСТ ПАТЕРНУ СТЕЙТ ===\n");

            var section = new LightElementNode("section", DisplayType.Block, ClosingType.Paired, new List<string> { "content" });
            section.Add(new LightTextNode("Цей текст видно, коли стан Visible."));

            Console.WriteLine("--- Поточний рендеринг: ---");
            Console.Write(section.OuterHtml);

            // Змінюємо стан на прихований
            section.SetState(new HiddenState());

            Console.WriteLine("\n--- Рендеринг після зміни стану: ---");
            Console.Write(section.OuterHtml);

            Console.ReadLine();
        }
    }
}
