using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace EnhancedMap.Core
{
    public enum SOUNDS_TYPE
    {
        NONE = 0,

        PANIC,
        CHATMSG,
        SIGNAL,
    }

    public static class SoundsManager
    {
        private static SoundPlayer _player = new SoundPlayer();
        private static SOUNDS_TYPE _latest = SOUNDS_TYPE.NONE;

        public static void Play(SOUNDS_TYPE type)
        {
            switch(type)
            {
                case SOUNDS_TYPE.CHATMSG:
                    if (_latest != type)
                        _player.Stream = EnhancedMap.Properties.Resources.stairs;
                    break;
                case SOUNDS_TYPE.PANIC:
                    if (_latest != type)
                        _player.Stream = EnhancedMap.Properties.Resources.unsure;
                    break;
                case SOUNDS_TYPE.SIGNAL:
                    if (_latest != type)
                        _player.Stream = EnhancedMap.Properties.Resources.light;
                    break;
                default:
                    return;
            }

            _latest = type;
            _player.Play();
        }
    }
}
