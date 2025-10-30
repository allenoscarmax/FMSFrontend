

namespace OSCARMAXFMS_V3.Models
{
    public class MagazinePara
    {
        // Magzine
        public bool ShouldScanEle { get; set; } = false;
        public bool ShouldScanPart { get; set; } = false;
        public bool RobotComing { get; set; } =  false; //手臂準備前往哪一艙
        public bool RobotComing_part { get; set; } = new bool();
        public bool[] Door_to_light { get; set; } = new bool[1] {false}; //亮燈
        public bool[] PartWarehouseSensor { get; set; } = new bool[8] { false, false, false, false, false, false, false, false};//工件倉1

        // ele門的狀態
        public bool[] EleMagzineDoorOpen { get; set; } = new bool[1] { false};
        public bool[] EleMagzineDoorSwithOn { get; set; } = new bool[1] { false};
        // part門的狀態
        public bool[] PartMagzineDoorOpen { get; set; } = new bool[1] { false };

    }
}
