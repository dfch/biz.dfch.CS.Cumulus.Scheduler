// Install-Package ncrontab 
// https://www.nuget.org/packages/ncrontab/
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace CumulusScheduler
{
    class ScheduledTask
    {
        private bool _fInitialised = false;
        private int _VersionDefault = 0;

        public ScheduledTaskParameters Parameters;
        public DateTime NextSchedule = DateTime.MinValue;
        public string Username
        {
            get { return _Credential.UserName; }
            set 
            {
                if (value.Contains("\\"))
                {
                    var DomainUsername = value.Split('\\');
                    _Credential.Domain = DomainUsername.First();
                    _Credential.UserName = DomainUsername.Last();
                }
                else
                {
                    _Credential.UserName = value;
                }
            }
        }
        public string Password
        {
            get { return _Credential.Password; }
            set { _Credential.Password = value; }
        }
        public string Domain
        {
            get { return _Credential.Domain; }
            set { _Credential.Domain = value; }
        }
        private NetworkCredential _Credential = new NetworkCredential();
        public NetworkCredential Credential
        {
            get { return _Credential; }
            set { _Credential = value; }
        }

        public ScheduledTask()
        {
            return;
        }
        public ScheduledTask(string parameters)
        {
            this.Initialise(parameters, this._VersionDefault, true);
        }
        public ScheduledTask(string parameters, int Version)
        {
            this.Initialise(parameters, Version, true);
        }
        public bool Initialise(string parameters, bool fThrowException = false)
        {
            return this.Initialise(parameters, this._VersionDefault, fThrowException);
        }
        public bool Initialise(string parameters, int Version, bool fThrowException = false)
        {
            var fReturn = false;
            if (_fInitialised) return fReturn;

            try
            {
                var jss = new JavaScriptSerializer();
                this.Parameters = jss.Deserialize<ScheduledTaskParameters>(parameters);
                fReturn = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("{0}@{1}: {2}\r\n{3}", ex.GetType().Name, ex.Source, ex.Message, ex.StackTrace));
                if(fThrowException)
                {
                    throw;
                } 
                else
                {
                    this.Parameters = null;
                    fReturn = false;
                }
            }
            finally
            {

            }
            this._fInitialised = fReturn;
            return fReturn;
        }
        public DateTime GetNextSchedule()
        {
            return GetNextSchedule(DateTime.Now);
        }
        public DateTime GetNextSchedule(DateTime WithinThisMinute)
        {
            var nextSchedule = DateTime.MinValue;
            try
            {
                if (!this.Parameters.Active)
                {
                    return DateTime.MinValue;
                }
                var schedule = CrontabSchedule.Parse(this.Parameters.CrontabExpression);
                var now = WithinThisMinute;
                var startMinute = new DateTime(now.Year, 1, 1, 0, 0, 0);
                //var endMinute = now.AddMinutes(1).AddSeconds(-1);
                var endMinute = now;
                nextSchedule = schedule.GetNextOccurrences(startMinute, endMinute).LastOrDefault();
                if (null == nextSchedule)
                {
                    Debug.WriteLine(string.Format(
                        "{0}: Getting next occurrence for time range '{1}-{2}' [{3}] FAILED. Check CrontabExpression or time range.", 
                        this.Parameters.CommandLine, 
                        startMinute.ToString("yyyy-MM-dd HH:mm:ss.fffzzz"), 
                        endMinute.ToString("yyyy-MM-dd HH:mm:ss.fffzzz"), 
                        this.Parameters.CrontabExpression
                        ));
                    return DateTime.MinValue;
                }
                if(nextSchedule.Minute < now.Minute)
                {
                    return DateTime.MinValue;
                }
                this.NextSchedule = nextSchedule;
                return this.NextSchedule;
            }
            catch (CrontabException ex)
            {
                Debug.WriteLine(string.Format("{0}@{1}: {2}\r\n{3}", ex.GetType().Name, ex.Source, ex.Message, ex.StackTrace));
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0}@{1}: {2}\r\n{3}", ex.GetType().Name, ex.Source, ex.Message, ex.StackTrace));
                throw;
            }
        }
        public bool IsScheduledToRun()
        {
            return IsScheduledToRun(DateTime.Now);
        }
        public bool IsScheduledToRun(DateTime WithinThisMinute)
        {
            var fReturn = false;
            var nextSchedule = GetNextSchedule(WithinThisMinute);
            if(!nextSchedule.Equals(DateTime.MinValue))
            {
                fReturn = true;
            }
            return fReturn;
        }
    }
}

/**
 *
 *
 * Copyright 2015 Ronald Rink, d-fens GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
