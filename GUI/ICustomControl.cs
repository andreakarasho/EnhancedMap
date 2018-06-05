using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.GUI
{
    public enum MouseState
    {
        HOVER,
        DOWN,
        OUT
    }

    public interface ICustomControl
    {
        MouseState MouseState { get; set; }
    }
}
