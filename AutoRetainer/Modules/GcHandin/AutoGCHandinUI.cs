namespace AutoRetainer.Modules.GcHandin;

internal static class AutoGCHandinUI
{
    internal static void Draw()
    {
        ImGui.Checkbox("繳交完成時顯示系統匣通知（需要 NotificationMaster）", ref C.GCHandinNotify);
    }
}
