namespace AutoRetainer.UI.NeoUI.Experiments;
public class Notifications : ExperimentUIEntry
{
    public override void Draw()
    {
        ImGui.Checkbox($"有雇員完成委託時顯示浮層通知", ref C.NotifyEnableOverlay);
        ImGui.Checkbox($"任務或戰鬥中不顯示浮層", ref C.NotifyCombatDutyNoDisplay);
        ImGui.Checkbox($"包含其他角色", ref C.NotifyIncludeAllChara);
        ImGui.Checkbox($"忽略未在多角色模式啟用的其他角色", ref C.NotifyIgnoreNoMultiMode);
        ImGui.Checkbox($"在遊戲聊天欄顯示通知", ref C.NotifyDisplayInChatX);
        ImGuiEx.Text($"遊戲非作用中時：（需要安裝並啟用 NotificationMaster）");
        ImGui.Checkbox($"雇員可用時傳送桌面通知", ref C.NotifyDeskopToast);
        ImGui.Checkbox($"閃爍工作列", ref C.NotifyFlashTaskbar);
        ImGui.Checkbox($"AutoRetainer 已啟用或多角色模式執行中時不通知", ref C.NotifyNoToastWhenRunning);
    }
}
