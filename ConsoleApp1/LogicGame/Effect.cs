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
        public int Damage { get; set; } // Базовый урон, если применимо
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
        public static Effect Poison { get; private set; }
        public static Effect Stun { get; private set; } // Добавление эффекта оглушения
        public static Effect BaseDamege { get; private set; }
        static Dot()
        {
            Effect beseDamege = new Effect
            {
                Name = "beseDamege",
                Duration = 0,
                IsPositive = true,
                Damage = 0,
            };
            Effect bleeding = new Effect
            {
                Name = "Кровотечение",
                Duration = 8,
                IsPositive = false,
                Damage = 2,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} получает эффект Кровотечение! и получает {5 + beseDamege.Damage} урона ");
                    Console.ResetColor();
                    target.DrainHP(5 + beseDamege.Damage); // Наносит 5 урона при применении
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} теряет {2 + beseDamege.Damage} здоровья от эффекта Кровотечение!");
                    Console.ResetColor();
                    target.DrainHP(2 + beseDamege.Damage); // Наносит 2 урона каждый ход
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
                    Console.WriteLine($"{warrior.Name} находится в Режиме Крепости.");
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
                    target.EvasionChance += 0.6; // Увеличение шанса уклонения на 60%
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{target.Name} продолжает быть невидимым. ");
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
                Damage = 5,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} получает эффект Огонь! и получает {10 + beseDamege.Damage} урона ");
                    Console.ResetColor();
                    target.DrainHP(10 + beseDamege.Damage); // Наносит 10 урона при применении
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{target.Name} теряет {3 + beseDamege.Damage} здоровья от эффекта Огонь!");
                    Console.ResetColor();
                    target.DrainHP(5 + beseDamege.Damage); // Наносит 3 урона каждый ход
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
                Duration = 4,
                IsPositive = false,
                ApplyEffect = (target) =>
                {
                    target.EvasionChance -= 1;
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
                    target.EvasionChance += 1;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{target.Name} избавляется от эффекта Снежная могила!");
                    Console.ResetColor();
                    // Действия при удалении эффекта, если нужно
                }
            };
            SnowGrave = snowGrave;
            Effect poison = new Effect()
            {
                Name = "Яд",
                Duration = 5,
                IsPositive = false,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{target.Name} получает эффект Яд! и получает {5 + beseDamege.Damage} урона ");
                    Console.ResetColor();
                    target.DrainHP(5 + beseDamege.Damage); // Наносит 5 урона при применении
                },
                TickEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"{target.Name} теряет {3 + beseDamege.Damage} здоровья от эффекта Яд!");
                    Console.ResetColor();
                    target.DrainHP(3 + beseDamege.Damage); // Наносит 3 урона каждый ход
                },
                RemoveEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{target.Name} избавляется от эффекта Яд!");
                    Console.ResetColor();
                    // Действия при удалении эффекта, если нужно
                }
            };
            Poison = poison;
            Effect stun = new Effect()
            {
                Name = "Оглушение",
                Duration = RandomNumberGenerator.Next(1,3),
                IsPositive = false,
                ApplyEffect = (target) =>
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{target.Name} получает эффект Оглушение! Он не может действовать в этом ходу.");
                    Console.ResetColor();
                    target.DrainStamina(target.Stamina);
                    target.EvasionChance -= 1; // Сброс шанса уклонения до 0
                },
                TickEffect = (target) =>
                {
                    target.DrainStamina(target.Stamina); // Сброс стамины до 0 каждый ход
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{target.Name} продолжает быть оглушённым. Осталось ходов: {target.ActiveEffects.FirstOrDefault(e => e.Name == "Оглушение")?.Duration}");
                    Console.ResetColor();
                },
                RemoveEffect = (target) =>
                {
                    target.EvasionChance += 1; // Восстановление шанса уклонения
                    target.Stamina = target.MaxStamina / 2;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"{target.Name} избавляется от эффекта Оглушение! и востанавливает часть стамины ");
                    Console.ResetColor();
                }
            };
            Stun = stun;

        }
    }
}
