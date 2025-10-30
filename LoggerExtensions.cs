using System.Diagnostics;

namespace ModbusSimulationServer
{
    /// <summary>
    /// Extension methods for convenient logging throughout the application
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Log an informational message
        /// </summary>
        public static void LogInfo(this Logger logger, string message)
        {
            logger.Log(message, Logger.MessageType.Info);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(this Logger logger, string message)
        {
            logger.Log(message, Logger.MessageType.Error);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void LogDebug(this Logger logger, string message)
        {
            logger.Log(message, Logger.MessageType.Debug);
        }

        /// <summary>
        /// Log an exception with full details
        /// </summary>
        public static void LogException(this Logger logger, Exception ex, string? additionalMessage = null)
        {
            var message = string.IsNullOrEmpty(additionalMessage) 
                ? $"Exception: {ex.GetType().Name} - {ex.Message}" 
                : $"{additionalMessage} | Exception: {ex.GetType().Name} - {ex.Message}";

            logger.Log(message, Logger.MessageType.Error);

            // Log stack trace as debug information
            if (ex.StackTrace != null)
            {
                logger.Log($"Stack trace: {ex.StackTrace}", Logger.MessageType.Debug);
            }

            // Log inner exception if present
            if (ex.InnerException != null)
            {
                logger.LogException(ex.InnerException, "Inner exception");
            }
        }
    }
}
