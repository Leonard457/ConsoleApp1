// WarriorGame.cs (обновленная версия)
using WarriorBattle;
namespace ConsoleApp1.LogicGame
{
    // Абстрактный базовый класс Воина
    public abstract class WarriorBase : IWarrior
    {
        public string Name { get; protected set; }
        public abstract string ClassName { get; }
        public int Health { get; set; }
        public int MaxHealth { get;  set; }
        public int Stamina { get; set; }
        public int MaxStamina { get;  set; }
        public int Mana { get; set; }
        public int MaxMana { get;  set; }
        public int AttackDamage { get;  set; }
        public int Armor { get;  set; }
        public double CritChance { get;  set; } 
        public double EvasionChance { get;  set; } 
        public bool IsDefending { get; set; }
        public int CountActions { get; } = 4; // Количество действий в текущем ходе
        public List<Effect> ActiveEffects { get; private set; } = new List<Effect>();

        protected static Random random = new Random(); 

        protected const int BASE_ATTACK_STAMINA_COST = 15; 
        protected const int DEFEND_STAMINA_COST = 20; 
        protected const int HEAL_STAMINA_COST = 20;
        protected const int HEAL_HP_GAIN = 10; 
        protected const int SKIP_STAMINA_GAIN = 30;


        public bool IsAlive => Health > 0;

