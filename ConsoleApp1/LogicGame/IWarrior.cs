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
        int MaxHealth { get; } // МАХ ХП
        int Stamina { get; set; }// Стамина
        int MaxStamina { get; } // МАХ СТАМИНА
        int Mana { get; set; } // Мана
        int MaxMana { get; } // МАХ МАНА
        int AttackDamage { get; } // Урон 
        int Armor { get; } // Броня
        double CritChance { get; } // Крит шанс
        double EvasionChance { get; } // Уворот шанс
        bool IsDefending { get; set; } // укреплённость
        bool IsAlive { get; } // Жив ? Да : Нет

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
        public int ChooseAiAction(IWarrior target, bool isPlayer); // Выбор действия ИИ
    }
}