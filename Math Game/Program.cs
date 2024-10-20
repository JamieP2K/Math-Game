using System.Diagnostics;
using System.Timers;

Random ran = new Random();
List<(List<(int, string, int, bool)>, int, Difficulty, double)> pastGames = [];
pastGames.Clear();
System.Timers.Timer timer = new System.Timers.Timer(10);
Stopwatch stopwatch = new Stopwatch();

Console.Title = "Math Game";

Menu();

void Addition(Difficulty difficulty)
{
	Game(difficulty, "addition");
}

void Subtraction(Difficulty difficulty)
{
	Game(difficulty, "subtraction");
}

void Multiplication(Difficulty difficulty)
{
	Game(difficulty, "multiplication");
}

void Division(Difficulty difficulty)	
{
	Game(difficulty, "division");
}

void RandomGame(Difficulty difficulty)
{
	Game(difficulty, "random");
}


void DisplayScores()
{
	Console.Clear();
	Console.WriteLine($"=========================================================================");
	for (int i = 0; i < pastGames.Count; i++)
	{
		List<(int, string, int, bool)> gameRecord = pastGames[i].Item1;
		int totalScore = pastGames[i].Item2;
		Difficulty difficulty = pastGames[i].Item3;
		double time = pastGames[i].Item4;
		
		Console.WriteLine($"-------------------------------------------------------------------------\nGame {i + 1}:\nTotal Score = {totalScore, -7}   Difficulty = {difficulty, -7}   Time Taken = {time:F2}\n");
		
		foreach (var record in gameRecord)
		{
			string questionNumber = "[" + Convert.ToString(record.Item1) + "]";
			string question = record.Item2;
			int answer = record.Item3;
			string isCorrect = record.Item4 ? "YES" : "NO";
			
			Console.WriteLine($"{questionNumber, -6}   Question: {question, -14}   Your Answer: {answer, -7}   Correct? {isCorrect}");
		}
		Console.WriteLine("\n\n");
	}
	Console.WriteLine("\n\n\n\nPress any key to return to menu.");
	while (true)
	{
		Console.ReadKey();	
		break;
	}
}

void OnTimedEvent(Object source, ElapsedEventArgs e)
{
	Console.Title = $"Math Game | Time: {stopwatch.Elapsed.TotalSeconds:F2}";
}

