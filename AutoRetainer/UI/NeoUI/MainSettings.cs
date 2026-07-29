namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
    public override string Path => "一般";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("延遲")
        .Widget(100f, "時間不同步補償", (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "從雇員探險結束時間額外扣除指定秒數，以減少遊戲與電腦時間不同步可能造成的問題。")
        .Widget(100f, "額外互動延遲（幀）", (x) => ImGuiEx.SliderInt(x, ref C.ExtraFrameDelay.ValidateRange(-10, 100), 0, 50), "數值越低，插件執行操作越快。若幀率較低或網路延遲較高，可提高此值；若希望插件運作更快，則可降低此值。")
        .Widget("額外記錄", (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "啟用大量除錯記錄。啟用期間會產生大量日誌並可能影響效能；重新載入插件或重啟遊戲後會自動停用。")

            .Section("操作")
        .Widget("指派＋重新指派", (x) =>
        {
            if(ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
        }, "若已啟用的雇員目前沒有進行中的探險，會自動指派快速探險，並重新指派目前的探險。")
        .Widget("領取", (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
        }, "只領取雇員探險獎勵，不會重新指派探險。\n與傳喚鈴互動時按住 CTRL，可暫時套用此模式。")
        .Widget("重新指派", (x) =>
        {
            if(ImGui.RadioButton("重新指派", !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
        }, "只重新指派雇員目前進行中的探險。")
        .Widget("雇員感應", (x) => ImGui.Checkbox(x, ref C.RetainerSense), "玩家進入傳喚鈴的互動範圍時，AutoRetainer 會自動啟用。必須保持靜止，否則會取消啟用。")
        .Widget(200f, "啟用時間", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000));


}
