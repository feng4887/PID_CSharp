//=============================================================================
// Description: Public Designer-compatible panel retaining the tank's paint buffering.
// Author: Jinwang DU / Codex
// Date: 2026-11-10
//=============================================================================
namespace pid_loop_CSharp
{
    public sealed class WaterTankScenePanel : Panel
    {
        public WaterTankScenePanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }
}
