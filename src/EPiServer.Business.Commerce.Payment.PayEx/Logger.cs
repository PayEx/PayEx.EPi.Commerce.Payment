using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using log4net;

namespace EPiServer.Business.Commerce.Payment.PayEx
{
    internal class Logger : ILogger
    {
        public const string DefaultApplicationLogger = "EPiServer.Business.Commerce.Payment.PayEx";

        public enum LogType
        {
            General,
            Notify
        }

        public void LogInfo(string message)
        {
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
            if (logger.IsInfoEnabled)
            {
                logger.Info(message);
            }
        }

        public void LogDebug(string message)
        {
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
            if (logger.IsDebugEnabled)
            {
                logger.Debug(message);
            }
        }

        public void LogWarning(string message)
        {
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
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
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
            if (logger.IsErrorEnabled)
            {
                logger.Error(message);
            }
        }

        public void LogFatal(string message)
        {
            ILog logger = LogManager.GetLogger(DefaultApplicationLogger);
            if (logger.IsFatalEnabled)
            {
                logger.Fatal(message);
            }
        }
    }
}
