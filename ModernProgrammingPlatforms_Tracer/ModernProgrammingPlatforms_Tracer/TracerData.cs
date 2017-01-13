using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace ModernProgrammingPlatforms_Tracer
{
    class TracerData
    {
        public class MethodInformation
        {
            public string MethodName { get; set; }
            public string ClassName { get; set; }
            public int ParamsCount { get; set; }
            public long Time { get; set; }
            public int Nesting { get; set; }
            public int Priority { get; set; }
            public int ThreadID { get; set; }

            public  MethodInformation(string methodname, string classname, int paramscount, long time, int nesting, int priority, int threadid)
            {
                MethodName = methodname;
                ClassName = classname;
                ParamsCount = paramscount;
                Time = time;
                Nesting = nesting;
                Priority = priority;
                ThreadID = threadid;
            }
        }
        public class Thread
        {
            public int ThreadId { get; set; }
            public long Time;
            public TimeInformation WorkTime = new TracerData.TimeInformation();

            public Stack<TracerData.MethodInformation> StackInformation = new Stack<MethodInformation>();
            
            public Thread (int threadid)
            {
                ThreadId = threadid;
            }
        }
        public class TreeNode
        {
            public MethodInformation Member;
            public List<TreeNode> MethodList = new List<TreeNode> { };

            public TreeNode(MethodInformation member)
            {
                Member = member;
            }
        }
        public class TimeInformation
        {
            public List<Stopwatch> TimeList = new List<Stopwatch> { };
        }
    }
}
