using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class UserInterface : NeoUIEntry
{
    public override string Path => "使用者介面";

    public override NuiBuilder Builder => new NuiBuilder()

        .Section("使用者介面")
        .Checkbox("匿名顯示雇員", () => ref C.NoNames, "一般介面會隱藏雇員名稱，但除錯選單與插件日誌仍會顯示。啟用後，各頁面中的角色與雇員編號不一定互相對應（例如雇員頁面的「雇員 1」不一定是統計頁面的同一位雇員）。")
        .Checkbox("在雇員介面顯示快速選單", () => ref C.UIBar)
        .Checkbox("顯示雇員詳細資訊", () => ref C.ShowAdditionalInfo, "在主介面顯示雇員的道具品級／獲得力／鑑別力，以及目前探險的名稱。")
        .Widget("按下 ESC 時不要關閉 AutoRetainer 視窗", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
        })
        .Checkbox("狀態列只顯示最重要的圖示", () => ref C.StatusBarMSI)
        .SliderInt(120f, "狀態列圖示大小", () => ref C.StatusBarIconWidth, 32, 128)
        .Checkbox("遊戲啟動時開啟 AutoRetainer 視窗", () => ref C.DisplayOnStart)
        //.Checkbox("Skip item sell/trade confirmation while plugin is active", () => ref C.SkipItemConfirmations)
        .Checkbox("啟用標題畫面按鈕（需要重新啟動插件）", () => ref C.UseTitleScreenButton)
        .Checkbox("隱藏角色搜尋", () => ref C.NoCharaSearch)
        .Checkbox("角色作業完成時不要閃爍背景", () => ref C.NoGradient)
        .Checkbox("不要警告同一目錄正在執行第二個遊戲執行個體", () => ref C.No2ndInstanceNotify, "第二個遊戲執行個體將自動略過載入 AutoRetainer；必須在第一個執行個體停用此選項後，才能於第二個執行個體載入")

        .Section("雇員分頁的角色排序")
        .Checkbox("啟用", () => ref C.EnableRetainerSort)
        .TextWrapped("這只會改變顯示順序，不會影響角色的處理順序。")
        .Widget(() => UIUtils.DrawSortableEnumList("rorder", C.RetainersVisualOrders))

        .Section("探索載具分頁的角色排序")
        .Checkbox("啟用", () => ref C.EnableDeployablesSort)
        .TextWrapped("這只會改變顯示順序，不會影響角色的處理順序。")
        .Widget(() => UIUtils.DrawSortableEnumList("dorder", C.DeployablesVisualOrders));



}
