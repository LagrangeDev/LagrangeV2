﻿using System.Diagnostics;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Serialization.Metadata;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class ProtoFieldInfo(int field, WireType wireType, Type declared, Type property)
{
    public int Field { get; } = field;
    
    public WireType WireType { get; } = wireType;
    
    public Type DeclaredType { get; } = declared;
    
    public Type PropertyType { get; } = property;
    
    public ProtoNumberHandling NumberHandling { get; init; } = ProtoNumberHandling.Default;
    // because it is init field, JIT would remove the branch condition if it is used in some branch
    
    internal ProtoConverter EffectiveConverter
    {
        get
        {
            Debug.Assert(_effectiveConverter != null);
            return _effectiveConverter;
        }
    }
    
    public Func<object, object?>? Get { get => _untypedGet; set => SetGetter(value); }
    public Action<object, object?>? Set { get => _untypedSet; set => SetSetter(value); }    
    
    private protected Func<object, object?>? _untypedGet;
    private protected Action<object, object?>? _untypedSet;
    
    private protected ProtoConverter? _effectiveConverter;
    private protected abstract void SetGetter(Delegate? getter);
    private protected abstract void SetSetter(Delegate? setter);

    public abstract void Read(ref ProtoReader reader, object target);
    
    public abstract void Write(ProtoWriter writer, object target);
    
    public abstract int Measure(object target);
        
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"Field = {Field}, WireType = {WireType}, PropertyType = {PropertyType}, DeclaredType = {DeclaredType}";
    
}

public class ProtoFieldInfo<T> : ProtoFieldInfo
{
    public ProtoFieldInfo(int field, WireType wireType, Type declared) : base(field, wireType, declared, typeof(T))
    {
        var converter = ProtoTypeResolver.GetConverter<T>();
        _effectiveConverter = converter;
        _typedEffectiveConverter = converter;
    }
    
    private Func<object, T>? _typedGet;
    private Action<object, T>? _typedSet;

    public new Func<object, T>? Get
    {
        get => _typedGet;
        set => SetGetter(value);
    }

    public new Action<object, T>? Set
    {
        get => _typedSet;
        set => SetSetter(value);
    }
    
    internal new ProtoConverter<T> EffectiveConverter
    {
        get
        {
            Debug.Assert(_typedEffectiveConverter != null);
            return _typedEffectiveConverter;
        }
    }

    private ProtoConverter<T>? _typedEffectiveConverter;

    private protected override void SetGetter(Delegate? getter)
    {
        Debug.Assert(getter is null or Func<object, object?> or Func<object, T>);

        switch (getter)
        {
            case null:
                _typedGet = null;
                _untypedGet = null;
                break;
            case Func<object, T> typedGetter:
                _typedGet = typedGetter;
                _untypedGet = getter as Func<object, object?> ?? (obj => typedGetter(obj));
                break;
            default:
                var untypedGet = (Func<object, object?>)getter;
                _typedGet = obj => (T)untypedGet(obj)!;
                _untypedGet = untypedGet;
                break;
        }
    }

    private protected override void SetSetter(Delegate? setter)
    {
        Debug.Assert(setter is null or Action<object, object?> or Action<object, T>);

        switch (setter)
        {
            case null:
                _typedSet = null;
                _untypedSet = null;
                break;
            case Action<object, T> typedSetter:
                _typedSet = typedSetter;
                _untypedSet = setter as Action<object, object?> ?? ((obj, value) => typedSetter(obj, (T)value!));
                break;
            default:
                var untypedSet = (Action<object, object?>)setter;
                _typedSet = (obj, value) => untypedSet(obj, value);
                _untypedSet = untypedSet;
                break;
        }
    }
    
    public override void Read(ref ProtoReader reader, object target)
    {
        Debug.Assert(_typedEffectiveConverter != null);
        
        T value = NumberHandling == ProtoNumberHandling.Default
            ? _typedEffectiveConverter.Read(Field, WireType, ref reader)
            : _typedEffectiveConverter.ReadWithNumberHandling(Field, WireType, ref reader, NumberHandling);
        _typedSet?.Invoke(target, value);
    }

    public override void Write(ProtoWriter writer, object target)
    {
        Debug.Assert(_typedEffectiveConverter != null && _typedGet != null);

        T value = _typedGet.Invoke(target);
        if (NumberHandling == ProtoNumberHandling.Default) _typedEffectiveConverter.Write(Field, WireType, writer, value);
        else _typedEffectiveConverter.WriteWithNumberHandling(Field, WireType, writer, value, NumberHandling);
    }
    
    public override int Measure(object target)
    {
        Debug.Assert(_typedEffectiveConverter != null && _typedGet != null);

        T value = _typedGet.Invoke(target);
        return _typedEffectiveConverter.Measure(Field, WireType, value);
    }
}

