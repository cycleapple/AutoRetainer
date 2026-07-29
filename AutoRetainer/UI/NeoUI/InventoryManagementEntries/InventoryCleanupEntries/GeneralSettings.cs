using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "物品欄整理/一般設定";

    private GeneralSettings()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
        .Checkbox("自動開啟探險寶箱", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableCofferAutoOpen, "僅適用於多重模式。登出前會開啟所有寶箱，除非物品欄空間不足。")
        .Checkbox("允許出售物品給雇員", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableAutoVendor, "AutoRetainer 檢查並重新指派雇員探險時，會依照物品欄整理計畫出售物品。")
        .Checkbox("允許出售物品給房屋 NPC", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell, "AutoRetainer 進入房屋時，會依照物品欄整理計畫出售物品。可收購物品的房屋 NPC 必須放在房屋入口附近（不是工房入口），進入後應能立即互動。")
            .Indent()
        .Checkbox("附近有雇員時略過 NPC", () => ref InventoryCleanupCommon.SelectedPlan.IMSkipVendorIfRetainer)
        .Widget("立即出售", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
        .Checkbox("自動分解物品", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesis)
        .Checkbox("啟用右鍵選單整合", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableContextMenu)
        .Checkbox("允許出售／丟棄兵裝庫中的物品", () => ref InventoryCleanupCommon.SelectedPlan.AllowSellFromArmory)
        .Checkbox("演示模式", () => ref InventoryCleanupCommon.SelectedPlan.IMDry, "不實際出售或丟棄物品，只在聊天視窗列出原本會出售的項目。")
            ;
    }
}
