using AutoRetainer.Modules.Voyage;
using Dalamud.Game;
using ECommons.GameHelpers;
using ECommons.Reflection;

namespace AutoRetainer.UI.MainWindow;
public static unsafe class TroubleshootingUI
{
    private static readonly Config EmptyConfig = new();
    public static void Draw()
    {
        ImGuiEx.TextWrapped("此分頁會檢查常見的設定問題，方便你在聯絡支援前自行排除。");

        if(!Svc.ClientState.ClientLanguage.EqualsAny(ClientLanguage.Japanese, ClientLanguage.German, ClientLanguage.French, ClientLanguage.English))
        {
            Warning("偵測到地區營運版本的客戶端。AutoRetainer 並未針對此版本完整測試，部分功能可能無法正常運作。");
        }

        if(C.DontLogout)
        {
            Error("已啟用 DontLogout 偵錯選項。");
        }

        foreach(var x in C.OfflineData)
        {
            if(x.WorkshopEnabled)
            {
                var a = x.OfflineSubmarineData.Select(x => x.Name);
                if(a.Count() > a.Distinct().Count())
                {
                    Error($"角色 {Censor.Character(x.Name, x.World)} 有重複的潛水艇名稱。潛水艇名稱必須唯一。");
                }
            }
        }

        if((C.GlobalTeleportOptions.Enabled || C.OfflineData.Any(x => x.TeleportOptionsOverride.Enabled == true)) && !Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Lifestream" && x.IsLoaded))
        {
            Error("已啟用傳送功能，但未安裝或載入 Lifestream。AutoRetainer 無法在此設定下運作；請停用傳送，或安裝並載入 Lifestream。");
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforcePlan)
            {
                Info($"潛水艇解鎖計畫「{x.Name.NullWhenEmpty() ?? x.GUID.ToString()}」設為強制執行；只要仍有可解鎖項目，就會覆寫個別潛水艇設定。");
            }
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforceDSSSinglePoint)
            {
                Info($"潛水艇解鎖計畫「{x.Name.NullWhenEmpty() ?? x.GUID.ToString()}」設為在深海航行單點出航，將忽略手動設定的解鎖行為。");
            }
        }

        try
        {
            if(DalamudReflector.IsOnStaging())
            {
                Error("偵測到非正式版 Dalamud 分支，可能造成問題。請輸入 /xlbranch 開啟分支切換器，改為「release」後重新啟動遊戲。");
            }
        }
        catch(Exception e)
        {
        }

        if(Player.Available)
        {
            if(Player.CurrentWorld != Player.HomeWorld)
            {
                Error("目前正在跨界旅行。必須返回原始世界，AutoRetainer 才能繼續處理此角色。");
            }
            if(C.Blacklist.Any(x => x.CID == Player.CID))
            {
                Error("目前角色已被 AutoRetainer 完全排除，不會進行任何處理。請前往「設定－排除項目」變更。");
            }
            if(Data.ExcludeRetainer)
            {
                Error("目前角色已從雇員清單排除。請前往「設定－排除項目」變更。");
            }
            if(Data.ExcludeWorkshop)
            {
                Error("目前角色已從探索載具清單排除。請前往「設定－排除項目」變更。");
            }
        }

        {
            var list = C.OfflineData.Where(x => x.GetAreTeleportSettingsOverriden());
            if(list.Any())
            {
                Info("部分角色使用自訂傳送選項。將游標移至此處查看清單。", list.Select(x => $"{x.Name}@{x.World}").Print("\n"));
            }
        }

        if(C.NoTeleportHetWhenNextToBell)
        {
            Warning("角色位於傳喚鈴旁時，將停用傳送及進入房屋／公寓。請留意房屋自動撤除期限。");
        }



        if(C.AllowSimpleTeleport)
        {
            Warning("已啟用簡易傳送。此方式不如在 Lifestream 登記房屋可靠；若遇到傳送問題，請停用此選項並改用 Lifestream 登記住址。");
        }

        if(!C.EnableEntrustManager && C.AdditionalData.Any(x => x.Value.EntrustPlan != Guid.Empty))
        {
            Warning("已全域停用委託管理器，但部分雇員仍指派了委託計畫。這些計畫只能手動執行。");
        }

        if(C.ExtraDebug)
        {
            Info("已啟用額外記錄，將產生大量日誌。請僅在收集偵錯資訊時使用。");
        }

