namespace мне_бы_жить_в_шоколаде.Entities;

public static class AppRoles
{
    public const string Requester = "requester";
    public const string LegacyRequester = "requster";
    public const string Technician = "technician";
    public const string Admin = "admin";

    public static bool IsRequester(string? role) => role == Requester || role == LegacyRequester;
    public static bool IsTechnician(string? role) => role == Technician;
    public static bool IsAdmin(string? role) => role == Admin;

    public static string ToDisplayName(string? role)
    {
        if (IsRequester(role))
        {
            return "Пользователь";
        }

        if (IsTechnician(role))
        {
            return "Техник";
        }

        return IsAdmin(role) ? "Администратор" : "Неизвестная роль";
    }
}
