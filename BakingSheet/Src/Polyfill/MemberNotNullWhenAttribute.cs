// <auto-generated>
//   This code file has automatically been added by the "Nullable" NuGet package (https://www.nuget.org/packages/Nullable).
//   Please see https://github.com/manuelroemer/Nullable for more information.
//
//   IMPORTANT:
//   DO NOT DELETE THIS FILE if you are using a "packages.config" file to manage your NuGet references.
//   Consider migrating to PackageReferences instead:
//   https://docs.microsoft.com/en-us/nuget/consume-packages/migrate-packages-config-to-package-reference
//   Migrating brings the following benefits:
//   * The "Nullable" folder and the nullable "*Attribute.cs" files don't appear in your project.
//   * The added files are immutable and can therefore not be modified by coincidence.
//   * Updating/Uninstalling the package will work flawlessly.
// </auto-generated>

#region License
// MIT License
//
// Copyright (c) Manuel Römer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

#if !NULLABLE_ATTRIBUTES_DISABLE
#nullable enable
#pragma warning disable

namespace System.Diagnostics.CodeAnalysis
{
    using global::System;

#if DEBUG
    /// <summary>
    ///     Specifies that the method or property will ensure that the listed field and property members have
    ///     non-<see langword="null"/> values when returning with the specified return value condition.
    /// </summary>
#endif
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
#if !NULLABLE_ATTRIBUTES_INCLUDE_IN_CODE_COVERAGE
    [ExcludeFromCodeCoverage, DebuggerNonUserCode]
#endif
    internal sealed class MemberNotNullWhenAttribute : Attribute
    {
#if DEBUG
        /// <summary>
        ///     Gets the return value condition.
        /// </summary>
#endif
        public bool ReturnValue { get; }

#if DEBUG
        /// <summary>
        ///     Gets field or property member names.
        /// </summary>
#endif
        public string[] Members { get; }

#if DEBUG
        /// <summary>
        ///     Initializes the attribute with the specified return value condition and a field or property member.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition. If the method returns this value,
        ///     the associated parameter will not be <see langword="null"/>.
        /// </param>
        /// <param name="member">
        ///     The field or property member that is promised to be not-<see langword="null"/>.
        /// </param>
#endif
        public MemberNotNullWhenAttribute(bool returnValue, string member)
        {
            ReturnValue = returnValue;
            Members = new[] { member };
        }

#if DEBUG
        /// <summary>
        ///     Initializes the attribute with the specified return value condition and list
        ///     of field and property members.
        /// </summary>
        /// <param name="returnValue">
        ///     The return value condition. If the method returns this value,
        ///     the associated parameter will not be <see langword="null"/>.
        /// </param>
        /// <param name="members">
        ///     The list of field and property members that are promised to be not-null.
        /// </param>
#endif
        public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
            ReturnValue = returnValue;
            Members = members;
        }
    }
}

#pragma warning restore
#nullable restore
#endif // NULLABLE_ATTRIBUTES_DISABLE