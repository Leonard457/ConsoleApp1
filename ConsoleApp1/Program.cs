// WarriorGame.cs (обновленная версия)
using ConsoleApp1.LogicGame;

namespace WarriorBattle
{
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