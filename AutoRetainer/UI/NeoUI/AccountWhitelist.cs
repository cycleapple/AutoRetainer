using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class AccountWhitelist : NeoUIEntry
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"可設定帳號白名單。使用不在白名單中的帳號登入時，AutoRetainer 不會記錄任何角色、雇員或潛水艇。");
        if(C.WhitelistedAccounts.Count == 0)
        {
            ImGuiEx.TextWrapped(EColor.GreenBright, "目前白名單狀態：已停用。新增帳號即可啟用。");
        }
        else
        {
            ImGuiEx.TextWrapped(EColor.YellowBright, "目前白名單狀態：已啟用。移除所有帳號即可停用。");
        }

        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.UserPlus, "新增目前帳號", enabled: Player.Available))
        {
            C.WhitelistedAccounts.Add(*P.Memory.MyAccountId);
        }

        foreach(var x in C.WhitelistedAccounts)
        {
            ImGuiEx.PushID(x.ToString());
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                new TickScheduler(() => C.WhitelistedAccounts.Remove(x));
            }
            ImGui.SameLine();
            ImGuiEx.TextV($"帳號 {x}");
            ImGui.PopID();
        }
    }
}
