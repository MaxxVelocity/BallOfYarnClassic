// https://github.com/MaxxVelocity/BallOfYarnClassic.git

using System;
using System.Collections.Generic;

namespace BallOfYarnClassic
{
    class Program
    {
        static void Main(string[] args)
        {
            StartupEvent();

            var app = new App();

            app.Run(args);

            ShutdownEvent();
        }

        //These don't seem to be working, not sure why.
        static void StartupEvent()
        {
            var eventAttributes = new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() }};
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("Application Startup.", eventAttributes);
        }

        static void ShutdownEvent()
        {
            var eventAttributes = new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() } };
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("Application Shutdown.", eventAttributes);
        }
    }
}
