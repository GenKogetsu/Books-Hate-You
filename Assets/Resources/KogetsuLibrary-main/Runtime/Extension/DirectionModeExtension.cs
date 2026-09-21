using System;
using Kogetsu.Library.Core;

namespace Kogetsu.Library.Extension
{
    public static class DirectionModeExtension
    {
        public static byte ToByte(this DirectionMode mode) => mode switch
        {
            DirectionMode.TwoDiraction  => 2,
            DirectionMode.FourDiraction => 4,
            DirectionMode.EightDiraction => 8,
            _ => 0
        };
    }
}