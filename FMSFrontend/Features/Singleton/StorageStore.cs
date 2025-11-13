using System.Collections.Generic;
using System.Windows;                              
using CommunityToolkit.Mvvm.ComponentModel;         // ✅ ObservableObject / ObservableProperty
using FMSFrontend.Features.Mappings;                 // ✅ ApplyTo 擴充方法
using FMSFrontend.Features.Dtos;                      // ✅ AsrsParameterDto
using FMSFrontend.Models;
using System.Reflection;                               // ✅ ProductionLinesPage
using System.Linq;
using FMSFrontend.Features.Services; // needed for FirstOrDefault
using System.Collections.ObjectModel;
using System;
using System.Windows.Threading;
using System.Diagnostics;

namespace FMSFrontend.Features.Singleton
{
    public partial class StorageStore : ObservableObject
    {
        [ObservableProperty] public StorageGroupModel storageGroup = new();
        public void ApplyStorageGroupDto(StorageGroupModel data)
        {
            var disp = Application.Current?.Dispatcher;
            void apply()
            {
                // 群組統計
                StorageGroup.WaitingTotal = data.WaitingTotal;
                StorageGroup.ProcessingTotal = data.ProcessingTotal;
                StorageGroup.ErrorTotal = data.ErrorTotal;
                StorageGroup.CompletedTotal = data.CompletedTotal;
                StorageGroup.RestrictionTotal = data.RestrictionTotal;
                StorageGroup.BookedTotal = data.BookedTotal;

                // 整體清單直接替換
                StorageGroup.Storage = data.Storage;

                // 對齊選取的 Storage 參考
                if (data.SelectStorage != null)
                {
                    // 優先指向清單內相同項目，否則就用來源的 SelectStorage
                    var fromSel = data.SelectStorage;
                    var matched = StorageGroup.Storage?
                        .FirstOrDefault(s => s.Name == fromSel.Name && s.Number == fromSel.Number)
                        ?? StorageGroup.Storage?.FirstOrDefault(s => s.Title == fromSel.Title)
                        ?? fromSel;

                    StorageGroup.SelectStorage = matched;
                }
                else if (StorageGroup.SelectStorage != null)
                {
                    // 嘗試以目前選擇在新清單中找到對應實例
                    var cur = StorageGroup.SelectStorage;
                    var matched = StorageGroup.Storage?
                        .FirstOrDefault(s => s.Name == cur.Name && s.Number == cur.Number)
                        ?? StorageGroup.Storage?.FirstOrDefault(s => s.Title == cur.Title);
                    if (matched != null)
                        StorageGroup.SelectStorage = matched;
                }

                if (StorageGroup.SelectStorage != null)
                {
                    var target = StorageGroup.SelectStorage;
                    var source = data.SelectStorage ?? target; // 若無來源選擇，使用目前選擇做就地刷新

                    // 同步基本屬性與統計
                    target.Name = source.Name;
                    target.Number = source.Number;
                    target.Serial = source.Serial;
                    target.Rows = source.Rows;
                    target.Columns = source.Columns;

                    target.WaitingCount = source.WaitingCount;
                    target.ProcessingCount = source.ProcessingCount;
                    target.ErrorCount = source.ErrorCount;
                    target.CompletedCount = source.CompletedCount;
                    target.RestrictionCount = source.RestrictionCount;
                    target.BookedCount = source.BookedCount;

                    // 對齊 Slots 長度
                    var targetSlots = target.Slots ?? new ObservableCollection<Slot>();
                    var sourceSlots = source.Slots ?? new ObservableCollection<Slot>();

                    if (target.Slots != targetSlots)
                        target.Slots = targetSlots;

                    if (targetSlots.Count != sourceSlots.Count)
                    {
                        targetSlots.Clear();
                        foreach (var s in sourceSlots)
                            targetSlots.Add(s);
                    }
                    else
                    {
                        // 就地更新（Slot 為可通知屬性）
                        for (int i = 0; i < sourceSlots.Count; i++)
                        {
                            var t = targetSlots[i];
                            var s = sourceSlots[i];
                            t.Name = s.Name;
                            t.Serial = s.Serial;
                            t.Kind = s.Kind;
                            t.MaterialStatus = s.MaterialStatus;
                            t.MaterialRestriction = s.MaterialRestriction;
                            t.StorageStatus = s.StorageStatus;
                            t.StorageRestriction = s.StorageRestriction;
                        }
                    }
                }
            }

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }

