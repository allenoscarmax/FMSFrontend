using FMSFrontend.Features.Dtos;
using FMSFrontend.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows.Media;

namespace FMSFrontend.Features.Mappings
{
    public static class PlcMapping
    {
        private static readonly SolidColorBrush LightOn = new(Color.FromRgb(0x61, 0xB4, 0x55));
        private static readonly SolidColorBrush LightOff = new(Color.FromRgb(0x00, 0x00, 0x00));
        private static readonly SolidColorBrush EleTilieColor  = new(Color.FromRgb(0x27, 0x79, 0xA7));
        private static readonly SolidColorBrush PortTilieColor = new(Color.FromRgb(0xE0, 0x8E, 0x45));

        public static void ApplyMagazineParaDto(this MagazineParaDto dto, MagazinePara magazinePara)
        {
            // 依 magazineParasNumber 初始化集合
            if (magazinePara.MagazineParas.Count != magazinePara.magazineParasNumber)
            {
                magazinePara.MagazineParas.Clear();
                for (int i = 0; i < magazinePara.magazineParasNumber; i++)
                {
                    magazinePara.MagazineParas.Add(new MagazineParaInfo
                    {
                        StorageId = i.ToString(), // 0-based 給 PLC API
                    });
                }
                // 設定標題與顏色 
                magazinePara.MagazineParas[0].LeftTitle = "W1";
                magazinePara.MagazineParas[0].RightTitle = "E1";
                magazinePara.MagazineParas[1].LeftTitle = "E2";
                magazinePara.MagazineParas[1].RightTitle = "";
                magazinePara.MagazineParas[0].LeftTitleBrush = PortTilieColor;
                magazinePara.MagazineParas[0].RightTitleBrush = EleTilieColor;
                magazinePara.MagazineParas[1].LeftTitleBrush = EleTilieColor;
                magazinePara.MagazineParas[1].RightTitleBrush = EleTilieColor;
            }

            magazinePara.IsDoorLightOn = dto.door_to_light.FirstOrDefault();

            // 目前 DTO 只提供單一門狀態 -> 套用到第 0 個
            if (magazinePara.MagazineParas.Count > 0)
            {
                var info = magazinePara.MagazineParas[0];
                info.UpperScanStatus = dto.shouldScanEle ? LightOn : LightOff;
                info.UpperDoorStatus = dto.eleMagzineDoorOpen.FirstOrDefault() ? LightOn : LightOff;
                info.LowerScanStatus = dto.shouldScanPart ? LightOn : LightOff;
                info.LowerDoorStatus = dto.partMagzineDoorOpen.FirstOrDefault() ? LightOn : LightOff;
            }
        }
    }
}
