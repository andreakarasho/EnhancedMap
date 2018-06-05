using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core
{
    public struct ChatEntry
    {
        public DateTime Time;
        public string Name;
        public string Message;

        public static readonly ChatEntry Invalid = new ChatEntry() { Time = DateTime.MinValue, Name = "", Message = "" };
    }

    public static class ChatManager
    {
        public static event EventHandler<ChatEntry> MessageWrited;

        /// <summary>
        /// Messages list for each users
        /// </summary>
        private static readonly Dictionary<string, List<ChatEntry>> _chatCache = new Dictionary<string, List<ChatEntry>>();

        public static void Add(string name, string msg)
        {
            if (!_chatCache.ContainsKey(name))
                _chatCache[name] = new List<ChatEntry>();

            var entry = new ChatEntry() { Time = DateTime.Now, Name = name, Message = msg };
            _chatCache[name].Add(entry);

            MessageWrited.Raise(entry);
        }

        public static void Clear() => _chatCache.Clear();

        public static ChatEntry GetMessageAt(string username, int index)
        {
            if (_chatCache.ContainsKey(username))
            {
                List<ChatEntry> entryList = _chatCache[username];
                if (index >= 0 && index < entryList.Count)
                    return entryList[index];
            }
            return ChatEntry.Invalid;
        }

        public static ChatEntry[] GetMessages(string username)
        {
            if (_chatCache.ContainsKey(username))
                return _chatCache[username].ToArray();
            return null;
        }

        public static Dictionary<string, List<ChatEntry>> GetAllMessages()
        {       
            return _chatCache;
        }
    }
}
