// WarriorGame.cs (обновленная версия)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WarriorBattle
{
    // Универсальный класс эффекта
    public class Effect
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public Action<IWarrior> ApplyEffect { get; set; }
        public Action<IWarrior> TickEffect { get; set; }
        public Action<IWarrior> RemoveEffect { get; set; }
    }

    // Интерфейс для Воина
    public interface IWarrior
    {
        string Name { get; }
        string ClassName { get; }
        int Health { get; set; }
        int MaxHealth { get; }
        int Stamina { get; set; }
        int MaxStamina { get; }
        int Mana { get; set; }
        int MaxMana { get; }
        int AttackDamage { get; }
        int Armor { get; }
        double CritChance { get; } // Новое свойство
        double EvasionChance { get; } // Новое свойство
        bool IsDefending { get; set; }
        bool IsAlive { get; }

        void PerformBaseAttack(IWarrior target);
        void PerformDefend();
        void PerformSkipTurn();
        void PerformHeal();
        void PerformUniqueAction1(IWarrior target);
        void PerformUniqueAction2(IWarrior target);
        void PerformUniqueAction3(IWarrior target);
        string GetUniqueAction1Name();
        string GetUniqueAction2Name();
        string GetUniqueAction3Name();
        bool CanPerformUniqueAction1(IWarrior target);
        bool CanPerformUniqueAction2(IWarrior target);
        bool CanPerformUniqueAction3(IWarrior target);
        void TakeDamage(int baseDamage, bool isCritical); // Изменен для крита
        void TakeDamage(int baseDamage, bool isCritical, bool ignoreArmor); // Перегрузка для магического урона
        void ResetTurnState();
        void DisplayStats();
        List<string> GetActionList();
        bool CanPerformAction(int actionChoice, IWarrior target);
        void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer);
        List<Effect> ActiveEffects { get; }
        void ApplyEffect(Effect effect);
        void DrainStamina(int amount);
        void DrainMana(int amount);
    }

    // Абстрактный базовый класс Воина
    public abstract class WarriorBase : IWarrior
    {
        public string Name { get; protected set; }
        public abstract string ClassName { get; }
        public int Health { get; set; }
        public int MaxHealth { get; protected set; }
        public int Stamina { get; set; }
        public int MaxStamina { get; protected set; }
        public int Mana { get; set; }
        public int MaxMana { get; protected set; }
        public int AttackDamage { get; protected set; }
        public int Armor { get; protected set; }
        public double CritChance { get; protected set; } // Новое свойство
        public double EvasionChance { get; protected set; } // Новое свойство
        public bool IsDefending { get; set; }
        public List<Effect> ActiveEffects { get; private set; } = new List<Effect>();

        protected static Random random = new Random();

        protected const int BASE_ATTACK_STAMINA_COST = 15;
        protected const int DEFEND_STAMINA_COST = 20;
        protected const int HEAL_STAMINA_COST = 10;
        protected const int HEAL_HP_GAIN = 20; // Было 5, теперь 20
        protected const int HEAL_STAMINA_GAIN = 20; // Было 10, теперь 20
        protected const int SKIP_STAMINA_GAIN = 30;

        public bool IsAlive => Health > 0;

        public WarriorBase(string name, int health, int stamina, int attackDamage, int armor, int mana = 0, double critChance = 0.05, double evasionChance = 0.0) // Базовый шанс крита 5%
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
            CritChance = critChance; // Инициализация шанса крита
            EvasionChance = evasionChance; // Инициализация шанса уклонения
            IsDefending = false;
        }

        // Вспомогательный метод для атаки с учетом крита
        protected void ApplyDamage(IWarrior target, int damageAmount, string attackName)
        {
            bool isCritical = random.NextDouble() < CritChance;
            int finalDamage = damageAmount;

            if (isCritical)
            {
                finalDamage *= 2; // Удвоение урона
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"КРИТИЧЕСКИЙ УДАР от {Name} с {attackName}!");
                Console.ResetColor();
            }
            target.TakeDamage(finalDamage, isCritical);
        }

        // Вспомогательный метод для атаки с учетом крита и игнорирования брони
        protected void ApplyDamage(IWarrior target, int damageAmount, string attackName, bool ignoreArmor = false)
        {
            bool isCritical = random.NextDouble() < CritChance;
            int finalDamage = damageAmount;

            if (isCritical)
            {
                finalDamage *= 2;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"КРИТИЧЕСКИЙ УДАР от {Name} с {attackName}!");
                Console.ResetColor();
            }
            if (target is WarriorBase wb)
                wb.TakeDamage(finalDamage, isCritical, ignoreArmor);
            else
                target.TakeDamage(finalDamage, isCritical);
        }

        // Прямое снятие ХП без брони и случайности
        protected int DirectHealthSteal(IWarrior target, int amount)
        {
            int steal = Math.Min(amount, target.Health);
            if (steal > 0)
            {
                var targetBase = target as WarriorBase;
                if (targetBase != null)
                {
                    targetBase.Health -= steal;
                    if (targetBase.Health < 0) targetBase.Health = 0;
                }
            }
            return steal;
        }

        public virtual void PerformBaseAttack(IWarrior target)
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST)
            {
                Stamina -= BASE_ATTACK_STAMINA_COST;
                Console.WriteLine($"{Name} атакует {target.Name}!");
                ApplyDamage(target, AttackDamage, "базовой атакой");
            }
            else
            {
                Console.WriteLine($"{Name} пытался атаковать, но не хватило стамины!");
            }
        }

        public virtual void PerformDefend()
        {
            if (Stamina >= DEFEND_STAMINA_COST)
            {
                Stamina -= DEFEND_STAMINA_COST;
                IsDefending = true;
                Console.WriteLine($"{Name} встал в защитную стойку!");
            }
            else
            {
                Console.WriteLine($"{Name} пытался защититься, но не хватило стамины!");
            }
        }

        public virtual void PerformSkipTurn()
        {
            Stamina = Math.Min(MaxStamina, Stamina + SKIP_STAMINA_GAIN);
            Console.WriteLine($"{Name} пропускает ход и восстанавливает {SKIP_STAMINA_GAIN} стамины.");
        }

        public virtual void PerformHeal()
        {
            if (Stamina >= HEAL_STAMINA_COST)
            {
                Stamina -= HEAL_STAMINA_COST;
                Health = Math.Min(MaxHealth, Health + HEAL_HP_GAIN);
                Stamina = Math.Min(MaxStamina, Stamina + HEAL_STAMINA_GAIN);
                Console.WriteLine($"{Name} лечится, восстанавливая {HEAL_HP_GAIN} ХП и {HEAL_STAMINA_GAIN} стамины (затратив {HEAL_STAMINA_COST}).");
            }
            else
            {
                Console.WriteLine($"{Name} пытался лечиться, но не хватило стамины!");
            }
        }

        public virtual void TakeDamage(int baseDamage, bool isCritical)
        {
            // Крит нельзя избежать
            if (!isCritical && random.NextDouble() < EvasionChance)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{Name} уклонился от атаки!");
                Console.ResetColor();
                // Сбросить все эффекты при уклонении, кроме тотема жизни
                if (ActiveEffects.Count > 0)
                {
                    for (int i = ActiveEffects.Count - 1; i >= 0; i--)
                    {
                        var effect = ActiveEffects[i];
                        if (effect.Name != "Тотем Жизни")
                        {
                            effect.RemoveEffect?.Invoke(this);
                            ActiveEffects.RemoveAt(i);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"Все эффекты на {Name} (кроме Тотема Жизни) были сброшены из-за уклонения!");
                    Console.ResetColor();
                }
                return; // Урон не наносится
            }

            int actualDamage = baseDamage;

            if (isCritical)
            {
                // Крит игнорирует броню и защиту
                Console.WriteLine($"Критический удар пробивает защиту {Name}!");
            }
            else
            {
                if (IsDefending)
                {
                    actualDamage = (int)(actualDamage * 0.75); // Уменьшение урона на четверть
                    Console.WriteLine($"{Name} защищается, уменьшая урон!");
                }
                actualDamage = Math.Max(0, actualDamage - Armor); // Броня поглощает урон
            }

            // Новый диапазон случайности: 0.85 - 1.15
            double randomMultiplier = 0.85 + random.NextDouble() * 0.3; // [0.85, 1.15)
            int randomizedDamage = Math.Max(0, (int)Math.Round(actualDamage * randomMultiplier));

            Health -= randomizedDamage;
            Console.WriteLine($"{Name} получает {randomizedDamage} урона. Осталось ХП: {Health}");
            if (!IsAlive)
            {
                Console.WriteLine($"{Name} повержен!");
            }
        }

        public virtual void TakeDamage(int baseDamage, bool isCritical, bool ignoreArmor)
        {
            // Крит нельзя избежать
            if (!isCritical && random.NextDouble() < EvasionChance)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{Name} уклонился от атаки!");
                Console.ResetColor();
                // Сбросить все эффекты при уклонении, кроме тотема жизни
                if (ActiveEffects.Count > 0)
                {
                    for (int i = ActiveEffects.Count - 1; i >= 0; i--)
                    {
                        var effect = ActiveEffects[i];
                        if (effect.Name != "Тотем Жизни")
                        {
                            effect.RemoveEffect?.Invoke(this);
                            ActiveEffects.RemoveAt(i);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"Все эффекты на {Name} (кроме Тотема Жизни) были сброшены из-за уклонения!");
                    Console.ResetColor();
                }
                return; // Урон не наносится
            }

            int actualDamage = baseDamage;

            if (isCritical)
            {
                // Крит игнорирует броню и защиту
                Console.WriteLine($"Критический удар пробивает защиту {Name}!");
            }
            else
            {
                if (IsDefending)
                {
                    actualDamage = (int)(actualDamage * 0.75); // Уменьшение урона на четверть
                    Console.WriteLine($"{Name} защищается, уменьшая урон!");
                }
                if (!ignoreArmor)
                    actualDamage = Math.Max(0, actualDamage - Armor); // Броня поглощает урон
            }

            // Новый диапазон случайности: 0.85 - 1.15
            double randomMultiplier = 0.85 + random.NextDouble() * 0.3; // [0.85, 1.15)
            int randomizedDamage = Math.Max(0, (int)Math.Round(actualDamage * randomMultiplier));

            Health -= randomizedDamage;
            Console.WriteLine($"{Name} получает {randomizedDamage} урона. Осталось ХП: {Health}");
            if (!IsAlive)
            {
                Console.WriteLine($"{Name} повержен!");
            }
        }

        public virtual void ResetTurnState()
        {
            IsDefending = false;
            // Обработка эффектов
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

        public virtual void DisplayStats()
        {
            Console.WriteLine($"{Name} ({ClassName}): ХП: {Health}/{MaxHealth}, Стамина: {Stamina}/{MaxStamina}" +
                              (MaxMana > 0 ? $", Мана: {Mana}/{MaxMana}" : "") +
                              $", Урон: {AttackDamage}, Броня: {Armor}, Шанс крита: {CritChance:P0}, Шанс уклонения: {EvasionChance:P0}"); // P0 для % без дробной части
        }

        public abstract void PerformUniqueAction1(IWarrior target);
        public abstract void PerformUniqueAction2(IWarrior target);
        public abstract void PerformUniqueAction3(IWarrior target);
        public abstract string GetUniqueAction1Name();
        public abstract string GetUniqueAction2Name();
        public abstract string GetUniqueAction3Name();
        public abstract bool CanPerformUniqueAction1(IWarrior target);
        public abstract bool CanPerformUniqueAction2(IWarrior target);
        public abstract bool CanPerformUniqueAction3(IWarrior target);

        public virtual List<string> GetActionList()
        {
            var actions = new List<string>
            {
                $"1. Атака (Стоимость: {BASE_ATTACK_STAMINA_COST} стамины)",
                $"2. Защита (Стоимость: {DEFEND_STAMINA_COST} стамины, уменьшает урон на 25%)",
                $"3. Пропустить ход (Восстанавливает {SKIP_STAMINA_GAIN} стамины)",
                $"4. Лечение (Стоимость: {HEAL_STAMINA_COST} стамины, +{HEAL_HP_GAIN} ХП, +{HEAL_STAMINA_GAIN} стамины)",
                $"5. {GetUniqueAction1Name()}",
                $"6. {GetUniqueAction2Name()}",
                $"7. {GetUniqueAction3Name()}"
            };
            return actions;
        }

        public virtual bool CanPerformAction(int actionChoice, IWarrior target)
        {
            switch (actionChoice)
            {
                case 1: return Stamina >= BASE_ATTACK_STAMINA_COST;
                case 2: return Stamina >= DEFEND_STAMINA_COST;
                case 3: return true;
                case 4: return Stamina >= HEAL_STAMINA_COST;
                case 5: return CanPerformUniqueAction1(target);
                case 6: return CanPerformUniqueAction2(target);
                case 7: return CanPerformUniqueAction3(target);
                default: return false;
            }
        }

        public void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer)
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
                case 5: PerformUniqueAction1(target); break;
                case 6: PerformUniqueAction2(target); break;
                case 7: PerformUniqueAction3(target); break;
                default: Console.WriteLine("Неверный выбор действия."); break;
            }
        }

        public void IncreaseAttackDamage(int amount)
        {
            AttackDamage += amount;
        }

        public void ApplyEffect(Effect effect)
        {
            ActiveEffects.Add(effect);
            effect.ApplyEffect?.Invoke(this);
        }

        public void DrainStamina(int amount)
        {
            Stamina = Math.Max(0, Stamina - amount);
        }

        public void DrainMana(int amount)
        {
            Mana = Math.Max(0, Mana - amount);
        }

        public virtual bool CheckEvasion()
        {
            if (random.NextDouble() < EvasionChance)
            {
                // Сбросить все эффекты при уклонении, кроме тотема жизни
                if (ActiveEffects.Count > 0)
                {
                    for (int i = ActiveEffects.Count - 1; i >= 0; i--)
                    {
                        var effect = ActiveEffects[i];
                        if (effect.Name != "Тотем Жизни")
                        {
                            effect.RemoveEffect?.Invoke(this);
                            ActiveEffects.RemoveAt(i);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"Все эффекты на {Name} (кроме Тотема Жизни) были сброшены из-за уклонения!");
                    Console.ResetColor();
                }
                return true;
            }
            return false;
        }
    }

    // Класс Рыцарь
    public class Knight : WarriorBase
    {
        public override string ClassName => "Рыцарь";
        private const int SHIELD_SLAM_STAMINA_COST = 30;
        private const int FORTIFY_STAMINA_COST = 25;
        private const int HOLY_STRIKE_STAMINA_COST = 50;
        private int baseArmor;

        public Knight(string name) : base(name, 120, 100, 12, 7, mana: 0, critChance: 0.07, evasionChance: 0.10) // Шанс крита Рыцаря 7%, уклонение 10%
        {
            baseArmor = Armor;
        }

        public override string GetUniqueAction1Name() => $"Удар щитом (Урон: {AttackDamage + 5}, Стоимость: {SHIELD_SLAM_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction1(IWarrior target) => Stamina >= SHIELD_SLAM_STAMINA_COST;
        public override void PerformUniqueAction1(IWarrior target) // Shield Slam
        {
            if (Stamina >= SHIELD_SLAM_STAMINA_COST)
            {
                Stamina -= SHIELD_SLAM_STAMINA_COST;
                Console.WriteLine($"{Name} использует 'Удар щитом' против {target.Name}!");
                ApplyDamage(target, AttackDamage + 5, "'Удар щитом'");
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Удар щитом', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction2Name() => $"Укрепиться (Броня +5 на 1 ход, Стоимость: {FORTIFY_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction2(IWarrior target) => Stamina >= FORTIFY_STAMINA_COST;
        public override void PerformUniqueAction2(IWarrior target)
        {
            if (Stamina >= FORTIFY_STAMINA_COST)
            {
                Stamina -= FORTIFY_STAMINA_COST;
                // Эффект брони через систему эффектов
                var armorBuff = new Effect
                {
                    Name = "Укрепиться",
                    Duration = 1,
                    ApplyEffect = (warrior) =>
                    {
                        if (warrior is Knight k)
                        {
                            k.Armor += 5;
                            k.IsDefending = true;
                        }
                    },
                    RemoveEffect = (warrior) =>
                    {
                        if (warrior is Knight k)
                        {
                            k.Armor = k.baseArmor;
                            Console.WriteLine($"{k.Name}: эффект 'Укрепиться' закончился, броня вернулась к {k.baseArmor}.");
                        }
                    }
                };
                ApplyEffect(armorBuff);
                Console.WriteLine($"{Name} использует 'Укрепиться', увеличивая броню и входя в защиту!");
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Укрепиться', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction3Name() => $"Святой удар (Кража ХП у противника, Стоимость: {HOLY_STRIKE_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction3(IWarrior target) => Stamina >= HOLY_STRIKE_STAMINA_COST && target != null && target.IsAlive;
        public override void PerformUniqueAction3(IWarrior target)
        {
            if (Stamina >= HOLY_STRIKE_STAMINA_COST && target != null && target.IsAlive)
            {
                Stamina -= HOLY_STRIKE_STAMINA_COST;
                // Новая механика: если ХП < 10% - полное восстановление и +30 к макс. ХП
                if ((double)Health / MaxHealth < 0.2)
                {
                    MaxHealth += 30;
                    Health = MaxHealth;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{Name} был на грани смерти! Святой удар полностью исцеляет и увеличивает максимальное ХП на 30!");
                    Console.ResetColor();
                }
                int stealAmount = 30; // Можно скорректировать
                int actualSteal = DirectHealthSteal(target, stealAmount);
                Health = Math.Min(MaxHealth, Health + actualSteal);

                // Эффект: броня +2 рыцарю, -1 врагу
                Armor += 2;
                if (target is Knight knightTarget)
                {
                    knightTarget.Armor = Math.Max(0, knightTarget.Armor - 1);
                }
                else if (target is IWarrior iw && !(target is Knight))
                {
                    var armorProp = target.GetType().GetProperty("Armor");
                    if (armorProp != null && armorProp.CanWrite)
                    {
                        int currentArmor = (int)armorProp.GetValue(target);
                        armorProp.SetValue(target, Math.Max(0, currentArmor - 1));
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{Name} получает +2 к броне, {target.Name} теряет 1 броню!");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{Name} использует 'Святой удар'! Крадёт {actualSteal} ХП у {target.Name}.");
                Console.ResetColor();
                if (!target.IsAlive)
                    Console.WriteLine($"{target.Name} повержен от Святого удара!");
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Святой удар', но не хватило стамины!");
            }
        }
    }

    // Класс Маг
    public class Mage : WarriorBase
    {
        public override string ClassName => "Маг";
        private const int FIREBALL_MANA_COST = 30;
        private const int FIREBALL_DAMAGE_MULTIPLIER = 3; // Урон = AttackDamage * 3
        private const int MANA_REGEN_STAMINA_COST = 5;
        private const int MANA_REGEN_AMOUNT = 40;
        private const int SOUL_CONTROL_STAMINA_COST = 30;
        private const int SOUL_CONTROL_MANA_COST = 40;

        public Mage(string name) : base(name, 80, 60, 6, 3, mana: 100, critChance: 0.10, evasionChance: 0.10) { } // Шанс крита Мага 10%, уклонение 10%

        public override string GetUniqueAction1Name() => $"Огненный шар (Урон: {AttackDamage * FIREBALL_DAMAGE_MULTIPLIER}, Стоимость: {FIREBALL_MANA_COST} маны)";
        public override bool CanPerformUniqueAction1(IWarrior target) => Mana >= FIREBALL_MANA_COST;
        public override void PerformUniqueAction1(IWarrior target) // Fireball
        {
            if (Mana >= FIREBALL_MANA_COST)
            {
                Mana -= FIREBALL_MANA_COST;
                // Проверка уклонения
                if (target is WarriorBase wb && wb.CheckEvasion())
                {
                    Console.WriteLine($"{target.Name} уклонился от 'Огненного шара'!");
                    return;
                }
                Console.WriteLine($"{Name} кастует 'Огненный шар' в {target.Name}!");
                ApplyDamage(target, AttackDamage * FIREBALL_DAMAGE_MULTIPLIER, "'Огненный шар'", true); // true = игнорировать броню
                // Новый огонь: 2 урона, 8 ходов
                var fireEffect = new Effect
                {
                    Name = "Огонь",
                    Duration = 8,
                    TickEffect = (warrior) =>
                    {
                        int fireDamage = 2;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{warrior.Name} горит, теряя {fireDamage} ХП.");
                        Console.ResetColor();
                        warrior.Health = Math.Max(0, warrior.Health - fireDamage);
                    },
                    RemoveEffect = (warrior) =>
                    {
                        Console.WriteLine($"Огонь на {warrior.Name} погас.");
                    }
                };
                target.ApplyEffect(fireEffect);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{target.Name} подожжён!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} пытался скастовать 'Огненный шар', но не хватило маны!");
            }
        }

        public override string GetUniqueAction2Name() => $"Регенерация маны (+{MANA_REGEN_AMOUNT} маны, Стоимость: {MANA_REGEN_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction2(IWarrior target) => Stamina >= MANA_REGEN_STAMINA_COST;
        public override void PerformUniqueAction2(IWarrior target) // Mana Regen
        {
            if (Stamina >= MANA_REGEN_STAMINA_COST)
            {
                Stamina -= MANA_REGEN_STAMINA_COST;
                Mana = Math.Min(MaxMana, Mana + MANA_REGEN_AMOUNT);
                int healAmount = 10;
                Health = Math.Min(MaxHealth, Health + healAmount);
                Console.WriteLine($"{Name} использует 'Регенерация маны', восстанавливая {MANA_REGEN_AMOUNT} маны и {healAmount} ХП.");
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Регенерация маны', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction3Name() => $"Контроль души (+60 макс. ХП, +30 ХП, Стоимость: {SOUL_CONTROL_STAMINA_COST} стамины, {SOUL_CONTROL_MANA_COST} маны)";
        public override bool CanPerformUniqueAction3(IWarrior target) => Stamina >= SOUL_CONTROL_STAMINA_COST && Mana >= SOUL_CONTROL_MANA_COST;
        public override void PerformUniqueAction3(IWarrior target)
        {
            if (Stamina >= SOUL_CONTROL_STAMINA_COST && Mana >= SOUL_CONTROL_MANA_COST)
            {
                Stamina -= SOUL_CONTROL_STAMINA_COST;
                Mana -= SOUL_CONTROL_MANA_COST;
                MaxHealth += 60;
                Health = Math.Min(MaxHealth, Health + 30);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{Name} использует 'Контроль души'! Макс. ХП увеличено на 60, восстановлено 30 ХП.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Контроль души', но не хватило ресурсов!");
            }
        }

        public override void ResetTurnState()
        {
            // Mage: если ХП < 20%, MaxHealth +5, HP +1
            if ((double)Health / MaxHealth < 0.2)
            {
                MaxHealth += 5;
                Health = Math.Min(MaxHealth, Health + 2);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{Name} на грани! Макс. ХП растёт (+5), восстановлено 2 ХП.");
                Console.ResetColor();
            }
            base.ResetTurnState();
        }
    }

    // Класс Лучник
    public class Archer : WarriorBase
    {
        public override string ClassName => "Лучник";
        private const int AIMED_SHOT_STAMINA_COST = 35;
        private const int AIMED_SHOT_DAMAGE_MULTIPLIER = 2; // Урон = AttackDamage * 2
        private const int DOUBLE_TAP_STAMINA_COST = 25;
        private const int DOUBLE_TAP_DAMAGE_MOD = -2; // Урон каждой стрелы = AttackDamage - 2
        private const int HEART_SHOT_STAMINA_COST = 40;

        private int rageTurnsLeft = 0;
        private bool wasInRage = false;

        public Archer(string name) : base(name, 90, 120, 10, 4, mana: 0, critChance: 0.15, evasionChance: 0.10) { } // Шанс крита Лучника 15%, уклонение 10%

        public override string GetUniqueAction1Name() => $"Прицельный выстрел (Урон: {AttackDamage * AIMED_SHOT_DAMAGE_MULTIPLIER}, Стоимость: {AIMED_SHOT_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction1(IWarrior target) => Stamina >= AIMED_SHOT_STAMINA_COST;

        private void ApplyArcherDamage(IWarrior target, int damageAmount, string attackName, bool ignoreArmor = false)
        {
            double originalCritChance = CritChance;
            if (target is IWarrior t && t.ActiveEffects.Any(e => e.Name == "Кровотечение"))
            {
                CritChance = 0.3;
            }
            if (ignoreArmor)
                base.ApplyDamage(target, damageAmount, attackName, true);
            else
                base.ApplyDamage(target, damageAmount, attackName);
            CritChance = originalCritChance;
        }

        public override void PerformBaseAttack(IWarrior target)
        {
            if (Stamina >= BASE_ATTACK_STAMINA_COST)
            {
                Stamina -= BASE_ATTACK_STAMINA_COST;
                Console.WriteLine($"{Name} атакует {target.Name}!");
                ApplyArcherDamage(target, AttackDamage, "базовой атакой");
            }
            else
            {
                Console.WriteLine($"{Name} пытался атаковать, но не хватило стамины!");
            }
        }

        public override void PerformUniqueAction1(IWarrior target) // Aimed Shot
        {
            if (Stamina >= AIMED_SHOT_STAMINA_COST)
            {
                Stamina -= AIMED_SHOT_STAMINA_COST;
                Console.WriteLine($"{Name} совершает 'Прицельный выстрел' в {target.Name}!");
                ApplyArcherDamage(target, AttackDamage * AIMED_SHOT_DAMAGE_MULTIPLIER, "'Прицельный выстрел'");
            }
            else
            {
                Console.WriteLine($"{Name} пытался совершить 'Прицельный выстрел', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction2Name() => $"Двойной выстрел (2х Урон: {AttackDamage + DOUBLE_TAP_DAMAGE_MOD}, Стоимость: {DOUBLE_TAP_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction2(IWarrior target) => Stamina >= DOUBLE_TAP_STAMINA_COST;

        public override void PerformUniqueAction2(IWarrior target) // Double Tap
        {
            if (Stamina >= DOUBLE_TAP_STAMINA_COST)
            {
                Stamina -= DOUBLE_TAP_STAMINA_COST;
                Console.WriteLine($"{Name} использует 'Двойной выстрел' против {target.Name}!");
                int damagePerShot = Math.Max(1, AttackDamage + DOUBLE_TAP_DAMAGE_MOD); // Урон не должен быть меньше 1

                ApplyArcherDamage(target, damagePerShot, "первой стрелой 'Двойного выстрела'");
                if (target.IsAlive)
                {
                    Console.WriteLine("Вторая стрела!");
                    ApplyArcherDamage(target, damagePerShot, "второй стрелой 'Двойного выстрела'");
                }
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Двойной выстрел', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction3Name() => $"Выстрел в сердце (Гарантированный крит, Стоимость: {HEART_SHOT_STAMINA_COST} стамины)";
        public override bool CanPerformUniqueAction3(IWarrior target) => Stamina >= HEART_SHOT_STAMINA_COST && target != null && target.IsAlive;
        public override void PerformUniqueAction3(IWarrior target)
        {
            if (Stamina >= HEART_SHOT_STAMINA_COST && target != null && target.IsAlive)
            {
                Stamina -= HEART_SHOT_STAMINA_COST;
                // Проверка уклонения
                if (target is WarriorBase wb && wb.CheckEvasion())
                {
                    Console.WriteLine($"{target.Name} уклонился от 'Выстрела в сердце'!");
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{Name} совершает 'Выстрел в сердце' по {target.Name}! Гарантированный крит.");
                Console.ResetColor();
                // Гарантированный крит: урон = обычный урон, но всегда крит
                int damage = AttackDamage;
                target.TakeDamage(damage * 2, true); // *2, как при крите
                // Новое кровотечение: 2 урона, 8 ходов
                var bleedEffect = new Effect
                {
                    Name = "Кровотечение",
                    Duration = 8,
                    TickEffect = (warrior) =>
                    {
                        int bleedDamage = 2;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"{warrior.Name} истекает кровью, теряя {bleedDamage} ХП.");
                        Console.ResetColor();
                        warrior.Health = Math.Max(0, warrior.Health - bleedDamage);
                    },
                    RemoveEffect = (warrior) =>
                    {
                        Console.WriteLine($"Кровотечение на {warrior.Name} остановилось.");
                    }
                };
                target.ApplyEffect(bleedEffect);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{target.Name} получил кровотечение!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Выстрел в сердце', но не хватило стамины!");
            }
        }

        public override void ResetTurnState()
        {
            bool isRageNow = ((double)Health / MaxHealth < 0.2);
            if (isRageNow && !wasInRage)
            {
                rageTurnsLeft = 2;
                wasInRage = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{Name} впадает в ярость! Критический шанс 100%! (2 хода только атаки)");
                Console.ResetColor();
            }
            else if (!isRageNow)
            {
                wasInRage = false;
            }
            if (rageTurnsLeft > 0)
            {
                rageTurnsLeft--;
            }
            // Крит 100% только если сейчас в ярости
            CritChance = (isRageNow ? 1.0 : 0.15);
            base.ResetTurnState();
        }

        public override bool CanPerformAction(int actionChoice, IWarrior target)
        {
            if (rageTurnsLeft > 0)
            {
                if (actionChoice != 1 && actionChoice != 3 && actionChoice != 5 && actionChoice != 6 && actionChoice != 7)
                    return false;
            }
            return base.CanPerformAction(actionChoice, target);
        }

        // Archer: если в ярости, обязан атаковать хотя бы раз перед лечением или другим действием (кроме пропуска из-за стамины)
        public new void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer)
        {
            bool isRage = ((double)Health / MaxHealth < 0.2);
            if (isRage && actionChoice != 1 && Stamina >= BASE_ATTACK_STAMINA_COST)
            {
                if (isPlayer)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{Name} в ярости и обязан атаковать хотя бы раз!");
                    Console.ResetColor();
                }
                PerformBaseAttack(target);
                return;
            }
            base.ExecuteAction(actionChoice, target, isPlayer);
        }
    }

    // Класс Разбойник
    public class Rogue : WarriorBase
    {
        public override string ClassName => "Разбойник";
        private const int POISON_STRIKE_COST = 20;
        private const int EVASIVE_STANCE_COST = 25;
        private const int EXECUTE_COST = 40;

        public Rogue(string name)
            : base(name, health: 85, stamina: 110, attackDamage: 9, armor: 2, mana: 0, critChance: 0.12, evasionChance: 0.20) { }

        public override string GetUniqueAction1Name() => $"Отравленный клинок (Урон: {AttackDamage}, вешает яд на 3 хода, Стоимость: {POISON_STRIKE_COST} стамины)";
        public override bool CanPerformUniqueAction1(IWarrior target) => Stamina >= POISON_STRIKE_COST;
        public override void PerformUniqueAction1(IWarrior target)
        {
            if (Stamina >= POISON_STRIKE_COST)
            {
                Stamina -= POISON_STRIKE_COST;
                // Проверка уклонения
                if (target is WarriorBase wb && wb.CheckEvasion())
                {
                    Console.WriteLine($"{target.Name} уклонился от 'Отравленного клинка'!");
                    return;
                }
                Console.WriteLine($"{Name} наносит удар 'Отравленным клинком' по {target.Name}!");
                ApplyDamage(target, AttackDamage + 3, "'Отравленным клинком'");
                // Новый яд: 2 урона, 8 ходов
                var poisonEffect = new Effect
                {
                    Name = "Яд",
                    Duration = 8,
                    TickEffect = (warrior) =>
                    {
                        int poisonDamage = 2;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"{warrior.Name} страдает от яда, теряя {poisonDamage} ХП.");
                        Console.ResetColor();
                        warrior.Health = Math.Max(0, warrior.Health - poisonDamage);
                    },
                    RemoveEffect = (warrior) =>
                    {
                        Console.WriteLine($"Эффект яда на {warrior.Name} закончился.");
                    }
                };
                target.ApplyEffect(poisonEffect);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"{target.Name} отравлен!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Отравленный клинок', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction2Name() => $"Ускользание (Уклонение +40% на 1 ход, Стоимость: {EVASIVE_STANCE_COST} стамины)";
        public override bool CanPerformUniqueAction2(IWarrior target) => Stamina >= EVASIVE_STANCE_COST;
        public override void PerformUniqueAction2(IWarrior target)
        {
            if (Stamina >= EVASIVE_STANCE_COST)
            {
                Stamina -= EVASIVE_STANCE_COST;
                // Эффект уклонения через систему эффектов
                var evadeEffect = new Effect
                {
                    Name = "Ускользание",
                    Duration = 1,
                    ApplyEffect = (warrior) =>
                    {
                        if (warrior is Rogue rogue)
                        {
                            rogue.EvasionChance += 0.40;
                            rogue.IsDefending = true;
                        }
                    },
                    RemoveEffect = (warrior) =>
                    {
                        if (warrior is Rogue rogue)
                        {
                            rogue.EvasionChance = 0.20;
                            Console.WriteLine($"{rogue.Name}: эффект 'Ускользания' закончился.");
                        }
                    }
                };
                ApplyEffect(evadeEffect);
                Console.WriteLine($"{Name} входит в стойку 'Ускользания', шанс уклонения увеличен!");
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Ускользание', но не хватило стамины!");
            }
        }

        public override string GetUniqueAction3Name() => $"Казнь (Огромный урон по целям < 35% ХП, Стоимость: {EXECUTE_COST} стамины)";
        public override bool CanPerformUniqueAction3(IWarrior target) => Stamina >= EXECUTE_COST && ((double)target.Health / target.MaxHealth) < 0.35;
        public override void PerformUniqueAction3(IWarrior target)
        {
            if (Stamina >= EXECUTE_COST && ((double)target.Health / target.MaxHealth) < 0.35)
            {
                Stamina -= EXECUTE_COST;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{Name} видит слабость и использует 'Казнь'!");
                Console.ResetColor();
                ApplyDamage(target, AttackDamage * 4, "'Казнь'");
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Казнь', но не хватило стамины!");
            }
        }
    }

    // Класс Шаман
    public class Shaman : WarriorBase
    {
        public override string ClassName => "Шаман";
        private const int CHAIN_LIGHTNING_MANA_COST = 35;
        private const int TOTEM_OF_VITALITY_MANA_COST = 40;
        private const int SPIRIT_LINK_COST = 60;
        private int vitalityHealPerTurn = 8;
        private int _turnCounter = 0;
        private int _attackCount = 0; // Счетчик атак шамана

        public Shaman(string name)
            : base(name, health: 100, stamina: 80, attackDamage: 8, armor: 5, mana: 80, critChance: 0.05, evasionChance: 0.10) { }

        public override string GetUniqueAction1Name() => $"Цепная молния (Урон: {AttackDamage * 2}, Стоимость: {CHAIN_LIGHTNING_MANA_COST} маны)";
        public override bool CanPerformUniqueAction1(IWarrior target) => Mana >= CHAIN_LIGHTNING_MANA_COST;
        public override void PerformBaseAttack(IWarrior target)
        {
            base.PerformBaseAttack(target);
            _attackCount++;
        }
        public override void PerformUniqueAction1(IWarrior target)
        {
            if (Mana >= CHAIN_LIGHTNING_MANA_COST)
            {
                Mana -= CHAIN_LIGHTNING_MANA_COST;
                // Проверка уклонения
                if (target is WarriorBase wb && wb.CheckEvasion())
                {
                    Console.WriteLine($"{target.Name} уклонился от 'Цепной молнии'!");
                    return;
                }
                Console.WriteLine($"{Name} призывает 'Цепную молнию'!");
                ApplyDamage(target, AttackDamage * 2, "'Цепная молния'", true);
                var fireEffect = new Effect
                {
                    Name = "Огонь",
                    Duration = 8,
                    TickEffect = (warrior) =>
                    {
                        int fireDamage = 2;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{warrior.Name} горит, теряя {fireDamage} ХП.");
                        Console.ResetColor();
                        warrior.Health = Math.Max(0, warrior.Health - fireDamage);
                    },
                    RemoveEffect = (warrior) =>
                    {
                        Console.WriteLine($"Огонь на {warrior.Name} погас.");
                    }
                };
                target.ApplyEffect(fireEffect);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{target.Name} подожжён!");
                Console.ResetColor();
                _attackCount++;
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Цепную молнию', но не хватило маны!");
            }
        }

        public override string GetUniqueAction2Name() => $"Тотем Жизни (+{vitalityHealPerTurn} ХП/ход на 3 хода, Стоимость: {TOTEM_OF_VITALITY_MANA_COST} маны)";
        public override bool CanPerformUniqueAction2(IWarrior target) => Mana >= TOTEM_OF_VITALITY_MANA_COST && !ActiveEffects.Any(e => e.Name == "Тотем Жизни");
        public override void PerformUniqueAction2(IWarrior target)
        {
            if (Mana >= TOTEM_OF_VITALITY_MANA_COST && !ActiveEffects.Any(e => e.Name == "Тотем Жизни"))
            {
                Mana -= TOTEM_OF_VITALITY_MANA_COST;
                int healBefore = Health;
                int maxHealthBefore = MaxHealth;
                var totemEffect = new Effect
                {
                    Name = "Тотем Жизни",
                    Duration = 3,
                    TickEffect = (warrior) =>
                    {
                        int heal = vitalityHealPerTurn;
                        int before = warrior.Health;
                        warrior.Health = Math.Min(warrior.MaxHealth, warrior.Health + heal);
                        int healed = warrior.Health - before;
                        if (warrior is Shaman sh && sh.Health > 90 && healed > 0)
                        {
                            healed = 8;
                            sh.MaxHealth += healed;
                            sh.Health = sh.MaxHealth;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Тотем Жизни увеличил максимальное ХП {sh.Name} на {8}!");
                            Console.ResetColor();
                            sh.MaxHealth = sh.MaxHealth + 8;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Тотем Жизни лечит {warrior.Name} на {healed} ХП.");
                            Console.ResetColor();
                        }
                    },
                    RemoveEffect = (warrior) =>
                    {
                        Console.WriteLine("Тотем Жизни иссяк.");
                    }
                };
                totemEffect.ApplyEffect = (warrior) =>
                {
                    if (warrior is WarriorBase wb)
                    {
                        wb.ActiveEffects.RemoveAll(e => e.Name == "Тотем Жизни");
                        wb.ActiveEffects.Add(totemEffect);
                    }
                };
                ActiveEffects.Add(totemEffect);
                totemEffect.ApplyEffect(this);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{Name} устанавливает 'Тотем Жизни'. Он будет лечить в течение 3 ходов.");
                Console.ResetColor();
                _attackCount = 0; // Сбросить счетчик атак после тотема
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Тотем Жизни', но не хватило маны или тотем уже активен!");
            }
        }

        public override string GetUniqueAction3Name() => $"Духовная связь (Крадет стамину и ману, Стоимость: {SPIRIT_LINK_COST} маны)";
        public override bool CanPerformUniqueAction3(IWarrior target) => Mana >= SPIRIT_LINK_COST;
        public override void PerformUniqueAction3(IWarrior target)
        {
            if (Mana >= SPIRIT_LINK_COST)
            {
                Mana -= SPIRIT_LINK_COST;
                target.DrainStamina(30);
                target.DrainMana(30);
                Stamina = Math.Min(MaxStamina, Stamina + 30);
                Mana = Math.Min(MaxMana, Mana + 30);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{Name} создает 'Духовную связь' с {target.Name}, похищая 30 стамины и 30 маны!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{Name} пытался использовать 'Духовную связь', но не хватило маны!");
            }
        }

        public override void ResetTurnState()
        {
            _turnCounter++;
            // Shaman: если ХП < 20%, MaxHealth +5, HP +1
            if ((double)Health / MaxHealth < 0.2)
            {
                MaxHealth += 5;
                Health = Math.Min(MaxHealth, Health + 1);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{Name} на грани! Макс. ХП растёт (+5), восстановлено 1 ХП.");
                Console.ResetColor();
            }
            // Пассивная регенерация маны раз в 5 ходов
            if (Mana < MaxMana && _turnCounter % 5 == 0)
            {
                int regen = 20;
                Mana = Math.Min(MaxMana, Mana + regen);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{Name} пассивно восстанавливает {regen} маны.");
                Console.ResetColor();
            }
            base.ResetTurnState();
        }

        public int AttackCount => _attackCount; // Открытое свойство только для чтения
    }

    // Класс игры (без изменений, т.к. логика крита инкапсулирована в воинах)
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
                    actionChoice = ChooseAiAction(currentPlayer, opponent); // Новый умный ИИ
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

        // Новый "умный" выбор действия для ИИ
        private int ChooseAiAction(IWarrior ai, IWarrior opponent)
        {
            // Абсолютный приоритет для рыцаря на Святом ударе
            if (ai is Knight && ai.CanPerformUniqueAction3(opponent))
                return 7;
            var possibleActions = new List<int>();
            for (int i = 1; i <= 7; i++)
            {
                if (ai.CanPerformAction(i, opponent))
                {
                    possibleActions.Add(i);
                }
            }
            if (!possibleActions.Any()) return 3;
            var actionScores = new Dictionary<int, float>();
            float healthPercent = (float)ai.Health / ai.MaxHealth;
            float opponentHealthPercent = (float)opponent.Health / opponent.MaxHealth;
            foreach (var action in possibleActions)
            {
                float score = 0;
                switch (action)
                {
                    case 1:
                        score = 10;
                        if (opponentHealthPercent < 0.2) score += 20;
                        break;
                    case 2:
                        score = 5;
                        if (healthPercent < 0.5) score += 15;
                        if (opponent.Stamina > 70 || opponent.Mana > 50) score += 10;
                        break;
                    case 3:
                        score = 1;
                        if (ai.Stamina < 20) score += 15;
                        break;
                    case 4:
                        if (healthPercent > 0.9) score = -10;
                        else score = 25 * (1 - healthPercent);
                        break;
                    case 5:
                    case 6:
                    case 7:
                        score = 20;
                        if (ai is Rogue && action == 7 && opponentHealthPercent < 0.35) score = 100;
                        if (ai is Archer && action == 7) score = 40;
                        if (ai is Mage && action == 7 && healthPercent < 0.6) score = 50;
                        if (ai is Mage && action == 6 && (float)ai.Mana / ai.MaxMana < 0.3) score = 30;
                        if (ai is Shaman && action == 6)
                        {
                            var sh = ai as Shaman;
                            if (sh != null && sh.AttackCount >= 2)
                                score = 999;
                            else if (healthPercent < 0.7) score = 60;
                            else if (healthPercent >= 0.99) score = 80;
                        }
                        if (ai is Knight && action == 7) score = 60;
                        if (opponent.Health < ai.AttackDamage * 2 && action > 4)
                        {
                            if (action != 4 && action != 2) score += 50;
                        }
                        break;
                }
                actionScores[action] = score;
            }
            int bestAction = actionScores.OrderByDescending(kvp => kvp.Value).First().Key;
            return bestAction;
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
                case 1: selectedWarrior = new Knight(name); break;
                case 2: selectedWarrior = new Mage(name); break;
                case 3: selectedWarrior = new Archer(name); break;
                case 4: selectedWarrior = new Rogue(name); break;
                case 5: selectedWarrior = new Shaman(name); break;
                default: throw new InvalidOperationException("Неверный выбор класса");
            }
            Console.WriteLine($"Создан {selectedWarrior.ClassName} {selectedWarrior.Name}");
            return selectedWarrior;
        }
        private string GetRandomBotName()
        {
            string[] names = { "Смертонос", "Крушитель", "Зак", "Сэм", "Тень", "Гром", "Астра", "Вайпер" };
            return names[random.Next(names.Length)];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}