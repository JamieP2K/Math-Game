using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;


bool voiceMode = true;

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
	int totalScore = 0;
	int questionNumber = 1;
	List<(int, string, int, bool)> gameRecord = new();
	timer.Elapsed += OnTimedEvent;
	timer.AutoReset = true;
	
	int upperLimit = GetUpperLimit(difficulty);
	int divisorLimit = GetDivisorLimit(difficulty);

	Console.WriteLine($"\n\nGet ready for 10 {operand} questions on {difficulty} mode!");
	
	while (questionNumber <= 10)
	{
		(int operand1, int operand2) = GenerateOperands(operand, upperLimit, divisorLimit);
		int correctAnswer = GetCorrectAnswer(operand, operand1, operand2);
		
		timer.Enabled = true;
		stopwatch.Start();

		Console.Clear();
		Console.WriteLine($"Question {questionNumber}!\n\n");
		Console.WriteLine($"What is {operand1} {GetOperandSymbol(operand)} {operand2}?\n");
		
		int userAnswer = GetUserInput(voiceMode);
		bool isCorrect = userAnswer == correctAnswer;
		
		// Process the result
		ProcessAnswer(gameRecord, ref totalScore, questionNumber, operand1, operand2, operand, userAnswer, correctAnswer, isCorrect);

		questionNumber++;
		if (operand == "random")
		{
			operand = GetRandomOperand();
		}
	}

	FinishGame(gameRecord, totalScore, difficulty);
}

void ProcessAnswer(List<(int, string, int, bool)> gameRecord, ref int totalScore, int questionNumber, int operand1, int operand2, string operand, int userAnswer, int correctAnswer, bool isCorrect)
{
	if (isCorrect)
	{
		Console.WriteLine("Correct!");
		totalScore++;
		Thread.Sleep(1500);
	}
	else
	{
		Console.WriteLine($"Incorrect. The answer was {correctAnswer}.");
		Thread.Sleep(1500);
	}
	gameRecord.Add((questionNumber, $"{operand1} {GetOperandSymbol(operand)} {operand2}", userAnswer, isCorrect));
}

(int, int) GenerateOperands(string operand, int upperLimit, int divisorLimit)
{
	int addend1 = ran.Next(1, upperLimit);
	int addend2 = ran.Next(1, upperLimit);

	if (operand == "division")
	{
		addend2 = ran.Next(1, divisorLimit); // Divisor is still capped by difficulty
		int quotient = ran.Next(1, upperLimit / addend2); // Generate a larger range for the quotient
		addend1 = addend2 * quotient; // Calculate dividend as divisor * quotient
	}

	return (addend1, addend2);
}

int GetCorrectAnswer(string operand, int addend1, int addend2)
{
	return operand switch
	{
		"addition" => addend1 + addend2,
		"subtraction" => addend1 - addend2,
		"multiplication" => addend1 * addend2,
		"division" => addend1 / addend2,
		_ => 0
	};
}

int GetUpperLimit(Difficulty difficulty)
{
	return difficulty switch
	{
		Difficulty.Easy => 25,
		Difficulty.Medium => 101,
		Difficulty.Hard => 501,
		_ => 101
	};
}

int GetDivisorLimit(Difficulty difficulty)
{
	return difficulty switch
	{
		Difficulty.Easy => 13,
		Difficulty.Medium => 51,
		Difficulty.Hard => 251,
		_ => 51
	};
}

string GetOperandSymbol(string operand)
{
	return operand switch
	{
		"addition" => "+",
		"subtraction" => "-",
		"multiplication" => "×",
		"division" => "÷",
		_ => "?"
	};
}

string GetRandomOperand()
{
	return new[] { "addition", "subtraction", "multiplication", "division" }[ran.Next(4)];
}

void FinishGame(List<(int, string, int, bool)> gameRecord, int totalScore, Difficulty difficulty)
{
	stopwatch.Stop();
	timer.Stop();
	Console.WriteLine($"\n\n\nYou got {totalScore} out of 10!");
	Console.WriteLine($"\nIt took you {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
	pastGames.Add((gameRecord, totalScore, difficulty, stopwatch.Elapsed.TotalSeconds));
	stopwatch.Reset();
	timer.Elapsed -= OnTimedEvent;
}

int GetUserInput(bool voiceMode)
{
	return voiceMode ? GetUserAnswer() : GetManualAnswer();
}

int GetManualAnswer()
{
	int userAnswer;
	while (true)
	{
		if (int.TryParse(Console.ReadLine(), out userAnswer))
		{
			return userAnswer;
		}
		else
		{
			Console.WriteLine("That was not a valid answer! Please enter numeric answers only (no decimals).");
		}
	}
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
			Thread.Sleep(1500);
		}
	}
}

