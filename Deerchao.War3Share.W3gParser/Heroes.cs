using System.Collections.Generic;

namespace Deerchao.War3Share.W3gParser
{
    public class Heroes
    {
        readonly List<Hero> heroes = new List<Hero>();
        readonly List<OrderItem> buildOrders = new List<OrderItem>();

        internal void Order(string name, int time)
        {
            foreach (Hero hero in heroes)
            {
                if (hero.Name == name)
                {
                    hero.Order(time);
                    return;
                }
            }
            Hero h = new Hero(name);
            heroes.Add(h);
            buildOrders.Add(new OrderItem(name, time));
        }

        public IEnumerable<Hero> Items
        {
            get
            {
                foreach (Hero hero in heroes)
                    yield return hero;
            }
        }

        public Hero this[string name]
        {
            get
            {
                foreach (Hero hero in heroes)
                    if (hero.Name == name)
                        return hero;
                return null;
            }
        }

        internal void Cancel(string name, int time)
        {
            foreach (Hero hero in heroes)
            {
                if (hero.Name == name)
                {
                    hero.Cancel(time);
                    return;
                }
            }
        }

        internal void Train(string ability, int time)
        {
            string heroName = ParserUtility.GetHeroByAbility(ability);
            foreach (Hero hero in heroes)
            {
                if (hero.Name == heroName)
                {
                    hero.Train(ability, time);
                }
            }
        }

        internal void PossibleRetrained(int time)
        {
            foreach (Hero hero in heroes)
                hero.PossibleRetrained(time);
        }

        public int Count
        {
            get
            {
                return heroes.Count;
            }
        }
    }
}