using Epinova.PayExProvider.Contracts;
using log4net;
using System;

namespace Epinova.PayExProvider
{
    public class Logger : ILogger
    {
        public const string DefaultApplicationLogger = "Epinova.PayExProvider";

        public enum LogType
        {
            General,
            Notify
        }

        public void LogInfo(string message)
        {
            LogInfo(message, LogType.General);
        }

        public static void LogInfo(string message, LogType type)
        {
            ILog logger = LogManager.GetLogger(type == LogType.Notify ? LogType.Notify.ToString() : DefaultApplicationLogger);
            if (logger.IsInfoEnabled)
            {
                logger.Info(message);
            }
        }

        public void LogDebug(string message)
        {
            LogDebug(message, LogType.General);
        }

        public static void LogDebug(string message, LogType type)
        {
            ILog logger = LogManager.GetLogger(type == LogType.Notify ? LogType.Notify.ToString() : DefaultApplicationLogger);
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message);
            }
        }

        public void LogWarning(string message)
        {
            LogWarning(message, LogType.General);
        }

        public void LogWarning(string message, LogType type)
        {
            ILog logger = LogManager.GetLogger(type == LogType.Notify ? LogType.Notify.ToString() : DefaultApplicationLogger);
            if (logger.IsWarnEnabled)
            {
                logger.Warn(message);
            }
        }

        public void LogWarning(string message, Exception error)
        {
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
            if (error.InnerException != null)
            {
                error = error.InnerException;
            }
            if (logger.IsWarnEnabled)
            {
                logger.Warn(message, error);
            }
        }

        public void LogError(string message, Exception error)
        {
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
            if (error.InnerException != null)
            {
                error = error.InnerException;
            }
            if (logger.IsErrorEnabled)
            {
                logger.Error(message, error);
            }
        }

        public void LogError(string message)
        {
            LogError(message, LogType.General);
        }

        public void LogError(string message, LogType type)
        {
            ILog logger = LogManager.GetLogger(type == LogType.Notify ? LogType.Notify.ToString() : DefaultApplicationLogger);
            if (logger.IsWarnEnabled)
            {
                logger.Warn(message);
            }
        }
    }
}
