using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public unsafe sealed class DiscardList : InventoryManagementBase
{
    public override string Name => "物品欄整理/丟棄清單";
    private InventoryManagementCommon InventoryManagementCommon = new();

    public override int DisplayPriority => -1;

    private DiscardList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("不論來源為何，只要物品的堆疊數量未超過下方設定值，就會一律丟棄。每次可能改變物品欄的操作前後都會頻繁執行丟棄；即使同一物品也位於出售或分解清單，仍以丟棄為優先。受保護的物品不會被丟棄。")
            .InputInt(150f, "允許丟棄的最大堆疊數量", () => ref InventoryCleanupCommon.SelectedPlan.IMDiscardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.Discard, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMDiscardList.Remove(itemId),
                InventoryCleanupCommon.SelectedPlan.IMDiscardList,
                (x) =>
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, InventoryCleanupCommon.SelectedPlan.IMDiscardIgnoreStack);
                    ImGui.PopFont();
                    ImGuiEx.Tooltip("此物品忽略堆疊數量設定");
                }))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMDiscardList);
            });
    }
}
