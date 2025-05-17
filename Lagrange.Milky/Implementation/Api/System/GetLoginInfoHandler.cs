using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Milky.Implementation.Common.Api.Results;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_login_info")]
public class GetLoginInfoHandler(BotContext bot) : IEmptyParamApiHandler
{
    public ValueTask<IApiResult> HandleAsync(CancellationToken token)
    {
        if (bot.BotInfo == null) return ValueTask.FromResult<IApiResult>(new ApiFailedResult
        {
            Retcode = -1,
            Message = "bot info is null",
        });

        return ValueTask.FromResult<IApiResult>(new ApiOkResult<GetLoginInfoResult>
        {
            Data = new GetLoginInfoResult
            {
                Uin = bot.BotUin,
                Nickname = bot.BotInfo.Name,
            },
        });
    }
}

public class GetLoginInfoResult
{
    public required long Uin { get; init; }

    public required string Nickname { get; init; }
}