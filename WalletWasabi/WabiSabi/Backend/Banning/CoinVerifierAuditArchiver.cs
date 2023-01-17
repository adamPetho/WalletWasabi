using NBitcoin;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WalletWasabi.WabiSabi.Backend.Banning;

public class CoinVerifierAuditArchiver
{
	public CoinVerifierAuditArchiver(string directoryPath)
	{
		IoHelpers.EnsureDirectoryExists(directoryPath);
		BaseDirectoryPath = directoryPath;
	}

	private string BaseDirectoryPath { get; }

	private AsyncLock FileAsyncLock { get; } = new();

	public async Task SaveAuditAsync(List<CoinVerifyInfo> checkedCoins, IEnumerable<Coin> missingCoins, IEnumerable<Coin> zeroCoordFeePayingCoins, Exception? exception, CancellationToken cancellationToken)
	{
		StringBuilder fileContent = new();

		foreach (var checkedCoinInfo in checkedCoins)
		{
			fileContent.AppendLine(ToLine(checkedCoinInfo));
		}

		foreach (var coin in missingCoins)
		{
			fileContent.AppendLine(ToLine(coin, isBanned: false, reason: exception is null ? "Timeout" : "Error", exception is null ? "" : exception.Message));
		}

		foreach (var zeroCoordFeeCoin in zeroCoordFeePayingCoins)
		{
			fileContent.AppendLine(ToLine(zeroCoordFeeCoin, isBanned: false, "ZeroCoordFee"));
		}

		await SaveToFileAsync(fileContent.ToString(), cancellationToken).ConfigureAwait(false);
	}

	private string ToLine(CoinVerifyInfo verifyInfo)
	{
		var reportId = verifyInfo.ApiResponseItem?.Report_info_section.Report_id;
		var ids = verifyInfo.ApiResponseItem?.Cscore_section.Cscore_info?.Select(x => x.Id);
		var categories = verifyInfo.ApiResponseItem?.Cscore_section.Cscore_info.Select(x => x.Name);

		var details = $"{reportId ?? "ReportID None"}:{(ids is null ? "None" : string.Join(',', ids))}:{(categories is null ? "None" : string.Join(',', categories))}";

		return ToLine(verifyInfo.Coin, verifyInfo.ShouldBan, verifyInfo.Reason.ToString(), details);
	}

	private string ToLine(Coin coin, bool isBanned, string reason, string details = "None")
	{
		return $"{DateTimeOffset.UtcNow.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff},{coin.Outpoint},{coin.ScriptPubKey.GetDestinationAddress(Network.Main)},{isBanned},{coin.Amount},{reason},{details}";
	}

	private async Task SaveToFileAsync(string fileContent, CancellationToken cancellationToken)
	{
		var currentDate = DateTimeOffset.UtcNow;

		string fileName = $"VerifierAudits.{currentDate:yyyy.MM}.txt";
		string filePath = Path.Combine(BaseDirectoryPath, fileName);

		using (await FileAsyncLock.LockAsync(cancellationToken))
		{
			await File.AppendAllLinesAsync(filePath, new[] { fileContent }, cancellationToken).ConfigureAwait(false);
		}
	}
}
