using System;
using System.Collections.Generic;
using System.Linq;
using Robust.Client.Graphics;
using Robust.Client.Placement.Collections;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;

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

        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        /// <summary>
        /// Drawing shader for drawing without being affected by lighting
        /// </summary>
        private ShaderInstance _drawingShader = default!;

        private readonly Dictionary<string, PlacementMode> _placementModes = new();
        private readonly Dictionary<string, PlacementCollection> _placementCollections = new();
        private string? _selectedPlacementMode;
        private string? _selectedPlacementCollection;

        private ScreenCoordinates? MouseStart;

        public override void Initialize()
        {
            base.Initialize();

            _drawingShader = _prototypeManager.Index<ShaderPrototype>("unshaded").Instance();

            _placementModes.Clear();
            foreach (var mode in _reflectionManager.GetAllChildren<PlacementMode>())
            {
                if(mode.IsAbstract) continue;
                if (mode.GetConstructors().All(x => x.GetParameters().Length != 0))
                {
                    Logger.Error($"No parameterless ctor found for {nameof(PlacementMode)} {mode.FullName}. Skipping.");
                    continue;
                }

                _placementModes[mode.Name] = (PlacementMode) Activator.CreateInstance(mode)!;
            }

            _placementCollections.Clear();
            foreach (var collection in _reflectionManager.GetAllChildren<PlacementCollection>())
            {
                if(collection.IsAbstract) continue;
                if (collection.GetConstructors().All(x => x.GetParameters().Length != 0))
                {
                    Logger.Error($"No parameterless ctor found for {nameof(PlacementCollection)} {collection.FullName}. Skipping.");
                    continue;
                }

                _placementCollections[collection.Name] = (PlacementCollection) Activator.CreateInstance(collection)!;
            }
        }

        public void RenderOverlay(DrawingHandleWorld handle)
        {
            if(_selectedPlacementMode == null)
                return;

            if (_selectedPlacementMode == null || !IsActive)
            {
                if (EraserRect.HasValue)
                {
                }
                return;
            }
            /* eraser
               handle.UseShader(_drawingShader);
               handle.DrawRect(EraserRect.Value, new Color(255, 0, 0, 50));

             */

            if (CurrentPlacementOverlayEntity == null || CurrentPlacementOverlayEntity.Deleted)
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


            if (CurrentPermission == null || CurrentPermission.Range <= 0 || !CurrentMode.RangeRequired
                || PlayerManager.LocalPlayer?.ControlledEntity == null)
                return;

            var worldPos = PlayerManager.LocalPlayer.ControlledEntity.Transform.WorldPosition;

            handle.DrawCircle(worldPos, CurrentPermission.Range, new Color(1, 1, 1, 0.25f));
        }

        #region maybe refactor

        /// <summary>
        /// The entity for placement overlay.
        /// Colour of this gets swapped around in PlacementMode.
        /// This entity needs to stay in nullspace.
        /// </summary>
        public IEntity? CurrentPlacementOverlayEntity { get; set; }

        private void PreparePlacement(string templateName)
        {
            var prototype = _prototypeManager.Index<EntityPrototype>(templateName);
            CurrentPrototype = prototype;
            IsActive = true;

            var lst = SpriteComponent.GetPrototypeTextures(prototype, ResourceCache, out var noRot).ToList();
            PreparePlacementTexList(lst, noRot);
        }

        public void PreparePlacementTexList(List<IDirectionalTextureProvider>? texs, bool noRot)
        {
            var sc = SetupPlacementOverlayEntity();
            if (texs != null)
            {
                // This one covers most cases (including Construction)
                foreach (var v in texs)
                {
                    if (v is RSI.State)
                    {
                        var st = (RSI.State) v;
                        sc.AddLayer(st.StateId, st.RSI);
                    }
                    else
                    {
                        // Fallback
                        sc.AddLayer(v.Default);
                    }
                }
            }
            else
            {
                sc.AddLayer(new ResourcePath("/Textures/UserInterface/tilebuildoverlay.png"));
            }
            sc.NoRotation = noRot;
        }

        private void PreparePlacementTile()
        {
            var sc = SetupPlacementOverlayEntity();
            sc.AddLayer(new ResourcePath("/Textures/UserInterface/tilebuildoverlay.png"));

            IsActive = true;
        }

        private void EnsureNoPlacementOverlayEntity()
        {
            if (CurrentPlacementOverlayEntity != null)
            {
                if (!CurrentPlacementOverlayEntity.Deleted)
                    CurrentPlacementOverlayEntity.Delete();
                CurrentPlacementOverlayEntity = null;
            }
        }

        private SpriteComponent SetupPlacementOverlayEntity()
        {
            EnsureNoPlacementOverlayEntity();
            CurrentPlacementOverlayEntity = EntityManager.SpawnEntity(null, MapCoordinates.Nullspace);
            return CurrentPlacementOverlayEntity.EnsureComponent<SpriteComponent>();
        }

        #endregion
    }
}
