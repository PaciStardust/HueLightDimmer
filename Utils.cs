using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueLightDimmer
{
    internal static class Utils
    {
        internal static string AskQuestion(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        internal static bool AskQuestionBool(string question)
        {
            bool? answer = null;
            while (answer is null)
            {
                var str = AskQuestion($"{question} [y = yes, n = no]");
                if (str.Length > 0)
                {
                    answer = str.ToLower()[0] switch
                    {
                        'y' => true,
                        'n' => false,
                        _ => null
                    };
                }
                if (answer is null)
                {
                    Console.WriteLine("Not a valid bool!");
                }
            }
            return answer.Value;
        }

        internal static int AskQuestionInt(string question, int minInclusive, int maxInclusive)
        {
            int? number = null;
            while (number is null)
            {
                var str = AskQuestion($"{question} [min {minInclusive}, max {maxInclusive}]");
                if (int.TryParse(str, out var parsed))
                {
                    if (parsed < minInclusive || parsed > maxInclusive)
                    {
                        Console.WriteLine($"Number must be between {minInclusive} and {maxInclusive} inclusive");
                    }
                    else
                    {
                        number = parsed;
                    }
                }
                else
                {
                    Console.WriteLine("Not a valid integer!");
                }
            }
            return number.Value;
        }
    }
}
