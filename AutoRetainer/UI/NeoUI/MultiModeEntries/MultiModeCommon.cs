namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "多角色模式／一般設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("一般設定")
        .Checkbox($"在登入畫面等待", () => ref C.MultiWaitOnLoginScreen, "若沒有任何角色有可處理的委託，會登出並等待，直到再次有角色可用。啟用此選項及多角色模式期間，標題畫面影片會停用。")
        .Checkbox($"手動登入時停用多角色模式", () => ref C.MultiDisableOnRelog, "透過 AutoRetainer 介面或指令重新登入時，停用多角色模式。")
        .Checkbox($"手動登入時不要重設優先角色", () => ref C.MultiNoPreferredReset, "透過 AutoRetainer 介面或指令重新登入時，不重設優先角色。")
        .Checkbox("允許進入共享房屋", () => ref C.SharedHET)
        .Checkbox("即使多角色模式已停用，登入時仍嘗試進入房屋", () => ref C.HETWhenDisabled)
        .Checkbox("已在傳喚鈴旁時，不要為雇員進行傳送或進入房屋", () => ref C.NoTeleportHetWhenNextToBell)

        .Section("遊戲啟動")
        .Checkbox($"遊戲啟動時啟用多角色模式", () => ref C.MultiAutoStart)
        .Widget("遊戲啟動時自動登入", (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "停用");
            dict.Add("~", "上次登入的角色");
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", "~", .. names], names: dict);
        })
        .SliderInt(150f, "延遲", () => ref C.AutoLoginDelay.ValidateRange(0, 60), 0, 20, "設定適當延遲，讓插件在登入前完全載入，也預留取消登入的時間。")

        .Section("物品欄警告")
        .InputInt(100f, $"雇員清單：剩餘物品欄警告", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"雇員清單：剩餘探險幣警告", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"探索載具清單：剩餘物品欄警告", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"探索載具清單：剩餘燃料警告", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"探索載具清單：剩餘修理材料警告", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("傳送")
        .Widget(() => ImGuiEx.Text("需要 Lifestream 插件"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .TextWrapped("若要讓此選項生效，必須為每個角色在 Lifestream 登錄房屋，或啟用簡易傳送。")
        .TextWrapped("可在角色設定選單中為每個角色自訂這些設定。")
        .Widget(() =>
        {
            if(Data != null && Data.GetAreTeleportSettingsOverriden())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "目前角色使用自訂傳送選項。");
            }
        })
        .Checkbox("啟用", () => ref C.GlobalTeleportOptions.Enabled)
        .Indent()
        .Checkbox("為雇員進行傳送……", () => ref C.GlobalTeleportOptions.Retainers)
        .Indent()
        .Checkbox("……前往個人房屋", () => ref C.GlobalTeleportOptions.RetainersPrivate)
        .Checkbox("……前往公會房屋", () => ref C.GlobalTeleportOptions.RetainersFC)
        .Checkbox("……前往公寓", () => ref C.GlobalTeleportOptions.RetainersApartment)
        .TextWrapped("若以上選項均停用或失敗，將傳送至旅館。")
        .Unindent()
        .Checkbox("處理探索載具時傳送至公會房屋", () => ref C.GlobalTeleportOptions.Deployables)
        .Checkbox("啟用簡易傳送", () => ref C.AllowSimpleTeleport)
        .Unindent()
        .Widget(() => ImGuiEx.HelpMarker("""
            無須在 Lifestream 登錄房屋即可傳送。請注意：傳送功能仍需要安裝 Lifestream 插件。

            警告：此選項的可靠性低於在 Lifestream 登錄房屋，請僅在必要時使用。
            """, EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("防卡死模組")
        .Checkbox("連線錯誤時自動關閉錯誤並重試登入", () => ref C.ResolveConnectionErrors, "中斷連線後，AutoRetainer 會嘗試重新登入。若工作階段已到期，則不會嘗試登入。")
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}
