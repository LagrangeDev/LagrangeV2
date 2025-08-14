namespace Lagrange.Core.Common.Entity;

public class BotStranger(long uin, string nickname, string uid, string personalSign, string remark, ulong level, BotGender gender, DateTime registrationTime, DateTime? birthday, ulong age, string qID) : BotContact
{
    public override long Uin { get; } = uin;

    public override string Nickname { get; } = nickname ?? string.Empty;

    public override string Uid { get; } = uid ?? string.Empty;

    public string PersonalSign { get; } = personalSign;

    public string Remark { get; } = remark;

    public ulong Level { get; } = level;

    public BotGender Gender { get; } = gender;

    public DateTime RegistrationTime { get; } = registrationTime;

    public DateTime? Birthday { get; } = birthday;

    public ulong Age { get; } = age;

    public string QID { get; } = qID;

    public long Source { get; init; }

    internal BotStranger CloneWithSource(long source) => new(
        Uin,
        Nickname,
        Uid,
        PersonalSign,
        Remark,
        Level,
        Gender,
        RegistrationTime,
        Birthday,
        Age,
        QID
    )
    { Source = source };
}