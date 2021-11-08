using Robust.Shared.GameObjects;
using Robust.Shared.Maths;

namespace Robust.Client.Placement
{
    public class PlacementSystem : EntitySystem
    {
        /// <summary>
        /// Color set to the ghost entity when it has a valid spawn position
        /// </summary>
        public static readonly Color ValidPlaceColor = new(20, 180, 20); //green

        /// <summary>
        /// Color set to the ghost entity when it has an invalid spawn position
        /// </summary>
        public static readonly Color InvalidPlaceColor = new(180, 20, 20); //red

    }
}
