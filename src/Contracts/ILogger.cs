using System;

namespace Epinova.PayExProvider.Contracts
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogDebug(string message);
        void LogWarning(string message);
        void LogWarning(string message, Exception exception);
        void LogError(string message, Exception exception);
        void LogError(string message);
    }
}
