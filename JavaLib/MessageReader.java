package Message;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;
import java.util.List;
public class MessageReader
{
    ByteBuffer reader;
    public MessageReader(byte[] buffer) {
        reader = ByteBuffer.wrap(buffer).order(ByteOrder.LITTLE_ENDIAN);
    }
    public boolean ReadBool() {
        return ReadInt8() == 1;
    }
    public byte ReadInt8() {
        return reader.get();
    }
    public short ReadInt16() {
        return reader.getShort();
    }
    public int ReadInt32() {
        return reader.getInt();
    }
    public long ReadInt64() {
        return reader.getLong();
    }
    public float ReadFloat() {
        return reader.getFloat();
    }
    public double ReadDouble() {
        return reader.getDouble();
    }
    public String ReadString() {
        return ReadString_impl();
    }
    private String ReadString_impl() {
    	try	{
            List<Byte> sb = new ArrayList<Byte>();
            byte ch;
            while ((ch = reader.get()) != 0)
            	sb.add(ch);
            byte[] bytes = new byte[sb.size()];
            for (int i=0;i<sb.size();++i)
            	bytes[i] = sb.get(i);
            return new String(bytes, "utf-8");
    	} catch (Exception e) {}
    	return "";
    }
    public void Close() {
    }
}