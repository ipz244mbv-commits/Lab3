using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MKR1
{
    public enum DisplayType { Block, Inline }
    public enum ClosingType { Single, Paired }

    // === ПАТЕРН 5: ВІДВІДУВАЧ (VISITOR) ===
    public interface IVisitor
    {
        void Visit(LightElementNode element);
        void Visit(LightTextNode text);
    }

    public class StatisticsVisitor : IVisitor
    {
        public int ElementCount { get; private set; } = 0;
        public int TextCount { get; private set; } = 0;

        public void Visit(LightElementNode element) => ElementCount++;
        public void Visit(LightTextNode text) => TextCount++;
        
        public void PrintReport() => Console.WriteLine($"[Visitor]: Всього елементів: {ElementCount}, Текстових вузлів: {TextCount}");
    }

    // === ПАТЕРН 4: СТЕЙТ (STATE) ===
    public interface INodeState { string Render(LightElementNode context); }
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
    public class HiddenState : INodeState { public string Render(LightElementNode context) => $"\n"; }

    // --- БАЗОВІ КЛАСИ ---
    public abstract class LightNode
    {
        public abstract string OuterHtml { get; }
        public abstract string InnerHtml { get; }
        
        // ПАТЕРН 5: Прийняття відвідувача
        public abstract void Accept(IVisitor visitor);

        // ПАТЕРН 1: ШАБЛОННИЙ МЕТОД
        public string Render()
        {
            OnCreated();
            OnStylesApplied();
            string output = OuterHtml;
            OnTextRendered();
            return output;
        }

        public virtual void OnCreated() { }
        public virtual void OnInserted() { }
        public virtual void OnStylesApplied() { }
        public virtual void OnTextRendered() { }
    }

    public class LightTextNode : LightNode
    {
        private string _text;
        public LightTextNode(string text) { _text = text; }
        public override string InnerHtml => _text;
        public override string OuterHtml => _text;
        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class LightElementNode : LightNode, IEnumerable<LightNode>
    {
        public string TagName { get; set; }
        public DisplayType DisplayType { get; set; }
        public ClosingType ClosingType { get; set; }
        public List<string> CssClasses { get; set; } = new List<string>();
        public List<LightNode> Children { get; set; } = new List<LightNode>();

        private INodeState _state = new VisibleState();

        public LightElementNode(string tagName, DisplayType displayType, ClosingType closingType)
        {
            TagName = tagName;
            DisplayType = displayType;
            ClosingType = closingType;
        }

        public void SetState(INodeState state) => _state = state;

        public void Add(LightNode node)
        {
            Children.Add(node);
            node.OnInserted();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in Children) child.Accept(visitor);
        }

        // ПАТЕРН 2: ІТЕРАТОР
        public IEnumerator<LightNode> GetEnumerator()
        {
            foreach (var child in Children)
            {
                yield return child;
                if (child is LightElementNode element) foreach (var sub in element) yield return sub;
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

        public override string OuterHtml => _state.Render(this);
    }

    // ПАТЕРН 3: КОМАНДА
    public class AddClassCommand
    {
        private LightElementNode _node;
        private string _class;
        public AddClassCommand(LightElementNode node, string cls) { _node = node; _class = cls; }
        public void Execute() { if (!_node.CssClasses.Contains(_class)) _node.CssClasses.Add(_class); }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Створюємо структуру
            var body = new LightElementNode("body", DisplayType.Block, ClosingType.Paired);
            var h1 = new LightElementNode("h1", DisplayType.Block, ClosingType.Paired);
            h1.Add(new LightTextNode("Модульна робота №1"));
            body.Add(h1);

            Console.WriteLine("=== ФІНАЛЬНИЙ ТЕСТ: ВСІ 5 ПАТЕРНІВ ПРАЦЮЮТЬ ===\n");

            // 1. Тест Команди
            new AddClassCommand(body, "main-page").Execute();
            
            // 2. Тест Ітератора
            Console.WriteLine("--- Обхід дерева (Iterator): ---");
            foreach (var node in body) Console.WriteLine($"Знайдено: {node.GetType().Name}");

            // 3. Тест Стейту
            h1.SetState(new HiddenState());
            Console.WriteLine("\n--- Рендеринг (Template Method + State): ---");
            Console.WriteLine(body.Render());

            // 4. Тест Відвідувача
            var stats = new StatisticsVisitor();
            body.Accept(stats);
            stats.PrintReport();

            Console.ReadLine();
        }
    }
}
