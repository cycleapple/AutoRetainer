using AutoRetainerAPI.Configuration;
using System.Collections.Frozen;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeContingency : NeoUIEntry
{
    private static readonly FrozenDictionary<WorkshopFailAction, string> WorkshopFailActionNames = new Dictionary<WorkshopFailAction, string>()
    {
        [WorkshopFailAction.StopPlugin] = "停止插件的所有操作",
        [WorkshopFailAction.ExcludeVessel] = "將探索載具排除於操作",
        [WorkshopFailAction.ExcludeChar] = "將角色排除於多重模式輪替",
    }.ToFrozenDictionary();

    public override string Path => "多重模式/應變措施";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("應變措施")
        .TextWrapped("發生常見失敗狀態或潛在操作錯誤時，可在此指定要執行的備援動作。")
        .EnumComboFullWidth(null, "青磷水缸耗盡", () => ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "青磷水缸不足以讓載具再次出航時，執行所選的備援動作。")
        .EnumComboFullWidth(null, "無法修理探索載具", () => ref C.FailureNoRepair, null, WorkshopFailActionNames, "魔導機械修理材料不足以修理載具時，執行所選的備援動作。")
        .EnumComboFullWidth(null, "物品欄已滿", () => ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "角色物品欄空間不足以領取航行報酬時，執行所選的備援動作。")
        .EnumComboFullWidth(null, "重大操作失敗", () => ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "發生未知或其他錯誤時，執行所選的備援動作。")
        .Widget("被 GM 移送至監獄", (x) =>
        {
            ImGui.BeginDisabled();
            ImGuiEx.SetNextItemFullWidth();
                if(ImGui.BeginCombo("##jailsel", "結束遊戲")) { ImGui.EndCombo(); }
            ImGui.EndDisabled();
        }, "插件運作期間若被 GM 移送至監獄，會執行所選的備援動作。祝你好運！");
}