        if(C.UnsyncCompensation > -5)
        {
            Warning("時間不同步補償設定過高（>-5），可能造成問題。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTIdle) < 10)
        {
            Warning("閒置時的目標幀率設定過低（<10），可能造成問題。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTRunning) < 20)
        {
            Warning("運作時的目標幀率設定過低（<20），可能造成問題。");
        }

        if(Data?.GetIMSettings().AllowSellFromArmory == true)
        {
            Info("已允許出售兵裝庫中的物品。請務必將零式裝備與絕境戰武器加入保護清單。");
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && !x.Enabled && x.RetainerData.Count > 0);
            if(list.Any())
            {
                Warning("部分擁有雇員的角色尚未啟用雇員多重模式。將游標移至此處查看清單。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && x.Enabled && x.RetainerData.Count > 0 && C.SelectedRetainers.TryGetValue(x.CID, out var rd) && !x.RetainerData.All(r => rd.Contains(r.Name)));
            if(list.Any())
            {
                Warning("部分角色並未啟用所有雇員進行處理。將游標移至此處查看清單。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && !x.WorkshopEnabled && (x.OfflineSubmarineData.Count + x.OfflineAirshipData.Count) > 0);
            if(list.Any())
            {
                Warning("部分已登記探索載具的角色尚未啟用探索載具多重模式。將游標移至此處查看清單。", list.Print("\n"));
            }
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && x.WorkshopEnabled && x.GetEnabledVesselsData(Internal.VoyageType.Airship).Count + x.GetEnabledVesselsData(Internal.VoyageType.Submersible).Count < Math.Min(x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count, 4));
            if(list.Any())
            {
                Warning("部分角色並未啟用所有探索載具進行處理。將游標移至此處查看清單。", list.Print("\n"));
            }
        }

        if(C.MultiModeType != AutoRetainerAPI.Configuration.MultiModeType.Everything)
        {
            Warning($"多重模式類型設為 {C.MultiModeType}，將限制 AutoRetainer 執行的功能。");
        }

        if(C.OfflineData.Any(x => x.MultiWaitForAllDeployables))
        {
            Info("部分角色已啟用「等待所有未完成的探索載具」。AutoRetainer 會等待該角色的所有載具歸航後才開始處理。將游標移至此處查看完整清單。", C.OfflineData.Where(x => x.MultiWaitForAllDeployables).Select(x => $"{x.Name}@{x.World}").Print("\n"));
        }

        if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
        {
            Info("已啟用全域選項「等待探索完成」。所有角色都會等待全部探索載具歸航後才開始處理，即使個別角色未啟用此選項亦同。");
        }

        if(C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
        {
            Info("探索載具已啟用「即使已登入也要等待」。即使目前已登入角色，AutoRetainer 仍會等待該角色所有載具歸航後才開始處理。");
        }

        if(C.DisableRetainerVesselReturn > 0)
        {
            if(C.DisableRetainerVesselReturn > 10)
            {
                Warning("「雇員探險處理截止時間」設得異常高。探索載具即將歸航時，重新指派雇員可能會明顯延遲。");
            }
            else
            {
                Info("已啟用「雇員探險處理截止時間」。探索載具即將歸航時，重新指派雇員可能會延遲。");
            }
        }

        if(C.MultiModeRetainerConfiguration.MultiWaitForAll)
        {
            Info("已啟用「等待探險完成」。AutoRetainer 會等待角色的所有雇員探險完成後，才登入並進行處理。");
        }

        if(C.MultiModeRetainerConfiguration.WaitForAllLoggedIn)
        {
            Info("雇員已啟用「即使已登入也要等待」。即使目前已登入角色，AutoRetainer 仍會等待所有雇員探險完成後才開始處理。");
        }

        {
            var manualList = new List<string>();
            var deletedList = new List<string>();
            foreach(var x in C.OfflineData)
            {
                foreach(var ret in x.RetainerData)
                {
                    var planId = Utils.GetAdditionalData(x.CID, ret.Name).EntrustPlan;
                    var plan = C.EntrustPlans.FirstOrDefault(s => s.Guid == planId);
                    if(plan != null && plan.ManualPlan) manualList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                    if(plan == null && planId != Guid.Empty) deletedList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                }
            }
            if(manualList.Count > 0)
            {
                Info("部分雇員使用手動委託計畫。重新指派探險後不會自動執行這些計畫，只能點擊浮動視窗上的按鈕手動處理。將游標移至此處查看清單。", manualList.Print("\n"));
            }
            if(deletedList.Count > 0)
            {
                Warning("部分雇員原先指派的委託計畫已被刪除，因此不會委託任何物品。將游標移至此處查看清單。", deletedList.Print("\n"));
            }
        }

        if(C.No2ndInstanceNotify)
        {
            Info("已啟用「不警告從同一目錄執行的第二個遊戲執行個體」。AutoRetainer 將自動略過使用同一 Dalamud 目錄之第二個遊戲執行個體的載入。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "SimpleTweaksPlugin" && x.IsLoaded))
        {
            Info("偵測到 Simple Tweaks。任何與雇員或潛水艇相關的調整都可能影響 AutoRetainer，請確認其設定不會互相干擾。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "PandorasBox" && x.IsLoaded))
        {
            Info("偵測到 Pandora's Box。AutoRetainer 運作期間自動使用技能可能造成干擾，請確認 Pandora's Box 不會在此期間自動使用技能。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Automaton" && x.IsLoaded))
        {
            Info("偵測到 Automaton。AutoRetainer 運作期間自動使用技能或輸入數值可能造成干擾，請確認 Automaton 不會在此期間自動執行這些操作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "RotationSolver" && x.IsLoaded))
        {
            Info("偵測到 RotationSolver。AutoRetainer 運作期間自動使用技能可能造成干擾，請確認 RotationSolver 不會在此期間自動使用技能。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName.StartsWith("BossMod") && x.IsLoaded))
        {
            Info("偵測到 BossMod。AutoRetainer 運作期間自動使用技能可能造成干擾，請確認 BossMod 不會在此期間自動使用技能。");
        }

        ImGui.Separator();
        ImGuiEx.TextWrapped("進階設定會改變開發者原先設計的行為。請確認問題不是由錯誤的進階設定造成。");
        CheckExpertSetting("沒有可領取探險時，存取傳喚鈴的動作", nameof(C.OpenBellBehaviorNoVentures));
        CheckExpertSetting("有可領取探險時，存取傳喚鈴的動作", nameof(C.OpenBellBehaviorWithVentures));
        CheckExpertSetting("存取傳喚鈴後的工作完成行為", nameof(C.TaskCompletedBehaviorAccess));
        CheckExpertSetting("手動啟用後的工作完成行為", nameof(C.TaskCompletedBehaviorManual));
        CheckExpertSetting("若有雇員將在 5 分鐘內完成探險，則留在雇員選單", nameof(C.Stay5));
        CheckExpertSetting("關閉雇員清單時自動停用插件", nameof(C.AutoDisable));
        CheckExpertSetting("不顯示插件狀態圖示", nameof(C.HideOverlayIcons));
        CheckExpertSetting("顯示多重模式類型選擇器", nameof(C.DisplayMMType));
        CheckExpertSetting("在工房顯示探索載具核取方塊", nameof(C.ShowDeployables));
        CheckExpertSetting("啟用脫困模組", nameof(C.EnableBailout));
        CheckExpertSetting("AutoRetainer 嘗試脫困前的逾時秒數", nameof(C.BailoutTimeout));
        CheckExpertSetting("停用排序及摺疊／展開", nameof(C.NoCurrentCharaOnTop));
        CheckExpertSetting("在插件介面列顯示多重模式核取方塊", nameof(C.MultiModeUIBar));
        CheckExpertSetting("雇員選單延遲（秒）", nameof(C.RetainerMenuDelay));
        CheckExpertSetting("不檢查探險規劃器錯誤", nameof(C.NoErrorCheckPlanner2));
        CheckExpertSetting("啟用多重模式時嘗試進入附近房屋", nameof(C.MultiHETOnEnable));
        CheckExpertSetting("Artisan 整合", nameof(C.ArtisanIntegration));
        CheckExpertSetting("使用伺服器時間而非電腦時間", nameof(C.UseServerTime));
    }

    private static void Error(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.RedBright, "\uf057");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.RedBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Warning(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.OrangeBright, "\uf071");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.OrangeBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Info(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.YellowBright, "\uf05a");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.YellowBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void CheckExpertSetting(string setting, string nameOfSetting)
    {
        var original = EmptyConfig.GetFoP(nameOfSetting);
        var current = C.GetFoP(nameOfSetting);
        if(!original.Equals(current))
        {
            Info($"進階設定「{setting}」與預設值不同", $"預設值：「{original}」；目前值：「{current}」。");
        }
    }
}
