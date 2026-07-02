namespace ProtonDesktop.Core.Enums;

[Flags]
public enum EmailFlag
{
    None = 0,
    Seen = 1,
    Flagged = 2,
    Answered = 4,
    Forwarded = 8,
    Draft = 16,
    Deleted = 32
}
