using ConsoleApp1.LogicGame;

namespace ConsoleApp1.SpecialClassWarrior
{
    public class Rogue : WarriorBase
    {
        public override string ClassName => "Разбойник";
        new public int CountActions => 7;
        public Rogue(string name)
            : base(
                name,
                health: 100,
                stamina: 90,
                attackDamage: 20,
                armor: 6,
                mana: 0,
                critChance: 0.05,
                evasionChance: 0.3
            )
        { }

        public void DoubleShot(IWarrior target)
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST * 2 + 5)
            {
                DrainStamina(BASE_ATTACK_STAMINA_COST * 2 + 5);
                int damage = (int)(AttackDamage * 1.5) / 2;
                for (int i = 0; i < 2; i++)
                {
                    if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{target.Name} уклонился от {i} стрелы Двойного выстрела!");
                        Console.ResetColor();
                        continue; // Пропускаем урон, если уклонился
                    }
                    bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                    if (0.40 > RandomNumberGenerator.NextDouble() || isCritical)
                    {
                        target.ApplyEffect(Dot.Bleeding);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{Name} накладывает эффект Кровотечение на {target.Name}!");
                        Console.ResetColor();
                    }
                    target.TakeDamage(damage, isCritical);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} не хватает стамины для Двойного выстрела!");
                Console.ResetColor();
            }
        }
        public void PoisonDart(IWarrior target)
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST + 5)
            {
                DrainStamina(BASE_ATTACK_STAMINA_COST + 5);
                int damage = (int)(AttackDamage / 4);
                for (int i = 0; i < 3; i++)
                {
                    if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{target.Name} уклонился от {i + 1} ядовитого дротика!");
                        Console.ResetColor();
                        continue; // Пропускаем урон, если уклонился
                    }
                    bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                    if (0.6 > RandomNumberGenerator.NextDouble() || isCritical)
                    {
                        target.ApplyEffect(Dot.Poison);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"{Name} накладывает эффект Яда на {target.Name}!");
                        Console.ResetColor();
                    }
                    target.TakeDamage(damage, isCritical);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} не хватает стамины для использования Ядовитого дротика!");
                Console.ResetColor();
            }
        }
        public void Dynamite(IWarrior target)
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5)
            {
                DrainStamina(BASE_ATTACK_STAMINA_COST * 3 + 5);
                int damage = (int)(AttackDamage * 2);
                if (target.EvasionChance > RandomNumberGenerator.NextDouble())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{target.Name} уклонился от взрыва динамита!");
                    Console.WriteLine($"Но взрывная волна задела {target.Name}!");
                    Console.ResetColor();
                    target.TakeDamage(damage / 3, false); // Урон от взрывной волны
                    return;
                }
                bool isCritical = RandomNumberGenerator.NextDouble() < CritChance;
                target.ApplyEffect(Dot.Fire);
                target.ApplyEffect(Dot.Stun);
                target.TakeDamage(damage, isCritical);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} использует Динамит против {target.Name}!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} не хватает стамины для использования Динамита!");
                Console.ResetColor();
            }
        }
        public override List<string> GetActionList()
        {
            List<string> actions = base.GetActionList();
            actions.Add($"5. Двойной выстрел (Стоимость: {BASE_ATTACK_STAMINA_COST * 2 + 5} стамины)");
            actions.Add($"6. Ядовитый дротик (Стоимость: {BASE_ATTACK_STAMINA_COST + 5} стамины)");
            actions.Add($"7. Динамит (Стоимость: {BASE_ATTACK_STAMINA_COST * 3 + 5} стамины)");
            return actions;
        }
        public override bool CanPerformAction(int actionChoice, IWarrior target)
        {
            switch (actionChoice)
            {
                case 5: return Stamina >= BASE_ATTACK_STAMINA_COST * 2 + 5;
                case 6: return Stamina >= BASE_ATTACK_STAMINA_COST + 5;
                case 7: return Stamina >= BASE_ATTACK_STAMINA_COST * 3 + 5;
                default: return base.CanPerformAction(actionChoice, target);
            }
        }
        public override void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer)
        {
            switch (actionChoice)
            {
                case 5:
                    DoubleShot(target);
                    break;
                case 6:
                    PoisonDart(target);
                    break;
                case 7:
                    Dynamite(target);
                    break;
                default:
                    base.ExecuteAction(actionChoice, target, isPlayer);
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
            foreach( var action in possibleActions)
            {
                float score = 0;
                switch (action)
                {
                    case 1: // Атака
                        score = Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        score += RandomNumberGenerator.Next(0, 40); // Случайный бонус для атаки
                        break;
                    case 2: // Защита
                        score = Stamina >= DEFEND_STAMINA_COST ? 20 : 0;
                        score += target.Stamina > target.MaxStamina / 2 ? 50 : 0; // Если у противника много стамины, защита более приоритетна
                        score -= target.Health > 0 ? target.MaxHealth / target.Health * 10 : 0; 
                        break;
                    case 3: // Пропуск хода
                        float scoreStamina = Stamina > 0 ? (float)MaxStamina / Stamina * 12 : 100;
                        score = scoreStamina  + RandomNumberGenerator.Next(0, 20);
                        score += (float)MaxHealth / Health * 4;
                        break;
                    case 4: // Лечение
                        score = (Health > 0 && Stamina >= HEAL_STAMINA_COST) ? (float)MaxHealth / Health * 11 : 100;
                        break;
                    case 5: // Двойной выстрел
                        score = 20;
                        score += Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        score += target.ActiveEffects.Any(e => e.Name == "Кровотечение") ? 0 : 30;
                        score += target.ActiveEffects.Any(e => e.Name == "Оглушение") ? 30 : 0;
                        score += target.Armor < BASE_ATTACK_STAMINA_COST / 3 ? 20 : 0; // Если броня противника низкая, приоритет выше
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                    case 6: // Ядовитый дротик
                        score = 25;
                        score += Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.MaxHealth * 10 : 0;
                        score += target.ActiveEffects.Any(e => e.Name == "Яд") ? 0 : 30;
                        score += target.ActiveEffects.Any(e => e.Name == "Оглушение") ? 30 : 0;
                        score += target.Armor > BASE_ATTACK_STAMINA_COST / 2 ? 20 : 0;
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                    case 7: // Динамит
                        score = 30;
                        float ScoreStamina = target.Stamina > target.MaxStamina / 2 ? 50 : 0;
                        float ScoreHealth = target.Health > target.MaxHealth /10 * 6 ? 50 : 0;
                        score += Health > 0  ? (float)MaxHealth / Health * 10 : 100;
                        score += RandomNumberGenerator.Next(0, 40);
                        score += ScoreStamina + ScoreHealth;
                        score += target.ActiveEffects.Any(ActiveEffects => !ActiveEffects.IsPositive) ? 50 : 0;
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
