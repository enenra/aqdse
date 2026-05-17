using ParallelTasks;
using RichHudFramework.Internal;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace RichHudFramework
{
    /// <summary>
    /// Simple aggregate exception class. Feed it a list of exceptions and out pops a new exception with their contents
    /// crammed into one message.
    /// </summary>
    public class AggregateException : Exception
    {
        public AggregateException(string aggregatedMsg) : base(aggregatedMsg)
        { }

        public AggregateException(IReadOnlyList<Exception> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        public AggregateException(IReadOnlyList<AggregateException> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        private static string GetExceptionMessages<T>(IReadOnlyList<T> exceptions) where T : Exception
        {
            StringBuilder sb = new StringBuilder(exceptions[0].Message.Length * exceptions.Count);

            for (int n = 0; n < exceptions.Count; n++)
                if (n != exceptions.Count - 1)
                    sb.Append(exceptions[n].ToString() + "\n");
                else
                    sb.Append(exceptions[n].ToString());

            return sb.ToString();
        }
    }

    /// <summary>
    /// Used to separate exceptions thrown manually in response to expected exceptions. Usually used in conjunction 
    /// with IO/serialization operations.
    /// </summary>
    public class KnownException : Exception
    {
        public KnownException() : base()
        { }

        public KnownException(string message) : base(message)
        { }

        public KnownException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public class TaskPool : RichHudComponentBase
    {
        /// <summary>
        /// Sets the limit for the total number of tasks running in all <see cref="TaskPool"/>s.
        /// </summary>
        public static int MaxTasksRunning { get { return maxTasksRunning; } set { maxTasksRunning = MathHelper.Clamp(value, 1, 10); } }
        private static int maxTasksRunning = 1, tasksRunningCount = 0;

        private readonly List<Task> tasksRunning;
        private readonly Queue<Action> tasksWaiting;
        private readonly ConcurrentQueue<Action> actions;
        private readonly Action<List<KnownException>, AggregateException> errorCallback;

        public TaskPool(Action<List<KnownException>, AggregateException> errorCallback) : base(true, true)
        {
            this.errorCallback = errorCallback;

            tasksRunning = new List<Task>();
            actions = new ConcurrentQueue<Action>();
            tasksWaiting = new Queue<Action>();
        }

        public override void Close()
        {
            tasksRunningCount = 0;
        }

        /// <summary>
        /// Updates public task/action queues and runs exception handling.
        /// </summary>
        public override void Draw()
        {
            TryStartWaitingTasks();
            UpdateRunningTasks();
            RunTaskActions();
        }

        /// <summary>
        /// Enqueues an action to run in parallel. Not thread safe; must be called from the main thread.
        /// </summary>
        public void EnqueueTask(Action action)
        {
            if (Parent == null && RichHudCore.Instance != null)
                RegisterComponent(RichHudCore.Instance);
            else if (ExceptionHandler.Unloading)
                throw new Exception("New tasks cannot be started while the mod is being unloaded.");

            tasksWaiting.Enqueue(action);
        }

        /// <summary>
        /// Enqueues an action to run on the main thread. Meant to be used by threads other than the main.
        /// </summary>
        public void EnqueueAction(Action action)
        {
            if (Parent == null && RichHudCore.Instance != null)
                RegisterComponent(RichHudCore.Instance);
            else if (ExceptionHandler.Unloading)
                throw new Exception("New tasks cannot be started while the mod is being unloaded.");

            actions.Enqueue(action);
        }

        /// <summary>
        /// Attempts to start any tasks in the waiting queue if the number of tasks running
        /// is below a set threshold.
        /// </summary>
        private void TryStartWaitingTasks()
        {
            Action action;

            while (tasksRunningCount < maxTasksRunning && (tasksWaiting.Count > 0) && tasksWaiting.TryDequeue(out action))
            {
                tasksRunning.Add(MyAPIGateway.Parallel.Start(action));
                tasksRunningCount++;
            }
        }

        /// <summary>
        /// Checks the task list for invalid tasks and tasks with exceptions then logs and throws exceptions as needed.
        /// </summary>
        private void UpdateRunningTasks()
        {
            List<KnownException> knownExceptions = new List<KnownException>();
            List<Exception> otherExceptions = new List<Exception>(); //unknown exceptions
            AggregateException unknownExceptions = null;

            for (int n = 0; n < tasksRunning.Count; n++)
            {
                Task task = tasksRunning[n];

                if (task.Exceptions != null && task.Exceptions.Length > 0)
                {
                    foreach (Exception exception in task.Exceptions)
                    {
                        if (exception is KnownException)
                            knownExceptions.Add((KnownException)exception);
                        else
                            otherExceptions.Add(exception);
                    }
                }

                if (!task.valid || task.IsComplete || (task.Exceptions != null && task.Exceptions.Length > 0))
                {
                    tasksRunning.Remove(task);
                    tasksRunningCount--;
                }
            }

            if (otherExceptions.Count > 0)
                unknownExceptions = new AggregateException(otherExceptions);

            errorCallback(knownExceptions, unknownExceptions);
        }

        /// <summary>
        /// Checks actions queue for any actions sent from tasks to be executed on the main 
        /// thread and executes them.
        /// </summary>
        private void RunTaskActions()
        {
            Action action;

            while (actions.Count > 0)
                if (actions.TryDequeue(out action))
                    action();
        }
    }
}