void Game(Difficulty difficulty, string operand)
{
	int upperLimit;
	int questionNumber = 1;
	int divisorLimit;
	bool isRandomMode = false;
	int totalScore = 0;
	(int, string, int, bool) questionRecord;
	List<(int, string, int, bool)> gameRecord = [];
	

	timer.Elapsed += OnTimedEvent;
	timer.AutoReset = true;
	
	Console.WriteLine($"\n\nGet ready for 10 {operand} questions on {difficulty} mode!");
	switch (difficulty)
	{
		case Difficulty.Easy:
			divisorLimit = 13;
			upperLimit = 25;
			break;
		case Difficulty.Medium:
			divisorLimit = 51;
			upperLimit = 101;
			break;
		case Difficulty.Hard:
			divisorLimit = 251;
			upperLimit = 501;
			break;
		default:
			divisorLimit = 51;
			upperLimit = 101;
			break;
	}
	
	while (questionNumber <= 10)
	{
		int addend1 = ran.Next(1, upperLimit);
		int addend2 = ran.Next(1, upperLimit);
		Thread.Sleep(2000);
		Console.Clear();
		
		if (operand == "random")
		{
			switch(ran.Next(1, 5))
			{
				case 1:
					operand = "addition";
					break;
				case 2:
					operand = "subtraction";
					break;
				case 3:
					operand = "multiplication";
					break;
				case 4:
					operand = "division";
					break;
			}
			isRandomMode = true;
		}
		
		timer.Enabled = true;
		stopwatch.Start();
		Console.WriteLine($"Question {questionNumber}!\n\n");
		switch(operand)
		{
			case "addition":
				Console.WriteLine($"What is {addend1} + {addend2}");
				break;
			case "subtraction":
				if (addend1 < addend2) (addend1, addend2) = (addend2, addend1);
				Console.WriteLine($"What is {addend1} - {addend2}");
				break;
			case "multiplication":
				if (addend1 < addend2) (addend1, addend2) = (addend2, addend1);
				Console.WriteLine($"What is {addend1} × {addend2}");
				break;
				
			//Creates the dividend by multiplying the divider. Also ensures that the dividend is never over 100
			case "division":
				addend2 = ran.Next(1, divisorLimit); // Divisor is still capped by difficulty
				int quotient = ran.Next(1, upperLimit / addend2); // Generate a larger range for the quotient
				addend1 = addend2 * quotient; // Calculate dividend as divisor * quotient
				Console.WriteLine($"What is {addend1} ÷ {addend2}");
				break;
		}
		
		int userAnswer;
		while (true)
		{
			if (int.TryParse(Console.ReadLine(), out userAnswer))
			{
				break;
			}
			else 
			{
				Console.WriteLine("That was not a valid answer! Please enter numeric answers only (no decimals).");
			}
		}
		switch (operand)
		{
			case "addition":
				if (addend1 + addend2 == userAnswer)
				{
					Console.WriteLine("Correct!");
					gameRecord.Add((questionNumber, $"{addend1} + {addend2}", userAnswer, true));
					totalScore++;
				} 
				else 
				{
					Console.WriteLine($"Incorrect. The answer was {addend1 + addend2}.");
					gameRecord.Add((questionNumber, $"{addend1} + {addend2}", userAnswer, false));
				}
				break;
			case "subtraction":
				if (addend1 - addend2 == userAnswer)
				{
					Console.WriteLine("Correct!");
					gameRecord.Add((questionNumber, $"{addend1} - {addend2}", userAnswer, true));
					totalScore++;
				} 
				else 
				{
					Console.WriteLine($"Incorrect. The answer was {addend1 - addend2}.");
					gameRecord.Add((questionNumber, $"{addend1} - {addend2}", userAnswer, false));
				}
				break;
			case "multiplication":
				if (addend1 * addend2 == userAnswer)
				{
					Console.WriteLine("Correct!");
					gameRecord.Add((questionNumber, $"{addend1} * {addend2}", userAnswer, true));
					totalScore++;
				} 
				else 
				{
					Console.WriteLine($"Incorrect. The answer was {addend1 * addend2}.");
					gameRecord.Add((questionNumber, $"{addend1} * {addend2}", userAnswer, false));
				}
				break;
			case "division":
				if (addend1 / addend2 == userAnswer)
				{
					Console.WriteLine("Correct!");
					gameRecord.Add((questionNumber, $"{addend1} / {addend2}", userAnswer, true));
					totalScore++;
				} 
				else 
				{
					Console.WriteLine($"Incorrect. The answer was {addend1 / addend2}.");
					gameRecord.Add((questionNumber, $"{addend1} / {addend2}", userAnswer, false));
				}
				break;
		}
		questionNumber++;
		if (isRandomMode) operand = "random";
	}
		stopwatch.Stop();
		timer.Stop();
		Thread.Sleep(2000);
		Console.WriteLine($"\n\n\nYou got {totalScore} out of 10!");
		Console.WriteLine($"\nIt took you {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
		pastGames.Add((gameRecord, totalScore, difficulty, stopwatch.Elapsed.TotalSeconds));
		Thread.Sleep(2000);
		stopwatch.Reset();
		timer.Elapsed -= OnTimedEvent;
}

char HandleMenuInput()
{
	string userInput;
	char menuInput;
	while (true)
	{
		userInput = Console.ReadLine();
		if (userInput.Length == 1)
		{
			menuInput = userInput[0];
			return menuInput;
		}
		else 
		{
			Console.WriteLine("Enter an alphanumeric character");
			Thread.Sleep(0500);
		}
	}
}

void Menu()
{
	bool menuInputMade = false;
	char menuInput;
	Difficulty difficulty;

	do 
	{
		Console.Clear();
		Console.WriteLine(" --------- \n|MATH GAME|\n --------- ");
		Console.WriteLine("\n\n1. Play\n2. View Scores\n\nQ. Quit");
		menuInput = HandleMenuInput();
		switch (menuInput)
		{
			case '1':
				break;
			case '2':
				DisplayScores();
				continue;
			case 'q':
			case 'Q':
				menuInputMade = true;
				continue;
			default:
				Console.WriteLine("Pick a valid option");
				Thread.Sleep(0500);
				continue;
		}
		
		
		Console.Clear();
		Console.WriteLine("Choose your difficulty (difficulty affects range of numbers)\n\n1. Easy\n2. Medium\n3. Hard\n\nB. Back");
		menuInput = HandleMenuInput();
		switch (menuInput)
		{
			case '1':
				difficulty = Difficulty.Easy;
				break;
			case '2':
				difficulty = Difficulty.Medium;
				break;
			case '3':
				difficulty = Difficulty.Hard;
				break;
			case 'b':
			case 'B':
				continue;
			default:
				Console.WriteLine("Pick a valid option");
				Thread.Sleep(0500);
				continue;
		}
		
		Console.Clear();
		Console.WriteLine("\n\n\nPick An Option Below\n");
		Console.WriteLine("1. Addition\n2. Subtraction\n3. Multiplication\n4. Division\n5. Random\n\nB. Back");
		menuInput = HandleMenuInput();
		switch (menuInput)
		{
			case '1':
				Addition(difficulty);
				break;
			case '2':
				Subtraction(difficulty);
				break;
			case '3':
				Multiplication(difficulty);
				break;
			case '4':
				Division(difficulty);
				break;
			case '5':
				RandomGame(difficulty);
				break;
			case 'b':
			case 'B':
				continue;
			default:
				Console.WriteLine("Pick a valid option");
				Thread.Sleep(0500);
				continue;
		}
	}
	while (!menuInputMade);
}

	enum Difficulty
	{
		Easy,
		Medium,
		Hard
	}
	
	public class GameRecord
	{
		public string Operation {get; set;}
		public (int operand1, int operand2) Operands {get; set;}
		public int UserAnswer {get; set;}
		public int CorrectAnswer { get; set; }
		public bool IsCorrect { get; set; }
	}