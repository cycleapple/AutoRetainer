namespace AutoRetainer.UI.NeoUI.Experiments;

internal class NightMode : ExperimentUIEntry
{
    public override string Name => "夜間模式";
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"夜間模式：\n" +
                $"- 強制啟用「在登入畫面等待」選項\n" +
                $"- 強制套用內建 FPS 限制\n" +
                $"- 遊戲失去焦點並等待時，將限制為 0.2 FPS\n" +
                $"- 遊戲看似停止回應屬正常現象；重新聚焦遊戲視窗後，最多等待 5 秒即可恢復。\n" +
                $"- 夜間模式預設僅處理探索載具\n" +
                $"- 停用夜間模式後，防卡死管理器會重新登入遊戲。");
        if(ImGui.Checkbox("啟用夜間模式", ref C.NightMode)) MultiMode.BailoutNightMode();
        ImGui.Checkbox("顯示夜間模式核取方塊", ref C.ShowNightMode);
        ImGui.Checkbox("夜間模式處理雇員", ref C.NightModeRetainers);
        ImGui.Checkbox("夜間模式處理探索載具", ref C.NightModeDeployables);
        ImGui.Checkbox("保留夜間模式狀態", ref C.NightModePersistent);
        ImGui.Checkbox("關閉指令改為啟用夜間模式，而非關閉遊戲", ref C.ShutdownMakesNightMode);
    }
}
