// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Text;

namespace IxMilia.ThreeMf.Test
{
    public abstract class ThreeMfAbstractTestBase
    {
        public static byte[] StringToBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
