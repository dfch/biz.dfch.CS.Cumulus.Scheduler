using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CumulusScheduler
{
    class ScheduledTaskParameters
    {
        bool _Active;
        public bool Active
        {
            get { return _Active; }
            set { _Active = value; }
        }
        string _CrontabExpression;
        public string CrontabExpression
        {
            get { return _CrontabExpression; }
            set { _CrontabExpression = value; }
        }
        string _CommandLine;
        public string CommandLine
        {
            get { return _CommandLine; }
            set { _CommandLine = value; }
        }
        string _WorkingDirectory;
        public string WorkingDirectory
        {
            get { return _WorkingDirectory; }
            set { _WorkingDirectory = value; }
        }
        string _ManagementCredential;
        public string ManagementCredential
        {
            get { return _ManagementCredential; }
            set { _ManagementCredential = value; }
        }
    }
}
