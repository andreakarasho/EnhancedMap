using System;

namespace EnhancedMapServerNetCore.Internals
{
    [Flags]
    public enum ACCOUNT_LEVEL
    {
        NORMAL,
        ROOM_ADMIN,
        SERVER_ADMIN
    }
}