using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Item 75: recipes. A recipe is a named way of cooking goods the garden already holds (fruit, jam, seeds),
    /// and it is found the first time it is cooked. Recipes use only the existing goods: no new food is stored,
    /// so there is no new value to price (see economy-v1.md, "Recipes"). Ids are fixed once shipped.
    /// </summary>
    public static class Recipes
    {
        /// <summary>Three fruit cooked into a jar of jam (the kitchen's MakeJam).</summary>
        public const int Jam = 0;
        /// <summary>How many recipes exist; ids are 0..Count-1.</summary>
        public const int Count = 1;

        /// <summary>Every recipe found in the mask: bit n is recipe n.</summary>
        public const int AllMask = (1 << Count) - 1;

        public static bool IsRecipe(int id) { return id >= 0 && id < Count; }

        /// <summary>The bit for a recipe id.</summary>
        public static int Mask(int id)
        {
            if (!IsRecipe(id)) throw new ArgumentOutOfRangeException(nameof(id));
            return 1 << id;
        }

        /// <summary>The recipe's name, as the almanac and kitchen show it.</summary>
        public static string Name(int id)
        {
            switch (id)
            {
                case Jam: return "Jam";
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        /// <summary>What it takes, in plain words.</summary>
        public static string Method(int id)
        {
            switch (id)
            {
                case Jam: return "Three fruit make a jar of jam.";
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        /// <summary>The names of the recipes found in <paramref name="world"/>, in id order. Empty until one is cooked.</summary>
        public static string[] Found(WorldModel world)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            var names = new System.Collections.Generic.List<string>();
            for (int id = 0; id < Count; id++)
                if (world.HasFound(id)) names.Add(Name(id));
            return names.ToArray();
        }
    }

    /// <summary>
    /// Recipe state of a garden: which recipes were found (a bitmask, never written while 0, Living gardens only).
    /// Cooking goes through the existing kitchen (MakeJam), which records the find.
    /// </summary>
    public sealed partial class WorldModel
    {
        /// <summary>Recipes found so far, as a mask over <see cref="Recipes"/> ids (0 none).</summary>
        public int RecipesFound { get { return state.recipes; } }

        /// <summary>Whether a recipe has been cooked in this garden at least once.</summary>
        public bool HasFound(int recipe)
        {
            return Recipes.IsRecipe(recipe) && (state.recipes & Recipes.Mask(recipe)) != 0;
        }

        /// <summary>Whether <see cref="CookRecipe"/> would succeed now, changing nothing.</summary>
        public bool CanCookRecipe(int recipe)
        {
            switch (recipe)
            {
                case Recipes.Jam: return CanMakeJam;
                default: return false;
            }
        }

        /// <summary>
        /// Cooks a recipe. Refused, changing nothing, when the garden lacks the goods or the store is full.
        /// The first successful cook finds the recipe.
        /// </summary>
        public bool CookRecipe(int recipe)
        {
            switch (recipe)
            {
                case Recipes.Jam: return MakeJam();
                default: return false;
            }
        }

        /// <summary>Fail-closed validation of the found-recipe mask: only defined bits, and only in a Living garden.</summary>
        private static bool ReadRecipes(WorldSnapshot source)
        {
            return source.recipes >= 0 && source.recipes <= Recipes.AllMask &&
                (source.mode == "living" || source.recipes == 0);
        }
    }
}
