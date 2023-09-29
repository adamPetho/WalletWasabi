using NBitcoin;
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

	public static async Task<int?> FetchTransactionFeeAsync(uint256 txid)
	{
		using CancellationTokenSource cts = new(TimeSpan.FromSeconds(20));
		if (MempoolSpaceApiClient is not null)
		{
			try
			{
				var response = await MempoolSpaceApiClient.GetTransactionInfosAsync(txid, cts.Token).ConfigureAwait(false);

				return response.Fee;

			}
			catch (Exception ex)
			{
				Logger.LogWarning($"Failed to fetch transaction fee. {ex}");
			}
		}

		return null;
	}
}
