using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data, bool isRetainer)
    {
        ImGuiEx.PushID(data.CID.ToString());
        SharedUI.DrawMultiModeHeader(data);
        var b = new NuiBuilder()

        .Section("角色專屬的一般設定")
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
        });
        if(isRetainer)
        {
            b = b.Section("雇員").Widget(() =>
            {
                ImGuiEx.Text($"自動籌備稀有品調度：");
                if(!AutoGCHandin.Operation)
                {
                    ImGuiEx.SetNextItemWidthScaled(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);
                }
                else
                {
                    ImGuiEx.Text($"目前無法變更此設定");
                }
            });
        }
        else
        {
            b = b.Section("探索載具").Widget(() =>
            {
                ImGui.Checkbox($"等待探索完成", ref data.MultiWaitForAllDeployables);
                ImGuiComponents.HelpMarker("""此設定與全域選項相同，但僅套用至個別角色。啟用後，AutoRetainer 會等待所有探索載具返回，再登入此角色。若因其他原因已登入此角色，仍會重新派遣已完成的潛水艇；除非全域設定中的「即使已登入也要等待」同時啟用。""");
            });
        }
        b = b.Section("傳送覆寫", data.GetAreTeleportSettingsOverriden() ? ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] with { X = 1f } : null, true)
        .Widget(() =>
        {
            ImGuiEx.Text($"可為每個角色個別覆寫傳送設定。");
            bool? demo = null;
            ImGuiEx.Checkbox("標有此記號的選項會使用全域設定值", ref demo);
            ImGuiEx.Checkbox("啟用", ref data.TeleportOptionsOverride.Enabled);
            ImGui.Indent();
            ImGuiEx.Checkbox("為雇員進行傳送……", ref data.TeleportOptionsOverride.Retainers);
            ImGui.Indent();
            ImGuiEx.Checkbox("……前往個人房屋", ref data.TeleportOptionsOverride.RetainersPrivate);
            ImGuiEx.Checkbox("……前往公會房屋", ref data.TeleportOptionsOverride.RetainersFC);
            ImGuiEx.Checkbox("……前往公寓", ref data.TeleportOptionsOverride.RetainersApartment);
            ImGui.Text("若以上選項均停用或失敗，將傳送至旅館。");
            ImGui.Unindent();
            ImGuiEx.Checkbox("處理探索載具時傳送至公會房屋", ref data.TeleportOptionsOverride.Deployables);
            ImGui.Unindent(); 
        }).Draw();
        SharedUI.DrawExcludeReset(data);
        ImGui.PopID();
    }
}