int GetUserAnswer()
{
	// This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
	string speechKey = "ENTER KEY";
	string speechRegion = "ENTER REGION";
	if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
	{
		Console.WriteLine("Missing SPEECH_KEY or SPEECH_REGION environment variable.");
		return -1;
	}

	int result = 0;
	bool recognized = false;

	async Task OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
	{
		switch (speechRecognitionResult.Reason)
		{
			case ResultReason.RecognizedSpeech:
				Console.WriteLine($"You said: {speechRecognitionResult.Text}");

				// Clean up the recognized text
				string cleanedText = CleanSpeechText(speechRecognitionResult.Text);

				if (int.TryParse(cleanedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
				{
					recognized = true;
				}
				else
				{
					Console.WriteLine("Could not parse speech result into a valid number.");
				}
				break;

			case ResultReason.NoMatch:
				Console.WriteLine("NOMATCH: Speech could not be recognized.");
				break;

			case ResultReason.Canceled:
				var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
				Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

				if (cancellation.Reason == CancellationReason.Error)
				{
					Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
					Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
					Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
				}
				break;
		}
	}

	async Task<int> RecognizeSpeechContinuously()
	{
		var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
		speechConfig.SpeechRecognitionLanguage = "en-US";

		using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
		using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

		Console.WriteLine("Speak into your microphone.\n");

		// Subscribe to events for continuous recognition
		speechRecognizer.Recognized += async (s, e) =>
		{
			await OutputSpeechRecognitionResult(e.Result);
		};

		speechRecognizer.Canceled += (s, e) =>
		{
			Console.WriteLine($"Recognition canceled: {e.Reason}");
		};

		// Start continuous recognition
		await speechRecognizer.StartContinuousRecognitionAsync();

		// Wait for the user to speak (this loop keeps waiting until a number is recognized)
		while (!recognized)
		{
			await Task.Delay(500); // Check every half second
		}

		// Stop continuous recognition after the answer is recognized
		await speechRecognizer.StopContinuousRecognitionAsync();

		return result;
	}

	// Run continuous recognition and wait for result
	return RecognizeSpeechContinuously().GetAwaiter().GetResult();
}

// Function to clean up the recognized speech text
string CleanSpeechText(string speechText)
{
	// Remove any non-digit characters, except for hyphen (which is used in word numbers)
	string cleanedText = new string(speechText.Where(c => char.IsDigit(c) || char.IsWhiteSpace(c) || c == '-').ToArray());

	// Attempt to convert written numbers to digits (e.g., "twenty-two" -> "22")
	cleanedText = ConvertWordsToNumbers(cleanedText);

	return cleanedText.Trim();
}

// Simple conversion for common number words to digits
string ConvertWordsToNumbers(string text)
{
	var numbers = new Dictionary<string, string>
	{
		{ "zero", "0" }, { "one", "1" }, { "two", "2" }, { "three", "3" }, { "four", "4" },
		{ "five", "5" }, { "six", "6" }, { "seven", "7" }, { "eight", "8" }, { "nine", "9" },
		{ "ten", "10" }, { "eleven", "11" }, { "twelve", "12" }, { "thirteen", "13" },
		{ "fourteen", "14" }, { "fifteen", "15" }, { "sixteen", "16" }, { "seventeen", "17" },
		{ "eighteen", "18" }, { "nineteen", "19" }, { "twenty", "20" }, { "thirty", "30" },
		{ "forty", "40" }, { "fifty", "50" }, { "sixty", "60" }, { "seventy", "70" },
		{ "eighty", "80" }, { "ninety", "90" }, { "hundred", "100" }, { "thousand", "1000" }
	};

	foreach (var kvp in numbers)
	{
		text = text.Replace(kvp.Key, kvp.Value);
	}

	// Further processing could be added here to handle more complex numbers

	return text;
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
		Console.WriteLine($"\n\n1. Play\n2. View Scores\n3. Toggle Voice Control (In-Game): {voiceMode}\n\nQ. Quit");
		menuInput = HandleMenuInput();
		switch (menuInput)
		{
			case '1':
				break;
			case '2':
				DisplayScores();
				continue;
			case '3':
				voiceMode = !voiceMode;
				continue;
			case 'q':
			case 'Q':
				menuInputMade = true;
				continue;
			default:
				Console.WriteLine("Pick a valid option");
				Thread.Sleep(1500);
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
				Thread.Sleep(1500);
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
				Thread.Sleep(1500);
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