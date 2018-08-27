﻿using Newtonsoft.Json;
using Paillave.Etl.ExecutionPlan;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public static partial class ExecutionStatusEx
    {
        public static PlotlySankeyDescription GetActualExecutionPlanPlotlySankey(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
            var links = executionStatus.JobDefinitionStructure.StreamToNodeLinks.GroupJoin(
                    executionStatus.StreamStatisticCounters,
                    i => i.SourceNodeName,
                    i => i.SourceNodeName,
                    (link, stat) => new
                    {
                        source = nameToIdDictionary[link.SourceNodeName],
                        target = nameToIdDictionary[link.TargetNodeName],
                        value = stat.DefaultIfEmpty(new StreamStatisticCounter { Counter = 0 }).Sum(i => i.Counter)
                    }
                ).ToList();
            return new PlotlySankeyDescription
            {
                NodeColors = executionStatus.JobDefinitionStructure.Nodes.OrderBy(i => nameToIdDictionary[i.Name]).Select(i =>
                {
                    if (executionStatus.StreamStatisticErrors.Any(e => e.NodeName == i.Name)) return "red";
                    return "blue";
                }).ToList(),
                NodeNames = executionStatus.JobDefinitionStructure.Nodes.OrderBy(i => nameToIdDictionary[i.Name]).Select(i => i.Name).ToList(),
                LinkSources = links.Select(i => i.source).ToList(),
                LinkTargets = links.Select(i => i.target).ToList(),
                LinkValues = links.Select(i => i.value).ToList()
            };
        }
        //public static async Task<string> GetJsonActualExecutionPlanPlotlySankeyAsync<T>(this ExecutionContext<T> executionStatus)
        //{
        //    return JsonConvert.SerializeObject(await executionStatus.GetActualExecutionPlanPlotlySankeyAsync());
        //}
        public static string GetActualExecutionPlanHtmlPlotlySankey(this ExecutionStatus executionStatus)
        {
            var stats = executionStatus.GetActualExecutionPlanPlotlySankey();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionPlan.ActualExecutionPlan.PlotySankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<NODE_NAMES>>'", JsonConvert.SerializeObject(stats.NodeNames));
            html = html.Replace("'<<NODE_COLORS>>'", JsonConvert.SerializeObject(stats.NodeColors));
            html = html.Replace("'<<LINK_SOURCES>>'", JsonConvert.SerializeObject(stats.LinkSources));
            html = html.Replace("'<<LINK_TARGETS>>'", JsonConvert.SerializeObject(stats.LinkTargets));
            html = html.Replace("'<<LINK_VALUES>>'", JsonConvert.SerializeObject(stats.LinkValues));
            return html;
        }
        public static void OpenActualExecutionPlanPlotlySankey(this ExecutionStatus executionStatus)
        {
            Tools.OpenFile(executionStatus.GetActualExecutionPlanHtmlPlotlySankey(), "html");
        }
    }
}
