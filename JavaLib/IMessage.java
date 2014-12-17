package Scorpio.Message;

public abstract class IMessage {
    protected int __Sign = 0;
    protected final void AddSign(int index) {
        __Sign = MessageUtil.AddSign(__Sign, index);
    }
    public final boolean HasSign(int index) {
        return MessageUtil.HasSign(__Sign, index);
    }
    public final byte[] Serialize() {
        MessageWriter writer = new MessageWriter();
        Write(writer);
        return writer.ToArray();
    }
    public abstract void Write(MessageWriter writer);
}