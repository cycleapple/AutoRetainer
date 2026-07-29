namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "多重模式/雇員";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多重模式－雇員")
        .Checkbox("等待探險完成", () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "多重模式切換至下一個角色前，AutoRetainer 會等待所有雇員返回。")
        .DragInt(60f, "提前重新登入", () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "繼續運作所需的最低物品欄空格", () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .Checkbox("同步雇員（一次性）", () => ref MultiMode.Synchronize, "AutoRetainer 會等待所有已啟用雇員的探險完成，之後自動停用此設定並處理所有角色。")
        .Checkbox("強制完整角色輪替", () => ref C.CharEqualize, "建議擁有超過 15 個角色的使用者啟用。多重模式會依序處理所有角色的探險，再回到輪替起點。")
        .Indent()
        .Checkbox("依探險完成時間排列角色", () => ref C.LongestVentureFirst, "較早完成探險的角色會優先檢查。")
        .Checkbox("依雇員等級與上限排列角色", () => ref C.CappedLevelsLast, "依序優先處理可升級的雇員、已達最高等級的雇員，最後才是受角色職業等級限制而未達最高等級的雇員。")
        .Unindent();
}
