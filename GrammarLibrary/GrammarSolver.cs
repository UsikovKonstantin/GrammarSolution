using System.Text;

namespace GrammarLibrary;

public class GrammarSolver
{
	#region Поля
	public string StartPosition { get; private set; } = "S";
    public Dictionary<string, HashSet<string>> Rules { get; private set; } = new Dictionary<string, HashSet<string>>();
	public bool PrintLines { get; private set; } = true;

    public HashSet<string> ResultWords { get; set; } = new HashSet<string>();
    public HashSet<string> CurrWords { get; private set; } = new HashSet<string>();
    public HashSet<string> NextWords { get; private set; } = new HashSet<string>();

	public HashSet<char> NonTerminals { get; set; } = new HashSet<char>();
	public HashSet<char> UnusedNonTerminals { get; set; } = new HashSet<char>() { 'Z', 'Y', 'X', 'W', 'V', 'U', 'T', 'S', 'R', 'Q', 'P', 'O', 'N', 'M', 'L', 'K', 'J', 'I', 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A' };
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
					{
						if (word == "")
                            Console.WriteLine("Пустое слово (эпсилон)");
						else
							Console.WriteLine($"{CountLetters(word)}: {word}");
					}
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
			line = line.Trim();

            if (line.Trim() == "")
                continue;

			if (line.StartsWith("start:"))
            {
				string[] parts = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 2)
                {
                    string start = parts[1].Trim();

					foreach (char item in start)
					{
						if (char.IsUpper(item) && char.IsLetter(item))
						{
							NonTerminals.Add(item);
							UnusedNonTerminals.Remove(item);
						}
					}

					StartPosition = parts[1].Trim().Replace("_", "");
                }
                else
                {
                    throw new Exception("Error in string: " + line);
                }
			}
			else
            {
				foreach (char item in line)
				{
					if (char.IsUpper(item) && char.IsLetter(item))
					{
						NonTerminals.Add(item);
						UnusedNonTerminals.Remove(item);
					}
				}

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
                        Rules[parts[0]].Add(word.Trim().Replace("_", ""));
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

	public override string ToString()
	{
		string res = "start: " + StartPosition + "\n";
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			string strVal = "";
			foreach(string val in values)
				strVal += (val == "" ? "_" : val) + " ";
			strVal = strVal.Substring(0, strVal.Length - 1);
			res += key + ": " + strVal + "\n";
		}
		return res;
	}

