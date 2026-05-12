using System;
using System.IO;

namespace AdapterPattern
{
    public interface ILogger
    {
        void Log(string message);
        void Error(string message);
        void Warn(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green; 
            Console.WriteLine($"[LOG]: {message}");
            Console.ResetColor();
        }

        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red; 
            Console.WriteLine($"[ERROR]: {message}");
            Console.ResetColor();
        }

        public void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow; 
            Console.WriteLine($"[WARN]: {message}");
            Console.ResetColor();
        }
    }

    public class FileWriter
    {
        private string _filePath;

        public FileWriter(string filePath)
        {
            _filePath = filePath;
        }

        public void Write(string text)
        {
            File.AppendAllText(_filePath, text);
        }

        public void WriteLine(string text)
        {
            File.AppendAllText(_filePath, text + Environment.NewLine);
        }
    }

    public class FileLoggerAdapter : ILogger
    {
        private FileWriter _fileWriter;

        public FileLoggerAdapter(FileWriter fileWriter)
        {
            _fileWriter = fileWriter;
        }

        public void Log(string message)
        {
            _fileWriter.WriteLine($"[LOG - SUCCESS]: {message}");
        }

        public void Error(string message)
        {
            _fileWriter.WriteLine($"[ERROR - CRITICAL]: {message}");
        }

        public void Warn(string message)
        {
            _fileWriter.WriteLine($"[WARN - WARNING]: {message}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("--- Робота ConsoleLogger ---");
            ILogger consoleLogger = new ConsoleLogger();
            consoleLogger.Log("Система успішно запущена.");
            consoleLogger.Warn("Пам'ять заповнена на 80%.");
            consoleLogger.Error("Відсутнє з'єднання з базою даних!");

            Console.WriteLine("\n--- Робота FileLoggerAdapter ---");

            string filePath = "log.txt";
            FileWriter writer = new FileWriter(filePath);
            ILogger fileLogger = new FileLoggerAdapter(writer); 

            fileLogger.Log("Цей текст записано у файл через адаптер.");
            fileLogger.Warn("Це попередження також у файлі.");
            fileLogger.Error("Критична помилка записана у файл!");

            Console.WriteLine($"Логи успішно записані у файл: {Path.GetFullPath(filePath)}");

            Console.ReadLine();
        }
    }
}