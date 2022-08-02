namespace GameLogic
{
    public static class GameHelper
    {
        public static Random Rnd = new Random();

        public static void Shuffle<T>(List<T> list)
        {
            // Fisher-Yates / Knuth shuffle
            for (int i = 0; i < list.Count - 1; i++)
            {
                var j = i + Rnd.Next(list.Count - i);
                (list[j], list[i]) = (list[i], list[j]);
            }

        }
    }
}