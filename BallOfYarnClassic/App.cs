using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NewRelic.Api.Agent;

namespace BallOfYarnClassic
{
    class App
    {
        private bool shutdownInitiated = false;

        private string lastKeyPress = String.Empty;

        public void Run(string[] args)
        {
            this.DoWebBeacon();

            var backgroundThread = new Thread(GetKeyPress);
            backgroundThread.Start(); 

            this.DoLoop();  
        }

        public void GetKeyPress()
        {
            while (!shutdownInitiated)
            {
                if (lastKeyPress == string.Empty)
                {
                    lastKeyPress = Console.ReadKey().KeyChar.ToString();
                }          
            }       
        }

        // This doesn't seem to work at all. Nothing shows up.
        // In theory, it should appear as a Web transaction even though this is a non-web console app.
        // TODO: find out why not
        [Transaction(Web = true)]
        public void DoWebBeacon()
        {
            Console.WriteLine("Starting up...");

            var uri = new Uri("http://Just/A/Test");
            NewRelic.Api.Agent.NewRelic.SetTransactionUri(uri);
        }

        // Thread entry points may not show up as a transaction.
        public void BackgroundProcessing()
        {
            this.DoLoop();
        }

        public void DoLoop()
        {
            Console.WriteLine("Press x to exit.");

            int count = 1;

            while (!shutdownInitiated)
            {
                this.SpinYarn(count++);

                this.HandleInput();
            }

            Console.WriteLine("Shutting down background process...");
            RaiseShutdownEvent();
        }

        // This shows up as a top-level transaction.
        [Transaction]
        public void SpinYarn(int count)
        {           
            // There must be some time spent with the thread inside the decorated method,
            // Otherwise you'll see a flat graph, which isn't great for demo purposes.
            Thread.Sleep(1000); 

            this.WriteToConsole();

            ApplyCustomParametersToTransaction(count);

            NewRelic.Api.Agent.NewRelic.RecordCustomEvent(
                "Stuff happens.",
                new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() } });
        }

        // This shows up as a top-level transaction.
        [Transaction]
        public void HandleInput()
        {
            Thread.Sleep(1000);

            try
            {
                if (lastKeyPress == "n")
                {
                    lastKeyPress = string.Empty;
                    Thread.Sleep(500);
                    Console.WriteLine("Someone set us up the bomb.");
                    throw new IOException("Someone set us up the bomb.");
                }

                if (lastKeyPress == "b")
                {
                    lastKeyPress = string.Empty;
                    Thread.Sleep(500);
                    Console.WriteLine("All your base are belong to us.");
                    throw new IOException("All your base are belong to us.");
                }

                if (lastKeyPress == "j")
                {
                    lastKeyPress = string.Empty;
                    Console.WriteLine("For Great Justice!");
                    var eventAttributes = new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() } };
                    NewRelic.Api.Agent.NewRelic.RecordCustomEvent("ForGreatJustice", eventAttributes);
                }

                if (lastKeyPress == "x")
                {
                    shutdownInitiated = true;
                }
            }
            catch (Exception ex)
            {
                // This API call ensures that the exception is metered even if swallowed
                NewRelic.Api.Agent.NewRelic.NoticeError(ex);
            }

            this.WriteToConsole();

            this.ChaosMetric();
        }

        // This method call shows up in the breakdown of both DoStuff and DoSomethingElse, since they both invoke it.
        [Trace]
        public void WriteToConsole()
        {
            //TODO: verify this works as expected
            // Sleep added here so that we can observe a "time" in the breakdown

            Thread.Sleep(500); 
            Console.WriteLine("Spinning thread into yarn...");
        }

        // As a nested transaction, this doesn't show up in the Transactions table
        // However, it does appear in the breakdown under the DoSomethingElse transaction.
        // As documented by NewRelic
        [Transaction]
        public void ChaosMetric()
        {
            //TODO: discover why this doesn't show up anywhere
            NewRelic.Api.Agent.NewRelic.RecordMetric("Chaos Factor", new Random().Next(0, 100));
        }

        // This line causes a metric to appear under the Attributes section of the DoStuff transaction.
        // These metrics do NOT appear as transactions themselves in the high-level overview, since there's no decoration
        private void ApplyCustomParametersToTransaction(int count)
        {
            NewRelic.Api.Agent.NewRelic.AddCustomParameter("Iteration #:", count);
            NewRelic.Api.Agent.NewRelic.AddCustomParameter("Details", "Yo dawg I herd you like custom parameters.");
        }

        // Have not been able to find any instances of this data in APM.
        // TODO: find out where custom events appear or how to correctly raise them
        //[Transaction]
        void RaiseShutdownEvent()
        {
            var eventAttributes = new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() } };
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("Application Shutdown.", eventAttributes);
        }

        // TODO: consider using Insights as the indicator of run completion, etc
        // https://insights.newrelic.com/accounts/2116180/explorer/events?eventType=Transaction&duration=1800000&facet=Stuff%20Count
    }
}