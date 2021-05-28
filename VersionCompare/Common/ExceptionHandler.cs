using NLog;
using NLog.Config;
using NLog.Targets;
using System;

namespace VersionCompare.Common
{
    public class ExceptionHandler
    {
        public static ExceptionHandler Current = new ExceptionHandler();

        private ExceptionHandler()
        {
            _logger = CreateLogger();
        }

        private Logger _logger;

        public void Log(Exception ex)
        {
            _logger.Debug(DateTime.Now);
            _logger.Debug(ex.ToString());
            _logger.Debug("");
        }

        private Logger CreateLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();
            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);
            fileTarget.FileName = "${basedir}/ErrorLogs/${date:format=yyyyMMdd}.txt";
            fileTarget.Layout = "${date:format=HH\\:mm\\:ss}|${message}";
            LoggingRule rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);
            LogFactory logfactory = new LogFactory(config);
            var logger = logfactory.GetLogger("");
            return logger;
        }
    }
}
