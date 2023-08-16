namespace Clinic.Logging
{
    public static class Logger
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Mutex mutex = new Mutex();

        public static void Init(string _LogFileName)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget();
            logfile.FileName = _LogFileName;
            logfile.ArchiveAboveSize = 100 * (int)Math.Pow(10, 3);
            logfile.ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date;

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        public static bool SaveMessage(string _Message, enumEventEntryType _Type)
        {
            mutex.WaitOne();

            try
            {
                switch (_Type)
                {
                    case enumEventEntryType.Error:
                        logger.Error(_Message);
                        break;
                    case enumEventEntryType.Warning:
                        logger.Warn(_Message);
                        break;
                    case enumEventEntryType.Information:
                        logger.Info(_Message);
                        break;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}