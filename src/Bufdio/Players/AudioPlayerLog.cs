using System;

namespace Bufdio.Players
{
    /// <summary>
    /// Represents simple object containing log information created by audio player.
    /// This class cannot be inherited.
    /// </summary>
    public sealed class AudioPlayerLog
    {
        internal AudioPlayerLog(DateTime date, string message, AudioPlayerLogType type)
        {
            Date = date;
            Message = message;
            Type = type;
        }

        internal AudioPlayerLog(string message, AudioPlayerLogType type) : this(DateTime.Now, message, type)
        {
        }

        /// <summary>
        /// Gets the date when the log is created.
        /// </summary>
        public DateTime Date { get; }
        
        /// <summary>
        /// Gets the log message.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets the log type.
        /// </summary>
        public AudioPlayerLogType Type { get; }

        /// <summary>
        /// Returns a <c>string</c> containing information in this log instance.
        /// </summary>
        /// <returns>A <c>string</c> containing information in this log instance.</returns>
        public override string ToString()
        {
            return $"[{Type}] [{Date:MM/dd/yyyy HH:mm:ss}] - {Message}";
        }

        internal static AudioPlayerLog Info(string message)
        {
            return new AudioPlayerLog(message, AudioPlayerLogType.Info);
        }
        
        internal static AudioPlayerLog Warning(string message)
        {
            return new AudioPlayerLog(message, AudioPlayerLogType.Warning);
        }
        
        internal static AudioPlayerLog Error(string message)
        {
            return new AudioPlayerLog(message, AudioPlayerLogType.Error);
        }
    }
}
