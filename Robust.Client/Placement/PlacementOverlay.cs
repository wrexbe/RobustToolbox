using Robust.Shared.Enums;
 using Robust.Client.Graphics;


namespace Robust.Client.Placement
{
    public partial class PlacementManager
    {
        internal class PlacementOverlay : Overlay
        {
            private readonly PlacementSystem _system;
            public override OverlaySpace Space => OverlaySpace.WorldSpace;

            public PlacementOverlay(PlacementSystem system)
            {
                _system = system;
                ZIndex = 100;
            }

            protected internal override void Draw(in OverlayDrawArgs args)
            {
                _system.RenderOverlay(args.WorldHandle);
            }
        }
    }
}
