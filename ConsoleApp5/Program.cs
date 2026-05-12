using System;
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

        // === ПАТЕРН 1: ШАБЛОННИЙ МЕТОД (TEMPLATE METHOD) ===
        // Визначає жорсткий скелет алгоритму рендерингу
        public string Render()
        {
            OnCreated();
            OnStylesApplied();
            OnClassListApplied();
            
            string output = OuterHtml; // Основна робота
            
            OnTextRendered();
            return output;
        }

        // Хуки життєвого циклу (пусті за замовчуванням)
        public virtual void OnCreated() { }
        public virtual void OnInserted() { }
        public virtual void OnRemoved() { }
        public virtual void OnStylesApplied() { }
        public virtual void OnClassListApplied() { }
        public virtual void OnTextRendered() { }
    }

    // Текстовий вузол
    public class LightTextNode : LightNode
    {
        private string _text;
        public LightTextNode(string text) { _text = text; }
        public override string InnerHtml => _text;
        public override string OuterHtml => _text;
    }

    // Елемент розмітки
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
            node.OnInserted(); // Викликаємо хук при додаванні в дерево
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

    // --- КЛАС ДЛЯ ДЕМОНСТРАЦІЇ ХУКІВ ---
    public class TrackedElementNode : LightElementNode
    {
        public TrackedElementNode(string tagName, DisplayType displayType, ClosingType closingType, List<string> cssClasses) 
            : base(tagName, displayType, closingType, cssClasses) { }

        // Перевизначаємо хуки, щоб побачити їх в консолі
        public override void OnCreated() => Console.WriteLine($"[Hook]: Елемент <{TagName}> готується до рендерингу.");
        public override void OnInserted() => Console.WriteLine($"[Hook]: Елемент вставлено в DOM.");
        public override void OnStylesApplied() => Console.WriteLine($"[Hook]: До <{TagName}> застосовано базові стилі.");
        public override void OnClassListApplied() => Console.WriteLine($"[Hook]: Перевірка CSS класів: {string.Join(", ", CssClasses)}");
        public override void OnTextRendered() => Console.WriteLine($"[Hook]: Вміст <{TagName}> успішно відрендерено на екран!\n");
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            Console.WriteLine("=== ПАТЕРН 1: ШАБЛОННИЙ МЕТОД (ЖИТТЄВИЙ ЦИКЛ) ===\n");
            
            // Створюємо елемент, який відслідковує свій життєвий цикл
            TrackedElementNode div = new TrackedElementNode("div", DisplayType.Block, ClosingType.Paired, new List<string> { "container", "highlight" });
            
            LightTextNode text = new LightTextNode("Привіт, це тест Шаблонного методу!");
            
            Console.WriteLine("--- Додаємо елемент ---");
            div.Add(text); // Викличе OnInserted

            Console.WriteLine("\n--- Починаємо рендеринг ---");
            // Викликаємо шаблонний метод Render() замість звичайного OuterHtml
            string result = div.Render(); 
            
            Console.WriteLine("--- Результат HTML ---");
            Console.WriteLine(result);

            Console.ReadLine();
        }
    }
}
