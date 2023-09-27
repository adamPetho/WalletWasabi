using NBitcoin;
using Nito.AsyncEx.Synchronous;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using WalletWasabi.Logging;

namespace WalletWasabi.WebClients.MempoolSpace;

public static class TransactionFeeFetcher
{
	private static MempoolSpaceApiClient? MempoolSpaceApiClient { get; set; }

	public static void Initialize(MempoolSpaceApiClient mempoolSpaceApiClient)
	{
		MempoolSpaceApiClient = mempoolSpaceApiClient;
	}

	public static bool TryFetchTransactionFee(uint256 txid, [NotNullWhen(true)] out Money? fee)
	{
		MempoolSpaceApiResponseItem response;
		fee = null;

		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(20));
		if (MempoolSpaceApiClient is not null)
		{
			try
			{
				var task = Task.Run(async () => await MempoolSpaceApiClient.GetTransactionInfosAsync(txid, cts.Token).ConfigureAwait(false));

				response = task.WaitAndUnwrapException();

				if (response is not null)
				{
					fee = Money.Satoshis(response.Fee);
					return true;
				}
			}
			catch (Exception ex)
			{
				Logger.LogWarning($"Failed to fetch transaction fee. {ex}");
			}
		}

		return false;
	}
}
