using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.CoreAudioApi;
using Spectre.Console;

//Used to determine whether input for answers will be through voice control or not. Toggled in the main menu
bool voiceMode = false;

//Used to determine whether a game has been finished, directing the user to the main menu.
bool gameFinised = false;

AudioConfig? selectedMic = null;
string selectedMicName = "None";

Random ran = new Random();
List<(List<(int, string, int, bool)>, int, Difficulty, double)> pastGames = [];
pastGames.Clear();
System.Timers.Timer timer = new System.Timers.Timer(10);
Stopwatch stopwatch = new Stopwatch();

Console.Title = "Math Game";

Menu();

//Methods to allow starting a game in a certain difficulty and operator
bool Addition(Difficulty difficulty)
{
	Game(difficulty, "addition", false);
	return false;
}

bool Subtraction(Difficulty difficulty)
{
	Game(difficulty, "subtraction", false);
	return false;
}

bool Multiplication(Difficulty difficulty)
{
	Game(difficulty, "multiplication", false);
	return false;
}

bool Division(Difficulty difficulty)	
{
	Game(difficulty, "division", false);
	return false;
}

bool RandomGame(Difficulty difficulty)
{
	Game(difficulty, "random", true);
	return false;
}

//Displays all recorded games. Games come in the form of a list containing questions, answers and whether the answer was correct. This is so that more indepth information can be stored from any game rather than basic information such as score.
void DisplayScores()
{
	Console.Clear();
	AnsiConsole.Write(new FigletText("Scoreboard").Centered().Color(Color.Red));
	AnsiConsole.Write(new Rule().RuleStyle("red dim"));
	
	var grid = new Grid();
	for (int i = 0; i < 4; i++)
	{
		grid.AddColumn();
	}
	
	for (int i = 0; i < pastGames.Count; i++)
	{
		List<(int, string, int, bool)> gameRecord = pastGames[i].Item1;
		int totalScore = pastGames[i].Item2;
		Difficulty difficulty = pastGames[i].Item3;
		double time = pastGames[i].Item4;
		
		grid.AddRow(new Text[]
		{
			new Text(" ", new Style(Color.Red, Color.Black)).LeftJustified(),
		});
		
		grid.AddRow(new Text[]
		{
			new Text($"Game {i + 1}:", new Style(Color.Red, Color.Black)).LeftJustified(),
		});
		
		grid.AddRow(new Text[]
		{
			new Text($"Difficulty: {difficulty}", new Style(Color.SeaGreen3, Color.Black)).LeftJustified(),
			new Text($"Time Taken: {time:F2}", new Style(Color.SeaGreen3, Color.Black)).LeftJustified(),
			new Text($"Score: {totalScore} / 10", new Style(Color.SeaGreen3, Color.Black)).LeftJustified()
		});
		
		foreach (var record in gameRecord)
		{
			string questionNumber = Convert.ToString(record.Item1);
			string question = record.Item2;
			string answer = Convert.ToString(record.Item3);
			bool isCorrect = record.Item4;
			
			Style style = isCorrect ? new Style(Color.Green, Color.Black) : new Style(Color.Red, Color.Black);
			
			
			grid.AddRow(new Text[]
			{
			new Text($"({questionNumber})").RightJustified(),
			new Text($"{question}").LeftJustified(),
			new Text($"{answer}", style).LeftJustified()
			});
		}
	}
	
	AnsiConsole.Write(grid);
	
	Console.WriteLine("\n\n\n\nPress any key to return to menu.");
	while (true)
	{
		Console.ReadKey();	
		break;
	}
}


//Used by the timer to update the time displayed on the app's title in real time
void OnTimedEvent(Object source, ElapsedEventArgs e)
{
	Console.Title = $"Math Game | Time: {stopwatch.Elapsed.TotalSeconds:F2}";
}

