namespace AutoRetainer.UI.NeoUI;
public class MiscTab : NeoUIEntry
{
    public override string Path => "其他";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("統計")
        .Checkbox($"記錄雇員探險統計", () => ref C.RecordStats)

        .Section("自動軍隊籌備品專家配送")
        .Checkbox("繳納完成時顯示系統匣通知（需要 NotificationMaster）", () => ref C.GCHandinNotify)

        .Section("效能")

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.BeginDisabled())
        .EndIf()

        .Checkbox($"插件運作時移除最小化狀態的 FPS 限制", () => ref C.UnlockFPS)
        .Checkbox($"－同時移除一般 FPS 限制", () => ref C.UnlockFPSUnlimited)
        .Checkbox($"－同時暫停 ChillFrames 插件", () => ref C.UnlockFPSChillFrames)
        .Checkbox($"插件運作時提高 FFXIV 處理程序優先順序", () => ref C.ManipulatePriority, "可能會使其他程式變慢")

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.EndDisabled())
        .EndIf();
}
