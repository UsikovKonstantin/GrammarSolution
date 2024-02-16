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

	public List<List<string>> ResultChains { get; set; } = new List<List<string>>();
    public List<List<string>> CurrChains { get; private set; } = new List<List<string>>();
    public List<List<string>> NextChains { get; private set; } = new List<List<string>>();

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

	#region Основные методы
	public void RemoveNonGenerativeCharacters1()
	{
		Console.WriteLine("УСТРАНЕНИЕ НЕПОРОЖДАЮЩИХ СИМВОЛОВ");
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

		if (generativeCharacters.Count > 0)
		{
			Console.Write("Множество порождающих нетерминалов: ");
			Console.ForegroundColor = ConsoleColor.Green;
			foreach (char item in generativeCharacters)
			{
				Console.Write(item + " ");
			}
			Console.ResetColor();
			Console.WriteLine();
		}
		else
		{
            Console.WriteLine("Порождающие нетерминалы не найдены.");
        }

		HashSet<char> nonGenerativeCharacters = new HashSet<char>();
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				foreach (char item in value)
				{
					if (char.IsUpper(item) && char.IsLetter(item) && !generativeCharacters.Contains(item))
					{
						nonGenerativeCharacters.Add(item);
					}
				}
			}
		}

		if (nonGenerativeCharacters.Count == 0)
		{
			Console.WriteLine("Непорождающие нетерминалы не найдены. Ничего удалять не нужно.\n");
			return;
		}
		else
		{
			Console.Write("Множество непорождающих нетерминалов: ");
			Console.ForegroundColor = ConsoleColor.Red;
			foreach (char item in nonGenerativeCharacters)
			{
				Console.Write(item + " ");
			}
			Console.ResetColor();
			Console.WriteLine();
		}

		Console.WriteLine("Удалим правила, содержащие непорождающие нетерминалы");
		Console.ForegroundColor = ConsoleColor.Red;
		foreach (string key in Rules.Keys)
		{
			bool deleted = false;
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				bool f = false;
				foreach (char item in value)
				{
					if (char.IsUpper(item) && char.IsLetter(item) && nonGenerativeCharacters.Contains(item))
					{
						f = true;
						break;
					}
				}
				if (f)
				{
					if (!deleted)
					{
						Console.Write($"{key}: ");
						deleted = true;
					}
                    Console.Write($"{value} ");
                    Rules[key].Remove(value);
					if (Rules[key].Count == 0)
					{
						Rules.Remove(key);
					}
				}
			}
			if (deleted)
			{
				Console.WriteLine();
			}
		}
		Console.ResetColor();

		Console.WriteLine("РЕЗУЛЬТАТ");
		Console.WriteLine(this);
	}

	public void RemoveUnreachableCharacters2()
	{
		Console.WriteLine("УСТРАНЕНИЕ НЕДОСТИЖИМЫХ СИМВОЛОВ");
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

		Console.Write("Множество достижимых нетерминалов: ");
		Console.ForegroundColor = ConsoleColor.Green;
		foreach (char item in reachableCharacters)
		{
			Console.Write(item + " ");
		}
		Console.ResetColor();
		Console.WriteLine();

		HashSet<char> nonReachableCharacters = new HashSet<char>();
		foreach (string key in Rules.Keys)
		{
			if (char.IsUpper(key[0]) && char.IsLetter(key[0]) && !reachableCharacters.Contains(key[0]))
			{
				nonReachableCharacters.Add(key[0]);
			}

			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				foreach (char item in value)
				{
					if (char.IsUpper(item) && char.IsLetter(item) && !reachableCharacters.Contains(item))
					{
						nonReachableCharacters.Add(item);
					}
				}
			}
		}

		if (nonReachableCharacters.Count == 0)
		{
			Console.WriteLine("Недостижимые нетерминалы не найдены. Ничего удалять не нужно.\n");
			return;
		}
		else
		{
			Console.Write("Множество недостижимых нетерминалов: ");
			Console.ForegroundColor = ConsoleColor.Red;
			foreach (char item in nonReachableCharacters)
			{
				Console.Write(item + " ");
			}
			Console.ResetColor();
			Console.WriteLine();
		}

		Console.WriteLine("Удалим правила, содержащие недостижимые нетерминалы в левой части");
		Console.ForegroundColor = ConsoleColor.Red;
		foreach (string key in Rules.Keys)
		{
			if (nonReachableCharacters.Contains(key[0]))
			{
                Console.Write($"{key}: ");
                HashSet<string> values = Rules[key];
				foreach (string value in values)
				{
                    Console.Write($"{value} ");
                }
                Console.WriteLine();
                Rules.Remove(key);
			}
		}
		Console.ResetColor();

		Console.WriteLine("РЕЗУЛЬТАТ");
		Console.WriteLine(this);
	}

	public void RemoveUselessCharacters3()
	{
		RemoveNonGenerativeCharacters1();
		RemoveUnreachableCharacters2();
	}

	public void RemoveEmptyRules4()
	{
        Console.WriteLine("ПРЕОБРАЗОВАНИЕ В ГРАММАТИКУ БЕЗ ЭПСИЛОН-ПРАВИЛ");
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

		if (emptyGenerativeCharacters.Count == 0)
		{
			Console.WriteLine("Эпсилон-порождающие нетерминалы не найдены. Ничего удалять не нужно.\n");
			return;
		}
		else
		{
			Console.Write("Эпсилон-порождающие нетерминалы: ");
			Console.ForegroundColor = ConsoleColor.Green;
			foreach (char item in emptyGenerativeCharacters)
			{
                Console.Write(item + " ");
            }
			Console.ResetColor();
            Console.WriteLine();
        }

		bool addedRule = false;
        foreach (string key in Rules.Keys)
		{
			Dictionary<string, HashSet<string>> toAdd = new Dictionary<string, HashSet<string>>();
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
						if (!toAdd.ContainsKey(value))
						{
							toAdd[value] = new HashSet<string>() { valueCopy };
						}
						else
						{
							toAdd[value].Add(valueCopy);
						}
                    }
				}
				if (key == StartPosition && value == "")
				{
					if (!toAdd.ContainsKey(value))
					{
						toAdd[value] = new HashSet<string>() { "" };
					}
					else
					{
						toAdd[value].Add("");
					}
				}
			}
			
			foreach (string rule in toAdd.Keys)
			{
				var vals = toAdd[rule];

				bool added = false;
				foreach (var item in vals)
				{
					if (item == "" && key == StartPosition)
					{
						addStartEmptyRule = true;
					}
					else if (item != "")
					{
						if (key != item)
						{
							if (vals.Count > 0 && !added && !Rules[key].Contains(item))
							{
								if (!addedRule)
								{
									Console.WriteLine("Добавим новые правила");
									addedRule = true;
								}
								Console.ForegroundColor = ConsoleColor.Green;
								Console.Write($"{key}: ");
								Console.ResetColor();
								added = true;
							}
							if (!Rules[key].Contains(item))
							{
								if (!addedRule)
								{
									Console.WriteLine("Добавим новые правила");
									addedRule = true;
								}
								Console.ForegroundColor = ConsoleColor.Green;
								Console.Write(item + " ");
								Console.ResetColor();
								Rules[key].Add(item);
							}
						}
					}
				}
				if (vals.Count > 0 && added)
					Console.WriteLine($"(для правила {key} -> {rule})");
			}
        }

		if (addStartEmptyRule)
		{
			if (!addedRule)
			{
				Console.WriteLine("Добавим новые правила");
				addedRule = true;
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write($"I: _ {StartPosition} ");
			Console.ResetColor();
			Console.WriteLine("(т.к. в исходной грамматике выводилось эпсилон)");
			Rules.Add("I", new HashSet<string>() { "", StartPosition });
			NonTerminals.Add('I');
			UnusedNonTerminals.Remove('I');
			StartPosition = "I";
		}

        Console.WriteLine("Удалим эпсилон-правила");
		Console.ForegroundColor = ConsoleColor.Red;
		foreach (string key in Rules.Keys)
		{
			if (key != StartPosition && Rules[key].Contains(""))
			{
                Console.WriteLine($"{key}: _");
                Rules[key].Remove("");
				if (Rules[key].Count == 0)
				{
					Rules.Remove(key);
				}
			}
		}
		Console.ResetColor();

		Console.WriteLine("РЕЗУЛЬТАТ");
		Console.WriteLine(this);
	}

	public void RemoveChainRules5()
	{
		Console.WriteLine("УСТРАНЕНИЕ ЦЕПНЫХ ПРАВИЛ");
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
		if (adjacencyList.Keys.Count == 0)
		{
			Console.WriteLine("Цепные правила не найдены. Ничего удалять не нужно.\n");
			return;
		}

		Console.WriteLine("Цепные правивила, имеющиеся непосредственно в грамматике (без базисов)");
		Console.ForegroundColor = ConsoleColor.Green;
		foreach (char key in adjacencyList.Keys)
		{
			HashSet<char> values = adjacencyList[key];
			if (values.Count > 0)
				Console.Write($"{key}: ");
			foreach (char rule in values)
			{
				Console.Write($"{rule} ");
			}
			if (values.Count > 0)
				Console.WriteLine();
		}
		Console.ResetColor();

		Dictionary<char, HashSet<char>> chainRules = new Dictionary<char, HashSet<char>>();
		foreach (char nonTerminal in NonTerminals)
		{
			chainRules[nonTerminal] = GetReachableVertices(adjacencyList, nonTerminal);
		}

		Console.WriteLine("Полное множество цепных правил (без базисов)");
		Console.ForegroundColor = ConsoleColor.Green;
		foreach (char key in chainRules.Keys)
		{
			HashSet<char> values = chainRules[key];
			if (values.Count > 0)
				Console.Write($"{key}: ");
			foreach (char rule in values)
			{
				Console.Write($"{rule} ");
			}
			if (values.Count > 0)
				Console.WriteLine();
		}
		Console.ResetColor();

		Dictionary<(string, char), List<string>> rulesToAdd = new Dictionary<(string, char), List<string>>();
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

					if (!Rules[key.ToString()].Contains(rule))
					{
						if (rulesToAdd.ContainsKey((key.ToString(), value)))
						{
							if (!rulesToAdd[(key.ToString(), value)].Contains(rule))
							{
								if (key.ToString() != rule)
								{
									rulesToAdd[(key.ToString(), value)].Add(rule);
								}
							}
						}
						else
						{
							if (key.ToString() != rule)
							{
								rulesToAdd[(key.ToString(), value)] = new List<string>() { rule };
							}
						}
					}
				}
			}
		}

		Console.WriteLine("Добавим новые правила");
		foreach ((string, char) key in rulesToAdd.Keys)
		{
			var values = rulesToAdd[key];
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write($"{key.Item1}: ");
			foreach (var item in values)
			{
				Console.Write(item + " ");
				if (!Rules.ContainsKey(key.Item1))
				{
					Rules[key.Item1] = new HashSet<string>() { item };
				}
				else
				{
					Rules[key.Item1].Add(item);
				}
			}
			Console.ResetColor();
			Console.WriteLine($"(для правила {key.Item1} -> {key.Item2})");
		}

		Console.WriteLine("Удалим цепные правила");
		Console.ForegroundColor = ConsoleColor.Red;
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			bool deleted = false;
			foreach (string value in values)
			{
				if (value.Length == 1 && char.IsUpper(value[0]) && char.IsLetter(value[0]))
				{
					if (!deleted)
					{
						Console.Write($"{key}: ");
						deleted = true;
					}
					Console.Write($"{value} ");
					Rules[key].Remove(value);
					if (Rules[key].Count == 0)
					{
						Rules.Remove(key);
					}
				}
			}
			if (deleted)
			{
				Console.WriteLine();
			}
		}
		Console.ResetColor();

		Console.WriteLine("РЕЗУЛЬТАТ");
		Console.WriteLine(this);
	}

	public void RemoveLongRules6()
	{
		Console.WriteLine("УСТРАНЕНИЕ ДЛИННЫХ ПРАВИЛ");

		bool contains = false;
        Dictionary<string, List<string>> rulesToAdd = new Dictionary<string, List<string>>();
		Dictionary<string, char> asso = new Dictionary<string, char>();
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				if (value.Length > 2)
				{
					if (!contains)
					{
						Console.WriteLine("Будем удалять длинные правила и для них добавлять короткие правила");
						contains = true;
					}

					Rules[key].Remove(value);
					Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"{key}: {value}  ");
                    Console.ResetColor();

					string[] chars = new string[value.Length - 1];
					for (int i = 0; i < chars.Length; i++)
					{
						chars[i] = value[i].ToString();
					}
					chars[chars.Length - 1] += value[value.Length - 1];

					string prev = key;
					char curr = UnusedNonTerminals.First();
					string need = value;
					bool found = false;
					for (int i = 0; i < chars.Length - 1; i++)
					{
						need = need.Substring(1, need.Length - 1);
						string kk = "";
						foreach (string k in Rules.Keys)
						{
							HashSet<string> vals = Rules[k];
							if (vals.Count == 1 && vals.First() == need)
							{
								kk = k;
								break;
							}
						}
						if (asso.ContainsKey(need))
						{
							kk = asso[need].ToString();
						}

						if (kk != "")
						{
							if (!rulesToAdd.ContainsKey(prev))
								rulesToAdd[prev] = new List<string>() { chars[i] + kk };
							else
								rulesToAdd[prev].Add(chars[i] + kk);

							Console.ForegroundColor = ConsoleColor.Green;
							Console.Write($"{prev}: {chars[i] + kk}  ");
							Console.ResetColor();
							found = true;
							break;
						}

						if (!rulesToAdd.ContainsKey(prev))
							rulesToAdd[prev] = new List<string>() { chars[i] + curr };
						else
							rulesToAdd[prev].Add(chars[i] + curr);

						asso[need] = curr;
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write($"{prev}: {chars[i] + curr}  ");
						Console.ResetColor();
						UnusedNonTerminals.Remove(curr);
						NonTerminals.Add(curr);
						prev = curr.ToString();
						curr = UnusedNonTerminals.First();
					}

					if (!found)
					{
						if (!rulesToAdd.ContainsKey(prev))
							rulesToAdd[prev] = new List<string>() { chars[chars.Length - 1] };
						else
							rulesToAdd[prev].Add(chars[chars.Length - 1]);

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"{prev}: {chars[chars.Length - 1]}");
						Console.ResetColor();
					}
					else
					{
						Console.WriteLine();
					}
				}
			}
		}

		if (!contains)
		{
			Console.WriteLine("Длинные правила не найдены. Ничего удалять не нужно.\n");
			return;
		}

		foreach (string key in rulesToAdd.Keys)
		{
			var values = rulesToAdd[key];
			foreach (var item in values)
			{
				if (!Rules.ContainsKey(key))
				{
					Rules[key] = new HashSet<string>() { item };
				}
				else
				{
					Rules[key].Add(item);
				}
			}
		}

		Console.WriteLine("РЕЗУЛЬТАТ");
		Console.WriteLine(this);
	}

	public void RemoveMultipleTerminals7()
	{
		Console.WriteLine("ИЗБАВЛЕНИЕ ОТ НЕСКОЛЬКИХ ТЕРМИНАЛОВ В ПРАВИЛЕ");
		Console.WriteLine("Будем удалять правила c несколькими терминалами и для них добавлять новые правила");
		Dictionary<char, char> toReplace = new Dictionary<char, char>();
		List<List<string>> rulesToAdd = new List<List<string>>();
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			foreach (string value in values)
			{
				string toAdd = "";
				bool deleted = false;
				if (value.Length == 2)
				{
					string s = "";

					if (char.IsLower(value[0]))
					{
						if (!deleted)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write($"{key}: {value}  ");
							Console.ResetColor();
							deleted = true;
						}
						if (!toReplace.ContainsKey(value[0]))
						{
							char c = ' ';
							foreach (string k in Rules.Keys)
							{
								HashSet<string> vals = Rules[k];
								if (vals.Count == 1 && vals.First() == value[0].ToString())
								{
									c = k[0];
									break;
								}
							}
							if (c == ' ')
							{
								c = UnusedNonTerminals.First();
								UnusedNonTerminals.Remove(c);
								NonTerminals.Add(c);
							}
		
							toReplace[value[0]] = c;
							if (!IsListInsideList(rulesToAdd, new List<string>() { c.ToString(), value[0].ToString() }))
							{
								toAdd += $"{c}: {value[0]}  ";
							}
							rulesToAdd.Add(new List<string> { c.ToString(), value[0].ToString() });
							s += c;
						}
						else
						{
							char c = toReplace[value[0]];
							if (!IsListInsideList(rulesToAdd, new List<string>() { c.ToString(), value[0].ToString() }))
							{
								toAdd += $"{c}: {value[0]}  ";
							}
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
						if (!deleted)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write($"{key}: {value}  ");
							Console.ResetColor();
							deleted = true;
						}
						if (!toReplace.ContainsKey(value[1]))
						{
							char c = ' ';
							foreach (string k in Rules.Keys)
							{
								HashSet<string> vals = Rules[k];
								if (vals.Count == 1 && vals.First() == value[1].ToString())
								{
									c = k[0];
									break;
								}
							}
							if (c == ' ')
							{
								c = UnusedNonTerminals.First();
								UnusedNonTerminals.Remove(c);
								NonTerminals.Add(c);
							}

							toReplace[value[1]] = c;
							if (!IsListInsideList(rulesToAdd, new List<string>() { c.ToString(), value[1].ToString() }))
							{
								toAdd += $"{c}: {value[1]}  ";
							}
							rulesToAdd.Add(new List<string> { c.ToString(), value[1].ToString() });
							s += c;
						}
						else
						{
							char c = toReplace[value[1]];
							if (!IsListInsideList(rulesToAdd, new List<string>() { c.ToString(), value[1].ToString() }))
							{
								toAdd += $"{c}: {value[1]}  ";
							}
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
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write($"{key}: {s}  ");
						Console.WriteLine(toAdd); 
						Console.ResetColor();
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

		Console.WriteLine("РЕЗУЛЬТАТ");
		Console.WriteLine(this);
	}
	#endregion

	#region Другие методы
	public void MakeWords(CancellationToken cancellationToken)
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

	public void MakeWordsDetailed(CancellationToken cancellationToken)
	{
		CurrChains.Add(new List<string>() { StartPosition });
		while (true)
		{
			if (CurrChains.Count == 0)
				break;

			foreach (List<string> chain in CurrChains)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				string word = chain[chain.Count - 1];

				bool canReplace = false;
				foreach (string ruleString in Rules.Keys)
				{
					int index = word.IndexOf(ruleString);
					if (index != -1)
					{
						foreach (string replaceWith in Rules[ruleString])
						{
							List<string> nextChain = new List<string>(chain);
							nextChain.Add(word.Remove(index, ruleString.Length).Insert(index, replaceWith));
							NextChains.Add(nextChain);
							canReplace = true;
						}
					}
				}
				if (!canReplace && word.All(char.IsLower) && !ResultWords.Contains(word))
				{
					if (PrintLines)
					{
						for (int i = 0; i < chain.Count - 1; i++)
						{
							Console.Write($"{(chain[i] == "" ? "_" : chain[i])} => ");
						}
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine(chain[chain.Count - 1] == "" ? "_" : chain[chain.Count - 1]);
						Console.ResetColor();
					}
					ResultChains.Add(chain);
					ResultWords.Add(word);
				}
			}

			CurrChains = new List<List<string>>(NextChains);
			NextChains = new List<List<string>>();
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

	public override string ToString()
	{
		string res = "start: " + StartPosition + "\n";
		foreach (string key in Rules.Keys)
		{
			HashSet<string> values = Rules[key];
			string strVal = "";
			foreach (string val in values)
				strVal += (val == "" ? "_" : val) + " ";
			strVal = strVal.Substring(0, strVal.Length - 1);
			res += key + ": " + strVal + "\n";
		}
		return res;
	}

	private static bool IsListInsideList<T>(List<List<T>> listOfLists, List<T> targetList)
	{
		return listOfLists.Any(list => list.SequenceEqual(targetList));
	}
	#endregion
}
