using ConsoleApp1.SpecialClassWarrior;

namespace ConsoleApp1.LogicGame
{
    public class Game
    {
        private IWarrior player1;
        private IWarrior player2;
        private bool isPlayer1Human;
        private bool isPlayer2Human;

        public void Start()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Для корректного отображения кириллицы в некоторых консолях
            Console.WriteLine("Добро пожаловать в 'Борьбу Воинов'!");
            Console.Write("Игрок 1, вы хотите участвовать? (да/нет): ");
            isPlayer1Human = Console.ReadLine().Trim().ToLower() == "да";

            player1 = CreateWarrior("Игрок 1", isPlayer1Human);

            Console.WriteLine("\n--- Создание Воина 2 ---");
            isPlayer2Human = false;
            if (isPlayer1Human)
            {
                Console.WriteLine("Игрок 2 будет компьютером.");
            }
            else
            {
                Console.WriteLine("Игрок 1 будет компьютером. Игрок 2 также будет компьютером.");
            }
            player2 = CreateWarrior("Игрок 2", isPlayer2Human);

            Console.WriteLine("\n--- Бой Начинается! ---");
            player1.DisplayStats();
            player2.DisplayStats();
            Console.WriteLine("---------------------------\n");

            IWarrior currentPlayer = player1;
            IWarrior opponent = player2;
            bool player1Turn = true;
            int turnCounter = 0; // Счетчик ходов

            while (player1.IsAlive && player2.IsAlive)
            {
                turnCounter++;
                if (turnCounter % 30 == 0)
                {
                    // Увеличиваем урон обоим бойцам
                    if (player1 is WarriorBase wb1) wb1.IncreaseAttackDamage(5);
                    if (player2 is WarriorBase wb2) wb2.IncreaseAttackDamage(5);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Прошло 30 ходов! Урон всех бойцов увеличен на +5!");
                    Console.ResetColor();
                }

                Console.WriteLine($"--- Ход {(player1Turn ? player1.Name : player2.Name)} ---");
                currentPlayer.DisplayStats(); // Показываем статы перед выбором действия
                currentPlayer.ResetTurnState();

                int actionChoice;
                bool isCurrentPlayerHuman = player1Turn ? isPlayer1Human : isPlayer2Human;

                if (isCurrentPlayerHuman)
                {
                    var actions = currentPlayer.GetActionList();
                    Console.WriteLine("Выберите действие:");
                    for (int i = 0; i < actions.Count; i++)
                    {
                        Console.WriteLine(actions[i] + (currentPlayer.CanPerformAction(i + 1, opponent) ? "" : " (Недоступно)"));
                    }

                    while (true)
                    {
                        Console.Write($"Ваш выбор (1-{actions.Count}): ");
                        if (int.TryParse(Console.ReadLine(), out actionChoice) && actionChoice >= 1 && actionChoice <= actions.Count)
                        {
                            if (currentPlayer.CanPerformAction(actionChoice, opponent))
                            {
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Недостаточно ресурсов для этого действия или оно недоступно. Попробуйте другое.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Неверный ввод. Введите число от 1 до {actions.Count}.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{currentPlayer.Name} (ИИ) обдумывает ход...");
                    Thread.Sleep(1000);
                    actionChoice = currentPlayer.ChooseAiAction(opponent); 
                    Console.WriteLine($"{currentPlayer.Name} (ИИ) выбирает действие: {currentPlayer.GetActionList()[actionChoice - 1].Split(new[] { " (" }, StringSplitOptions.None)[0].Trim()}");
                }

                currentPlayer.ExecuteAction(actionChoice, opponent, isCurrentPlayerHuman);

                Console.WriteLine();
                // Отобразим статы после хода, чтобы видеть изменения сразу
                if (player1.IsAlive) player1.DisplayStats(); else Console.WriteLine($"{player1.Name} повержен.");
                if (player2.IsAlive) player2.DisplayStats(); else Console.WriteLine($"{player2.Name} повержен.");
                Console.WriteLine("---------------------------\n");

                if (!opponent.IsAlive) break;

                if (player1Turn)
                {
                    currentPlayer = player2;
                    opponent = player1;
                }
                else
                {
                    currentPlayer = player1;
                    opponent = player2;
                }
                player1Turn = !player1Turn;
                if (player1.IsAlive && player2.IsAlive) Thread.Sleep(1500); // Пауза для читаемости, если бой продолжается
            }

            Console.WriteLine("--- Бой Окончен! ---");
            if (player1.IsAlive)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{player1.Name} победил!");
            }
            else if (player2.IsAlive)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{player2.Name} победил!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Ничья! Оба воина пали.");
            }
            Console.ResetColor();
        }
        private IWarrior CreateWarrior(string defaultNamePrefix, bool isHumanControlled)
        {
            string name;
            if (isHumanControlled)
            {
                Console.Write($"Введите имя для {defaultNamePrefix}: ");
                name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name)) name = defaultNamePrefix;
            }
            else
            {
                name = $"{defaultNamePrefix} {GetRandomBotName()}";
            }


            Console.WriteLine("Выберите класс для " + name + ":");
            Console.WriteLine("1. Рыцарь");
            Console.WriteLine("2. Крестьянин");
            Console.WriteLine("3. Лучник ");
            Console.WriteLine("4. Маг "); 

            int classChoice;
            if (isHumanControlled)
            {
                while (true)
                {
                    Console.Write("Ваш выбор (1-4): ");
                    if (int.TryParse(Console.ReadLine(), out classChoice) && classChoice >= 1 && classChoice <= 4)
                    {
                        break;
                    }
                    Console.WriteLine("Неверный ввод. Введите число от 1 до 4.");
                }
            }
            else
            {
                Console.WriteLine("ИИ выбирает класс случайным образом...(пока за него)");
                classChoice = Convert.ToInt32(Console.ReadLine()); // Случайный выбор класса для ИИ
                Console.WriteLine($"{name} (ИИ) выбирает класс номер {classChoice}.");
            }

            IWarrior selectedWarrior;
            switch (classChoice)
            {
                case 1: selectedWarrior = new Knight(name); break;
                case 2: selectedWarrior = new Peasant(name); break;
                case 3: selectedWarrior = new Archer(name); break;
                case 4: selectedWarrior = new Mage(name); break;
                default: throw new InvalidOperationException("Неверный выбор класса");
            }
            Console.WriteLine($"Создан {selectedWarrior.ClassName} {selectedWarrior.Name}");
            return selectedWarrior;
        }

        private string GetRandomBotName() // Метод для генерации случайных имен ботов
        {
            string[] names = { "Смертонос", "Крушитель", "Зак", "Сэм", "Тень", "Гром", "Астра", "Вайпер" };
            return names[RandomNumberGenerator.Next(names.Length)];
        }
        
    }
}
