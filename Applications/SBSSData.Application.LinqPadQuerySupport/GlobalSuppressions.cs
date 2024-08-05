﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization",
                           Justification = "This occurs when using .ToList(), use with caution because using a slice does not create a new list.",
                           Scope = "namespaceanddescendants",
                           Target = "~N:SBSSData.Application.LinqPadQuerySupport")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement",
                           Justification = "The longer version is easier to read and maintain; use short version with caution",
                           Scope = "namespaceanddescendants",
                           Target = "~N:SBSSData.Application.LinqPadQuerySupport")]
