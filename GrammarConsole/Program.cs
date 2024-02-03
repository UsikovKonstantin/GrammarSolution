using GrammarLibrary;

GrammarSolver solver = new GrammarSolver("input.txt");

CancellationTokenSource cts = new CancellationTokenSource();
Task solverTask = Task.Run(() => solver.Solve(cts.Token));

Task consoleTask = Task.Run(() =>
{
	Console.WriteLine("Press Enter to cancel...");
	Console.ReadLine();
	cts.Cancel();
});

await solverTask;
