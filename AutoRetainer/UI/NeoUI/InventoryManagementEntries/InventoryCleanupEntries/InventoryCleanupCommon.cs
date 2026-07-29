using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public static unsafe class InventoryCleanupCommon
{
    public static Guid SelectedPlanGuid = Guid.Empty;

    public static InventoryManagementSettings SelectedPlan
    {
        get
        {
            if(SelectedPlanGuid == Guid.Empty)
            {
                return C.DefaultIMSettings;
            }
            else
            {
                var planIndex = C.AdditionalIMSettings.IndexOf(x => x.GUID == SelectedPlanGuid);
                if(planIndex == -1)
                {
                    SelectedPlanGuid = Guid.Empty;
                    return C.DefaultIMSettings;
                }
                else
                {
                    return C.AdditionalIMSettings[planIndex];
                }
            }
        }
    }

    public static NuiBuilder CreateCleanupHeaderBuilder()
    {
        return new NuiBuilder().Section("物品欄整理計畫選擇").Widget(DrawPlanSelector);
    }

    public static void DrawPlanSelector()
    {
        var selectedPlan = C.AdditionalIMSettings.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selimplan", selectedPlan?.DisplayName ?? "預設計畫"))
            {
                if(ImGui.Selectable("預設計畫", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalIMSettings)
                {
                    ImGuiEx.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new InventoryManagementSettings()
                {
                    AllowSellFromArmory = C.DefaultIMSettings.AllowSellFromArmory,
                    IMEnableContextMenu = C.DefaultIMSettings.IMEnableContextMenu,
                    IMEnableCofferAutoOpen = C.DefaultIMSettings.IMEnableCofferAutoOpen,
                    IMSkipVendorIfRetainer = C.DefaultIMSettings.IMSkipVendorIfRetainer,
                    IMEnableAutoVendor = C.DefaultIMSettings.IMEnableAutoVendor,
                    IMEnableNpcSell = C.DefaultIMSettings.IMEnableNpcSell,
                };
                C.AdditionalIMSettings.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("新增計畫");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultIMSettings).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("複製");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<InventoryManagementSettings>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();
                    C.AdditionalIMSettings.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("貼上");
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultIMSettings = selectedPlan.DSFClone();
                    C.DefaultIMSettings.GUID.Regenerate();
                    C.DefaultIMSettings.Name = "";
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("將此計畫設為預設計畫；目前的預設計畫會被覆寫。按住 CTRL 並點擊。");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("刪除此計畫。按住 CTRL 並點擊。");
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint("##name", "輸入計畫名稱", ref selectedPlan.Name, 100);

            if(Data != null)
            {
                if(Data.InventoryCleanupPlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"目前角色正在使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("取消指派"))
                    {
                        Data.InventoryCleanupPlan = Guid.Empty;
                    }
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, UiBuilder.IconFont, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, $"目前角色未使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("指派"))
                    {
                        Data.InventoryCleanupPlan = selectedPlan.GUID;
                    }
                }
                ImGui.SameLine();
            }

            var charas = C.OfflineData.Where(x => x.ExchangePlan == selectedPlan.GUID).ToArray();
            if(charas.Length > 0)
            {
                ImGuiEx.Text($"共有 {charas.Length} 個角色使用");
                ImGuiEx.Tooltip($"{charas.Select(x => x.NameWithWorldCensored)}");
            }
            else
            {
                ImGuiEx.Text($"沒有任何角色使用");
            }

            ImGuiEx.Text("將此計畫的清單與預設計畫合併：");
            ImGui.Indent();
            ImGui.Checkbox("合併快速探險出售清單", ref selectedPlan.AdditionModeSoftSellList);
            ImGuiEx.HelpMarker("同時包含於此計畫與預設計畫中的快速探險取得物品會被出售。");
            ImGui.Checkbox("合併無條件出售清單", ref selectedPlan.AdditionModeHardSellList);
            ImGuiEx.HelpMarker("同時包含於此計畫與預設計畫中的物品會被出售。若物品同時存在於兩者，會採用目前計畫的堆疊數量略過選項；目前計畫的「出售堆疊上限」也會覆寫預設計畫。");
            ImGui.Checkbox("合併丟棄清單", ref selectedPlan.AdditionModeDiscardList);
            ImGuiEx.HelpMarker("同時包含於此計畫與預設計畫中的物品會被丟棄。若物品同時存在於兩者，會採用目前計畫的堆疊數量略過選項；目前計畫的「丟棄堆疊上限」也會覆寫預設計畫。");
            ImGui.Checkbox("合併保護清單", ref selectedPlan.AdditionModeProtectList);
            ImGuiEx.HelpMarker("同時包含於此計畫與預設計畫中的物品不會自動出售或繳交給軍隊，即使也存在於其他清單中。");
            ImGui.Unindent();
        }
    }
}
