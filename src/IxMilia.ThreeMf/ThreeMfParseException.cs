// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace IxMilia.ThreeMf
{

    public class ThreeMfParseException : Exception
    {
        public ThreeMfParseException(string message)
            : base(message)
        {
        }

        public ThreeMfParseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
