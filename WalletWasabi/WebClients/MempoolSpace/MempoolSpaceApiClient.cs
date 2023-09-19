using NBitcoin;
using WalletWasabi.Tor.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WalletWasabi.Tor.Http.Extensions;
using System.Net.Http;

namespace WalletWasabi.WebClients.MempoolSpace;
public class MempoolSpaceApiClient
{
	public MempoolSpaceApiClient(IHttpClient httpClient, Network network)
	{
		HttpClient = httpClient;
	}

	private IHttpClient HttpClient { get; }
	public async Task<MempoolSpaceApiResponseItem?> GetTransactionInfosAsync(uint256 txid, CancellationToken cancel)
	{
		HttpResponseMessage response;

		// Ensure not being banned by Mempool.space's API
		await Task.Delay(1000, cancel).ConfigureAwait(false);

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/tx/{txid}");
		response = await HttpClient.SendAsync(request, cancel).ConfigureAwait(false);
		
		if (!response.IsSuccessStatusCode)
		{
			// Tx was not found in mempool.space's node.
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				return null;
			}
			throw new InvalidOperationException($"There was an unexpected error with request to mempool.space. {nameof(HttpStatusCode)} was {response?.StatusCode}.");
		}

		return await response.Content.ReadAsJsonAsync<MempoolSpaceApiResponseItem>().ConfigureAwait(false);
	}
}
