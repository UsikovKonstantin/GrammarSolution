﻿using GrammarLibrary;

// Выберите нужное действие
// MakeWords - составление слов по известным правилам (из input.txt, для отмены можно нажать Enter)
// SimplifyGrammar - приведение грамматики к приведенной форме (из input.txt) 
// ConvertToChomskyNormalForm - приведение грамматики к нормальной форме Хомского (из input.txt)  
// CompareGrammars - сравнение двух грамматик (из input.txt и input2.txt, нужно будет подождать 10с)
ActionType actionType = ActionType.MakeWords;


if (actionType == ActionType.MakeWords)
{
	GrammarSolver solver = new GrammarSolver("input.txt");
	Console.WriteLine("Нажмите Enter для отмены...");
	Console.WriteLine("Найденные слова");
	CancellationTokenSource cts = new CancellationTokenSource();
	Task solverTask = Task.Run(() => solver.Solve(cts.Token));
	Task consoleTask = Task.Run(() =>
	{
        Console.ReadLine();
		cts.Cancel();
	});
	await solverTask;
}
else if (actionType == ActionType.SimplifyGrammar)
{
	GrammarSolver solver = new GrammarSolver("input.txt");
	Console.WriteLine("Начальное состояние");
	Console.WriteLine(solver.ToString());

	solver.RemoveEmptyRules4();
	Console.WriteLine("После удаления пустых правил");
	Console.WriteLine(solver.ToString());

	solver.RemoveChainRules5();
	Console.WriteLine("После удаления цепных правил");
	Console.WriteLine(solver.ToString());

	solver.RemoveNonGenerativeCharacters1();
	Console.WriteLine("После удаления непорождающих символов");
	Console.WriteLine(solver.ToString());

	solver.RemoveUnreachableCharacters2();
	Console.WriteLine("После удаления недостижимых символов");
	Console.WriteLine(solver.ToString());

	File.WriteAllText("..\\..\\..\\input2.txt", solver.ToString());
}
else if (actionType == ActionType.ConvertToChomskyNormalForm)
{
	GrammarSolver solver = new GrammarSolver("input.txt");
    Console.WriteLine("Начальное состояние");
    Console.WriteLine(solver.ToString());

	solver.RemoveLongRules6();
	Console.WriteLine("После удаления длинных правил");
	Console.WriteLine(solver.ToString());

	solver.RemoveEmptyRules4();
	Console.WriteLine("После удаления пустых правил");
	Console.WriteLine(solver.ToString());

	solver.RemoveChainRules5();
	Console.WriteLine("После удаления цепных правил");
	Console.WriteLine(solver.ToString());

	solver.RemoveNonGenerativeCharacters1();
	Console.WriteLine("После удаления непорождающих символов");
	Console.WriteLine(solver.ToString());

	solver.RemoveUnreachableCharacters2();
	Console.WriteLine("После удаления недостижимых символов");
	Console.WriteLine(solver.ToString());

	solver.RemoveMultipleTerminals7();
	Console.WriteLine("После избавления от ситуаций, когда в правиле встречаются несколько терминалов");
	Console.WriteLine(solver.ToString());

	File.WriteAllText("..\\..\\..\\input2.txt", solver.ToString());
}
else if (actionType == ActionType.CompareGrammars)
{
	GrammarSolver solver1 = new GrammarSolver("input.txt", false);
	GrammarSolver solver2 = new GrammarSolver("input2.txt", false);
	CancellationTokenSource cts = new CancellationTokenSource();
	Task solverTask = Task.Run(() => solver1.Solve(cts.Token));
	Task solverTask2 = Task.Run(() => solver2.Solve(cts.Token));
	Thread.Sleep(10000);
	cts.Cancel();
	Console.WriteLine("Первая грамматика: за 10с найдено " + solver1.ResultWords.Count + " слов");
	Console.WriteLine("Вторая грамматика: за 10с найдено " + solver2.ResultWords.Count + " слов");
	solver1.ResultWords = solver1.ResultWords.OrderBy(w => w.Length).ThenBy(w => w).ToHashSet();
	solver2.ResultWords = solver2.ResultWords.OrderBy(w => w.Length).ThenBy(w => w).ToHashSet();
	HashSet<string> copy1;
	HashSet<string> copy2;
	if (solver1.ResultWords.Count < solver2.ResultWords.Count)
	{
		copy1 = new HashSet<string>(solver1.ResultWords);
		copy2 = new HashSet<string>(solver2.ResultWords);
	}
	else
	{
		copy1 = new HashSet<string>(solver2.ResultWords);
		copy2 = new HashSet<string>(solver1.ResultWords);
	}
	copy1.IntersectWith(copy2);
	Console.WriteLine("Совпадений: " + copy1.Count);
	Console.WriteLine("Максимальная длина совпавшего слова: " +
		copy1.OrderByDescending(i => i.Length).Select(i => i.Length).FirstOrDefault());
	int? firstMismatchLength = solver1.ResultWords.Except(solver2.ResultWords).Select(w => w.Length).FirstOrDefault();
	Console.WriteLine("Длина первого несовпавшего слова: " + (firstMismatchLength.HasValue ? firstMismatchLength.ToString() : "-"));
}
