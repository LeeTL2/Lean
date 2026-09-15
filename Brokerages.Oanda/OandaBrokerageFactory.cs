using QuantConnect.Brokerages;
using QuantConnect.Brokerages.Oanda;
using QuantConnect.Interfaces;
using QuantConnect.Data;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using QuantConnect.Configuration;

namespace QuantConnect.Brokerages.Oanda;

public class OandaBrokerageFactory : BrokerageFactory
{
    public OandaBrokerageFactory()
        : base(typeof(OandaBrokerage))
    {
    }

    public override void Dispose()
    {
    }

    public override Dictionary<string, string> BrokerageData
    {
        get
        {
            return new Dictionary<string, string>
            {
                { "oanda-access-token", System.Environment.GetEnvironmentVariable("OANDA_ACCESS_TOKEN") ?? Config.Get("oanda-access-token") },
                { "oanda-account-id", System.Environment.GetEnvironmentVariable("OANDA_ACCOUNT_ID") ?? Config.Get("oanda-account-id") },
                { "oanda-agent", Config.Get("oanda-agent", OandaRestApiBase.OandaAgentDefaultValue) }
            };
        }
    }

    public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider)
    {
        return new OandaBrokerageModel();
    }

    public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
    {
        var errors = new List<string>();

        var accessToken = Read<string>(
            job.BrokerageData,
            "oanda-access-token",
            errors);

        var accountId = Read<string>(
            job.BrokerageData,
            "oanda-account-id",
            errors);

        var agent = Read<string>(
            job.BrokerageData,
            "oanda-agent",
            errors);

        if (errors.Count != 0)
        {
            throw new Exception(string.Join(
                System.Environment.NewLine,
                errors));
        }

        var brokerage = new OandaBrokerage(
            algorithm.Transactions,
            algorithm.Portfolio,
            Composer.Instance.GetExportedValueByTypeName<IDataAggregator>(
                Config.Get(
                    "data-aggregator",
                    "QuantConnect.Lean.Engine.DataFeeds.AggregationManager"),
                forceTypeNameOnExisting: false),
            accessToken,
            accountId,
            agent);

        Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

        return brokerage;
    }
}
