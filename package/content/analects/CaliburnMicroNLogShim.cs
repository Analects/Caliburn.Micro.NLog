using System;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Targets;
using CMLogManager = Caliburn.Micro.LogManager;
using NLogManager = NLog.LogManager;

namespace Caliburn.Micro.NLog
{
    public class CaliburnMicroNLogShim : ILog
    {
        public static void ConfigureLogging(bool fullCaliburnMicroLogging = false)
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("vslevel", typeof(VSLevelLayoutRenderer));

            var config = new LoggingConfiguration();

            var debugTarget = new DebuggerTarget();
            debugTarget.Layout = "${logger} ${vslevel}: ${message}${onexception:inner=${newline}${exception:format=tostring}}";
            config.AddTarget("debug", debugTarget);

            // Quiet Caliburn.Micro
            if (!fullCaliburnMicroLogging)
            {
                config.LoggingRules.Add(new LoggingRule("Screen", LogLevel.Warn, debugTarget) { Final = true });
                config.LoggingRules.Add(new LoggingRule("Action", LogLevel.Warn, debugTarget) { Final = true });
                config.LoggingRules.Add(new LoggingRule("ViewModelBinder", LogLevel.Warn, debugTarget) { Final = true });
            }

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, debugTarget));
            NLogManager.Configuration = config;

            CMLogManager.GetLog = type => new CaliburnMicroNLogShim(type);
        }

        private readonly Logger innerLogger;

        public CaliburnMicroNLogShim(Type type)
        {
            innerLogger = NLogManager.GetLogger(type.Name);
        }

        public void Error(Exception exception)
        {
            innerLogger.ErrorException(exception.Message, exception);
        }

        public void Info(string format, params object[] args)
        {
            innerLogger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            innerLogger.Warn(format, args);
        }
    }

    [ThreadAgnostic, LayoutRenderer("vslevel")]
    public class VSLevelLayoutRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Level == LogLevel.Info)
                builder.Append("Information");
            else
                builder.Append(logEvent.Level.ToString());
        }
    }
}