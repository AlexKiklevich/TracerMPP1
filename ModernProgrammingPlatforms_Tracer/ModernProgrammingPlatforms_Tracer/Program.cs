using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace ModernProgrammingPlatforms_Tracer
{
    class Program
    {
        public static Tracer tracer = new Tracer();
        public static void Main(string[] args)
        {
            tracer.StartTrace();

            Thread myThread = new Thread(new ThreadStart(meth8));
            myThread.Start();
            meth5();
            meth1(2);

            tracer.StopTrace();
            tracer.PrintToConsole();
            tracer.BuildXml();
            Console.ReadKey();
        }
        public static void meth1(int a)
        {
            tracer.StartTrace();
            int b = meth2(a);
            meth4(3, 6, 7);
            tracer.StopTrace();
        }
        public static int meth2(int a)
        {
            tracer.StartTrace();
            int b = meth3(a);

            tracer.StopTrace();
            return b;
        }
        public static int meth3(int a)
        {
            tracer.StartTrace();
            int b = a;
            tracer.StopTrace();
            return b;
        }
        public static void meth4(int a,int d, int c)
        {
            tracer.StartTrace();
            int b = a;
            a = c;
            c = d;
            tracer.StopTrace();
        }
        public static void meth5()
        {
            tracer.StartTrace();
            int c = 6;
            meth6();
            int k = c;
            tracer.StopTrace();
        }
        public static void meth6()
        {
            tracer.StartTrace();
            int c = 6;
            int k = c;
            tracer.StopTrace();
        }
        public static void meth7()
        {
            tracer.StartTrace();
            int c = 6;
            meth2(1);
            int k = c;
            tracer.StopTrace();
        }
        public static void meth8()
        {
            tracer.StartTrace();
            int c = 6;
            meth7();
            int k = c;
            tracer.StopTrace();
        }
    }
    
    class Tracer
    {
        private static List<TracerData.TreeNode> TreeList = new List<TracerData.TreeNode> { };
        private static List<TracerData.Thread> ThreadList = new List<TracerData.Thread> { };
        private object thisLock = new Object();
        public void StartTrace()
        {
            lock (thisLock)
            {
                if (ThreadList.Count > 0)
                {
                    if (!ThreadCheck(ThreadList, Thread.CurrentThread.ManagedThreadId))
                    {
                        ThreadList.Add(ThreadListMember(Thread.CurrentThread.ManagedThreadId));
                    }
                }
                else
                    ThreadList.Add(ThreadListMember(Thread.CurrentThread.ManagedThreadId));
                foreach (TracerData.Thread t in ThreadList)
                {
                    if (t.ThreadId == Thread.CurrentThread.ManagedThreadId)
                    {
                        t.WorkTime.TimeList.Add(Stopwatch.StartNew());
                    }
                }
                
            }
        }

        public void StopTrace()
        {
            lock (thisLock)
            {
                StackTrace stacktrace = new StackTrace();
                for (int i = 0; i < stacktrace.FrameCount; i++)
                {
                    if (stacktrace.GetFrame(i).GetMethod().DeclaringType != typeof(Tracer))
                    {
                        foreach (TracerData.Thread thread in ThreadList)
                        {
                            if (thread.ThreadId == Thread.CurrentThread.ManagedThreadId)
                            {
                                thread.WorkTime.TimeList[thread.WorkTime.TimeList.Count - 1].Stop();
                            }
                            
                        }
                        
                        foreach (TracerData.Thread t in ThreadList)
                        {
                            if (t.ThreadId == Thread.CurrentThread.ManagedThreadId)
                            {
                                t.StackInformation.Push(new TracerData.MethodInformation(stacktrace.GetFrame(i).GetMethod().Name,
                                                                                                               stacktrace.GetFrame(i).GetMethod().DeclaringType.Name,
                                                                                                               stacktrace.GetFrame(i).GetMethod().GetParameters().Length,
                                                                                                               t.WorkTime.TimeList[t.WorkTime.TimeList.Count - 1].ElapsedMilliseconds,
                                                                                                               t.WorkTime.TimeList.Count,
                                                                                                               t.StackInformation.Count,
                                                                                                               t.ThreadId));
                                t.Time += t.WorkTime.TimeList[t.WorkTime.TimeList.Count - 1].ElapsedMilliseconds;
                            }
                        }
                        foreach (TracerData.Thread t in ThreadList)
                        {
                            if (t.ThreadId == Thread.CurrentThread.ManagedThreadId)
                            {
                                t.WorkTime.TimeList.RemoveAt(t.WorkTime.TimeList.Count - 1);
                            }
                            
                        }
                        
                        return;
                    }
                }
            }
        }
        
        private static List<TracerData.TreeNode> TreeBuild(List<TracerData.TreeNode> TreeList)
        {
             foreach (TracerData.Thread t in ThreadList)
             {
                 while (t.StackInformation.Count > 0)
                 {
                     TreeList = TreeMemberAdd(TreeListMember(t.StackInformation.Pop()), TreeList);
                 }
             }
            
            TreeList = TreeSort(TreeList);
            return TreeList;
        }   
        public void BuildXml()
        {
            
            XDocument xmldoc = new XDocument();
            XElement root = new XElement("root");
            foreach (TracerData.Thread t in ThreadList)
            {
                string buf = t.ThreadId.ToString();
                XElement thread = new XElement("thread");
                XAttribute id = new XAttribute("id", t.ThreadId);
                XAttribute time = new XAttribute("time", t.Time + "ms");
                thread.Add(id, time);
                XmlTreeBuild(TreeList, thread, buf);
                root.Add(thread);
            }
            xmldoc.Add(root);
            xmldoc.Save("output.xml");
        }

        private static XElement XmlTreeBuild(List<TracerData.TreeNode> TreeList, XElement thread, string buf)
        {
            foreach (TracerData.TreeNode t in TreeList)
            {
                if (t.Member.ThreadID.ToString() == buf)
                {

                    thread.Add(ElemAdd(t));

                    if (t.MethodList.Count != 0)
                    {
                        XmlTreeBuild(t.MethodList, thread.LastNode as XElement, buf);
                    }
                }

            }

            return thread;
        }
        private static XElement ElemAdd ( TracerData.TreeNode node)
        {
            XElement method = new XElement("method");

            XAttribute name = new XAttribute("name", node.Member.MethodName);
            XAttribute time = new XAttribute("time", node.Member.Time + "ms");
            XAttribute package = new XAttribute("package", node.Member.ClassName);
            XAttribute paramscount = new XAttribute("paramsCount", node.Member.ParamsCount);

            method.Add(name, time, package, paramscount);
            
            return method;
        }

        public void PrintToConsole()
        {
            TreeList = TreeBuild(TreeList);
            string OutputToConsole = "<root>\n";
            foreach (TracerData.Thread t in ThreadList)
            {
                OutputToConsole += "-ThreadId = " + t.ThreadId + " time = " + t.Time + "\n";
                OutputToConsole = CLR(TreeList, OutputToConsole, 1, t.ThreadId);
            }
            OutputToConsole += "-</method>\n</root> " + Environment.NewLine;
            Console.WriteLine(OutputToConsole);
        }

        private static string CLR(List<TracerData.TreeNode> TreeList, string OutputToConsole, int buf, int threadid)
        {
            

            if (TreeList.Count > 0)
            {
                foreach (TracerData.TreeNode T in TreeList)
                {
                    if (T.Member.ThreadID == threadid)
                    {
                        for (int i = 0; i < buf; i++)
                        {
                            OutputToConsole += "-";
                        }
                        OutputToConsole += "<method name = " + T.Member.MethodName + "; " +
                          "time = " + T.Member.Time + "ms" + "; " +
                          "package = " + T.Member.ClassName + "; " +
                          "paramsCount = " + T.Member.ParamsCount + ">\n";
                        if (T.MethodList.Count > 0)
                        {
                            buf += 2;
                            OutputToConsole = CLR(T.MethodList, OutputToConsole, buf, threadid);
                            buf -= 2;
                        }
                    }
                }
            }
            return OutputToConsole;
        }

        private static List<TracerData.TreeNode> TreeMemberAdd ( TracerData.TreeNode node, List<TracerData.TreeNode> TreeList)
        {
            if (TreeList.Count == 0 )
            {
                TreeList.Add(node);
            }
            else
                if (TreeList[TreeList.Count - 1].Member.ThreadID != node.Member.ThreadID)
                {
                   TreeList.Add(node);
                }
                else
               {
                if (((node.Member.Nesting - TreeList[TreeList.Count - 1].Member.Nesting) == 1))
                {
                    TreeList[TreeList.Count - 1].MethodList.Add(node);
                }
                else
                {
                    if ((node.Member.Nesting - TreeList[TreeList.Count - 1].Member.Nesting) > 1)
                    {
                        TreeList[TreeList.Count - 1].MethodList = TreeMemberAdd(node, TreeList[TreeList.Count - 1].MethodList);
                    }
                }
            }
               
             return TreeList;
        }

        private static TracerData.TreeNode TreeListMember (TracerData.MethodInformation member)
        {
            TracerData.TreeNode t = new TracerData.TreeNode(member);
            return t;
        }
        private static TracerData.Thread ThreadListMember (int id)
        {
            TracerData.Thread t = new TracerData.Thread(id);
            return t; 
        }
       
        private static List<TracerData.TreeNode> TreeSort(List<TracerData.TreeNode> TreeList)
        {
            for (int i = 0; i == (TreeList.Count - 2); i++)
            {
                if (TreeList[i].Member.Priority > TreeList[i + 1].Member.Priority)
                {
                    TracerData.TreeNode buf = TreeList[i];
                    TreeList[i] = TreeList[i + 1];
                    TreeList[i + 1] = buf;
                }
            }
            foreach (TracerData.TreeNode t in TreeList)
            {
                if (t.MethodList.Count > 1)
                {
                    TreeSort(t.MethodList);
                }
            }
            return TreeList;
        }
        private static bool ThreadCheck (List<TracerData.Thread> ThreadList, int id)
        {
            foreach (TracerData.Thread t in ThreadList)
            {
                if (t.ThreadId == id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
