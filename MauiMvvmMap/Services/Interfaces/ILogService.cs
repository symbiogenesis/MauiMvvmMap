namespace MauiMvvmMap.Services.Interfaces;

public interface ILogService
{
    void LogException(Exception exception);

    void LogMessage(string message);
}
