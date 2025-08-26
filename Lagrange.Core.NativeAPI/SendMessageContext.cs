using System.Text;
using Lagrange.Core.Message;

namespace Lagrange.Core.NativeAPI
{
    public class SendMessageContext
    {
        public SendMessageContext(BotContext context)
        {
            _context = context;
        }

        private BotContext _context;
        public Dictionary<int, MessageBuilder> MessageBuilders { get; } = new();

        public int CreateMessageBuilder()
        {
            var random = new Random();
            int randomNumber = random.Next(1, int.MaxValue);
            while (MessageBuilders.ContainsKey(randomNumber))
            {
                randomNumber = random.Next(1, int.MaxValue);
            }

            MessageBuilders.Add(randomNumber, new MessageBuilder());
            return randomNumber;
        }

        public void RemoveMessageBuilder(int id)
        {
            MessageBuilders.Remove(id);
        }

        public MessageChain? Build(int id)
        {
            if (!MessageBuilders.TryGetValue(id, out var builder))
            {
                return null;
            }

            var message = builder.Build();
            MessageBuilders.Remove(id);
            return message;
        }

        public void AddText(int id, byte[] text)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                builder.Text(Encoding.UTF8.GetString(text));
            }
        }

        public void AddImage(int id, byte[] image, byte[]? summary = null, 
            int subType = 0)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                if (summary != null)
                {
                    builder.Image(image, Encoding.UTF8.GetString(summary), subType);
                }
                else
                {
                    builder.Image(image: image, subType: subType);
                }
            }
        }

        public void AddLocalImage(int id, byte[] path, byte[]? summary = null, 
            int subType = 0)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                if (summary != null)
                {
                    builder.Image(
                        Encoding.UTF8.GetString(path),
                        Encoding.UTF8.GetString(summary),
                        subType
                    );
                }
                else
                {
                    builder.Image(path: Encoding.UTF8.GetString(path), subType: subType);
                }
            }
        }

        public void AddMention(int id, long uin, byte[]? display = null)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                builder.Mention(uin, display is null ? null : Encoding.UTF8.GetString(display));
            }
        }

        public void AddReply(int id /*, BotMessage messages */)
        {
            throw new NotImplementedException();
        }

        public void AddMultiMsg(int id /*, List<BotMessage> messages */)
        {
            throw new NotImplementedException();
        }

        public void AddMultiMsg(int id, byte[] resId)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                builder.MultiMsg(Encoding.UTF8.GetString(resId));
            }
        }

        public void AddRecord(int id, byte[] record)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                builder.Record(record);
            }
        }

        public void AddLocalRecord(int id, byte[] path)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                builder.Record(Encoding.UTF8.GetString(path));
            }
        }

        public void AddVideo(int id, byte[] video, byte[]? thumbnail)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                builder.Video(video, thumbnail);
            }
        }

        public void AddLocalVideo(int id, byte[] path, byte[]? thumbPath)
        {
            if (MessageBuilders.TryGetValue(id, out var builder))
            {
                string? thumb = thumbPath == null ? null : Encoding.UTF8.GetString(thumbPath);
                builder.Video(Encoding.UTF8.GetString(path), thumb);
            }
        }
    }
}
