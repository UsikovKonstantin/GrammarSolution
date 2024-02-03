using System.Text;

namespace GrammarLibrary;

public class GrammarSolver
{
	#region Поля
	public string StartPosition { get; private set; } = "S";
    public Dictionary<string, HashSet<string>> Rules { get; private set; } = new Dictionary<string, HashSet<string>>();
	public bool PrintLines { get; private set; } = true;

    public HashSet<string> ResultWords { get; private set; } = new HashSet<string>();
    public HashSet<string> CurrWords { get; private set; } = new HashSet<string>();
    public HashSet<string> NextWords { get; private set; } = new HashSet<string>();
	#endregion

	#region Конструкторы
	public GrammarSolver(string startPosition, Dictionary<string, HashSet<string>> rules, bool printLines = true)
    {
        StartPosition = startPosition;
        Rules = rules;
		PrintLines = printLines;
	}

    public GrammarSolver(string fileName, bool printLines = true)
    {
		PrintLines = printLines;

		string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
		if (File.Exists(filePath))
		{
			string[] lines = File.ReadAllLines(filePath);
			ParseInputLines(lines);	
		}
        else
        {
			throw new Exception("File not found: " + filePath);
		}
	}
	#endregion

	#region Методы
	public void Solve(CancellationToken cancellationToken)
    {
		CurrWords.Add(StartPosition);
		while (true) 
        {
            if (CurrWords.Count == 0)
                break;

            foreach (string word in CurrWords)
            {
				if (cancellationToken.IsCancellationRequested)
					break;

				bool canReplace = false;
                foreach (string ruleString in Rules.Keys)
                {
					int index = word.IndexOf(ruleString);
					if (index != -1)
                    {
                        foreach (string replaceWith in Rules[ruleString])
                        {
							NextWords.Add(word.Remove(index, ruleString.Length).Insert(index, replaceWith));
							canReplace = true;
						}
                    }
                }
                if (!canReplace && word.All(char.IsLower) && !ResultWords.Contains(word))
                {
					if (PrintLines)
						Console.WriteLine($"{CountLetters(word)}: {word}");
                    ResultWords.Add(word);
                }
            }

            CurrWords = new HashSet<string>(NextWords);
            NextWords = new HashSet<string>();
        }
    }

    private void ParseInputLines(string[] lines) 
    {
        foreach (string lineWithComments in lines)
        {
            string[] arr = lineWithComments.Split('#');
			string line = arr.Length == 0 ? "" : arr[0];

            if (line.Trim() == "")
                continue;

			if (line.StartsWith("start:"))
            {
				string[] parts = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 2)
                {
                    string start = parts[1].Trim();
					StartPosition = parts[1].Trim().Replace("_", "");
                }
                else
                {
                    throw new Exception("Error in string: " + line);
                }
			}
            else
            {
				string[] parts = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 2)
                {
					string[] words = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (!Rules.ContainsKey(parts[0]))
                    {
                        Rules[parts[0]] = new HashSet<string>();
                    }
					foreach (string word in words)
					{
                        Rules[parts[0]].Add(word.Replace("_", ""));
					}
				}
				else
				{
					throw new Exception("Error in string: " + line);
				}
			}
        }
    }

	private static string CountLetters(string input)
	{
		if (string.IsNullOrEmpty(input))
			return string.Empty;

		StringBuilder result = new StringBuilder();
		char currentChar = input[0];
		int count = 1;
		for (int i = 1; i < input.Length; i++)
		{
			if (input[i] == currentChar)
			{
				count++;
			}
			else
			{
				result.Append($"{count}{currentChar}");
				currentChar = input[i];
				count = 1;
			}
		}
		result.Append($"{count}{currentChar}");

		return result.ToString();
	}
	#endregion
}
