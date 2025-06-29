using ConsoleApp1.LogicGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.SpecialClassWarrior
{
    public class Mage : WarriorBase
    {
        public override string ClassName => "Маг";
        new public int CountActions => 7; // Количество действий в текущем ходе
        public Mage(string name)
            : base(
                name,
                health: 80,
                stamina: 70,
                attackDamage: 16,
                armor: 6,
                mana: 100,
                critChance: 0.15,
                evasionChance: 0.15
            )
        { }
        // Дополнительные методы и свойства для класса Маг могут быть добавлены здесь
        public void Fireball(IWarrior target)
        {
            if (Mana >= 25 && Stamina >= BASE_ATTACK_STAMINA_COST)
            {
                DrainMana(25);
                DrainStamina(BASE_ATTACK_STAMINA_COST);
                int damage = (int)(AttackDamage * 1.5);
                bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                {
                    Console.WriteLine($"{Name} промахивается по {target.Name} с Огненным шаром!");
                }
                else
                {
                    if (RandomNumberGenerator.NextDouble() < 0.95 || isCritical)
                    {
                        target.ApplyEffect(Dot.Fire);
                        target.TakeDamage(damage, isCritical);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{Name} наносит  урон Огненным шаром по {target.Name} и накладывает эффект Огня!");
                        Console.ResetColor();
                    }
                    else
                    {
                        target.TakeDamage(damage, isCritical);
                        Console.WriteLine($"{Name} наносит урон Огненным шаром по {target.Name}!");
                    }
                }


            }
            else
            {
                Console.WriteLine($"{Name} недостаточно маны или стамины для использования Огненного шара!");
            }
        }
        public void Blizzard(IWarrior target)
        {
            if (Mana >= 65 && Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5)
            {
                DrainMana(65);
                DrainStamina(BASE_ATTACK_STAMINA_COST * 3 + 5);
                int damage = (int)(AttackDamage * 1.2);
                bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                {
                    Console.WriteLine($"{Name} промахивается по {target.Name} с Ледяным штормом!");
                }
                else
                {
                    target.ActiveEffects.Add(Dot.SnowGrave);
                    target.TakeDamage(damage, isCritical);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"{Name} наносит урон Ледяным штормом по {target.Name} и накладывает эффект Ледяной могилы!");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine($"{Name} недостаточно маны или стамины для использования Ледяного шторма!");
            }
        }
        public void SoulControl(IWarrior target)
        {
            if (Mana >= 75 && Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5)
            {
                DrainMana(75);
                DrainStamina(BASE_ATTACK_STAMINA_COST * 3);
                int damage = (int)(AttackDamage * 1.5);
                bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                MaxHealth += 30; // Увеличиваем максимальное здоровье на 30
                Health = Math.Min(MaxHealth, Health + 40);
                AttackDamage += 2; // Увеличиваем урон на 2
                Armor += 1; // Увеличиваем броню на 2

                ActiveEffects.RemoveAll(effect => !effect.IsPositive);
                target.ActiveEffects.RemoveAll(effect => effect.IsPositive);

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine($"{Name} использует Контроль души на {target.Name}!");
                Console.WriteLine($"{Name} восстанавливает 40 здоровья, увеличивает максимальное здоровье на 30 и урон с бронёй на 2!");
                Console.WriteLine($"{Name} Сбросил с себя все негативные эффекты ,а с {target.Name} все положительные");
                Console.ResetColor();
                target.TakeDamage(damage, isCritical);
                Console.WriteLine($"{Name} наносит урон Контролем души по {target.Name}!");
            }
            else
            {
                Console.WriteLine($"{Name} недостаточно маны или стамины для использования Контроля души!");
            }
        }
        public override void PerformSkipTurn()
        {
            Mana = Math.Min(MaxMana, Mana + 30); // Восстанавливаем 30 маны за пропуск хода
            base.PerformSkipTurn();
        }
        public override List<string> GetActionList()
        {
            List<string> actions = base.GetActionList();
            actions.Add($"5. Огненный шар  (Стоимость: {BASE_ATTACK_STAMINA_COST} стамины и {25} маны)");
            actions.Add($"6. Ледяной шторм (Стоимость: {BASE_ATTACK_STAMINA_COST * 3 + 5} стамины и {65} маны)");
            actions.Add($"7. Контроль души (Стоимость: {BASE_ATTACK_STAMINA_COST * 3 + 5} стамины и {75} маны)");
            actions[2] = $"3. Пропустить ход (Восстанавливает {SKIP_STAMINA_GAIN} стамины и {30} маны)"; // Обновляем действие пропуска хода
            return actions;
        }
        public override bool CanPerformAction(int actionChoice, IWarrior target)
        {
            switch (actionChoice)
            {
                case 5: return Mana >= 25 && Stamina >= BASE_ATTACK_STAMINA_COST; // Огненный шар
                case 6: return Mana >= 65 && Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5; // Ледяной шторм
                case 7: return Mana >= 75 && Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5; // Контроль души
                default: return base.CanPerformAction(actionChoice, target); // Остальные действия
            }
        }
        public override void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer)
        {
            switch (actionChoice)
            {
                case 5:
                    Fireball(target);
                    break;
                case 6:
                    Blizzard(target);
                    break;
                case 7:
                    SoulControl(target);
                    break;
                default:
                    base.ExecuteAction(actionChoice, target, isPlayer); // Выполняем действие из базового класса
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
                    case 5: // Огненный шар
                        score = 20;
                        score += Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.Health * 10 : 0;
                        score += RandomNumberGenerator.Next(0, 40); // Случайный бонус для атаки
                        if (target.ActiveEffects.Any(e => e.Name == "Огонь")) score += 15;
                        if (target.ActiveEffects.Any(e => e.Name == "Снежная могила")) score -= 100; 

                        break;
                    case 6: // Ледяной шторм
                        if (target.Health > target.MaxHealth / 2)
                        {
                            score = 40; // Если здоровье противника высокое, приоритет на Ледяной шторм
                        }
                        else
                        {
                            score = 20; // Если здоровье противника низкое, приоритет на Ледяной шторм
                        }
                        score += 20; 
                        score += target.Stamina > target.MaxStamina / 2 ? 50 : 0;
                        score += Health > 0 ? (float)MaxHealth / Health * 8 : 100;
                        score += RandomNumberGenerator.Next(0, 40);
                        score -= target.EvasionChance > 0 ? (float)(target.EvasionChance * 200) : 100; // Учитываем шанс уклонения противника
                        score += target.Stamina > (float)(target.MaxStamina * 0.9) ? 1000 : 0;
                        break;
                    case 7: // Контроль души
                        score = 80;
                        float negstivEffectScore = ActiveEffects.Count(e => !e.IsPositive) * 15; // Считаем количество отрицательных эффектов
                        float positiveEffectScore = target.ActiveEffects.Count(e => e.IsPositive) * 10; // Считаем количество положительных эффектов у противника
                        score += (negstivEffectScore + positiveEffectScore) / 2;
                        score += Health > 0 ? (float)MaxHealth / Health * 20 : 100; // Если здоровье низкое, контроль души более приоритетен
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                    case 1: // Атака
                        score = Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        break;
                    case 2: // Защита
                        score = Stamina >= DEFEND_STAMINA_COST ? 20 : 0;
                        score += target.Stamina > target.MaxStamina * 0.9 ? 20 : 0; // Если у противника много стамины, защита более приоритетна
                        break;
                    case 3: // Пропуск хода
                        float scoreStamina = Stamina > 0 ? (float)MaxStamina / Stamina * 10 : 100;
                        float scoreMana = Mana > 0 ? (float)MaxMana / Mana * 10 : 100;
                        score = (scoreStamina + (float)(scoreMana * 1.4) / 2 + RandomNumberGenerator.Next(0, 20));
                        score += (float)MaxHealth / Health * 4;
                        score += Health < MaxHealth / 2 && Stamina < MaxStamina / 2 && Mana < 45 ? 100 : 0;
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
