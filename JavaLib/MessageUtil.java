package Scorpio.Message;

public final class MessageUtil {
    public static boolean HasSign(int sign, int index) {
        return (sign & (1 << index)) != 0;
    }
    public static int AddSign(int sign, int index) {
        if ((sign & (1 << index)) == 0) {
            sign |= (1 << index);
        }
        return sign;
    }
    public static byte ToInt8(Object value) {
    	return ((Number)value).byteValue();
    }
    public static short ToInt16(Object value) {
    	return ((Number)value).shortValue();
    }
    public static int ToInt32(Object value) {
    	return ((Number)value).byteValue();
    }
    public static long ToInt64(Object value) {
    	return ((Number)value).byteValue();
    }
    public static float ToFloat(Object value) {
    	return ((Number)value).byteValue();
    }
    public static double ToDouble(Object value) {
    	return ((Number)value).byteValue();
    }
}