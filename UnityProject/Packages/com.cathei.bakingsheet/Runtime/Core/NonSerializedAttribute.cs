﻿// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;

namespace Cathei.BakingSheet
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class NonSerializedAttribute : Attribute
    {

    }
}
