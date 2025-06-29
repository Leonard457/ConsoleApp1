using ConsoleApp1.LogicGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.SpecialClassWarrior
{
    public class Archer : WarriorBase
    {
        public override string ClassName => "Лучник";

        new public int CountActions => 7; // Лучник может выполнять 7 действия за ход
        public Archer(string name)
             : base(
                name,
                health: 90,
                stamina: 80,
                attackDamage: 19,
                armor: 6,
                mana: 0,
                critChance: 0.25,
                evasionChance: 0.25
             )
        { }
        public void MultipleShot(IWarrior target)
        {
            int damage = (int)(AttackDamage * 1.8 / 8);
            if (Stamina >= BASE_ATTACK_STAMINA_COST + 5)
            {
                if (target.ActiveEffects.Any(e => e.Name == "Кровотечение"))
                {
                    Console.WriteLine($"{Name} наносит Множественный выстрел по {target.Name} с учётом эффекта Кровотечение!");
                    for (int i = 0; i < 8; i++)
                    {
                        if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                        {
                            continue; // Пропускаем урон, если уклонился
                        }
                        target.TakeDamage(damage, true);
                    }
                    DrainStamina(BASE_ATTACK_STAMINA_COST + 5);
                }
                else
                {
                    Console.WriteLine($"{Name} наносит Множественный выстрел по {target.Name}!");
                    for (int i = 0; i < 8; i++)
                    {
                        if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                        {
                            continue; // Пропускаем урон, если уклонился
                        }
                        bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                        target.TakeDamage(damage, isCritical);
                    }
                    DrainStamina(BASE_ATTACK_STAMINA_COST + 5);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} не хватает стамины для Множественного выстрела!");
                Console.ResetColor();
            }
        }
        public void EvasiveRoll()
        {
            if (Stamina >= DEFEND_STAMINA_COST * 2)
            {
                DrainStamina(DEFEND_STAMINA_COST * 2);
                ApplyEffect(Dot.Stealth); // Применение эффекта Скрытность
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{Name} использует Скрытность!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} не хватает стамины для Скрытность!");
                Console.ResetColor();
            }
        }

        public void AccurateShot(IWarrior target)
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5)
            {
                DrainStamina(BASE_ATTACK_STAMINA_COST * 3 + 5);
                target.ApplyEffect(Dot.Bleeding); // Применение эффекта Кровотечение
                target.TakeDamage(AttackDamage, true); // Двойной урон
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{Name} наносит Точный выстрел по {target.Name} с эффектом Кровотечение!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} не хватает стамины для Точного выстрела!");
                Console.ResetColor();
            }
        }
        // Переопределяем список действий, чтобы показать уникальные способности
        public override List<string> GetActionList()
        {
            var actions = base.GetActionList(); // Получаем базовые действия (Атака, Защита и т.д.)
            actions.Add($"5. Множественный выстрел (Стоимость: {BASE_ATTACK_STAMINA_COST + 5} стамины)");
            actions.Add($"6. Уклонение в тень (Стоимость: {DEFEND_STAMINA_COST * 2} стамины)");
            actions.Add($"7. Точный выстрел (Стоимость: {BASE_ATTACK_STAMINA_COST * 3 + 5} стамины)");
            return actions;
        }
        // Переопределяем проверку возможности выполнения действия
        public override bool CanPerformAction(int actionChoice, IWarrior target)
        {
            switch (actionChoice)
            {
                case 5: return Stamina >= BASE_ATTACK_STAMINA_COST + 5;
                case 6: return Stamina >= DEFEND_STAMINA_COST * 2;
                case 7: return Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5;
                default:
                    return base.CanPerformAction(actionChoice, target); // Для действий 1-4 используем базовую проверку
            }
        }

        // Переопределяем выполнение действия
        public override void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer)
        {
            switch (actionChoice)
            {
                case 5:
                    MultipleShot(target);
                    break;
                case 6:
                    EvasiveRoll();
                    break;
                case 7:
                    AccurateShot(target);
                    break;
                default:
                    base.ExecuteAction(actionChoice, target, isPlayer); // Для действий 1-4 используем базовую логику
                    break;
            }
        }
        public override int ChooseAiAction(IWarrior target)
        {
            // 1. ПОДГОТОВКА
            var possibleActions = new List<int>();
            for (int i = 1; i <= CountActions; i++)
            {
                if (this.CanPerformAction(i, target))
                {
                    possibleActions.Add(i);
                }
            }

            if (possibleActions.Count == 0)
            {
                return 3;
            }

            // 2. ИНИЦИАЛИЗАЦИЯ
            var actionScores = new Dictionary<int, float>();
            // 3. ОЦЕНКА
            foreach (var action in possibleActions)
            {
                float score = 0;
                switch(action)
                {
                    case 5:
                        score = 20;
                        score += Stamina >= BASE_ATTACK_STAMINA_COST + 5 ? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        score += target.ActiveEffects.Any(e => e.Name == "Кровотечение") ? 100 : 0; // Множественный выстрел
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                     case 6:
                        score = 20; 
                        score += Health > 0 ? MaxHealth / Health * 10 : 0; // Уклонение в тень
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                     case 7: // Точный выстрел
                        score = 25;
                        score += Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        if (!target.ActiveEffects.Any(e => e.Name == "Кровотечение")) 
                        {
                            score += 200; 
                        }
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                    case 1: // Атака
                        score = Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        score += RandomNumberGenerator.Next(0, 40); // Случайный бонус для атаки
                        break;
                    case 2: // Защита
                        score = Stamina >= DEFEND_STAMINA_COST ? 20 : 0;
                        score += target.Stamina > target.MaxStamina / 2 && Health < MaxHealth / 3 ? 50 : 0; // Если у противника много стамины, защита более приоритетна
                        break;
                    case 3: // Пропуск хода
                        score = Stamina > 0 ? (float)MaxStamina / Stamina * 10 : 100; // Если стамина 0 — максимальный приоритет
                        break;
                    case 4: // Лечение
                        score = (Health > 0 && Stamina >= HEAL_STAMINA_COST) ? (float)MaxHealth / Health * 10 : 0;
                        break;
                }
                actionScores[action] = score;
            }
            // 4. ВЫБОР с элементом случайности
            var finalScores = actionScores.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value + RandomNumberGenerator.Next(0, 40)
            );

            int bestAction = finalScores.OrderByDescending(kvp => kvp.Value).First().Key;
            return bestAction;
        }
    }
}
