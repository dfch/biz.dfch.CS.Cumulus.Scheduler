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
        bool fInitialised = false;
        int _VersionDefault = 0;

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
            if (fInitialised) return fReturn;

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
            this.fInitialised = fReturn;
            return fReturn;
        }
        public DateTime GetNextSchedule()
        {
            return GetNextSchedule(DateTime.Now);
        }
        public DateTime GetNextSchedule(DateTime WithinThisMinute)
        {
            var NextSchedule = DateTime.MinValue;
            try
            {
                if (!this.Parameters.Active)
                {
                    return DateTime.MinValue;
                }
                var schedule = CrontabSchedule.Parse(this.Parameters.CrontabExpression);
                var Now = WithinThisMinute;
                var StartMinute = new DateTime(Now.Year, 1, 1, 0, 0, 0);
                //var EndMinute = Now.AddMinutes(1).AddSeconds(-1);
                var EndMinute = Now;
                NextSchedule = schedule.GetNextOccurrences(StartMinute, EndMinute).Last();
                if(NextSchedule.Minute < Now.Minute)
                {
                    return DateTime.MinValue;
                }
                if (this.NextSchedule != NextSchedule)
                {
                    this.NextSchedule = NextSchedule;
                }
                return NextSchedule;
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
            var NextSchedule = GetNextSchedule(WithinThisMinute);
            if(!NextSchedule.Equals(DateTime.MinValue))
            {
                fReturn = true;
            }
            return fReturn;
        }
    }
}