        public WarriorBase(string name, int health, int stamina, int attackDamage, int armor, int mana = 0, double critChance = 0.1, double evasionChance = 0.1)
        {
            Name = name;
            MaxHealth = health;
            Health = health;
            MaxStamina = stamina;
            Stamina = stamina;
            AttackDamage = attackDamage;
            Armor = armor;
            MaxMana = mana;
            Mana = mana;
            CritChance = critChance; 
            EvasionChance = evasionChance;
            IsDefending = false;
        }
        public void PerformBaseAttack(IWarrior target) // Базовая атака
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST)
            {
                DrainStamina(BASE_ATTACK_STAMINA_COST);
                int damage = CritChance > RandomNumberGenerator.NextDouble() ? (AttackDamage * 2) : AttackDamage;
                bool isCritical = damage > AttackDamage;
                if (target is WarriorBase targetWarrior && targetWarrior.CheckEvasion())
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{Name} атакует {target.Name}, но {target.Name} уклоняется от атаки!");
                    Console.ResetColor();
                    return;
                }
                target.TakeDamage(damage, isCritical);
            }
            else
            {
                Console.WriteLine($"{Name} пытался атаковать, но не хватило стамины!");
            }
        }

        public virtual void TakeDamage(int baseDamage, bool isCritical) // урон + крит
        {
            int damage = baseDamage;
            if (!isCritical)
            {
                damage = IsDefending ? (int)(damage * 0.75) : damage;
                damage = Math.Max(0, damage - Armor);
            }
            damage = damage * RandomNumberGenerator.Next(85, 115) / 100; // Случайный множитель от 0.85 до 1.15
            Health -= damage;
            if (isCritical)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{Name} получает критический удар: {damage} урона!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} получает {damage} урона!");
            }
        }

        public virtual void PerformDefend() // Базовая защита
        {
            if (Stamina >= DEFEND_STAMINA_COST)
            {
                DrainStamina(DEFEND_STAMINA_COST);
                IsDefending = true;
                Console.WriteLine($"{Name} встал в защитную стойку!");
            }
            else
            {
                Console.WriteLine($"{Name} пытался защититься, но не хватило стамины!");
            }
        }

        public virtual void PerformSkipTurn() // Пропуск хода
        {
            Stamina = Math.Min(MaxStamina, Stamina + SKIP_STAMINA_GAIN);
            Console.WriteLine($"{Name} пропускает ход и восстанавливает {SKIP_STAMINA_GAIN} стамины.");
        }

        public virtual void PerformHeal() // Базовое лечение
        {
            if (Stamina >= HEAL_STAMINA_COST)
            {
                DrainStamina(HEAL_STAMINA_COST);
                Health = Math.Min(MaxHealth, Health + HEAL_HP_GAIN);
                Stamina = Math.Min(MaxStamina, Stamina);
                Console.WriteLine($"{Name} лечится, восстанавливая {HEAL_HP_GAIN} ХП (затратив {HEAL_STAMINA_COST}).");
            }
            else
            {
                Console.WriteLine($"{Name} пытался лечиться, но не хватило стамины!");
            }
        }

        public virtual void ResetTurnState()
        {
            // Если сработало уклонение — сбросить только отрицательные эффекты
            if (CheckEvasion() && RandomNumberGenerator.NextDouble() > EvasionChance)
            {
                for (int i = ActiveEffects.Count - 1; i >= 0; i--)
                {
                    var effect = ActiveEffects[i];
                    if (!effect.IsPositive)
                    {
                        effect.RemoveEffect?.Invoke(this);
                        ActiveEffects.RemoveAt(i);
                    }
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Отрицательные эффекты на {Name} были сброшены из-за уклонения!");
                Console.ResetColor();
                // Положительные эффекты продолжают действовать
            }

            // Обычная обработка эффектов
            for (int i = ActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = ActiveEffects[i];
                effect.Duration--;
                effect.TickEffect?.Invoke(this);
                if (effect.Duration <= 0)
                {
                    effect.RemoveEffect?.Invoke(this);
                    ActiveEffects.RemoveAt(i);
                }
            }
        }

        public virtual void DisplayStats() // Показ статы
        {
            Console.WriteLine($"{Name} ({ClassName}): ХП: {Health}/{MaxHealth}, Стамина: {Stamina}/{MaxStamina}" +
                              (MaxMana > 0 ? $", Мана: {Mana}/{MaxMana}" : "") +
                              $", Урон: {AttackDamage}, Броня: {Armor}, Шанс крита: {CritChance:P0}, Шанс уклонения: {EvasionChance:P0}"); // P0 для % без дробной части
        }

        public virtual List<string> GetActionList()// Получение списка действий
        {
            var actions = new List<string>
            {
                $"1. Атака (Стоимость: {BASE_ATTACK_STAMINA_COST} стамины)",
                $"2. Защита (Стоимость: {DEFEND_STAMINA_COST} стамины, уменьшает урон на 25%)",
                $"3. Пропустить ход (Восстанавливает {SKIP_STAMINA_GAIN} стамины)",
                $"4. Лечение (Стоимость: {HEAL_STAMINA_COST} стамины, + {HEAL_HP_GAIN} ХП)"
            };
            return actions;
        }

        public virtual bool CanPerformAction(int actionChoice, IWarrior target) // Проверка возможности действия
        {
            switch (actionChoice)
            {
                case 1: return Stamina >= BASE_ATTACK_STAMINA_COST;
                case 2: return Stamina >= DEFEND_STAMINA_COST;
                case 3: return true;
                case 4: return Stamina >= HEAL_STAMINA_COST;
                default: return false;
            }
        }

        public virtual void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer) // Выполнение действия
        {
            if (!CanPerformAction(actionChoice, target))
            {
                if (isPlayer) Console.WriteLine("Недостаточно ресурсов для этого действия или оно некорректно!");
                else Console.WriteLine($"{Name} не смог выполнить выбранное действие из-за нехватки ресурсов и пропускает ход.");
                return;
            }

            switch (actionChoice)
            {
                case 1: PerformBaseAttack(target); break;
                case 2: PerformDefend(); break;
                case 3: PerformSkipTurn(); break;
                case 4: PerformHeal(); break;
                default: Console.WriteLine("Неверный выбор действия."); break;
            }
        }

        public void IncreaseAttackDamage(int amount) // Увеличение урона
        {
            AttackDamage += amount;
        }

        public void ApplyEffect(Effect effect) // Применение эффекта
        {
            ActiveEffects.Add(effect);
            effect.ApplyEffect?.Invoke(this);
        }

        public void DrainStamina(int amount) // Сброс стамины
        {
            Stamina = Math.Max(0, Stamina - amount);
        }

        public void DrainMana(int amount) // Сброс маны
        {
            Mana = Math.Max(0, Mana - amount);
        }
        public void DrainArmor(int amount) // Сброс брони
        {
            Armor = Math.Max(0, Armor - amount);
        }
        public void DrainHP(int amount)
        {
            Health = Math.Max(0, Health - amount);
        }

        public virtual bool CheckEvasion()  // Проверка уклонения 
        {
            if (RandomNumberGenerator.NextDouble() < EvasionChance) return true;
            return false;
        }
        public virtual int ChooseAiAction(IWarrior target) // Выбор действия ИИ
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
                    case 1: // Атака
                        score = Stamina >= BASE_ATTACK_STAMINA_COST ? AttackDamage / (float)target.MaxHealth * 20 : 0;
                        score += RandomNumberGenerator.Next(0, 40); // Случайный бонус для атаки
                        break;
                    case 2: // Защита
                        score = Stamina >= DEFEND_STAMINA_COST ? 20 : 0;
                        score += target.Stamina > target.MaxStamina / 2 ? 50 : 0; // Если у противника много стамины, защита более приоритетна
                        break;
                    case 3: // Пропуск хода
                        score = Stamina > 0 ? (float)MaxStamina / Stamina * 10 : 100; // Если стамина 0 — максимальный приоритет
                        break;
                    case 4: // Лечение
                        score = (Health > 0 && Stamina >= HEAL_STAMINA_COST) ? (float)MaxHealth / Health * 20 : 0;
                        break;
                }
                actionScores[action] = score;
            }
            // 4. ВЫБОР с элементом случайности
            var finalScores = actionScores.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value + RandomNumberGenerator.Next(0, 50)
            );
            int bestAction = finalScores.OrderByDescending(kvp => kvp.Value).First().Key;
            return bestAction;

        }
        public static int Generating_valid_values(IWarrior target) // Генерация случайного действия, которое возможно выполнить
        {
            int targetAction = RandomNumberGenerator.Next(1, target.CountActions + 1);
            while (true)
            {
                if (target.CanPerformAction(targetAction, target))
                {
                    return targetAction; // Возвращаем случайное действие, если оно возможно
                }
                targetAction = RandomNumberGenerator.Next(1, target.CountActions + 1); // Иначе выбираем другое действие
            }
        }
    }
}