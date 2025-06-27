using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsoleApp1.LogicGame
{
    public class Game
    {
        private IWarrior player1;
        private IWarrior player2;
        private bool isPlayer1Human;
        private bool isPlayer2Human;
        private static Random random = new Random();
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
                currentPlayer.ResetTurnState(); // Сброс состояния хода

                if (currentPlayer.IsAlive)
                {
                    currentPlayer.DisplayStats();
                    opponent.DisplayStats();
                    if (isPlayer1Human && player1Turn)
                    {
                        Console.WriteLine("Выберите действие:");
                        List<string> actions = currentPlayer.GetActionList();
                        for (int i = 0; i < actions.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {actions[i]}");
                        }
                        int actionChoice;
                        while (true)
                        {
                            Console.Write("Ваш выбор: ");
                            if (int.TryParse(Console.ReadLine(), out actionChoice) && actionChoice >= 1 && actionChoice <= actions.Count)
                            {
                                break;
                            }
                            Console.WriteLine("Неверный ввод. Пожалуйста, выберите действие от 1 до " + actions.Count);
                        }
                        if (currentPlayer.CanPerformAction(actionChoice - 1, opponent))
                        {
                            currentPlayer.ExecuteAction(actionChoice - 1, opponent, true);
                        }
                        else
                        {
                            Console.WriteLine("Вы не можете выполнить это действие.");
                        }
                    }
                    else
                    {
                        int aiAction = currentPlayer.ChooseAiAction(opponent, isPlayer1Human);
                        currentPlayer.ExecuteAction(aiAction, opponent, false);
                    }
                }
                // Проверка на конец боя
                if (!opponent.IsAlive)
                {
                    Console.WriteLine($"{opponent.Name} пал в бою!");
                    break;
                }
                // Смена игроков
                var temp = currentPlayer;
                currentPlayer = opponent;
                opponent = temp;
                player1Turn = !player1Turn; // Смена хода
            }
        }

        private IWarrior CreateWarrior(string defaultNamePrefix, bool isHumanControlled) // Метод для создания воина
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
            Console.WriteLine("1. Рыцарь (HP:120, Stam:100, Dmg:12, Arm:7, Crit:7%)");
            Console.WriteLine("2. Маг (HP:80, Stam:60, Dmg:6, Arm:3, Mana:100, Crit:10%)");
            Console.WriteLine("3. Лучник (HP:90, Stam:120, Dmg:10, Arm:4, Crit:15%)");
            Console.WriteLine("4. Разбойник (HP:85, Stam:110, Dmg:9, Arm:2, Evade:20%)");
            Console.WriteLine("5. Шаман (HP:100, Stam:80, Dmg:8, Arm:5, Mana:80)");

            int classChoice;
            if (isHumanControlled)
            {
                while (true)
                {
                    Console.Write("Ваш выбор (1-5): ");
                    if (int.TryParse(Console.ReadLine(), out classChoice) && classChoice >= 1 && classChoice <= 5)
                    {
                        break;
                    }
                    Console.WriteLine("Неверный ввод. Введите число от 1 до 5.");
                }
            }
            else
            {
                classChoice = random.Next(1, 6);
                Console.WriteLine($"{name} (ИИ) выбирает класс номер {classChoice}.");
            }

            IWarrior selectedWarrior;

            switch (classChoice)
            {
                // Knight
                // Mage
                // Archer
                // Rogue
                // Shaman
                default: throw new InvalidOperationException("Неверный выбор класса");
            }
            Console.WriteLine($"Создан {selectedWarrior.ClassName} {selectedWarrior.Name}");
            return selectedWarrior;
        }
        private string GetRandomBotName() // Метод для генерации случайных имен ботов
        {
            string[] names = { "Смертонос", "Крушитель", "Зак", "Сэм", "Тень", "Гром", "Астра", "Вайпер" };
            return names[random.Next(names.Length)];
        }
        
    }
}
