
namespace Scorpio.Message
{
    public abstract class IMessage
    {
        protected int __Sign = 0;
        private void AddSign(int index) {
            if ((__Sign & (1 << index)) == 0)
                __Sign |= (1 << index);
        }
        public bool HasSign(int index) {
            return (__Sign & (1 << index)) != 0;
        }
        public byte[] Serialize() {
            MessageWriter writer = new MessageWriter();
            Write(writer);
            return writer.ToArray();
        }
        public abstract void Write(MessageWriter writer);
    }
}