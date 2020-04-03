using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TRRemoveDiacritics.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputPath = args.ElementAtOrDefault(0);
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                throw new ArgumentNullException(nameof(inputPath), "The input path is required.");
            }
            
            var outputPath = args.ElementAtOrDefault(1);
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                throw new ArgumentNullException(nameof(inputPath), "The output path is required.");
            }

            var encodingText = args.ElementAtOrDefault(2) ?? "windows-1252";

            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"The file was not found in {inputPath}.");
            }

            if (File.Exists(outputPath))
            {
                File.Move(outputPath, $"{outputPath}-{DateTime.Now:yy-MM-dd-HH-mm}");
            }

            Console.WriteLine("==========================================================================");
            Console.WriteLine("Input: {0}", inputPath);
            Console.WriteLine("Output: {0}", outputPath);
            Console.WriteLine("Encoding: {0}", encodingText);
            Console.WriteLine("==========================================================================");
            Console.WriteLine();
            
            if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(outputPath))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
            }
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var encoding = Encoding.GetEncoding(encodingText);
            
            using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(input, encoding);
            using var output = File.OpenWrite(outputPath);
            using var writer = new StreamWriter(output, encoding);

            while (reader.Peek() > 0)
            {
                writer.WriteLine(RemoveIncorrectCharacters(RemoveDiacritics(reader.ReadLine())));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("The file was successfully generated.");
            Console.ResetColor();

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.Error.WriteLine(((Exception) eventArgs.ExceptionObject).Message);
            };
        }

        private static string RemoveIncorrectCharacters(string text)
        {
            return Regex.Replace(text, "[^; \\w]", string.Empty);
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        } 
    }
}
