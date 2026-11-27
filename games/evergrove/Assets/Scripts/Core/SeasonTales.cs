using System;

namespace Evergrove.Core
{
    /// <summary>A note a neighbour leaves when the season turns.</summary>
    public sealed class SeasonNote
    {
        public string From;
        public string Body;
    }

    /// <summary>
    /// I311 (idea 26, after farming life games where the seasons greet you) and I312 (idea 38,
    /// after A Thousand and One Nights: one tale at a time, never all at once). When the
    /// garden's progress season turns, a neighbour leaves a note in the letterbox: the season
    /// named, and a short tale of their own. The tellers take turns, so each turn brings a new
    /// voice. The tales are their memories, never claims about the player or pressure to play.
    /// </summary>
    public static class SeasonTales
    {
        private static readonly string[] Tellers = { "Mira", "Orin", "Fen" };

        // [teller][season]: Spring, Summer, Autumn, Winter.
        private static readonly string[][] Tales =
        {
            new[] // Mira, who keeps the trees
            {
                "Spring is in the garden. When I was small, my grandmother tied a ribbon to the first tree to leaf each year. For luck, she said, but really it was so we'd stop and look. I've tied one on yours.",
                "Summer, and the shade is deep again. An old beech once stood by my mother's well. On the hottest days the whole street sat under it, and nobody owned that shade. Yours is like that now.",
                "Autumn has turned the leaves. My father kept the first fallen leaf of every year pressed in a book. Forty leaves, forty autumns, and the book still smells of them.",
                "Winter is here, and the trees are resting. A bare tree isn't sleeping, my grandmother said; it's listening for spring. I like to think yours are listening too."
            },
            new[] // Orin, who builds and mends
            {
                "Spring melt is running high. The first bridge I ever built washed away in a spring like this one. The second one held. I still don't know which taught me more.",
                "Summer evenings are long. Folk used to stand on the old stone bridge to watch the swallows skim the water. It's still the best seat in the valley, if you ask me.",
                "Autumn, and the river is low and clear. That's when you can see the stepping stones my uncle laid, worn smooth by sixty years of feet. Good work outlasts the worker.",
                "Winter frost is on the rails. On the coldest mornings my father went out early to scatter ash on the bridge, so the baker could cross. Nobody ever thanked him, and he never minded."
            },
            new[] // Fen, who grows the flowers
            {
                "Spring has come, and the first shoots are up. My aunt planted a bulb for every child born in our village. Some of those flowers are older than I am, and they still come up each year.",
                "Summer, and the bees are busy. We used to say a garden belongs to everyone the day the bees find it. They'll find yours, if they haven't already.",
                "Autumn is gathering in. My grandmother saved seed from her best flowers in paper twists, each labelled in her careful hand. I found a twist last year, forty years on, and it grew.",
                "Winter is quiet in the beds. Under the snow, the snowdrops are already making up their minds. My mother called them the bravest flower: they come first, when no one else will."
            }
        };

        /// <summary>The note for the season the garden has just turned to, on its nth turn.</summary>
        public static SeasonNote For(Season season, int turn)
        {
            int teller = ((turn % Tellers.Length) + Tellers.Length) % Tellers.Length;
            return new SeasonNote { From = Tellers[teller], Body = Tales[teller][(int)season] };
        }
    }
}
