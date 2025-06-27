using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.LogicGame
{
    public class Effect
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public Action<IWarrior> ApplyEffect { get; set; }
        public Action<IWarrior> TickEffect { get; set; }
        public Action<IWarrior> RemoveEffect { get; set; }
        public bool IsPositive { get; set; } // true — положительный, false — отрицательный

        public Effect()
        {
            IsPositive = false; // По умолчанию отрицательный
        }
    }
}
