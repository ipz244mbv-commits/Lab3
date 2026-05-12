using System;

namespace DecoratorPattern
{
    public abstract class Hero
    {
        public string Name { get; protected set; }

        public abstract int GetPower();

        public abstract string GetDescription();
    }

    public class Warrior : Hero
    {
        public Warrior() { Name = "Воїн"; }
        public override int GetPower() => 10; 
        public override string GetDescription() => Name;
    }

    public class Mage : Hero
    {
        public Mage() { Name = "Маг"; }
        public override int GetPower() => 8;
        public override string GetDescription() => Name;
    }

    public class Palladin : Hero
    {
        public Palladin() { Name = "Паладин"; }
        public override int GetPower() => 12;
        public override string GetDescription() => Name;
    }

    public abstract class InventoryDecorator : Hero
    {
        protected Hero _hero; 

        public InventoryDecorator(Hero hero)
        {
            _hero = hero;
        }
    }

    public class Weapon : InventoryDecorator
    {
        private int _damage;
        private string _weaponName;

        public Weapon(Hero hero, string weaponName, int damage) : base(hero)
        {
            _weaponName = weaponName;
            _damage = damage;
        }

        public override int GetPower() => _hero.GetPower() + _damage;

        public override string GetDescription() => $"{_hero.GetDescription()} + зброя [{_weaponName}]";
    }

    public class Armor : InventoryDecorator
    {
        private int _defense;
        private string _armorName;

        public Armor(Hero hero, string armorName, int defense) : base(hero)
        {
            _armorName = armorName;
            _defense = defense;
        }

        public override int GetPower() => _hero.GetPower() + _defense;
        public override string GetDescription() => $"{_hero.GetDescription()} + броня [{_armorName}]";
    }

    public class Artifact : InventoryDecorator
    {
        private int _magicPower;
        private string _artifactName;

        public Artifact(Hero hero, string artifactName, int magicPower) : base(hero)
        {
            _artifactName = artifactName;
            _magicPower = magicPower;
        }

        public override int GetPower() => _hero.GetPower() + _magicPower;
        public override string GetDescription() => $"{_hero.GetDescription()} + артефакт [{_artifactName}]";
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== СТВОРЕННЯ ГЕРОЯ ===");

            Hero myHero = new Warrior();
            Console.WriteLine($"Базовий герой: {myHero.GetDescription()} | Сила: {myHero.GetPower()}");

            myHero = new Armor(myHero, "Сталева кіраса", 15);
            myHero = new Weapon(myHero, "Меч тисячі істин", 25);
            myHero = new Artifact(myHero, "Амулет сили", 10);

            myHero = new Artifact(myHero, "Каблучка здоров'я", 5);

            Console.WriteLine($"\nОдягнений герой: {myHero.GetDescription()}");
            Console.WriteLine($"Загальна сила: {myHero.GetPower()}");

            Console.WriteLine("\n=== СТВОРЕННЯ ІНШОГО ГЕРОЯ ===");

            Hero mage = new Artifact(new Weapon(new Mage(), "Посох вогню", 30), "Мантія невидимості", 5);

            Console.WriteLine($"Герой: {mage.GetDescription()}");
            Console.WriteLine($"Загальна сила: {mage.GetPower()}");

            Console.ReadLine();
        }
    }
}