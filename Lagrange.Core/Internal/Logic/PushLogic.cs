using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Notify;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Extension;
using ProtoHelper = Lagrange.Core.Utility.ProtoHelper;

namespace Lagrange.Core.Internal.Logic;

[EventSubscribe<PushMessageEvent>(Protocols.All)]
internal class PushLogic : ILogic
{

    private BotContext context;
    
    private readonly FrozenDictionary<MsgMatchKey, List<MsgPushProcessorBase>> _processors;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public PushLogic(BotContext ctx)
    {
        context = ctx;
        
        var handlers = new Dictionary<MsgMatchKey, List<MsgPushProcessorBase>>();
        foreach (var type in typeof(MsgPushProcessorBase).Assembly.GetTypes())
        {
            if (!type.HasImplemented<MsgPushProcessorBase>() ||
                Activator.CreateInstance(type) is not MsgPushProcessorBase instance)
                continue;
            
            var attributes = type.GetCustomAttributes<MsgPushProcessorAttribute>();
            
            foreach (var attribute in attributes)
            {
                var msgType = new MsgMatchKey(attribute.MsgType, attribute.SubType, attribute.RequireContent);
                if (!handlers.TryGetValue(msgType, out var set))
                {
                    set = [];
                    handlers[msgType] = set;
                }
                
                set.Add(instance);
            }
        }
        _processors = handlers.ToFrozenDictionary();
    }
    
    public async ValueTask Incoming(ProtocolEvent e)
    {
        if (e is not PushMessageEvent msgEvt) return;
        var msgType = (MsgType)msgEvt.MsgPush.CommonMessage.ContentHead.Type;
        var subType = msgEvt.MsgPush.CommonMessage.ContentHead.SubType;
        var hasContent = msgEvt.MsgPush.CommonMessage.MessageBody?.MsgContent is not null;
        var content = msgEvt.MsgPush.CommonMessage.MessageBody?.MsgContent;

        if (_processors.TryGetValue(new MsgMatchKey(msgType, subType, hasContent), out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (await handler.Handle(context, msgType, subType, msgEvt, content))
                    return;
            }
        } 
        
        if (_processors.TryGetValue(new MsgMatchKey(msgType, -1, hasContent), out handlers))
        {
            foreach (var handler in handlers)
            {
                if (await handler.Handle(context, msgType, subType, msgEvt, content))
                    return;
            }
        }
    }
}

internal enum MsgType
{
    GroupMemberIncreaseNotice = 33,
    GroupMemberDecreaseNotice = 34,
    GroupMessage = 82,
    GroupJoinNotification = 84,
    TempMessage = 141,
    PrivateMessage = 166,
    Event0x20D = 525,
    Event0x210 = 528,  // friend related event
    Event0x2DC = 732,  // group related event
}

internal readonly record struct MsgMatchKey(MsgType MsgType, int SubType = -1, bool RequireContent = false);

internal abstract class MsgPushProcessorBase
{
    internal abstract ValueTask<bool> Handle(BotContext context, MsgType msgType, int subType, PushMessageEvent msgEvt, ReadOnlyMemory<byte>? content);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class MsgPushProcessorAttribute : Attribute
{
    public MsgType MsgType { get; init; }
    public int SubType { get; }
    public bool RequireContent { get; } = false;
    
    public MsgPushProcessorAttribute(MsgType msgType, bool requireContent = false)
        : this(msgType, -1, requireContent) { }
    
    public MsgPushProcessorAttribute(MsgType msgType, int subType, bool requireContent = false)
    {
        MsgType = msgType;
        SubType = subType;
        RequireContent = requireContent;
    }
}