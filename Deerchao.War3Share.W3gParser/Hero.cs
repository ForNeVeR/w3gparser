using System.Collections.Generic;

namespace Deerchao.War3Share.W3gParser
{
    public class Hero
    {
        private static readonly int trainSkillDelay = 2000;
        private static readonly int retrainTimeDelayAfterUsingTome = 15000;
        private int lastRetrainTime;
        private int lastTrainTime;

        private readonly string name;
        private int level;
        private Dictionary<string, int> abilities = new Dictionary<string, int>();
        readonly List<Dictionary<string, int>> abilitySets = new List<Dictionary<string, int>>();
        private readonly List<int> reviveTimes = new List<int>();

        private int possibleRetrainedTime;

        public Hero(string name)
        {
            this.name = name;
        }

        public int Level
        {
            get { return level; }
        }

        public string Name
        {
            get { return name; }
        }

        public IEnumerable<string> GetAbilityNames()
        {
            foreach (string ability in abilities.Keys)
                yield return ability;
        }

        public int GetAbilityLevel(string ability)
        {
            if (!abilities.ContainsKey(ability))
                return 0;
            return abilities[ability];
        }

        public int GetAbilitySetCount()
        {
            return abilitySets.Count;
        }

        public IEnumerable<string> GetAbilityNames(int abilitySetIndex)
        {
            foreach (string ability in abilitySets[abilitySetIndex].Keys)
                yield return ability;
        }

        public int GetAbilityLevel(string ability, int abilitySetIndex)
        {
            return abilitySets[abilitySetIndex][ability];
        }

        internal void Order(int time)
        {
            reviveTimes.Add(time);
        }

        public void Train(string ability, int time)
        {
            if (time - possibleRetrainedTime < retrainTimeDelayAfterUsingTome)
            {
                if (lastRetrainTime != possibleRetrainedTime)
                {
                    lastRetrainTime = possibleRetrainedTime;
                    level = 0;
                    abilities = new Dictionary<string, int>();
                    abilitySets.Add(abilities);
                }

                if (abilities.ContainsKey(ability))
                    abilities[ability]++;
                else
                    abilities.Add(ability, 1);

                level++;
            }
            else if (time - lastTrainTime > trainSkillDelay)
            {
                if (abilities.ContainsKey(ability))
                    abilities[ability]++;
                else
                    abilities.Add(ability, 1);

                level++;
                lastTrainTime = time;
            }
        }

        internal void PossibleRetrained(int time)
        {
            possibleRetrainedTime = time;
        }

        internal void Cancel(int time)
        {
            reviveTimes.Remove(time);
        }
    }
}