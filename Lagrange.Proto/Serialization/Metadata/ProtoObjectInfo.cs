﻿using System.Diagnostics;

namespace Lagrange.Proto.Serialization.Metadata;

[DebuggerDisplay("Fields = {Fields.Count}")]
public class ProtoObjectInfo<T>
{
    public Dictionary<int, ProtoFieldInfo> Fields { get; init; } = new();
    
    public Func<T>? ObjectCreator { get; init; }
    
    public bool IgnoreDefaultFields { get; init; }
}