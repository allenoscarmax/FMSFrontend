using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FMSFrontend.Features.Mappings
{
    public static class RobotMapping
    {
        public static void ApplyAsrsDto(this AsrsParameterDto dto, Robot robot)
        {
            // 直接對應
            // robot.Name = $"Robot #{dto.robotNumber ?? 1}";
            robot.CurrentLocation = dto.robotPosition ?? "Unknown";
            robot.CurrentAction = string.IsNullOrWhiteSpace(dto.robotDoingNow) ? "-" : dto.robotDoingNow;
            robot.NextAction = string.IsNullOrWhiteSpace(dto.robotDoingNext) ? "-" : dto.robotDoingNext;
            robot.IsRobotConnected = dto.isRobotConnected;

            robot.Status = !dto.isRobotConnected ? "離線中" :
                dto.robotStatus switch
                {
                    0 => "離線中", // 灰色
                    1 => "運作中", // 綠色
                    2 => "閒置中", // 黃色
                    3 => "急停中", // 紅色
                    _ => "離線中" // 灰色
                };
            robot.StatusBrush = !dto.isRobotConnected ? new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)) :
                dto.robotStatus switch
                {
                    0 => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)), // 灰色
                    1 => new SolidColorBrush(Color.FromRgb(0x3D, 0xBE, 0x4C)), // 綠色
                    2 => new SolidColorBrush(Color.FromRgb(0xF2, 0xC2, 0x30)), // 黃色
                    3 => new SolidColorBrush(Color.FromRgb(0xC8, 0x3A, 0x33)), // 紅色
                    _ => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)) // 灰色
                };

            // 互斥收斂（依你後端邏輯可調整優先序）
            robot.AsrsState =
                dto.asrsControlStop ? AsrsControlState.Stopped :
                dto.asrsControlPause ? AsrsControlState.Paused :
                dto.asrsControlStart ? AsrsControlState.Started :
                                       AsrsControlState.Unknown;

            robot.DispatchEnabled = dto.dispatchSwitch;
        }
        // === 2. 從 DB_GetAllRobots 更新 ===
        public static void ApplyRobotDto(this RobotDto dto, Robot robot)
        {
            robot.Name =  dto.robotCode ?? "Robot";
            robot.OnDeckObjSerial = dto.onDeckObjSerial == "Null" ? "" : dto.onDeckObjSerial;

            // 這裡 materialName / materialKind 要看你後端後續是否補傳；
            // 若目前 Robot API 沒回這兩個欄位，就先清空或保留原值：
            robot.MaterialName = string.IsNullOrEmpty(robot.MaterialName) ? "-" : robot.MaterialName;
            robot.MaterialKind = string.IsNullOrEmpty(robot.MaterialKind) ? "-" : robot.MaterialKind;
            robot.EquipmentType = dto.robotType;
            // 判斷連線狀態

        }
    }
}
