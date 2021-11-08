using System;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Robust.Client.Placement.Modes
{
    public class SnapgridBorder : SnapgridCenter
    {
        public override bool HasLineMode => true;
        public override bool HasGridMode => true;

        public SnapgridBorder(PlacementManager pMan) : base(pMan)
        {
        }


        public override void AlignPlacementMode(ScreenCoordinates mouseScreen)
        {
            MouseCoords = ScreenToCursorGrid(mouseScreen);

            var gridId = MouseCoords.GetGridId(PlacementManager.EntityManager);
            SnapSize = 1f;
            if (gridId.IsValid())
            {
                Grid = PlacementManager.MapManager.GetGrid(gridId);
                SnapSize = Grid.TileSize; //Find snap size for the grid.
            }
            else
            {
                Grid = null;
            }

            GridDistancing = SnapSize;

            var mouselocal = new Vector2( //Round local coordinates onto the snap grid
                (float) MathF.Round(MouseCoords.X / SnapSize, MidpointRounding.AwayFromZero) * SnapSize,
                (float) MathF.Round(MouseCoords.Y / SnapSize, MidpointRounding.AwayFromZero) * SnapSize);

            //Convert back to original world and screen coordinates after applying offset
            MouseCoords =
                new EntityCoordinates(
                    MouseCoords.EntityId, mouselocal + new Vector2(PlacementManager.PlacementOffset.X, PlacementManager.PlacementOffset.Y));
        }
    }
}
