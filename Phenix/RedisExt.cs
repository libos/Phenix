﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phenix
{
    class RedisExt
    {
        static public string GetString(byte[] stringBytes)
        {
            return Encoding.UTF8.GetString(stringBytes);
        }
    }
}