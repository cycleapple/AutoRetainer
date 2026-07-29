using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow;

internal static class SharedUI
{
    internal static void DrawLockout(OfflineCharacterData data)
    {
        if(data.IsLockedOut())
        {
            FontAwesome.PrintV(EColor.RedBright, FontAwesomeIcon.Lock);
            ImGuiEx.Tooltip("此角色位於目前暫時停用的資料中心。請前往設定解除停用。");
            ImGui.SameLine();
        }
    }

    internal static void DrawMultiModeHeader(OfflineCharacterData data, string overrideTitle = null)
    {
        var b = true;
        ImGui.CollapsingHeader($"{Censor.Character(data.Name)} {overrideTitle ?? "設定"}##conf", ref b, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
        if(b == false)
        {
            ImGui.CloseCurrentPopup();
        }
        ImGui.Dummy(new(500, 1));
    }

    internal static void DrawServiceAccSelector(OfflineCharacterData data)
    {
        ImGuiEx.Text($"服務帳號選擇");
        ImGuiEx.SetNextItemWidthScaled(150);
        if(ImGui.BeginCombo("##Service Account Selection", $"服務帳號 {data.ServiceAccount + 1}", ImGuiComboFlags.HeightLarge))
        {
            for(var i = 1; i <= 10; i++)
            {
                if(ImGui.Selectable($"服務帳號 {i}"))
                {
                    data.ServiceAccount = i - 1;
                }
            }
            ImGui.EndCombo();
        }
    }

    internal static void DrawPreferredCharacterUI(OfflineCharacterData data)
    {
        if(ImGui.Checkbox("優先角色", ref data.Preferred))
        {
            foreach(var z in C.OfflineData)
            {
                if(z.CID != data.CID)
                {
                    z.Preferred = false;
                }
            }
        }
        ImGuiComponents.HelpMarker("多角色模式運作時，若沒有其他角色即將完成需要領取的委託，會重新登入優先角色。");
    }

    internal static void DrawExcludeReset(OfflineCharacterData data)
    {
        new NuiBuilder().Section("清除／重設角色資料", collapsible: true)
        .Widget(() =>
        {
            if(ImGuiEx.ButtonCtrl("排除此角色"))
            {
                C.Blacklist.Add((data.CID, data.Name));
            }
            ImGuiComponents.HelpMarker("排除此角色會立即重設其設定、將其從清單移除，並排除其所有雇員的自動處理。你仍可對其雇員執行手動工作；此操作可在設定中取消。");
            if(ImGuiEx.ButtonCtrl("重設角色資料"))
            {
                new TickScheduler(() => C.OfflineData.RemoveAll(x => x.CID == data.CID));
            }
            ImGuiComponents.HelpMarker("移除此角色的已儲存資料，但不排除角色。下次重新登入此角色時會重新產生資料。");
        }).Draw();
    }
}
