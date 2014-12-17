
namespace Scorpio.Message
{
    public abstract class IMessage
    {
        protected int __Sign = 0;
        protected void AddSign(int index) {
            __Sign = MessageUtil.AddSign(__Sign, index);
        }
        public bool HasSign(int index) {
            return MessageUtil.HasSign(__Sign, index);
        }
        public byte[] Serialize() {
            MessageWriter writer = new MessageWriter();
            Write(writer);
            return writer.ToArray();
        }
        public abstract void Write(MessageWriter writer);
    }
}