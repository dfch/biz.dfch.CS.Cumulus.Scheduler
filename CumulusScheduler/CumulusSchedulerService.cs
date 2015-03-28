using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CumulusScheduler
{
    public partial class CumulusSchedulerService : ServiceBase
    {
        public ManualResetEvent fAbort = new ManualResetEvent(false);
        ScheduledTaskWorker _worker;

        public CumulusSchedulerService()
        {
            this.CanPauseAndContinue = true; 
            InitializeComponent();
        }

        internal void OnStartInteractive(string[] args)
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            try
            {
                OnStart(args);
                fAbort.WaitOne();
                Debug.WriteLine(string.Format("CancelKeyPress detected. Stopping interactive mode."));
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("{0}@{1}: {2}\r\n{3}", ex.GetType().Name, ex.Source, ex.Message, ex.StackTrace));
                Debug.WriteLine(string.Format("Stopping interactive mode."));
            }
            finally
            {
                OnStop();
            }
        }
        protected override void OnStart(string[] args)
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            try
            {
                var Uri = string.Empty;
                Uri = ConfigurationManager.AppSettings["Uri"];
                if (2 <= args.Length) Uri = args[0];
                Trace.Assert(!string.IsNullOrWhiteSpace(Uri), "Uri: Parameter validation FAILED.");

                var ManagementUri = string.Empty;
                ManagementUri = ConfigurationManager.AppSettings["ManagementUri"];
                if (2 <= args.Length) ManagementUri = args[1];
                Trace.Assert(!string.IsNullOrWhiteSpace(ManagementUri), "ManagementUri: Parameter validation FAILED.");
                
                var UpdateIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["UpdateIntervalMinutes"]);

                var ServerNotReachableRetries = Convert.ToInt32(ConfigurationManager.AppSettings["ServerNotReachableRetries"]);

                _worker = new ScheduledTaskWorker(Uri, ManagementUri, UpdateIntervalMinutes, ServerNotReachableRetries);
            }
            catch(Exception ex)
            {
                var msg = string.Format("{0}@{1}: {2}\r\n{3}", ex.GetType().Name, ex.Source, ex.Message, ex.StackTrace);
                Debug.WriteLine(msg);
                Environment.FailFast(msg, ex);
            }
            
            base.OnStart(args);
        }
        protected override void OnStop()
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            _worker.Active = false;

            base.OnStop();
        }
        protected override void OnPause()
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            _worker.Active = false;

            base.OnPause();
        }
        protected override void OnContinue()
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            _worker.Active = true;
            _worker.UpdateScheduledTasks();

            base.OnContinue();
        }
        protected override void OnCustomCommand(int command)
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            base.OnCustomCommand(command);
        }
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            base.OnSessionChange(changeDescription);
        }
        protected override void OnShutdown()
        {
            Debug.WriteLine("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);

            _worker.Active = false;

            base.OnShutdown();
        }
    }
}
