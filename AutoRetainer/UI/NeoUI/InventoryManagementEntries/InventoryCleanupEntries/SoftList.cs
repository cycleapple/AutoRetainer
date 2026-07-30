namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class SoftList : InventoryManagementBase
{
    public override string Name => "物品欄整理/快速探險出售清單";
    private InventoryManagementCommon InventoryManagementCommon = new();
    private SoftList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("透過快速探險取得這些物品時會將其出售；若已與相同物品合併堆疊則不會出售。")
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.SoftSell, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMAutoVendorSoft.Remove(itemId), InventoryCleanupCommon.SelectedPlan.IMAutoVendorSoft,
                filter: item => item.PriceLow != 0))
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMAutoVendorSoft);
            });
    }
}