	public void RemoveNonGenerativeCharacters1()
	{
		HashSet<char> generativeCharacters = new HashSet<char>();
		int count;
		do
		{
			count = generativeCharacters.Count;
			foreach (string key in Rules.Keys)
			{
				HashSet<string> values = Rules[key];
				foreach (string value in values)
				{
					bool f = true;
					foreach (char item in value)
					{
						if (char.IsUpper(item) && char.IsLetter(item) && !generativeCharacters.Contains(item))
						{
							f = false;
							break;
						}
					}
					if (f)
					{
						generativeCharacters.Add(key[0]);
					}
				}
			}
		} while (generativeCharacters.Count != count);

		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				bool f = false;
				foreach (char item in value)
				{
					if (char.IsUpper(item) && char.IsLetter(item) && !generativeCharacters.Contains(item))
					{
						f = true;
						break;
					}
				}
				if (f)
				{
					Rules[key].Remove(value);
					if (Rules[key].Count == 0)
					{
						Rules.Remove(key);
					}
				}
			}
		}
	}

	public void RemoveUnreachableCharacters2()
	{
		HashSet<char> reachableCharacters = new HashSet<char>() { StartPosition[0] };
		int count;
		do
		{
			count = reachableCharacters.Count;
			foreach (string key in Rules.Keys)
			{
				if (!reachableCharacters.Contains(key[0]))
					continue;
				HashSet<string> values = Rules[key];
				foreach (string value in values)
				{
					foreach (char item in value)
					{
						if (char.IsUpper(item) && char.IsLetter(item) && !reachableCharacters.Contains(item))
						{
							reachableCharacters.Add(item);
						}
					}
				}
			}
		} while (reachableCharacters.Count != count);

		foreach (string key in Rules.Keys)
		{
			if (!reachableCharacters.Contains(key[0]))
			{
				Rules.Remove(key);
			}
		}
	}

	public void RemoveUselessCharacters3()
	{
		RemoveNonGenerativeCharacters1();
		RemoveUnreachableCharacters2();
	}

	private static List<List<bool>> GenerateBooleanLists(int n)
	{
		List<List<bool>> result = new List<List<bool>>();
		GenerateBooleanListsHelper(n, new List<bool>(), result);
		return result;
	}

	private static void GenerateBooleanListsHelper(int n, List<bool> currentList, List<List<bool>> result)
	{
		if (currentList.Count == n)
		{
			// Исключаем списки, состоящие только из false и только из true
			if (!currentList.All(b => !b))
			{
				result.Add(new List<bool>(currentList));
			}
			return;
		}

		// Рекурсивно генерируем все возможные комбинации
		currentList.Add(true);
		GenerateBooleanListsHelper(n, currentList, result);
		currentList.RemoveAt(currentList.Count - 1);

		currentList.Add(false);
		GenerateBooleanListsHelper(n, currentList, result);
		currentList.RemoveAt(currentList.Count - 1);
	}

	public void RemoveEmptyRules4()
	{
		HashSet<char> emptyGenerativeCharacters = new HashSet<char>();
		bool addStartEmptyRule = false;
		int count;
		do
		{
			count = emptyGenerativeCharacters.Count;
			foreach (string key in Rules.Keys)
			{
				HashSet<string> values = Rules[key];
				foreach (string value in values)
				{
					bool f = true;
					foreach (char item in value)
					{
						if (!emptyGenerativeCharacters.Contains(item))
						{
							f = false;
							break;
						}
					}
					if (f)
					{
						emptyGenerativeCharacters.Add(key[0]);
					}
				}
			}
		} while (emptyGenerativeCharacters.Count != count);

		foreach (string key in Rules.Keys)
		{
			List<string> toAdd = new List<string>();
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				List<int> indexes = new List<int>();
				for (int i = 0; i < value.Length; i++)
				{
					if (emptyGenerativeCharacters.Contains(value[i]))
					{
						indexes.Add(i);
					}
				}
				if (indexes.Count >= 1)
				{
					List<List<bool>> boolLists = GenerateBooleanLists(indexes.Count);
					foreach (var lst in boolLists)
					{
						string valueCopy = new string(value);
						for (int i = lst.Count - 1; i >= 0; i--)
						{
							if (lst[i])
							{
								valueCopy = valueCopy.Substring(0, indexes[i]) + "" + valueCopy.Substring(indexes[i] + 1);
							}
						}
						toAdd.Add(valueCopy);
                    }
				}
				if (key == StartPosition && value == "")
				{
					toAdd.Add("");
				}
			}
			foreach (var item in toAdd)
			{
				if (item == "" && key == StartPosition)
				{
					addStartEmptyRule = true;
				}
				else if (item != "")
				{
					if (key != item)
					{
						Rules[key].Add(item);
					}
				}
			}
        }

		if (addStartEmptyRule)
		{
			Rules.Add("I", new HashSet<string>() { StartPosition, "" });
			NonTerminals.Add('I');
			UnusedNonTerminals.Remove('I');
			StartPosition = "I";
		}

		foreach (string key in Rules.Keys)
		{
			if (key != StartPosition)
			{
				Rules[key].Remove("");
				if (Rules[key].Count == 0)
				{
					Rules.Remove(key);
				}
			}
		}
	}

	public static HashSet<char> GetReachableVertices(Dictionary<char, HashSet<char>> adjacencyList, char startVertex)
	{
		HashSet<char> visited = new HashSet<char>();
		Stack<char> stack = new Stack<char>();
		stack.Push(startVertex);

		while (stack.Count > 0)
		{
			char currentVertex = stack.Pop();

			if (!visited.Contains(currentVertex))
			{
				visited.Add(currentVertex);
				if (adjacencyList.ContainsKey(currentVertex))
				{
					foreach (char neighbor in adjacencyList[currentVertex])
					{
						stack.Push(neighbor);
					}
				}
			}
		}

		visited.Remove(startVertex);
		return visited;
	}

	public void RemoveChainRules5()
	{
		Dictionary<char, HashSet<char>> adjacencyList = new Dictionary<char, HashSet<char>>();
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				if (value.Length == 1 && char.IsUpper(value[0]) && char.IsLetter(value[0]))
				{
					if (!adjacencyList.ContainsKey(key[0]))
					{
						adjacencyList[key[0]] = new HashSet<char>() { value[0] };
					}
					else
					{
						adjacencyList[key[0]].Add(value[0]);
					}
				}
			}
		}

		Dictionary<char, HashSet<char>> chainRules = new Dictionary<char, HashSet<char>>();
		foreach (char nonTerminal in NonTerminals)
		{
			chainRules[nonTerminal] = GetReachableVertices(adjacencyList, nonTerminal);
		}

		foreach (char key in chainRules.Keys)
		{
			HashSet<char> values = chainRules[key];
			foreach (char value in values)
			{
				HashSet<string> rules = Rules.ContainsKey(value.ToString()) ? Rules[value.ToString()] : new HashSet<string>();
				foreach (string rule in rules)
				{
					if (rule.Length == 1 && char.IsUpper(rule[0]) && char.IsLetter(rule[0]) && chainRules[key].Contains(rule[0]))
					{
						continue;
					}
					if (!Rules.ContainsKey(key.ToString()))
					{
						Rules[key.ToString()] = new HashSet<string>() { rule };
					}
					else
					{
						Rules[key.ToString()].Add(rule);
					}
				}
			}
		}

		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				if (value.Length == 1 && char.IsUpper(value[0]) && char.IsLetter(value[0]))
				{
					Rules[key].Remove(value);
					if (Rules[key].Count == 0)
					{
						Rules.Remove(key);
					}
				}
			}
		}
	}

	public void RemoveLongRules6()
	{
		List<List<string>> rulesToAdd = new List<List<string>>();
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				if (value.Length > 2)
				{
					Rules[key].Remove(value);

					string[] chars = new string[value.Length - 1];
					for (int i = 0; i < chars.Length; i++)
					{
						chars[i] = value[i].ToString();
					}
					chars[chars.Length - 1] += value[value.Length - 1];

					string prev = key;
					char curr = UnusedNonTerminals.First();
					for (int i = 0; i < chars.Length - 1; i++)
					{
						rulesToAdd.Add(new List<string>() { prev, chars[i] + curr });
						UnusedNonTerminals.Remove(curr);
						NonTerminals.Add(curr);
						prev = curr.ToString();
						curr = UnusedNonTerminals.First();
					}

					rulesToAdd.Add(new List<string>() { prev, chars[chars.Length - 1] });
				}
			}
		}

		foreach (var ruleParts in rulesToAdd)
		{
			if (!Rules.ContainsKey(ruleParts[0]))
			{
				Rules[ruleParts[0]] = new HashSet<string>() { ruleParts[1] };
			}
			else
			{
				Rules[ruleParts[0]].Add(ruleParts[1]);
			}
		}
	}

	public void RemoveMultipleTerminals7()
	{
		Dictionary<char, char> toReplace = new Dictionary<char, char>();
		List<List<string>> rulesToAdd = new List<List<string>>();
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				if (value.Length == 2)
				{
					string s = "";

					if (char.IsLower(value[0]))
					{
						if (!toReplace.ContainsKey(value[0]))
						{
							char c = UnusedNonTerminals.First();
							toReplace[value[0]] = c;
							UnusedNonTerminals.Remove(c);
							NonTerminals.Add(c);
							rulesToAdd.Add(new List<string> { c.ToString(), value[0].ToString() });
							s += c;
						}
						else
						{
							char c = toReplace[value[0]];
							rulesToAdd.Add(new List<string> { c.ToString(), value[0].ToString() });
							s += c;
						}
					}
					else
					{
						s += value[0];
					}

					if (char.IsLower(value[1]))
					{
						if (!toReplace.ContainsKey(value[1]))
						{
							char c = UnusedNonTerminals.First();
							toReplace[value[1]] = c;
							UnusedNonTerminals.Remove(c);
							NonTerminals.Add(c);
							rulesToAdd.Add(new List<string> { c.ToString(), value[1].ToString() });
							s += c;
						}
						else
						{
							char c = toReplace[value[1]];
							rulesToAdd.Add(new List<string> { c.ToString(), value[1].ToString() });
							s += c;
						}
					}
					else
					{
						s += value[1];
					}

					if (value != s)
					{
						Rules[key].Remove(value);
						if (Rules[key].Count == 0)
						{
							Rules.Remove(key);
						}
						rulesToAdd.Add(new List<string> { key, s });
					}
				}
			}
		}

		foreach (var ruleParts in rulesToAdd)
		{
			if (!Rules.ContainsKey(ruleParts[0]))
			{
				Rules[ruleParts[0]] = new HashSet<string>() { ruleParts[1] };
			}
			else
			{
				Rules[ruleParts[0]].Add(ruleParts[1]);
			}
		}
	}
	#endregion
}
