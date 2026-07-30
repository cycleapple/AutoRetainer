using ECommons.GameHelpers;
using ECommons.Singletons;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.Scheduler.Tasks;
public static unsafe class TaskDeliverItems
{
    public static void Enqueue(bool force = false)
    {
        var gcInfo = GCContinuation.GetGCInfo();
        if(gcInfo == null)
        {
            Notify.Error("尚未加入大國防聯軍");
            return;
        }
        if(S.LifestreamIPC.IsBusy())
        {
            Notify.Error("Lifestream 忙碌中");
            return;
        }
        if(!force && Utils.IsBusy)
        {
            Notify.Error("AutoRetainer 忙碌中");
            return;
        }
        if(Vector3.Distance(gcInfo.Value.Position, Player.Position) > 1f)
        {
            P.TaskManager.Enqueue(() => S.LifestreamIPC.ExecuteCommand("gc " + Player.GrandCompany switch
            {
                ECommons.ExcelServices.GrandCompany.ImmortalFlames => "if",
                ECommons.ExcelServices.GrandCompany.Maelstrom => "m",
                ECommons.ExcelServices.GrandCompany.TwinAdder => "ta",
                _ => throw new ArgumentOutOfRangeException()
            }));
        }
        P.TaskManager.Enqueue(() => !S.LifestreamIPC.IsBusy(), new(timeLimitMS: 5 * 60 * 1000));
        P.TaskManager.Enqueue(() => GCContinuation.EnqueueInitiation(true));
    }
}
