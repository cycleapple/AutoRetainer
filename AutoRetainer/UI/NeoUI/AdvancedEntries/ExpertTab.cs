using ECommons.Configuration;
using ECommons.Reflection;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class ExpertTab : NeoUIEntry
{
    public override string Path => "進階／專家設定";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("行為")
        .EnumComboFullWidth(null, "沒有可領取委託時使用傳喚鈴的動作：", () => ref C.OpenBellBehaviorNoVentures)
        .EnumComboFullWidth(null, "有可領取委託時使用傳喚鈴的動作：", () => ref C.OpenBellBehaviorWithVentures)
        .EnumComboFullWidth(null, "使用傳喚鈴後的工作完成行為：", () => ref C.TaskCompletedBehaviorAccess)
        .EnumComboFullWidth(null, "手動啟用後的工作完成行為：", () => ref C.TaskCompletedBehaviorManual)
        .EnumComboFullWidth(null, "插件運作期間的工作完成行為：", () => ref C.TaskCompletedBehaviorAuto)
        .TextWrapped(ImGuiColors.DalamudGrey, "多角色模式運作期間，前述三項設定會強制使用「關閉雇員清單並停用插件」。")
        .Checkbox("若有雇員將在 5 分鐘內完成委託，則留在雇員選單", () => ref C.Stay5, "多角色模式運作期間會強制啟用此選項。")
        .Checkbox($"關閉雇員清單時自動停用插件", () => ref C.AutoDisable, "僅在你自行離開選單時適用；其他情況會套用上方設定。")
        .Checkbox($"不要顯示插件狀態圖示", () => ref C.HideOverlayIcons)
        .Checkbox($"顯示多角色模式類型選擇器", () => ref C.DisplayMMType)
        .Checkbox($"在地下工房顯示探索載具核取方塊", () => ref C.ShowDeployables)
        .Checkbox("啟用防卡死模組", () => ref C.EnableBailout)
        .InputInt(150f, "AutoRetainer 嘗試解除卡死前的逾時秒數", () => ref C.BailoutTimeout)

        .Section("設定")
        .Checkbox($"停用排序及摺疊／展開", () => ref C.NoCurrentCharaOnTop)
        .Checkbox($"在插件介面列顯示多角色模式核取方塊", () => ref C.MultiModeUIBar)
        .SliderIntAsFloat(100f, "雇員選單延遲（秒）", () => ref C.RetainerMenuDelay.ValidateRange(0, 2000), 0, 2000)
        .Checkbox($"允許委託計時器顯示負值", () => ref C.TimerAllowNegative)
        .Checkbox($"不要檢查委託規劃器錯誤", () => ref C.NoErrorCheckPlanner2)
        .Checkbox("啟用手動重新登入的角色後續處理", () => ref C.AllowManualPostprocess, "AutoRetainer 鎖定於後續處理時，允許手動呼叫指令。")
        .Widget("市場冷卻浮層", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.MarketCooldownOverlay))
            {
                if(C.MarketCooldownOverlay)
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Enable();
                }
                else
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Disable();
                }
            }
        })

        .Section("整合")
        .Checkbox($"Artisan 整合", () => ref C.ArtisanIntegration, "委託可領取且附近有傳喚鈴時，會暫停 Artisan 並自動啟用 AutoRetainer。處理完委託後會重新啟用 Artisan，繼續原本的工作。")

        .Section("伺服器時間")
        .Checkbox("使用伺服器時間而非電腦時間", () => ref C.UseServerTime)

        .Section("工具")
        .Widget("清理幽靈雇員", (x) =>
        {
            if(ImGui.Button(x))
            {
                var i = 0;
                foreach(var d in C.OfflineData)
                {
                    i += d.RetainerData.RemoveAll(x => x.Name == "");
                }
                DuoLog.Information($"Cleaned {i} entries");
            }
        })

        .Section("匯入／匯出")
        .Widget(() =>
        {
            if(ImGui.Button("匯出（不含角色資料）"))
            {
                var clone = C.JSONClone();
                clone.OfflineData = null;
                clone.AdditionalData = null;
                clone.FCData = null;
                clone.SelectedRetainers = null;
                clone.Blacklist = null;
                clone.AutoLogin = "";
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone, false));
            }
            if(ImGui.Button("匯入並與角色資料合併"))
            {
                try
                {
                    var c = EzConfig.DefaultSerializationFactory.Deserialize<Config>(Paste());
                    c.OfflineData = C.OfflineData;
                    c.AdditionalData = C.AdditionalData;
                    c.FCData = C.FCData;
                    c.SelectedRetainers = C.SelectedRetainers;
                    c.Blacklist = C.Blacklist;
                    c.AutoLogin = C.AutoLogin;
                    if(c.GetType().GetFieldPropertyUnions().Any(x => x.GetValue(c) == null)) throw new NullReferenceException();
                    EzConfig.SaveConfiguration(C, $"Backup_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.json");
                    P.SetConfig(c);
                }
                catch(Exception e)
                {
                    e.LogDuo();
                }
            }
        });
}