void Game(Difficulty difficulty, string operand, bool isRandomMode)
{
	int totalScore = 0;
	int questionNumber = 1;
	List<(int, string, int, bool)> gameRecord = new();
	timer.Elapsed += OnTimedEvent;
	timer.AutoReset = true;
	
	int upperLimit = GetUpperLimit(difficulty);
	int divisorLimit = GetDivisorLimit(difficulty);
	
	while (questionNumber <= 10)
	{
		if (isRandomMode)
		{
			operand = GetRandomOperand();
		}
		
		(int operand1, int operand2) = GenerateOperands(operand, upperLimit, divisorLimit);
		int correctAnswer = GetCorrectAnswer(operand, operand1, operand2);
		
		timer.Enabled = true;
		stopwatch.Start();

		Console.Clear();
		
		AnsiConsole.Write(new Rule($"[red]Question {questionNumber}[/]").LeftJustified().RuleStyle("red"));
		Console.WriteLine($"What is {operand1} {GetOperandSymbol(operand)} {operand2}?\n");
		
		int userAnswer = GetUserInput(voiceMode);
		bool isCorrect = userAnswer == correctAnswer;
		if (isCorrect) totalScore++;
		
		// Process the result
		ProcessAnswer(gameRecord, ref totalScore, questionNumber, operand1, operand2, operand, userAnswer, correctAnswer, isCorrect);

		questionNumber++;
	}

	FinishGame(gameRecord, totalScore, difficulty);
}

//Checks the answer to determine correctness. Then stores the question and answer as a game record to be displayed later.
void ProcessAnswer(List<(int, string, int, bool)> gameRecord, ref int totalScore, int questionNumber, int operand1, int operand2, string operand, int userAnswer, int correctAnswer, bool isCorrect)
{
	gameRecord.Add((questionNumber, $"{operand1} {GetOperandSymbol(operand)} {operand2} = {correctAnswer}", userAnswer, isCorrect));
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
	if (operand == "subtraction" && addend1 < addend2)
	{
		(addend1, addend2) = (addend2, addend1);
	}
	if (operand == "multiplication")
	{
		addend1 = ran.Next(2, divisorLimit / 3);
		addend2 = ran.Next(2, divisorLimit);
		if (addend1 < addend2) (addend1, addend2) = (addend2, addend1);
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
		Difficulty.Easy => 26,
		Difficulty.Medium => 101,
		Difficulty.Hard => 501,
		_ => 101
	};
}


//Used to generate more fair division and multiplication questions
int GetDivisorLimit(Difficulty difficulty)
{
	return difficulty switch
	{
		Difficulty.Easy => 13,
		Difficulty.Medium => 25,
		Difficulty.Hard => 101,
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
	pastGames.Add((gameRecord, totalScore, difficulty, stopwatch.Elapsed.TotalSeconds));
	stopwatch.Reset();
	timer.Elapsed -= OnTimedEvent;
	Console.Title = "Math Game";
	DisplayScores();
	gameFinised = true;
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
			AnsiConsole.Write(new Text("Enter whole numeric answers only.\n", new Style(Color.Red3, Color.Black)).LeftJustified());
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

		using var audioConfig = selectedMic;
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

	return text;
}


/*
MENUS
*/

void Menu()
{
	bool menuRunning = true;
	bool defaultMic = true;

	while (menuRunning)
	{
		Console.Clear();
		ShowMainTitle();

		// Dictionary is used here to allow the text in the menu and the string used in logic to differ (e.g OFF and ON in toggle voice)
		var choices = new Dictionary<string, string>
		{
			{ "Play", "Play" },
			{ "View Scores", "View Scores" },
			{ "Toggle Voice", voiceMode ? "Toggle Voice: [green]ON[/]" : "Toggle Voice: [red]OFF[/]" },
			{"Mic Settings", "Microphone Settings"},
			{ "Quit", "[red]Quit[/]" }
		};

		if (selectedMicName == "None")
		{
			choices["Toggle Voice"] = "Toggle Voice: [red]OFF[/] [yellow](SELECT A MICROPHONE BEFORE ENABLING)[/]";
		}

		var input = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title("\nPick an Option")
				.WrapAround(true)
				.PageSize(10)
				.MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
				.AddChoices(choices.Keys)
				.UseConverter(choice => choices[choice]));


		// Switch case logic with simple values
		switch (input)
		{
			case "Play":
				PlayMenu();
				break;
			case "View Scores":
				DisplayScores();
				break;
			case "Toggle Voice":
				if (selectedMicName == "None") {break;}
				ToggleVoiceMode();
				break;
			case "Mic Settings":
				MicMenu();
				break;
			case "Quit":
				Console.Clear();
				menuRunning = false; // Ends the loop and quits the program
				break;
		}
	}
}

