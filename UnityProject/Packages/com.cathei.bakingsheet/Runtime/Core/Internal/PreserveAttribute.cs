// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;

namespace Cathei.BakingSheet.Internal
{
    // Prevent Unity's code stripper
    [AttributeUsage(AttributeTargets.All)]
    public class PreserveAttribute : Attribute
    {
    }
}
