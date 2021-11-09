using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Map;

namespace Robust.Client.Placement.Collections
{
    public abstract class PlacementCollection
    {
        public abstract string[] PlacementModes { get; }

        //todo sane way to do grid/line

        /// <summary>
        /// Erases all qualifying element based on the <paramref name="mouseCoordinates"/>
        /// </summary>
        /// <param name="mouseCoordinates">Position of the mouse in screenspace.</param>
        public abstract void Erase(ScreenCoordinates mouseCoordinates);

        /// <summary>
        /// Renders indicators for all qualifiying elements that would be erased based on the <paramref name="mouseCoordinates"/>.
        /// </summary>
        /// <param name="mouseCoordinates">Position of the mouse in screenspace.</param>
        /// <param name="handle">The drawingworldhandle.</param>
        public abstract void RenderErase(ScreenCoordinates mouseCoordinates, DrawingHandleWorld handle);

        public abstract void RenderErase(ScreenCoordinates topLeft, ScreenCoordinates bottomRight, DrawingHandleWorld handle);

        /// <summary>
        /// Creates and returns a control for editing the collection-variables.
        /// </summary>
        /// <returns>The control to be added to the editor.</returns>
        public abstract Control CreateEditorControl();

        /// <summary>
        /// Place element at specified mouseCoordinates.
        /// </summary>
        /// <param name="mouseCoordinates">Position of the mouse in screenspace.</param>
        public abstract void Place(ScreenCoordinates mouseCoordinates);
    }
}
