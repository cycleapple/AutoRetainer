using AutoRetainerAPI.Configuration;
using ECommons.MathHelpers;
using RetainerDescriptor = (ulong CID, string RetainerName);

namespace AutoRetainer.UI.NeoUI;
public class RetainersTab : NeoUIEntry
{
    public override string Path => "雇員";
    private int MassMinLevel = 0;
    private int MassMaxLevel = 100;
    private VenturePlan SelectedVenturePlan;
    private EntrustPlan SelectedEntrustPlan;
    private HashSet<RetainerDescriptor> SelectedRetainers = [];

    public override NuiBuilder Builder { get; init; }

    public RetainersTab()
    {
        Builder = new NuiBuilder()
                 .Section("批次變更設定")
                 .Widget(MassConfigurationChangeWidget);
    }

    private void MassConfigurationChangeWidget()
    {
        ImGuiEx.Text("選擇雇員：");
        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##sel", $"已選擇 {SelectedRetainers.Count} 位", ImGuiComboFlags.HeightLarge))
        {
            ref var search = ref Ref<string>.Get("Search");
            ImGui.InputTextWithHint("##searchRetainers", "搜尋角色", ref search, 100);
            foreach(var x in C.OfflineData)
            {
                if((search.Length > 0 && !(x.Name + "@" + x.World).Contains(search, StringComparison.OrdinalIgnoreCase)) || x.RetainerData.Count <= 0)
                {
                    continue;
                }
                ImGuiEx.PushID(x.CID.ToString());
                ImGuiEx.CollectionCheckbox(Censor.Character(x.Name, x.World), x.RetainerData.Select(r => (x.CID, r.Name)), SelectedRetainers);
                ImGui.Indent();
                foreach(var r in x.RetainerData)
                {
                    ImGuiEx.CollectionCheckbox(Censor.Retainer(r.Name), (x.CID, r.Name), SelectedRetainers);
                }
                ImGui.Unindent();
                ImGui.PopID();
            }
            ImGui.EndCombo();
        }
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61527, "全部取消選取"))
        {
            SelectedRetainers.Clear();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61525, "全部選取"))
        {
            SelectedRetainers.Clear();
            foreach(var x in C.OfflineData)
            {
                foreach(var v in x.RetainerData)
                {
                    SelectedRetainers.Add((x.CID, v.Name));
                }
            }
        }

        ImGui.Separator();

        ImGuiEx.TextV("依等級：");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        ImGui.DragInt("##minlevel", ref MassMinLevel, 0.1f);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        ImGui.DragInt("##maxlevel", ref MassMaxLevel, 0.1f);
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61543, "依等級將雇員加入選取"))
        {
            foreach(var x in C.OfflineData)
            {
                foreach(var r in x.RetainerData)
                {
                    if(r.Level.InRange(MassMinLevel, MassMaxLevel, includeEnd: true))
                    {
                        SelectedRetainers.Add((x.CID, r.Name));
                    }
                }
            }
        }

        ImGui.Separator();

        ImGuiEx.Text("操作：");
        ImGui.Separator();
        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo("##ventureplans", SelectedVenturePlan?.Name ?? "尚未選擇", (ImGuiComboFlags)8))
        {
            foreach(var plan in C.SavedPlans)
            {
                if(ImGui.Selectable(plan.Name + "##" + plan.GUID))
                {
                    SelectedVenturePlan = plan;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)62073, "使用探險計畫啟用規劃器"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null && SelectedVenturePlan != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.VenturePlan = SelectedVenturePlan;
                    //adata.VenturePlanIndex = (uint)(C.SavedPlans.IndexOf(SelectedVenturePlan) + 1);
                    adata.EnablePlanner = true;
                    num++;
                }
            }
            Notify.Success($"已套用至 {num} 位雇員");
        }

        ImGui.Separator();

        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo("##entrustplans", SelectedEntrustPlan?.Name ?? "尚未選擇", ImGuiComboFlags.HeightLarge))
        {
            foreach(var plan in C.EntrustPlans)
            {
                if(ImGui.Selectable($"{plan.Name}##{plan.Guid}"))
                {
                    SelectedEntrustPlan = plan;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)62566, "設定委託保管計畫"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.EntrustPlan = SelectedEntrustPlan.Guid;
                    num++;
                }
            }
            Notify.Success($"已套用至 {num} 位雇員");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61526, "移除所選雇員的委託保管計畫"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.EntrustPlan = Guid.Empty;
                    num++;
                }
            }
            Notify.Success($"已套用至 {num} 位雇員");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61526, "停用所選雇員的探險規劃器"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.EnablePlanner = false;
                    num++;
                }
            }
            Notify.Success($"已套用至 {num} 位雇員");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61452, "啟用所選雇員"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var retainers = P.GetSelectedRetainers(x.CID);
                retainers.Add(x.RetainerName);
                num++;
            }
            Notify.Success($"已套用至 {num} 個角色");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61453, "停用所選雇員"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var retainers = P.GetSelectedRetainers(x.CID);
                retainers.Remove(x.RetainerName);
                num++;
            }
            Notify.Success($"已套用至 {num} 個角色");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61528, "為所選雇員的角色啟用雇員多角色模式"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata is { Enabled: false })
                {
                    odata.Enabled = true;
                    num++;
                }
            }
            Notify.Success($"已套用至 {num} 個角色");
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61527, "為所選雇員的角色停用雇員多角色模式"))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata is { Enabled: true })
                {
                    odata.Enabled = false;
                    num++;
                }
            }
            Notify.Success($"已套用至 {num} 個角色");
        }
    }
}
