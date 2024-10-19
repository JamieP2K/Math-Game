/*
MATH GAME

What mode do you want to play:
1. Addition
2. Subtraction
3. Multiplication
4. Division
5. Random

6. Scores
Q. Quit


What difficulty do you want to play on:
1. Easy
2. Medium
3. Hard



*/


Menu();


void Addition(Difficulty difficulty)
{
	
}

void Subtraction(Difficulty difficulty)
{
	
}

void Multiplication(Difficulty difficulty)
{
	
}

void Division(Difficulty difficulty)
{
	
}

void RandomGame(Difficulty difficulty)
{
	
}



void DisplayScores()
{
	
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
		Console.WriteLine("Choose your difficulty (difficulty affects range of numbers)\n\n1. Easy\n2. Medium\n3. Hard\n\nQ. Quit");
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
		Console.WriteLine("\n\n\nPick An Option Below\n");
		Console.WriteLine("1. Addition\n2. Subtraction\n3. Multiplication\n4. Division\n5. Random\n\n6. Scores\n7. Back\nQ. Quit");
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
			case '6':
				DisplayScores();
				break;
			case '7':
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
	}
	while (!menuInputMade);
}

	enum Difficulty
	{
		Easy,
		Medium,
		Hard
	}