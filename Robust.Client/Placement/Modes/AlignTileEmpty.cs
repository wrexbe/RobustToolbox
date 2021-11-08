using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Robust.Client.Placement.Modes
{
    public class AlignTileEmpty : PlacementMode
    {
        public override bool HasLineMode => true;
        public override bool HasGridMode => true;

        public AlignTileEmpty(PlacementManager pMan) : base(pMan) { }

        public override void AlignPlacementMode(ScreenCoordinates mouseScreen)
        {
            MouseCoords = ScreenToCursorGrid(mouseScreen);

            var tileSize = 1f;
            var gridId = MouseCoords.GetGridId(PlacementManager.EntityManager);

            if (gridId.IsValid())
            {
                var mapGrid = PlacementManager.MapManager.GetGrid(MouseCoords.GetGridId(PlacementManager.EntityManager));
                CurrentTile = mapGrid.GetTileRef(MouseCoords);
                tileSize = mapGrid.TileSize; //convert from ushort to float
            }

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

            var map = MouseCoords.GetMapId(PlacementManager.EntityManager);
            var bottomLeft = new Vector2(CurrentTile.X, CurrentTile.Y);
            var topRight = new Vector2(CurrentTile.X + 0.99f, CurrentTile.Y + 0.99f);
            var box = new Box2(bottomLeft, topRight);

            return !IoCManager.Resolve<IEntityLookup>().AnyEntitiesIntersecting(map, box);
        }
    }
}
