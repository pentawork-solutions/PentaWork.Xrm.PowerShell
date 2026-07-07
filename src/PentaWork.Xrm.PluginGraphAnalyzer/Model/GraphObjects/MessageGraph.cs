using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    public class MessageGraph
    {
        public MessageGraph(string message)
        {
            Message = message;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            if (!Stages.ContainsKey(pluginStepInfo.Stage))
            {
                Stages.Add(pluginStepInfo.Stage, new List<PluginStepInfo>());
            }
            Stages[pluginStepInfo.Stage].Add(pluginStepInfo);
        }

        public string Message { get; }
        public Dictionary<Stage, List<PluginStepInfo>> Stages { get; } = new();

        /// <summary>
        /// MainTemplate.tt groups steps by stage, then sync/async, then rank - a Scriban template
        /// can't express that grouping itself, so precompute it here into flat, template-friendly lists.
        /// </summary>
        public List<StageGraph> StageGraphs => Stages
            .Select(kv => new StageGraph(kv.Key, kv.Value))
            .ToList();
    }

    public class StageGraph
    {
        public StageGraph(Stage stage, List<PluginStepInfo> steps)
        {
            StageName = stage.ToString();
            Steps = steps;
            ModeGroups = steps
                .GroupBy(s => s.Async)
                .Select(modeGroup => new ModeGroup(
                    modeGroup.Key,
                    modeGroup
                        .GroupBy(s => s.Rank)
                        .OrderBy(rankGroup => rankGroup.Key)
                        .Select(rankGroup => new RankGroup(rankGroup.Key, rankGroup.ToList()))
                        .ToList()))
                .ToList();
        }

        public string StageName { get; }
        public List<PluginStepInfo> Steps { get; }
        public List<ModeGroup> ModeGroups { get; }
    }

    public class ModeGroup
    {
        public ModeGroup(bool async, List<RankGroup> rankGroups)
        {
            ModeLabel = async ? "Async" : "Sync";
            RankGroups = rankGroups;
        }

        public string ModeLabel { get; }
        public List<RankGroup> RankGroups { get; }
    }

    public class RankGroup
    {
        public RankGroup(int? rank, List<PluginStepInfo> steps)
        {
            RankLabel = rank?.ToString() ?? "(default)";
            Steps = steps;
        }

        public string RankLabel { get; }
        public List<PluginStepInfo> Steps { get; }
    }
}
