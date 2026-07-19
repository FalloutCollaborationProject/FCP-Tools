using System.Collections.Generic;
using UnityEngine;

namespace FCP.Core.Robotics
{
    public readonly struct RobotPaintOption
    {
        public readonly string label;
        public readonly Color color;

        public RobotPaintOption(string label, Color color)
        {
            this.label = label;
            this.color = color;
        }
    }

    public static class RobotPaintOptions
    {
        public static readonly List<RobotPaintOption> General = new List<RobotPaintOption>
        {
            new RobotPaintOption("FCP_RobotPaint_FactoryGrey", ColorFrom(90, 90, 90)),
            new RobotPaintOption("FCP_RobotPaint_GunmetalBlack", ColorFrom(35, 35, 38)),
            new RobotPaintOption("FCP_RobotPaint_WastelandRust", ColorFrom(120, 65, 40)),
            new RobotPaintOption("FCP_RobotPaint_MilitaryOlive", ColorFrom(75, 83, 51)),
            new RobotPaintOption("FCP_RobotPaint_DesertTan", ColorFrom(176, 155, 120)),
            new RobotPaintOption("FCP_RobotPaint_PreWarWhite", ColorFrom(225, 222, 210)),
            new RobotPaintOption("FCP_RobotPaint_WarningYellow", ColorFrom(196, 160, 40)),
            new RobotPaintOption("FCP_RobotPaint_SecuritronBlue", ColorFrom(40, 120, 180)),
        };

        public static readonly List<RobotPaintOption> Factional = new List<RobotPaintOption>
        {
            new RobotPaintOption("FCP_RobotPaint_NCRBronze", ColorFrom(150, 130, 90)),
            new RobotPaintOption("FCP_RobotPaint_LegionCrimson", ColorFrom(120, 25, 25)),
            new RobotPaintOption("FCP_RobotPaint_BrotherhoodSteel", ColorFrom(95, 105, 115)),
            new RobotPaintOption("FCP_RobotPaint_BrotherhoodOutcast", ColorFrom(45, 48, 52)),
            new RobotPaintOption("FCP_RobotPaint_EnclaveBlack", ColorFrom(20, 20, 22)),
            new RobotPaintOption("FCP_RobotPaint_VaultTecBlue", ColorFrom(60, 90, 150)),
            new RobotPaintOption("FCP_RobotPaint_GreatKhanLeather", ColorFrom(100, 70, 45)),
            new RobotPaintOption("FCP_RobotPaint_ChildrenOfAtomGreen", ColorFrom(95, 140, 70)),
            new RobotPaintOption("FCP_RobotPaint_RaiderRust", ColorFrom(110, 55, 30)),
        };

        private static Color ColorFrom(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }
}
