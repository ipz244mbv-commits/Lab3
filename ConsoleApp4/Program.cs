using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ProxyPattern
{
    public interface ITextReader
    {
        char[][] ReadText(string filePath);
    }

    public class SmartTextReader : ITextReader
    {
        public char[][] ReadText(string filePath)
        {

            string[] lines = File.ReadAllLines(filePath);

            char[][] result = new char[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                result[i] = lines[i].ToCharArray(); 
            }
            return result;
        }
    }

    public class SmartTextChecker : ITextReader
    {
        private SmartTextReader _realReader;

        public SmartTextChecker(SmartTextReader realReader)
        {
            _realReader = realReader;
        }

        public char[][] ReadText(string filePath)
        {
            Console.WriteLine($"[Лог]: Відкриття файлу '{filePath}'...");

            char[][] result = _realReader.ReadText(filePath);

            Console.WriteLine($"[Лог]: Файл успішно прочитано.");

            int totalLines = result.Length;
            int totalChars = 0;
            foreach (var line in result)
            {
                totalChars += line.Length;
            }

            Console.WriteLine($"[Лог]: Загальна кількість рядків: {totalLines}");
            Console.WriteLine($"[Лог]: Загальна кількість символів: {totalChars}");
            Console.WriteLine($"[Лог]: Закриття файлу '{filePath}'...\n");

            return result;
        }
    }

    public class SmartTextReaderLocker : ITextReader
    {
        private ITextReader _realReader;
        private Regex _restrictedPattern;

        public SmartTextReaderLocker(ITextReader realReader, string regexPattern)
        {
            _realReader = realReader;
            _restrictedPattern = new Regex(regexPattern);
        }

        public char[][] ReadText(string filePath)
        {
            if (_restrictedPattern.IsMatch(filePath))
            {
                Console.WriteLine($"[Блокування]: Спроба доступу до '{filePath}'");
                Console.WriteLine("Access denied!\n");
                return new char[0][]; 
            }
            else
            {
                return _realReader.ReadText(filePath); 
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string publicFile = "public_document.txt";
            string secretFile = "secret_passwords.txt";
            File.WriteAllText(publicFile, "Привіт!\nЦе публічний файл.\nТут 3 рядки.");
            File.WriteAllText(secretFile, "Супер секретні дані: 12345");

            Console.WriteLine("--- ТЕСТ 1: Читання з логуванням ---");
            SmartTextReader realReader = new SmartTextReader();
            ITextReader checkerProxy = new SmartTextChecker(realReader);

            checkerProxy.ReadText(publicFile);

            Console.WriteLine("--- ТЕСТ 2: Читання з перевіркою доступу ---");
            ITextReader lockerProxy = new SmartTextReaderLocker(checkerProxy, @"secret.*\.txt$");

            lockerProxy.ReadText(publicFile);

            lockerProxy.ReadText(secretFile);

            Console.ReadLine();
        }
    }
}