        /*
        private void EnsureTimer()
        {
            if (_applyTimer != null) return;
            var disp = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            _applyTimer = new DispatcherTimer(WorkTickInterval, DispatcherPriority.Background, OnApplyTick, disp);
        }

        private void OnApplyTick(object? sender, EventArgs e)
        {
            // 無待更新資料 => 停止
            if (_pendingUpdate == null)
            {
                _applyTimer?.Stop();
                return;
            }

            // 休息相位：切回工作相位，下一個 tick 再做事
            if (!_inWorkPhase)
            {
                _inWorkPhase = true;
                _applyTimer!.Interval = WorkTickInterval;
                _phaseStopwatch.Restart();
                return; // 本次休息 tick 不做更新
            }

            var data = _pendingUpdate;

            // 確保有 StorageGroup 實例
            if (StorageGroup == null)
                StorageGroup = new StorageGroupModel();

            // 第一次先同步群組統計與清單長度（較重的操作一次做完），之後逐筆更新內容
            if (!_listSynced)
            {
                // 1) 群組統計
                StorageGroup.WaitingTotal = data.WaitingTotal;
                StorageGroup.ProcessingTotal = data.ProcessingTotal;
                StorageGroup.ErrorTotal = data.ErrorTotal;
                StorageGroup.CompletedTotal = data.CompletedTotal;
                StorageGroup.RestrictionTotal = data.RestrictionTotal;
                StorageGroup.BookedTotal = data.BookedTotal;

                // 2) 清單長度對齊（保留 Storage 集合實例）
                var targetList = StorageGroup.Storage;
                var sourceList = data.Storage;

                if (targetList == null)
                {
                    StorageGroup.Storage = new ObservableCollection<StorageModel>(sourceList ?? new());
                }
                else if (sourceList == null)
                {
                    targetList.Clear();
                }
                else if (targetList.Count != sourceList.Count)
                {
                    targetList.Clear();
                    foreach (var s in sourceList)
                        targetList.Add(s);
                }

                _listSynced = true; // 下一個 tick 再逐筆更新內容

                // 若已超過 100ms 視窗，切換到休息相位
                if (_phaseStopwatch.Elapsed >= WorkWindow)
                {
                    EnterCooldown();
                }
                return;
            }

            // 逐筆就地同步 Storage 項目
            var target = StorageGroup.Storage;
            var source = data.Storage;

            int sourceCount = source?.Count ?? 0;
            int targetCount = target?.Count ?? 0;

            if (target == null || source == null || targetCount == 0 || sourceCount == 0)
            {
                // 無資料可處理，改做選擇同步並結束
                SyncSelection(data);
                _pendingUpdate = null;
                _applyTimer?.Stop();
                return;
            }

            if (_applyIndex < Math.Min(sourceCount, targetCount))
            {
                var from = source[_applyIndex];
                var to = target[_applyIndex];

                // 基本屬性與統計
                to.Name = from.Name;
                to.Number = from.Number;
                to.Serial = from.Serial;
                to.Rows = from.Rows;
                to.Columns = from.Columns;

                to.WaitingCount = from.WaitingCount;
                to.ProcessingCount = from.ProcessingCount;
                to.ErrorCount = from.ErrorCount;
                to.CompletedCount = from.CompletedCount;
                to.RestrictionCount = from.RestrictionCount;
                to.BookedCount = from.BookedCount;

                // Slots：就地同步
                var toSlots = to.Slots;
                var fromSlots = from.Slots;
                if (toSlots == null)
                {
                    to.Slots = new ObservableCollection<Slot>(fromSlots ?? new());
                }
                else if (fromSlots == null)
                {
                    toSlots.Clear();
                }
                else if (toSlots.Count != fromSlots.Count)
                {
                    toSlots.Clear();
                    foreach (var slot in fromSlots)
                        toSlots.Add(slot);
                }
                else
                {
                    for (int j = 0; j < fromSlots.Count; j++)
                        toSlots[j] = fromSlots[j];
                }

                _applyIndex++; // 本次工作步驟完成

                // 若已超過 100ms 視窗，切換到休息相位
                if (_phaseStopwatch.Elapsed >= WorkWindow)
                {
                    EnterCooldown();
                }
                return;
            }

            // 所有 Storage 都已同步，處理 SelectStorage 後結束
            SyncSelection(data);
            _pendingUpdate = null;
            _applyTimer?.Stop();
        }

        private void EnterCooldown()
        {
            _inWorkPhase = false;
            _applyTimer!.Interval = CooldownWindow;
            _phaseStopwatch.Restart();
        }

        private void SyncSelection(StorageGroupModel data)
        {
            // 同步選擇中的 Storage（指向現有清單中的對應實例，並就地同步內容）
            StorageModel? sourceSelected = null;
            if (data.SelectStorage != null)
            {
                sourceSelected = StorageGroup.Storage
                    .FirstOrDefault(s => s.Name == data.SelectStorage.Name && s.Number == data.SelectStorage.Number)
                    ?? StorageGroup.Storage.FirstOrDefault(s => s.Title == data.SelectStorage.Title)
                    ?? data.SelectStorage;
            }
            else if (StorageGroup.SelectStorage != null)
            {
                var cur = StorageGroup.SelectStorage;
                sourceSelected = StorageGroup.Storage
                    .FirstOrDefault(s => s.Name == cur.Name && s.Number == cur.Number)
                    ?? StorageGroup.Storage.FirstOrDefault(s => s.Title == cur.Title);
            }

            if (StorageGroup.SelectStorage == null)
            {
                if (sourceSelected != null)
                    StorageGroup.SelectStorage = sourceSelected;
            }
            else if (sourceSelected != null)
            {
                var selected = StorageGroup.SelectStorage;

                selected.Name = sourceSelected.Name;
                selected.Number = sourceSelected.Number;
                selected.Serial = sourceSelected.Serial;
                selected.Rows = sourceSelected.Rows;
                selected.Columns = sourceSelected.Columns;

                selected.WaitingCount = sourceSelected.WaitingCount;
                selected.ProcessingCount = sourceSelected.ProcessingCount;
                selected.ErrorCount = sourceSelected.ErrorCount;
                selected.CompletedCount = sourceSelected.CompletedCount;
                selected.RestrictionCount = sourceSelected.RestrictionCount;
                selected.BookedCount = sourceSelected.BookedCount;

                var targetSlots = selected.Slots;
                var sourceSlots = sourceSelected.Slots;
                if (targetSlots == null)
                {
                    selected.Slots = new ObservableCollection<Slot>(sourceSlots ?? new());
                }
                else if (sourceSlots == null)
                {
                    targetSlots.Clear();
                }
                else if (targetSlots.Count != sourceSlots.Count)
                {
                    targetSlots.Clear();
                    foreach (var slot in sourceSlots)
                        targetSlots.Add(slot);
                }
                else
                {
                    for (int i = 0; i < sourceSlots.Count; i++)
                        targetSlots[i] = sourceSlots[i];
                }
            }
        }
        */
    }
}
