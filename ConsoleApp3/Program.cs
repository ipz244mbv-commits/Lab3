using System;

namespace BridgePattern
{
    public interface IRenderer
    {
        void Render(string shapeName);
    }

    public class VectorRenderer : IRenderer
    {
        public void Render(string shapeName)
        {
            Console.WriteLine($"Drawing {shapeName} as lines (Vector graphics).");
        }
    }

    public class RasterRenderer : IRenderer
    {
        public void Render(string shapeName)
        {
            Console.WriteLine($"Drawing {shapeName} as pixels (Raster graphics).");
        }
    }

    public abstract class Shape
    {
        protected IRenderer _renderer;

        public Shape(IRenderer renderer)
        {
            _renderer = renderer;
        }

        public abstract void Draw();
    }

    public class Circle : Shape
    {
        public Circle(IRenderer renderer) : base(renderer) { }
        public override void Draw() => _renderer.Render("Circle");
    }

    public class Square : Shape
    {
        public Square(IRenderer renderer) : base(renderer) { }
        public override void Draw() => _renderer.Render("Square");
    }

    public class Triangle : Shape
    {
        public Triangle(IRenderer renderer) : base(renderer) { }
        public override void Draw() => _renderer.Render("Triangle");
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            IRenderer vectorRenderer = new VectorRenderer();
            IRenderer rasterRenderer = new RasterRenderer();

            Console.WriteLine("--- Векторна графіка ---");
            Shape vectorCircle = new Circle(vectorRenderer);
            Shape vectorSquare = new Square(vectorRenderer);

            vectorCircle.Draw();
            vectorSquare.Draw();

            Console.WriteLine("\n--- Растрова графіка ---");
            Shape rasterTriangle = new Triangle(rasterRenderer);
            Shape rasterCircle = new Circle(rasterRenderer);

            rasterTriangle.Draw();
            rasterCircle.Draw();

            Console.ReadLine();
        }
    }
}