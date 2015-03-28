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
