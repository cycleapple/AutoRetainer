namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class HardList : InventoryManagementBase
{
    public override string Name => "物品欄整理/無條件出售清單";
    private InventoryManagementCommon InventoryManagementCommon = new();

    private HardList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("不論來源為何，只要物品的堆疊數量未超過下方設定值，就會一律出售。此外，只有此清單中的物品會出售給 NPC。")
            .InputInt(150f, "允許出售的最大堆疊數量", () => ref InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardStackLimit)
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.HardSell, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard.Remove(itemId),
                InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard, 
                (x) =>
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGuiEx.CollectionButtonCheckbox(FontAwesomeIcon.Database.ToIconString(), x, InventoryCleanupCommon.SelectedPlan.IMAutoVendorHardIgnoreStack);
                    ImGui.PopFont();
                    ImGuiEx.Tooltip("此物品忽略堆疊數量設定");
                },
                filter: item => item.PriceLow != 0))
            .Separator()
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMAutoVendorHard);
            });
    }
}
