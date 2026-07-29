using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.ExcelServices;

namespace AutoRetainer.UI.Statistics;

public sealed class GilDisplayManager
{
    private GilDisplayManager() { }

    public void Draw()
    {
        ImGuiEx.SetNextItemWidthScaled(200f);
        ImGui.InputInt("忽略金幣低於此數值的角色／雇員", ref C.MinGilDisplay.ValidateRange(0, int.MaxValue));
        ImGuiComponents.HelpMarker($"被忽略的雇員金幣仍會計入角色／資料中心總額。僅當角色及其所有雇員的金幣皆低於此數值時，才會忽略該角色；被忽略的角色不會計入資料中心總額。");
        ref var filter = ref Ref<string>.Get();
        ImGui.Checkbox("僅顯示角色總額", ref C.GilOnlyChars);
        ImGui.SameLine();
        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##fltr", "篩選……", ref filter, 50);
        Dictionary<ExcelWorldHelper.Region, List<OfflineCharacterData>> data = [];
        foreach(var x in C.OfflineData)
        {
            if(ExcelWorldHelper.TryGet(x.World, out var world))
            {
                if(!data.ContainsKey((ExcelWorldHelper.Region)world.DataCenter.Value.Region))
                {
                    data[(ExcelWorldHelper.Region)world.DataCenter.Value.Region] = [];
                }
                data[(ExcelWorldHelper.Region)world.DataCenter.Value.Region].Add(x);
            }
        }
        var globalTotal = 0L;
        foreach(var x in data)
        {
            ImGuiEx.Text($"{x.Key}:");
            var dcTotal = 0L;
            foreach(var c in x.Value)
            {
                if(c.NoGilTrack) continue;
                if(filter != "" && !$"{c.Name}@{c.World}".Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
                FCData fcdata = null;
                var charTotal = c.Gil + c.RetainerData.Sum(s => s.Gil);
                foreach(var fc in C.FCData)
                {
                    if(S.FCData.GetHolderChara(fc.Key, fc.Value) == c && fc.Value.GilCountsTowardsChara)
                    {
                        fcdata = fc.Value;
                        charTotal += fcdata.Gil;
                        break;
                    }
                }
                if(charTotal > C.MinGilDisplay)
                {
                    if(!C.GilOnlyChars)
                    {
                        ImGuiEx.Text($"    {Censor.Character(c.Name, c.World)}: {c.Gil:N0}");
                        foreach(var r in c.RetainerData)
                        {
                            if(r.Gil > C.MinGilDisplay)
                            {
                                ImGuiEx.Text($"        {Censor.Retainer(r.Name)}: {r.Gil:N0}");
                            }
                        }
                        if(fcdata != null && fcdata.Gil > 0)
                        {
                            ImGuiEx.Text(ImGuiColors.DalamudYellow, $"        公會 {fcdata.Name}：{fcdata.Gil:N0}");
                        }
                    }
                    ImGuiEx.Text(ImGuiColors.DalamudViolet, $"    {Censor.Character(c.Name, c.World)}{(fcdata != null && fcdata.Gil > 0 ? "＋公會" : "")} 總計：{charTotal:N0}");
                    if(ImGuiEx.HoveredAndClicked("點擊以重新登入"))
                    {
                        if(!MultiMode.Relog(c, out var error, Internal.RelogReason.Command))
                        {
                            Notify.Error(error);
                        }
                    }
                    dcTotal += charTotal;
                    ImGui.Separator();
                }
            }
            ImGuiEx.Text(ImGuiColors.DalamudOrange, $"資料中心總計（{x.Key}）：{dcTotal:N0}");
            globalTotal += dcTotal;
            ImGui.Separator();
            ImGui.Separator();
        }
        ImGuiEx.Text(ImGuiColors.DalamudOrange, $"全部總計：{globalTotal:N0}");
    }
}
