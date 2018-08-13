using System.Media;
using EnhancedMap.Properties;

namespace EnhancedMap.Core
{
    public enum SOUNDS_TYPE
    {
        NONE = 0,

        PANIC,
        CHATMSG,
        SIGNAL
    }

    public static class SoundsManager
    {
        private static readonly SoundPlayer _player = new SoundPlayer();
        private static SOUNDS_TYPE _latest = SOUNDS_TYPE.NONE;

        public static void Play(SOUNDS_TYPE type)
        {
            switch (type)
            {
                case SOUNDS_TYPE.CHATMSG:
                    if (_latest != type)
                        _player.Stream = Resources.stairs;
                    break;
                case SOUNDS_TYPE.PANIC:
                    if (_latest != type)
                        _player.Stream = Resources.unsure;
                    break;
                case SOUNDS_TYPE.SIGNAL:
                    if (_latest != type)
                        _player.Stream = Resources.light;
                    break;
                default:
                    return;
            }

            _latest = type;
            _player.Play();
        }
    }
}