﻿using System;
using System.Collections.Generic;
using System.Linq;
// Install-Package Microsoft.Net.Http
// https://www.nuget.org/packages/Microsoft.Net.Http
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Management;
// Install-Package Newtonsoft.Json
// https://www.nuget.org/packages/Newtonsoft.Json/6.0.1
// http://james.newtonking.com/json/help/index.html?topic=html/SelectToken.htm#
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CumulusScheduler
{
    class ScheduledTaskWorker
    {
        private const int _updateIntervalMinutesDefault = 5;
        private int _updateIntervalMinutes;
        private const int _serverNotReachableRetriesDefault = 3;
        private int _serverNotReachableRetryMinutes;
        bool _fInitialised = false;
        DateTime _lastInitialised;
        private DateTime _lastUpdate = DateTime.Now;
        private Timer _stateTimer = null;
        private TimerCallback _timerDelegate;
        private List<ScheduledTask> _list = new List<ScheduledTask>();
        private Uri _uri;
        private ApplicationData.ApplicationData _svcApplicationData;
        private Utilities.Utilities _svcUtilities;
        private string _managementUri = Environment.MachineName;

        private bool _active = true;
        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public ScheduledTaskWorker(string Uri, string ManagementUri, int UpdateIntervalMinutes, int ServerNotReachableRetries)
        {
            string fn = string.Format("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(fn);

            this.Initialise(Uri, ManagementUri, UpdateIntervalMinutes, ServerNotReachableRetries, true);
        }
        public bool UpdateScheduledTasks()
        {
            string fn = string.Format("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(fn);

            var fReturn = false;

            if (!_active) return fReturn;

            try
            {
                var Now = DateTime.Now;
                var MgmtUri = _svcApplicationData.ManagementUris
                    .Where(
                        e => e.Name.Equals(_managementUri, StringComparison.OrdinalIgnoreCase) &&
                        e.Type.Equals("CumulusScheduler", StringComparison.OrdinalIgnoreCase)
                    ).SingleOrDefault();
                if(null == MgmtUri)
                {
                    Debug.WriteLine("{0}: ManagementUri not found at '{1}'. Will retry later.", _managementUri, _svcApplicationData.BaseUri);
                    if (_serverNotReachableRetryMinutes <= (Now - _lastUpdate).TotalMinutes)
                    {
                        throw new TimeoutException();
                    }
                    goto Success;
                }

                var jtoken = JToken.Parse(MgmtUri.Value);
                if (jtoken is JArray)
                {
                    lock (_list)
                    {
                        _list.Clear();
                        var ja = JArray.Parse(MgmtUri.Value);
                        foreach (var j in ja)
                        {
                            var task = new ScheduledTask(j.ToString());
                            var MgmtCredential = _svcUtilities.ManagementCredentialHelpers
                                .Where(
                                    e => e.Name.Equals(task.Parameters.ManagementCredential, StringComparison.OrdinalIgnoreCase)
                                ).Single();

                            task.Username = MgmtCredential.Username;
                            task.Password = MgmtCredential.Password;

                            Debug.WriteLine(string.Format("{0}: Adding '{1}' ...", _managementUri, MgmtUri.Value));
                            _svcUtilities.Detach(MgmtCredential);
                            _list.Add(task);
                        }
                    }
                }
                else if (jtoken is JObject)
                {
                    var task = new ScheduledTask(MgmtUri.Value);
                    var MgmtCredential = _svcUtilities.ManagementCredentialHelpers
                            .Where(
                                e => e.Name.Equals(task.Parameters.ManagementCredential, StringComparison.OrdinalIgnoreCase)
                            ).Single();

                    task.Username = MgmtCredential.Username;
                    task.Password = MgmtCredential.Password;

                    Debug.WriteLine(string.Format("{0}: Adding '{1}' ...", _managementUri, MgmtUri.Value));
                    _svcApplicationData.Detach(MgmtUri);
                    _svcUtilities.Detach(MgmtCredential);
                    lock (_list)
                    {
                        _list.Clear();
                        _list.Add(task);
                    }
                }
                else
                {
                    lock (_list)
                    {
                        _list.Clear();
                    }
                }
                _svcApplicationData.Detach(MgmtUri);
            }
            catch(InvalidOperationException ex)
            {
                LogBase.WriteException(ex);
                Debug.WriteLine(string.Format("{0}: ManagementUri not found at '{1}'. Aborting ...", _managementUri, _svcApplicationData.BaseUri.AbsoluteUri));
                throw;
            }
            catch (TimeoutException ex)
            {
                LogBase.WriteException(ex);
                Debug.WriteLine(string.Format("{0}: Timeout retrieving ManagementUri at '{1}'. Aborting ...", _managementUri, _svcApplicationData.BaseUri.AbsoluteUri));
                throw;
            }

Success :
            _lastInitialised = DateTime.Now;
            fReturn = true;
            return fReturn;
        }
        protected bool Initialise(string Uri, string ManagementUri, int UpdateIntervalMinutes, int ServerNotReachableRetries, bool fThrowException)
        {
            string fn = string.Format("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(fn);

            var fReturn = false;
            if (_fInitialised) return fReturn;

            try
            {
                _updateIntervalMinutes = (0 != UpdateIntervalMinutes) ? UpdateIntervalMinutes : _updateIntervalMinutesDefault;
                _serverNotReachableRetryMinutes = _updateIntervalMinutes * (0 != ServerNotReachableRetries ? ServerNotReachableRetries : _serverNotReachableRetriesDefault);

                _uri = new Uri(Uri);
                Debug.WriteLine(string.Format("Uri: '{0}'", this._uri.AbsoluteUri));
                _managementUri = ManagementUri;

                _svcApplicationData = new ApplicationData.ApplicationData(new Uri(string.Format("{0}ApplicationData.svc", _uri.AbsoluteUri)));
                _svcApplicationData.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                _svcApplicationData.Format.UseJson();

                _svcUtilities = new Utilities.Utilities(new Uri(string.Format("{0}api/Utilities.svc", _uri.AbsoluteUri)));
                _svcUtilities.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                _svcUtilities.Format.UseJson();

                UpdateScheduledTasks();

                _timerDelegate = new TimerCallback(this.RunTasks);
                //var MilliSecondsToWait = (60 - DateTime.Now.Second) * 1000;
                //Debug.WriteLine(string.Format("Waiting {0}ms for begin of next minute ...", MilliSecondsToWait));
                //System.Threading.Thread.Sleep(MilliSecondsToWait);
                _stateTimer = new Timer(_timerDelegate, null, 1000, (1000 * 60) - 20);
                
                fReturn = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0}@{1}: {2}\r\n{3}", ex.GetType().Name, ex.Source, ex.Message, ex.StackTrace));
                if (fThrowException)
                {
                    throw;
                }
                else
                {
                    fReturn = false;
                }
            }
            finally
            {
                // N/A
            }
            this._fInitialised = fReturn;
            this._active = fReturn;
            return fReturn;
        }
        ~ScheduledTaskWorker()
        {
            string fn = string.Format("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(fn);

            if (null != this._stateTimer)
            {
                _stateTimer.Dispose();
            }
        }
        // The state object is necessary for a TimerCallback.
        protected void RunTasks(object stateObject)
        {
            var fReturn = false;
            var Now = DateTime.Now;

            if (!_active || !_fInitialised) return;

            string fn = string.Format("{0}:{1}.{2}", this.GetType().Namespace, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(fn);
            try
            {
                lock (_list)
                {
                    //Debug.WriteLine(string.Format("Iterating list ... '{0}'", _list.Count));
                    foreach (var task in _list)
                    {
                        //var NextSchedule = task.GetNextSchedule();
                        //Debug.WriteLine(string.Format("Checked '{0}' [{1}].", task.Parameters.CommandLine, NextSchedule.ToString()));
                        if (task.IsScheduledToRun(Now))
                        {
                            Debug.WriteLine(string.Format("Invoking '{0}' as '{1}' [{2}] ...", task.Parameters.CommandLine, task.Username, task.NextSchedule.ToString()));

                            Process.StartProcess(task.Parameters.CommandLine, task.Parameters.WorkingDirectory, task.Credential, true);
                        }
                    }
                }
                if (_updateIntervalMinutes <= (Now - _lastInitialised).TotalMinutes)
                {
                    fReturn = UpdateScheduledTasks();
                }
            }
            catch (TimeoutException ex)
            {
                LogBase.WriteException(ex);
                var msg = string.Format("{0}: Timeout retrieving ManagementUri at '{1}'. Aborting ...", _managementUri, _svcApplicationData.BaseUri.AbsoluteUri);
                Debug.WriteLine(msg);
                Environment.FailFast(msg);
                throw;
            }
            catch (Exception ex)
            {
                LogBase.WriteException(ex);
                throw;
            }
        }
    }
}
