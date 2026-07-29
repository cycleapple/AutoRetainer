using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "軍隊籌備／一般設定";

    public override void Draw()
    {
        ImGui.Checkbox("啟用稀有品調度連續處理", ref C.AutoGCContinuation);
        ImGui.Indent();
        ImGuiEx.TextWrapped($"""
            啟用稀有品調度連續處理後：
            - 插件會自動花費可用的軍票，購買兌換清單中設定的物品。
            - 若兌換清單為空，則只會購買探險幣。

            花費軍票後：
            - 稀有品調度會自動繼續。
            - 此流程會重複執行，直到沒有可繳交的物品或軍票已用盡。
            """);
        ImGui.Unindent();
    }
}
