using System;
using System.Collections.Generic;
using System.Threading;
using NewRelic.Api.Agent;

namespace BallOfYarnClassic
{
    class App
    {
        private bool shutdownInitiated = false;

        private string lastKeyPress;

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
            Console.WriteLine("Press any key to exit.");

            int count = 1;

            while (!shutdownInitiated)
            {
                this.DoStuff(count++);

                this.DoSomethingElse();

                try
                {
                    if (lastKeyPress == "n")
                    {
                        Console.WriteLine();
                        Console.WriteLine("Someone set us up the bomb.");
                        throw new Exception("Detected an n bomb!");
                    }

                    if (lastKeyPress == "x")
                    {
                        shutdownInitiated = true;
                    }                  
                }
                catch (Exception ex)
                {
                    NewRelic.Api.Agent.NewRelic.NoticeError(ex);
                }
                finally
                {
                    lastKeyPress = string.Empty;
                }
            }

            Console.WriteLine("Shutting down background process...");
            RaiseShutdownEvent();
        }

        // This shows up as a top-level transaction.
        [Transaction]
        public void DoStuff(int count)
        {
            Thread.Sleep(2500);

            this.WriteToConsole();

            this.ApplyStuffCountAttribute(count);

            NewRelic.Api.Agent.NewRelic.RecordCustomEvent(
                "Stuff happens.",
                new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() } });
        }

        // This shows up as a top-level transaction.
        [Transaction]
        public void DoSomethingElse()
        {
            Thread.Sleep(2500);

            this.WriteToConsole();

            this.ChaosMetric();
        }

        // This shows up in the breakdown of both DoStuff and DoSomethingElse, since they both invoke it.
        [Trace]
        public void WriteToConsole()
        {
            Console.WriteLine("Still running...");
        }

        // As a nested transaction, this doesn't show up in the Transactions table
        // However, it does appear in the breakdown under the DoSomethingElse transaction.
        // TODO: determine what, if any, is the difference between a Trace and a nested transaction
        [Transaction]
        public void ChaosMetric()
        {
            NewRelic.Api.Agent.NewRelic.RecordMetric("Chaos Factor", new Random().Next(0, 100));
        }

        // This line causes a metric to appear under the Attributes section of the DoStuff transaction.
        // These metrics do NOT appear as transactions themselves in the high-level overview.
        private void ApplyStuffCountAttribute(int count)
        {
            
            NewRelic.Api.Agent.NewRelic.AddCustomParameter("Stuff Count", count);
        }

        // Have not been able to find any instances of this data in APM.
        // TODO: find out where custom events appear or how to correctly raise them
        static void RaiseShutdownEvent()
        {
            var eventAttributes = new Dictionary<string, object> { { "Local Time:", DateTime.Now.ToLocalTime().ToShortDateString() } };
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("Application Shutdown.", eventAttributes);
        }

        // TODO: consider using Insights as the indicator of run completion, etc
        // https://insights.newrelic.com/accounts/2116180/manage/api_keys
    }
}
