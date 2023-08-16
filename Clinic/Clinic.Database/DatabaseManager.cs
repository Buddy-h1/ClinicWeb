using Clinic.Common.Common;
using Clinic.Logging;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic.Database
{
    public class DatabaseManager
    {
        public static string ConnectionString = "";

        [NotMapped]
        public Exception? lastException { get; set; }

        [NotMapped]
        public Exception? LastException
        {
            get
            {
                return lastException;
            }
            protected set
            {
                lastException = value;
                if (lastException != null)
                    Logger.SaveMessage($"DatabaseManager. {ExceptionHelper.GetFullText(lastException, true)}", enumEventEntryType.Error);
            }
        }
    }
}