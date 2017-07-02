using Quartz;
using Quartz.Impl;
using System;
using System.Linq;
using TqCcnetDashboard.Config;
using TqCcnetDashboard.Quartz.Job;

namespace TqCcnetDashboard.Quartz
{
    public class QuartzTqoonDevTeamCiServerJobManager
    {
        private static ISchedulerFactory schedFact;
        private static IScheduler sched;
        private static object _lock = new object();

        static QuartzTqoonDevTeamCiServerJobManager()
        {
            schedFact = new StdSchedulerFactory();
            sched = schedFact.GetScheduler();
            if (!sched.IsStarted)
            {
                sched.Start();
            }
        }

        public static void Start()
        {
            string server = ConfigManager.Get("server", "127.0.0.1");
            foreach (var host in server.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
            {
                Add(host);
            }
        }

        public static void TriggerImmediately()
        {
            string server = ConfigManager.Get("server", "127.0.0.1");
            foreach (var host in server.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct())
            {
                sched.TriggerJob(GetJobKey(host));
            }
        }

        public static void Add(string host)
        {
            var jobKey = GetJobKey(host);
            if (!sched.CheckExists(jobKey))
            {
                lock (_lock)
                {
                    if (!sched.CheckExists(jobKey))
                    {
                        IJobDetail job = JobBuilder.Create<CcnetProjectStatusGetter>()
                        .WithIdentity(jobKey)
                        .UsingJobData("HOST", host)
                        .Build();

                        var triggerKey = GetTriggerKey(host);
                        ITrigger trigger = TriggerBuilder.Create()
                            .WithIdentity(triggerKey)
                            .WithSimpleSchedule(t => t.WithIntervalInSeconds(5).RepeatForever())
                            .StartNow()
                            .Build();

                        sched.ScheduleJob(job, trigger);
                    }
                }
            }
        }

        public static void Remove(string host)
        {
            var jobKey = GetJobKey(host);
            var triggerKey = GetTriggerKey(host);
            if (sched.CheckExists(jobKey))
            {
                lock (_lock)
                {
                    if (sched.CheckExists(jobKey))
                    {
                        sched.ResumeJob(jobKey);
                        sched.ResumeTrigger(triggerKey);
                    }
                }
            }
        }

        private static JobKey GetJobKey(string host)
        {
            return new JobKey(host, "TqoonDevTeamCCNETJOB");
        }

        private static TriggerKey GetTriggerKey(string host)
        {
            return new TriggerKey(host, "TqoonDevTeamCCNETJOB");
        }
    }
}