using System;
using System.Collections.Generic;

namespace EnhancedMapServerNetCore
{
    public static class GuidGenerator
    {
        private static readonly HashSet<Guid> _guids = new HashSet<Guid>();

        public static Guid GenerateNew()
        {
            Guid g;
            do
            {
                g = Guid.NewGuid();
            } while (_guids.Contains(g));

            _guids.Add(g);
            return g;
        }
    }
}