public class ProtoMapFieldInfo<TMap, TKey, TValue>(int field, WireType keyWireType, WireType valueWireType, Type property) 
    : ProtoFieldInfo(field, WireType.LengthDelimited, typeof(TMap), property)
    where TMap : IDictionary<TKey, TValue>, new()
    where TKey : notnull
{
    public WireType KeyWireType { get; } = keyWireType;

    public WireType ValueWireType { get; } = valueWireType;
    
    public ProtoNumberHandling ValueNumberHandling { get; init; } = ProtoNumberHandling.Default;

    private Func<object, TMap>? _typedGet;
    private Action<object, TMap>? _typedSet;

    public new Func<object, TMap>? Get
    {
        get => _typedGet;
        set => SetGetter(value);
    }

    public new Action<object, TMap>? Set
    {
        get => _typedSet;
        set => SetSetter(value);
    }
    
    private protected override void SetGetter(Delegate? getter)
    {
        Debug.Assert(getter is null or Func<object, object?> or Func<object, TMap>);

        switch (getter)
        {
            case null:
                _typedGet = null;
                _untypedGet = null;
                break;
            case Func<object, TMap> typedGetter:
                _typedGet = typedGetter;
                _untypedGet = getter as Func<object, object?> ?? (obj => typedGetter(obj));
                break;
            default:
                var untypedGet = (Func<object, object?>)getter;
                _typedGet = obj => (TMap)untypedGet(obj)!;
                _untypedGet = untypedGet;
                break;
        }
    }

    private protected override void SetSetter(Delegate? setter)
    {
        Debug.Assert(setter is null or Action<object, object?> or Action<object, TMap>);

        switch (setter)
        {
            case null:
                _typedSet = null;
                _untypedSet = null;
                break;
            case Action<object, TMap> typedSetter:
                _typedSet = typedSetter;
                _untypedSet = setter as Action<object, object?> ?? ((obj, value) => typedSetter(obj, (TMap)value!));
                break;
            default:
                var untypedSet = (Action<object, object?>)setter;
                _typedSet = (obj, value) => untypedSet(obj, value);
                _untypedSet = untypedSet;
                break;
        }
    }
    
    private ProtoConverter<TKey>? _typedKeyConverter = ProtoTypeResolver.GetConverter<TKey>();
    private ProtoConverter<TValue>? _typedValueConverter = ProtoTypeResolver.GetConverter<TValue>();
    
    internal new ProtoConverter EffectiveConverter => throw new InvalidOperationException("Unreachable");

    internal ProtoConverter<TKey> KeyEffectiveConverter
    {
        get
        {
            Debug.Assert(_typedKeyConverter != null);
            return _typedKeyConverter;
        }
    }
    
    internal ProtoConverter<TValue> ValueEffectiveConverter
    {
        get
        {
            Debug.Assert(_typedValueConverter != null);
            return _typedValueConverter;
        }
    }
    
    public override void Read(ref ProtoReader reader, object target)
    {
        Debug.Assert(_typedSet != null && _typedKeyConverter != null && _typedValueConverter != null);

        var map = new TMap();
        
        while (true)
        {
            TKey? key = default;
            TValue? value = default;
            
            if (NumberHandling == ProtoNumberHandling.Default)
            {
                while (key is null || value is null)
                {
                    switch (reader.DecodeVarInt<byte>() >> 3)
                    {
                        case 1:
                            key = _typedKeyConverter.Read(Field, KeyWireType, ref reader);
                            break;
                        case 2:
                            value = _typedValueConverter.Read(Field, ValueWireType, ref reader);
                            break;
                        default:
                            ThrowHelper.ThrowInvalidDataException_MalformedMessage();
                            break;
                    }
                }
            }
            else
            {
                while (key is null || value is null)
                {
                    switch (reader.DecodeVarInt<byte>() >> 3)
                    {
                        case 1:
                            key = _typedKeyConverter.ReadWithNumberHandling(Field, KeyWireType, ref reader, NumberHandling);
                            break;
                        case 2:
                            value = _typedValueConverter.ReadWithNumberHandling(Field, ValueWireType, ref reader, ValueNumberHandling);
                            break;
                        default:
                            ThrowHelper.ThrowInvalidDataException_MalformedMessage();
                            break;
                    }
                }
            }
            
            map.Add(key, value);
            if (reader.DecodeVarInt<int>() >> 3 != Field) break;
        }

        reader.Rewind(-ProtoHelper.GetVarIntLength((Field << 3) | (byte)WireType));
        
        _typedSet(target, map);
    }

    public override void Write(ProtoWriter writer, object target)
    {
        Debug.Assert(_typedGet != null && _typedKeyConverter != null && _typedValueConverter != null);
        
        var map = _typedGet(target);
        int tag = (Field << 3) | (byte)WireType;
        bool first = true;
        
        foreach (var (key, value) in map)
        {
            if (first) first = false;
            else writer.EncodeVarInt(tag);

            writer.EncodeVarInt(_typedKeyConverter.Measure(1, KeyWireType, key) + _typedValueConverter.Measure(2, ValueWireType, value) + 2); // 2 for the tag of key and value
            
            writer.EncodeVarInt(8 | (byte)WireType);
            if (NumberHandling == ProtoNumberHandling.Default) _typedKeyConverter.Write(1, KeyWireType, writer, key);
            else _typedKeyConverter.WriteWithNumberHandling(1, KeyWireType, writer, key, NumberHandling);
            
            writer.EncodeVarInt(16 | (byte)WireType);
            if (ValueNumberHandling == ProtoNumberHandling.Default) _typedValueConverter.Write(2, ValueWireType, writer, value);
            else _typedValueConverter.WriteWithNumberHandling(2, ValueWireType, writer, value, ValueNumberHandling);
        }
    }

    public override int Measure(object target)
    {
        Debug.Assert(_typedGet != null && _typedKeyConverter != null && _typedValueConverter != null);
        
        var map = _typedGet(target);
        int tag = (Field << 3) | (byte)WireType;
        
        int size = ProtoHelper.GetVarIntLength(tag) * (map.Count - 1); // the length of the first item is not counted as it would be added by the caller
        foreach (var (key, value) in map)
        {
            size += _typedKeyConverter.Measure(1, KeyWireType, key) + _typedValueConverter.Measure(2, ValueWireType, value);
            size += 2; // 2 for the tag of key and value
        }
        
        return size;
    }
}