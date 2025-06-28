using ConsoleApp1.LogicGame;

namespace ConsoleApp1.SpecialClassWarrior
{
    public class Peasant : WarriorBase
    {
        public override string ClassName => "Крестьянин";

        public Peasant(string name)
            : base(
                name,
                health: 70,
                stamina: 50,
                attackDamage: 10,
                armor: 5,
                mana: 0,
                critChance: 0.25,
                evasionChance: 0.25
            )
        {

        }
    }
}
