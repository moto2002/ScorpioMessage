using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scorpio.Message
{
    public static class MessageUtil
    {
        public static bool HasSign(int sign, int index)
        {
            return (sign & (1 << index)) != 0;
        }
        public static int AddSign(int sign, int index)
        {
            if ((sign & (1 << index)) == 0)
                sign |= (1 << index);
            return sign;
        }
        public static sbyte ToInt8(object value)
        {
            return Convert.ToSByte(value);
        }
        public static short ToInt16(object value)
        {
            return Convert.ToInt16(value);
        }
        public static int ToInt32(object value)
        {
            return Convert.ToInt32(value);
        }
        public static long ToInt64(object value)
        {
            return Convert.ToInt64(value);
        }
        public static float ToFloat(object value)
        {
            return Convert.ToSingle(value);
        }
        public static double ToDouble(object value)
        {
            return Convert.ToDouble(value);
        }
    }
}
