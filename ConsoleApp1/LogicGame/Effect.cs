using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.LogicGame
{
    public class Effect
    {
        public string Name { get; set; }
        public int Duration { get; set; } // Продолжительность эффекта в ходах
        public Action<IWarrior> ApplyEffect { get; set; }
        public Action<IWarrior> TickEffect { get; set; }
        public Action<IWarrior> RemoveEffect { get; set; }
        public bool IsPositive { get; set; } // true — положительный, false — отрицательный

        public Effect()
        {
            IsPositive = false; // По умолчанию отрицательный
        }
    }
    public class Dot
    {
        public static Effect Bleeding { get; private set; }
        public static Effect Defender { get; private set; }
        public static Effect Stealth { get; private set; }
        public static Effect Fire { get; private set; }
        public static Effect SnowGrave { get; private set; }
        static Dot()
        {
            Effect bleeding = new Effect
            {
                Name = "Кровотечение",
                Duration = 8,
                IsPositive = false,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} получает эффект Кровотечение! и получает 5 урона ");
                    Console.ResetColor();
                    target.DrainHP(5); // Наносит 5 урона при применении
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} теряет 2 здоровья от эффекта Кровотечение!");
                    Console.ResetColor();
                    target.DrainHP(2); // Наносит 2 урона каждый ход
                },
                RemoveEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{target.Name} избавляется от эффекта Кровотечение!");
                    Console.ResetColor();
                    // Действия при удалении эффекта, если нужно
                }
            };
            Bleeding = bleeding; // Сохранение эффекта Кровотечение в статическом поле

            Effect defender = new Effect()
            {
                Name = "Режим Крепость",
                Duration = 3,
                ApplyEffect = (warrior) =>
                {
                    warrior.IsDefending = true;
                    warrior.Armor += 10; // Увеличение брони на 10
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{warrior.Name} активирован Режим Крепости! Броня увеличена на 10.");
                    Console.ResetColor();
                },
                TickEffect = (warrior) =>
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"{warrior.Name} находится в Режиме Крепости. Осталось ходов: {warrior.ActiveEffects.FirstOrDefault(e => e.Name == "Режим Крепость")?.Duration}");
                    Console.ResetColor();
                },
                RemoveEffect = (warrior) =>
                {
                    warrior.IsDefending = false;
                    warrior.Armor -= 10; // Восстановление брони
                    Console.WriteLine($"{warrior.Name} вышел из Режима Крепости! Броня восстановлена.");
                },
                IsPositive = true // Положительный эффект
            };
            Defender = defender; // Сохранение эффекта Режим Крепость в статическом поле
            Effect stealth = new Effect()
            {
                Name = "Скрытность",
                Duration = 4,
                IsPositive = true,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{target.Name} становится невидимым благодаря эффекту Скрытность!");
                    Console.ResetColor();
                    target.EvasionChance += 0.3; // Увеличение шанса уклонения на 30%
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{target.Name} продолжает быть невидимым. Осталось ходов: {target.ActiveEffects.FirstOrDefault(e => e.Name == "Скрытность")?.Duration}");
                    Console.ResetColor();
                },
                RemoveEffect = (target) =>
                {
                    target.EvasionChance -= 0.3; // Восстановление шанса уклонения
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{target.Name} больше не невидим.");
                    Console.ResetColor();
                }
            };
            Stealth = stealth; // Сохранение эффекта Скрытность в статическом поле
            Effect fire = new Effect()
            {
                Name = "Огонь",
                Duration = 3,
                IsPositive = false,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} получает эффект Огонь! и получает 10 урона ");
                    Console.ResetColor();
                    target.DrainHP(10); // Наносит 10 урона при применении
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} теряет 3 здоровья от эффекта Огонь!");
                    Console.ResetColor();
                    target.DrainHP(3); // Наносит 3 урона каждый ход
                },
                RemoveEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{target.Name} избавляется от эффекта Огонь!");
                    Console.ResetColor();
                    // Действия при удалении эффекта, если нужно
                }
            };
            Fire = fire; // Сохранение эффекта Огонь в статическом поле
            Effect snowGrave = new Effect()
            {
                Name = "Снежная могила",
                Duration = 3,
                IsPositive = false,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"{target.Name} получает эффект Снежная могила! Он не может пошевелится (Стамина = 0) ");
                    Console.ResetColor();
                    target.DrainStamina(target.Stamina); // Сбрасывает стамину до 0
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"{target.Name} не может пошевелится (Стамина = 0) от эффекта Снежная могила!");
                    Console.ResetColor();
                    target.DrainStamina(target.Stamina); // Сбрасывает стамину до 0 каждый ход
                },
                RemoveEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{target.Name} избавляется от эффекта Снежная могила!");
                    Console.ResetColor();
                    // Действия при удалении эффекта, если нужно
                }
            };
            SnowGrave = snowGrave;

        }
    }
}
