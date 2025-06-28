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
                attackDamage: 16,
                armor: 8,
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
                Effect Defender = new Effect()
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
                ApplyEffect(Defender);
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
                    target.DrainStamina(10); // Сброс стамины противника
                    Console.WriteLine($"{targetWarrior.Name} потерял 10 стамины при увороте от Святого Удара!");
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
            // Логика выбора действия ИИ для Рыцаря
            if (Stamina <= 10)
            {
                return 3; // Пропустить ход, если стамины мало
            }
            if (Health < MaxHealth / 4 && Stamina >= BASE_ATTACK_STAMINA_COST * 4)
            {
                return RandomNumberGenerator.NextDouble() < 0.8 ? 7 : Generating_valid_values(this); // Использовать Святой Удар или Базовую атаку с вероятностью 50%
            }
            if (Stamina >= DEFEND_STAMINA_COST * 2 && RandomNumberGenerator.NextDouble() < 0.3)
            {
                if (target.Stamina > target.MaxStamina / 2)
                {
                    return RandomNumberGenerator.NextDouble() < 0.5 ? 6 : Generating_valid_values(this); // Использовать Режим Крепость или Базовую атаку с вероятностью 50%
                }
                return RandomNumberGenerator.NextDouble() < 0.25 ? 6 : Generating_valid_values(this); // Использовать Режим Крепость или Базовую атаку с вероятностью 25%
            }
            if (Stamina >= BASE_ATTACK_STAMINA_COST + 5 && RandomNumberGenerator.NextDouble() < 0.9)
            {
                return RandomNumberGenerator.NextDouble() < 0.6 ? 5 : Generating_valid_values(this); // Использовать Щитовой Удар или Базовую атаку с вероятностью 50%
            }
            if (Health < MaxHealth / 3 && Stamina >= HEAL_STAMINA_COST)
            {
                return RandomNumberGenerator.NextDouble() < 0.5 ? 4 : Generating_valid_values(this); // 50% шанс на лечение, иначе случайное действие
            }
            if (Stamina >= BASE_ATTACK_STAMINA_COST && RandomNumberGenerator.NextDouble() < 0.30)
            {
                if (target.Health < target.MaxHealth / 3)
                {
                    return RandomNumberGenerator.NextDouble() < 0.5 ? 1 : Generating_valid_values(this); // 50% шанс на атаку, иначе случайное действие
                }
                return RandomNumberGenerator.NextDouble() < 0.25 ? 1 : Generating_valid_values(this); // 25% шанс на атаку, иначе случайное действие
            }
            if (Stamina >= DEFEND_STAMINA_COST && RandomNumberGenerator.NextDouble() < 0.20)
            {
                if (target.Stamina > target.MaxStamina / 2)
                {
                    return RandomNumberGenerator.NextDouble() < 0.5 ? 2 : Generating_valid_values(this); // 50% шанс на защиту, иначе случайное действие
                }
                return RandomNumberGenerator.NextDouble() < 0.3 ? 2 : Generating_valid_values(this); // 30% шанс на защиту, иначе случайное действие
            }
            return Generating_valid_values(this); // Случайное действие
        }
    }
}
