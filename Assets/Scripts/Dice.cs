using System;

namespace DefaultNamespace
{
    using System;
    public class Dice
    {
        private readonly Random _random = new Random();

        public int RollD20()
        {
            return _random.Next(1, 21);
        }
    }
}