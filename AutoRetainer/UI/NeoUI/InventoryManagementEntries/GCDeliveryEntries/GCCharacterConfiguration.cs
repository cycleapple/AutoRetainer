using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GCCharacterConfiguration : InventoryManagementBase
{
    public override string Name { get; } = "軍隊籌備／角色設定";

    public override int DisplayPriority => -10;

    public override void Draw()
    {
        ImGuiEx.TextWrapped($"可在此將預先設定的兌換清單指派給已登錄角色，並選擇繳交模式。");
        ImGuiEx.SetNextItemFullWidth();
        ImGuiEx.FilteringInputTextWithHint("##search", "搜尋……", out var filter);
        if(ImGuiEx.BeginDefaultTable(["~角色", "計畫", "繳交模式"]))
        {
            foreach(var characterData in C.OfflineData)
            {
                if(filter != "" && !characterData.NameWithWorld.Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                ImGuiEx.PushID(characterData.Identity);
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(characterData.NameWithWorldCensored);
                ImGui.TableNextColumn();
                var plan = characterData.ExchangePlan == Guid.Empty ? null : C.AdditionalGCExchangePlans.FirstOrDefault(p => p.GUID == characterData.ExchangePlan);
                ImGui.SetNextItemWidth(200f);
                if(ImGui.BeginCombo("##chPlan", plan?.DisplayName ?? "預設計畫", ImGuiComboFlags.HeightLarge))
                {
                    if(ImGui.Selectable("預設計畫", plan == null)) characterData.ExchangePlan = Guid.Empty;
                    ImGui.Separator();
                    foreach(var exchangePlan in C.AdditionalGCExchangePlans)
                    {
                        ImGuiEx.PushID(exchangePlan.ID);
                        if(ImGui.Selectable($"{exchangePlan.DisplayName}"))
                        {
                            characterData.ExchangePlan = exchangePlan.GUID;
                        }
                        ImGui.PopID();
                    }
                    ImGui.EndCombo();
                }
                ImGuiEx.DragDropRepopulate("Plan", plan?.GUID ?? Guid.Empty, ref characterData.ExchangePlan);

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.EnumCombo("##deliveryMode", ref characterData.GCDeliveryType);
                ImGuiEx.DragDropRepopulate("Mode", characterData.GCDeliveryType, ref characterData.GCDeliveryType);

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    }
}
