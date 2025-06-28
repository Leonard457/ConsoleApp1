// WarriorGame.cs (обновленная версия)
using WarriorBattle;

namespace ConsoleApp1.LogicGame
{
    // Интерфейс для Воина
    public interface IWarrior
    {
        string Name { get; } // ИМЯ
        string ClassName { get; } // Класс
        int Health { get; set; } // ХП
        int MaxHealth { get; set; } // МАХ ХП
        int Stamina { get; set; }// Стамина
        int MaxStamina { get; set; } // МАХ СТАМИНА
        int Mana { get; set; } // Мана
        int MaxMana { get; set; } // МАХ МАНА
        int AttackDamage { get; set; } // Урон 
        int Armor { get; set; } // Броня
        double CritChance { get; set; } // Крит шанс
        double EvasionChance { get; set; } // Уворот шанс
        bool IsDefending { get; set; } // укреплённость
        bool IsAlive { get; } // Жив ? Да : Нет
        public abstract int CountActions { get; }  // Количество действий в текущем ходе

        void PerformBaseAttack(IWarrior target); //Баз атака
        void PerformDefend();// Баз защита
        void PerformSkipTurn(); // скип хода
        void PerformHeal(); // Баз лечение
        void TakeDamage(int baseDamage, bool isCritical); // Баз урон + крит
        void ResetTurnState(); // Сброс состояния хода
        void DisplayStats(); // Показ статы
        List<string> GetActionList(); // Получить список действий
        bool CanPerformAction(int actionChoice, IWarrior target);  // Проверка возможности действия
        void ExecuteAction(int actionChoice, IWarrior target, bool isPlayer); // Выполнить действие
        List<Effect> ActiveEffects { get; } // АКТИв ЭФЕК
        void ApplyEffect(Effect effect); // Применить эффект
        void DrainStamina(int amount);// Сброс стамины
        void DrainMana(int amount);// Сброс маны
        void DrainArmor(int amount); // Сброс брони
        public int ChooseAiAction(IWarrior target); // Выбор действия ИИ
    }
}