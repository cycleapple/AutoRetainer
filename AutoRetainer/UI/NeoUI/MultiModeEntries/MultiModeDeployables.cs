namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "多重模式/探索載具";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多重模式－探索載具")
        .Checkbox("等待航行完成", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, "啟用後，AutoRetainer 會等待所有探索載具歸航後才登入角色。若因其他原因已登入，仍會讓已完成的潛水艇再次出航，除非同時啟用全域設定「即使已登入也要等待」。")
        .Indent()
        .Checkbox("即使已登入也要等待", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, "改變「等待航行完成」（全域與個別角色）的行為。已登入時不會讓單艘潛水艇立即再次出航，而是等待所有潛水艇歸航後才採取行動。")
        .InputInt(120f, "最長等待時間（分鐘）", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, "若等待其他探索載具歸航會超過此分鐘數，AutoRetainer 將忽略「等待航行完成」及「即使已登入也要等待」。")
        .Unindent()
        .DragInt(60f, "提前重新登入（秒）", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "在此角色的潛水艇可再次出航前，AutoRetainer 提前登入的秒數。")
        .DragInt(120f, "雇員探險處理截止時間（分鐘）", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "若大於 0，任一角色預定讓潛水艇再次出航前的指定分鐘數內，AutoRetainer 將停止處理所有雇員。")
        .Checkbox("進入工房時定期檢查公會金庫的金幣", () => ref C.FCChestGilCheck, "進入工房時定期檢查公會金庫，以更新金幣計數。")
        .Indent()
        .SliderInt(150f, "檢查頻率（小時）", () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("重設冷卻時間", (x) =>
        {
            if(ImGuiEx.Button(x, C.FCChestGilCheckTimes.Count > 0)) C.FCChestGilCheckTimes.Clear();
        })
        .Unindent();
}
