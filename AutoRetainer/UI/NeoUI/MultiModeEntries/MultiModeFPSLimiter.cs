namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeFPSLimiter : NeoUIEntry
{
    public override string Path => "多重模式/FPS 限制器";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("FPS 限制器")
        .TextWrapped("FPS 限制器只會在多重模式啟用時生效。")
        .Widget("閒置時的目標幀率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS(x, ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
        })
        .Widget("閒置時的目標幀率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS("運作時的目標幀率", ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
        })
        .Checkbox("遊戲視窗作用中時解除 FPS 限制", () => ref C.NoFPSLockWhenActive)
        .Checkbox("允許極低的 FPS 限制值", () => ref C.ExtraFPSLockRange, "啟用後若多重模式發生任何錯誤，將不提供支援。")
        .Checkbox("僅在設定關機計時器時啟用限制器", () => ref C.FpsLockOnlyShutdownTimer);
}
