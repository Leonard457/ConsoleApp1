using ConsoleApp1.LogicGame;

namespace ConsoleApp1.SpecialClassWarrior
{
    public class Knight : WarriorBase
    {
        public override string ClassName => "Рыцарь";
        new public int CountActions => 7; // Количество действий в текущем ходе
        public Knight(string name)
            : base(
                name,
                health: 120,
                stamina: 80,
                attackDamage: 26,
                armor: 10,
                mana: 0,
                critChance: 0.1,
                evasionChance: 0.1
            )
        { }
        public void ShieldBash(IWarrior target)
        {
            int cost = BASE_ATTACK_STAMINA_COST + 5;
            if (Stamina >= cost)
            {
                DrainStamina(cost);
                int baseDamage = AttackDamage + 5;
                bool isCritical = CritChance > RandomNumberGenerator.NextDouble();
                int damage = isCritical ? baseDamage * 2 : baseDamage;
                if (target is WarriorBase targetWarrior && targetWarrior.CheckEvasion())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{targetWarrior.Name} уклонился от Щитового Удара!");
                    Console.ResetColor();
                    return;
                }
                target.TakeDamage(damage, isCritical);
            }
            else
            {
                Console.WriteLine($"{Name} не хватает стамины для использования Щитового Удара!");
            }
        }
        public void FortressMode()
        {
            int cost = DEFEND_STAMINA_COST * 2;
            if (Stamina >= cost)
            {
                DrainStamina(cost);
                ApplyEffect(Dot.Defender);// Применение эффекта Режим Крепость
            }
            else
            {
                Console.WriteLine($"{Name} не хватает стамины для использования Режима Крепости!");
                return;
            }
        }
        public void HolyStrike(IWarrior target) // Святой Удар
        {
            bool CriticalHP = Health < MaxHealth / 10; // Проверка на критическое здоровье
            int cost = BASE_ATTACK_STAMINA_COST * 4;
            if (Stamina >= cost)
            {
                DrainStamina(cost);
                int baseDamage = AttackDamage * 15 / 10;
                bool isCritical = CritChance + 0.2 > RandomNumberGenerator.NextDouble();
                baseDamage = isCritical ? baseDamage * 2 : baseDamage;
                if (target is WarriorBase targetWarrior && targetWarrior.CheckEvasion())
                {
                    Console.WriteLine($"{targetWarrior.Name} уклонился от Святого Удара! Но был на грани!");
                    target.DrainStamina(100); // Сброс стамины противника
                    Console.WriteLine($"{targetWarrior.Name} потерял много стамины при увороте от Святого Удара!");
                    return;
                }
                target.TakeDamage(baseDamage, isCritical);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{Name} использует Святой Удар по {target.Name}!");
                Console.ResetColor();
                Console.WriteLine();
                if (CriticalHP)
                {
                    MaxHealth += 30;
                    Health = MaxHealth;
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine($"{Name} использует Святой Удар в критическом состоянии! Максимальное здоровье увеличено на 30 и восстановлено до максимума!");
                    Console.ResetColor();
                }
                Armor += 1; // Увеличение брони на 1
                target.DrainArmor(1); // Сброс брони противника на 1
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Святой удар меняет броню {Name} + 1 ,а {target.Name} -1 ");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} не хватает стамины для использования Святого Удара!");
            }
        }
        public override List<string> GetActionList()
        {
            var actions = base.GetActionList();
            actions.Add($"5. Щитовой Удар (Стоимость: {BASE_ATTACK_STAMINA_COST + 5} стамины)");
            actions.Add($"6. Режим Крепость (Стоимость: {DEFEND_STAMINA_COST * 2} стамины,  +10 к броне на Хода )");
            actions.Add($"7. Святой Удар(Стоимость: {BASE_ATTACK_STAMINA_COST * 4} АБСАЛЮТНЫЙ УДАР");
            return actions;
        }
        public override bool CanPerformAction(int actionChoice, IWarrior target)
        {

            switch (actionChoice)
            {
                case 5: // Щитовой Удар
                    return Stamina >= BASE_ATTACK_STAMINA_COST + 5;

                case 6: // Режим Крепость
                    return Stamina >= DEFEND_STAMINA_COST * 2;
                case 7: // Святой удар
                    return Stamina >= BASE_ATTACK_STAMINA_COST * 4;
                default:
                    return base.CanPerformAction(actionChoice, target);
            }
        }
        public override void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer)
        {
            if (!CanPerformAction(actionChoice, target))
            {
                base.ExecuteAction(actionChoice, target, isPlayer);

            }
            switch (actionChoice)
            {
                case 5: // Щитовой Удар
                    ShieldBash(target);
                    break;
                case 6: // Режим Крепость
                    FortressMode();
                    break;
                case 7: // Святой удар
                    HolyStrike(target);
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
            foreach (var action in possibleActions)
            {
                float score = 0;
                switch (action)
                {
                    case 1: // Базовая Атака
                        score += 10;
                        if (target.Health <= this.AttackDamage) score += 50;
                        break;
                    case 2: // Защита
                        score = Stamina >= DEFEND_STAMINA_COST ? 20 : 0;
                        score += target.Stamina > target.MaxStamina * 0.5 ? 20 : 0;
                        break;
                    case 3: // Пропустить ход
                        score = Stamina > 0 ? (float)MaxStamina / Stamina * 10 : 100; // Если стамина 0 — максимальный приоритет
                        break;
                    case 4: // Лечение
                        score += 5;
                        score += Health > 0 ? MaxHealth / Health * 10 : 0; // Чем больше здоровья, тем меньше вероятность использования
                        break;
                    case 5: // Щитовой Удар
                        score += 18;
                        if (target.Health <= (this.AttackDamage + 5)) score += 55;
                        score += this.Stamina >= BASE_ATTACK_STAMINA_COST + 5 ? 20 : -100;
                        score += this.ActiveEffects.Any(e => e.Name == "Режим Крепость") ? 20 : 0;
                        score += RandomNumberGenerator.Next(0, 40);
                        break;
                    case 6: // Режим Крепость
                        score += 15;
                        if (this.ActiveEffects.Any(e => e.Name == "Режим Крепость")) score -= 200;
                        else if (this.Health < this.MaxHealth * 0.7) score += 20;
                        score += target.Stamina > target.MaxStamina * 0.5 ? 30 : 0; // Если противник тратит много стамины
                        break;
                    case 7: // Святой Удар
                        score += 10;
                        score += Stamina >= BASE_ATTACK_STAMINA_COST * 4 ? 25 : -100;
                        score += Health > 0 ? MaxHealth / Health * 40: 0; // Чем больше здоровья, тем меньше вероятность использования
                        score += RandomNumberGenerator.Next(0, 40); // Случайный бонус для Святого Удара
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