void MicMenu()
{
	// Enumerate all input devices (microphones)
	var enumerator = new MMDeviceEnumerator();
	var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
	
	
	//Checks to see if there is any microphones, if not then returns to the menu.
	//Prevents crashes from no microphone
	if (devices.Count == 0) 
	{
		Console.WriteLine("No Microphones Detected\n\nPress Any Key To Return");
		while (true)
		{
			Console.ReadKey();
			return;
		}
	}
	var choices = new Dictionary<Object, string>{};
	
	
	for (int i = 0; i < devices.Count; i++)
	{
		choices.Add(devices[i], devices[i].FriendlyName);
	}
	
	choices.Add(new object(), "[red]Go Back[/]");

	var input = AnsiConsole.Prompt(
		new SelectionPrompt<Object>()
			.Title($"\nPick Microphone (Current Microphone: {selectedMicName})")
			.WrapAround(true)
			.PageSize(10)
			.MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
			.AddChoices(choices.Keys)
			.UseConverter(choice => choices[choice]));
	
	if (!(input is MMDevice)) {return;}

	MMDevice mic = (MMDevice)input;
	
	
	// Select the microphone
	selectedMic = AudioConfig.FromMicrophoneInput(mic.ID);
	selectedMicName = mic.FriendlyName;
}


void PlayMenu()
{
	while (true)
	{
		Difficulty difficulty = ChooseDifficulty();

		// Check if user wants to go back to the Main Menu
		if (difficulty == Difficulty.Invalid)
		{
			break; // Exit to the Main Menu
		}

		// Loop for game modes until the user selects "Back"
		while (true)
		{
			if (ChooseGameMode(difficulty)) // This will return true if the user chooses to go back
			{
				break; // Go back to the Difficulty Menu
			}
			else
			{
				// If we get here, it means a game was played and finished, so we break to go back to the Main Menu
				break; // This will take us out to the PlayMenu's while loop, sending us back to the Main Menu
			}
		}
	}
}


Difficulty ChooseDifficulty()
{
	
	if (gameFinised)
	{
		gameFinised = false;
		return Difficulty.Invalid;
	}
	var choices = new Dictionary<string, string>
	{
		{ "Easy", "Easy" },
		{ "Medium", "Medium" },
		{ "Hard", "Hard" },
		{ "Back", "[red]Go Back[/]" }
	};

	var input = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("\nChoose Difficulty")
			.WrapAround(true)
			.PageSize(10)
			.MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
			.AddChoices(choices.Keys)
			.UseConverter(choice => choices[choice]));
			

	switch (input)
	{
		case "Easy":
			return Difficulty.Easy;
		case "Medium":
			return Difficulty.Medium;
		case "Hard":
			return Difficulty.Hard;
		case "Back":
			return Difficulty.Invalid; // Return Invalid to indicate going back
		default:
			return Difficulty.Invalid;
	}
}


bool ChooseGameMode(Difficulty difficulty)
{
	var choices = new Dictionary<string, string>
	{
		{ "Addition", "Addition" },
		{ "Subtraction", "Subtraction" },
		{ "Multiplication", "Multiplication" },
		{ "Division", "Division" },
		{ "Random", "Random"},
		{ "Back", "[red]Go Back[/]"}
	};

	var input = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("\nChoose Game Mode")
			.WrapAround(true)
			.PageSize(10)
			.MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
			.AddChoices(choices.Keys)
			.UseConverter(choice => choices[choice]));

	switch (input)
	{
		case "Addition":
			Addition(difficulty); // You may want to pass the score here
			return false; // Game finished, return to Main Menu
		case "Subtraction":
			Subtraction(difficulty);
			return false; // Game finished, return to Main Menu
		case "Multiplication":
			Multiplication(difficulty);
			return false; // Game finished, return to Main Menu
		case "Division":
			Division(difficulty);
			return false; // Game finished, return to Main Menu
		case "Random":
			RandomGame(difficulty);
			return false; // Game finished, return to Main Menu
		case "Back":
			return true; // Go back to the Difficulty Menu
		default:
			return true; // Stay in the game mode menu if unexpected input
	}
}


void ShowMainTitle()
{
	AnsiConsole.Write(new FigletText("Math Game").Centered().Color(Color.Red));
	AnsiConsole.Write(new Rule().RuleStyle("red dim"));
}


void ToggleVoiceMode()
{
	voiceMode = !voiceMode;
}

	enum Difficulty
	{
		Easy,
		Medium,
		Hard,
		Invalid
}