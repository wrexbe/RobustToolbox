﻿using System;
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
        /// Aligns the location of placement based on cursor location
        /// </summary>
        /// <param name="mouseScreen"></param>
        /// <returns>Returns whether the current position is a valid placement position</returns>
        public abstract void AlignPlacementMode(ScreenCoordinates mouseScreen);

        /// <summary>
        /// Verifies the location of placement is a valid position to place at
        /// </summary>
        /// <param name="mouseScreen"></param>
        /// <returns></returns>
        public abstract bool IsValidPosition(EntityCoordinates position);

        public virtual void Render(DrawingHandleWorld handle)
        {
            var sce = PlacementManager.CurrentPlacementOverlayEntity;
            if (sce == null || sce.Deleted)
                return;
            var sc = sce.GetComponent<SpriteComponent>();

            IEnumerable<EntityCoordinates> locationcollection;
            switch (PlacementManager.PlacementType)
            {
                case PlacementManager.PlacementTypes.None:
                    locationcollection = SingleCoordinate();
                    break;
                case PlacementManager.PlacementTypes.Line:
                    locationcollection = LineCoordinates();
                    break;
                case PlacementManager.PlacementTypes.Grid:
                    locationcollection = GridCoordinates();
                    break;
                default:
                    locationcollection = SingleCoordinate();
                    break;
            }

            var dirAng = PlacementManager.Direction.ToAngle();
            foreach (var coordinate in locationcollection)
            {
                if (!coordinate.IsValid(PlacementManager.EntityManager))
                    return; // Just some paranoia just in case
                var entity = coordinate.GetEntity(PlacementManager.EntityManager);
                var worldPos = coordinate.ToMapPos(PlacementManager.EntityManager);
                var worldRot = entity.Transform.WorldRotation + dirAng;

                sc.Color = IsValidPosition(coordinate) ? ValidPlaceColor : InvalidPlaceColor;
                sc.Render(handle, PlacementManager.eyeManager.CurrentEye.Rotation, worldRot, worldPos);
            }
        }

        public IEnumerable<EntityCoordinates> SingleCoordinate()
        {
            yield return MouseCoords;
        }

        public IEnumerable<EntityCoordinates> LineCoordinates()
        {
            var (x, y) = MouseCoords.ToMapPos(PlacementManager.EntityManager) - PlacementManager.StartPoint.ToMapPos(PlacementManager.EntityManager);
            float iterations;
            Vector2 distance;
            if (Math.Abs(x) > Math.Abs(y))
            {
                iterations = Math.Abs(x / GridDistancing);
                distance = new Vector2(x > 0 ? 1 : -1, 0) * GridDistancing;
            }
            else
            {
                iterations = Math.Abs(y / GridDistancing);
                distance = new Vector2(0, y > 0 ? 1 : -1) * GridDistancing;
            }

            for (var i = 0; i <= iterations; i++)
            {
                yield return new EntityCoordinates(PlacementManager.StartPoint.EntityId, PlacementManager.StartPoint.Position + distance * i);
            }
        }

        // This name is a nice reminder of our origins. Never forget.
        public IEnumerable<EntityCoordinates> GridCoordinates()
        {
            var placementdiff = MouseCoords.ToMapPos(PlacementManager.EntityManager) - PlacementManager.StartPoint.ToMapPos(PlacementManager.EntityManager);
            var distanceX = new Vector2(placementdiff.X > 0 ? 1 : -1, 0) * GridDistancing;
            var distanceY = new Vector2(0, placementdiff.Y > 0 ? 1 : -1) * GridDistancing;

            var iterationsX = Math.Abs(placementdiff.X / GridDistancing);
            var iterationsY = Math.Abs(placementdiff.Y / GridDistancing);

            for (var x = 0; x <= iterationsX; x++)
            {
                for (var y = 0; y <= iterationsY; y++)
                {
                    yield return new EntityCoordinates(PlacementManager.StartPoint.EntityId, PlacementManager.StartPoint.Position + distanceX * x + distanceY * y);
                }
            }
        }

        /// <summary>
        ///     Returns the tile ref for a grid, or a map.
        /// </summary>
        public TileRef GetTileRef(EntityCoordinates coordinates)
        {
            var mapCoords = coordinates.ToMap(PlacementManager.EntityManager);
            var gridId = coordinates.GetGridId(PlacementManager.EntityManager);
            return gridId.IsValid() ? PlacementManager.MapManager.GetGrid(gridId).GetTileRef(MouseCoords)
                : new TileRef(mapCoords.MapId, gridId,
                    MouseCoords.ToVector2i(PlacementManager.EntityManager, PlacementManager.MapManager), Tile.Empty);
        }

        public TextureResource GetSprite(string key)
        {
            return PlacementManager.ResourceCache.GetResource<TextureResource>(new ResourcePath("/Textures/") / key);
        }

        public bool TryGetSprite(string key, [NotNullWhen(true)] out TextureResource? sprite)
        {
            return PlacementManager.ResourceCache.TryGetResource(new ResourcePath(@"/Textures/") / key, out sprite);
        }

        /// <summary>
        /// Checks if the player is spawning within a certain range of his character if range is required on this mode
        /// </summary>
        /// <returns></returns>
        public bool RangeCheck(EntityCoordinates coordinates)
        {
            if (!RangeRequired)
                return true;

            if (PlacementManager.PlayerManager.LocalPlayer?.ControlledEntity == null)
            {
                return false;
            }

            var range = PlacementManager.CurrentPermission!.Range;
            if (range > 0 && !PlacementManager.PlayerManager.LocalPlayer.ControlledEntity.Transform.Coordinates.InRange(PlacementManager.EntityManager, coordinates, range))
                return false;
            return true;
        }

        public bool IsColliding(EntityCoordinates coordinates)
        {
            var bounds = PlacementManager.ColliderAABB;
            var mapCoords = coordinates.ToMap(PlacementManager.EntityManager);
            var (x, y) = mapCoords.Position;

            var collisionBox = Box2.FromDimensions(
                bounds.Left + x,
                bounds.Bottom + y,
                bounds.Width,
                bounds.Height);

            return EntitySystem.Get<SharedPhysicsSystem>().TryCollideRect(collisionBox, mapCoords.MapId);
        }

        protected Vector2 ScreenToWorld(Vector2 point)
        {
            return PlacementManager.eyeManager.ScreenToMap(point).Position;
        }

        protected Vector2 WorldToScreen(Vector2 point)
        {
            return PlacementManager.eyeManager.WorldToScreen(point);
        }

        protected EntityCoordinates ScreenToCursorGrid(ScreenCoordinates coords)
        {
            var mapCoords = PlacementManager.eyeManager.ScreenToMap(coords.Position);
            if (!PlacementManager.MapManager.TryFindGridAt(mapCoords, out var grid))
            {
                return EntityCoordinates.FromMap(PlacementManager.MapManager, mapCoords);
            }

            return EntityCoordinates.FromMap(PlacementManager.EntityManager, grid.GridEntityId, mapCoords);
        }
    }
}
