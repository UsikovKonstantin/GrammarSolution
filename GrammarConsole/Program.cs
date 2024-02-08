using GrammarLibrary;

GrammarSolver solver = new GrammarSolver("input.txt");


// ДЛЯ СОСТАВЛЕНИЯ СЛОВ
//CancellationTokenSource cts = new CancellationTokenSource();
//Task solverTask = Task.Run(() => solver.Solve(cts.Token));
//Task consoleTask = Task.Run(() =>
//{
//	Console.WriteLine("Press Enter to cancel...");
//	Console.ReadLine();
//	cts.Cancel();
//});
//await solverTask;


// ДЛЯ ЗАДАНИЯ 3
//Console.WriteLine(solver.ToString());
//solver.RemoveLongRules6();
//solver.RemoveEmptyRules4();
//solver.RemoveChainRules5();
//solver.RemoveUselessCharacters3();
//solver.RemoveMultipleTerminals7();
//Console.WriteLine(solver.ToString());


// ДЛЯ ЗАДАНИЯ 2
//Console.WriteLine(solver.ToString());
//solver.RemoveEmptyRules4();
//solver.RemoveChainRules5();
//solver.RemoveUselessCharacters3();
//Console.WriteLine(solver.ToString());
