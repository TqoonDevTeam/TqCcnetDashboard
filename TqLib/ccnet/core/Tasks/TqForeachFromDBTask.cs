using Exortech.NetReflector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;
using ThoughtWorks.CruiseControl.Remote;
using TqLib.ccnet.Core.Util;
using TqLib.zTest.ccnet.Core.Util;

namespace TqLib.ccnet.Core.Tasks
{
    [ReflectorType("TqForeachFromDB")]
    public class TqForeachFromDBTask : TaskContainerBase
    {
        [ReflectorProperty("provider")]
        public string Provider { get; set; }

        [ReflectorProperty("connectionString")]
        public string ConnectionString { get; set; }

        [DataType(DataType.MultilineText)]
        [ReflectorProperty("query")]
        public string Query { get; set; }

        [ReflectorProperty("tasks", Required = false)]
        public override ITask[] Tasks { get { return base.Tasks; } set { base.Tasks = value; } }

        protected override bool Execute(IIntegrationResult result)
        {
            result.BuildProgressInformation.SignalStartRunTask("Executing TqForeachFromDB");

            var loopTarget = GetData();
            int numberOfTasks = Tasks?.Length ?? 0;
            int successCount = 0;
            int failureCount = 0;

            result.BuildProgressInformation.SignalStartRunTask($"Running tasks {numberOfTasks} rows {loopTarget}");

            ITask task = null;
            ApplyDbDataUtil applyDbDataUtil;
            foreach (var data in loopTarget)
            {
                for (int i = 0; i < numberOfTasks; i++)
                {
                    applyDbDataUtil = new ApplyDbDataUtil();

                    try
                    {
                        var taskResult = result.Clone();
                        taskResult.Status = IntegrationStatus.Unknown;

                        task = Tasks[i];

                        applyDbDataUtil.ApplyDbData(task, data);
                        RunTask(task, taskResult, new RunningSubTaskDetails(i, result));

                        result.Merge(taskResult);
                    }
                    catch (Exception ex)
                    {
                        result.ExceptionResult = ex;
                        result.Status = IntegrationStatus.Failure;
                    }
                    finally
                    {
                        if (task != null)
                        {
                            applyDbDataUtil.RestoreDbData(task, data);
                        }
                    }

                    if (result.Status != IntegrationStatus.Success)
                    {
                        failureCount++;
                        break;
                    }
                }
                successCount++;
            }

            // Clean up
            if (numberOfTasks > 0) CancelTasks();

            return failureCount == 0;
        }

        private IList<Dictionary<string, object>> GetData()
        {
            var ado = SpringContextHelper.GetAdoTemplate(Provider, ConnectionString);
            return ado.QueryWithRowMapper(System.Data.CommandType.Text, Query, new StringDictionaryMapper());
        }

        protected override string GetStatusInformation(RunningSubTaskDetails Details)
        {
            string str = (!string.IsNullOrEmpty(base.Description) ? base.Description : string.Format("Running tasks ({0} task(s))", (int)this.Tasks.Length));
            if (Details != null)
            {
                str = string.Concat(str, string.Format(": [{0}] {1}", Details.Index, (!string.IsNullOrEmpty(Details.Information) ? Details.Information : "No information")));
            }
            return str;
        }
    }
}