using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Client.Graphics;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Broadphase;
using Robust.Shared.Utility;

namespace Robust.Client.Placement
{
    public abstract class PlacementMode
    {
        protected readonly PlacementManager PlacementManager;

        protected PlacementMode(PlacementManager pMan)
        {
            PlacementManager = pMan;
        }

        /// <summary>
        /// Used for line and grid placement to determine how spaced apart the entities should be
        /// </summary>
        protected float GridDistancing = 1f;

        /// <summary>
        /// Whether this mode can use the line placement mode
        /// </summary>
        public virtual bool HasLineMode => false;

        /// <summary>
        /// Whether this mode can use the grid placement mode
        /// </summary>
        public virtual bool HasGridMode => false;

        /// <summary>
        /// Returns the actual position to use for placement/preview rendering based on the <paramref name="mousePosition"/>.
        /// </summary>
        /// <param name="mousePosition">Position of the mouse in screenspace.</param>
        /// <param name="alignedCoordinates">Position to use for placement/preview rendering.</param>
        /// <returns>True if a valid position was found.</returns>
        public abstract bool TryGetCoordinates(ScreenCoordinates mousePosition, [NotNullWhen(true)] out EntityCoordinates? alignedCoordinates);
    }
}
