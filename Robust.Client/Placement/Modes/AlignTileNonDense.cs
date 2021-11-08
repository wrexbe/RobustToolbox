using Robust.Shared.Map;

namespace Robust.Client.Placement.Modes
{
    public class AlignTileNonDense : PlacementMode
    {
        public override bool HasLineMode => true;
        public override bool HasGridMode => true;

        public AlignTileNonDense(PlacementManager pMan) : base(pMan) { }

        public override void AlignPlacementMode(ScreenCoordinates mouseScreen)
        {
            MouseCoords = ScreenToCursorGrid(mouseScreen);

            var tileSize = 1f;

            var gridId = MouseCoords.GetGridId(PlacementManager.EntityManager);
            if (gridId.IsValid())
            {
                var mapGrid = PlacementManager.MapManager.GetGrid(gridId);
                tileSize = mapGrid.TileSize; //convert from ushort to float
            }

            CurrentTile = GetTileRef(MouseCoords);
            GridDistancing = tileSize;

            if (PlacementManager.CurrentPermission!.IsTile)
            {
                MouseCoords = new EntityCoordinates(MouseCoords.EntityId,
                    (CurrentTile.X + tileSize / 2, CurrentTile.Y + tileSize / 2));
            }
            else
            {
                MouseCoords = new EntityCoordinates(MouseCoords.EntityId,
                    (CurrentTile.X + tileSize / 2 + PlacementManager.PlacementOffset.X,
                        CurrentTile.Y + tileSize / 2 + PlacementManager.PlacementOffset.Y));
            }
        }

        public override bool IsValidPosition(EntityCoordinates position)
        {
            if (!RangeCheck(position))
            {
                return false;
            }
            return PlacementManager.CurrentPermission!.IsTile || !IsColliding(position);
        }
    }
}
