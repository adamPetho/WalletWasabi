using NBitcoin;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WalletWasabi.WabiSabi.Backend.Banning;

public class CoinVerifierAuditArchiver
{
	public CoinVerifierAuditArchiver(string directoryPath)
	{
		IoHelpers.EnsureDirectoryExists(directoryPath);
		BaseDirectoryPath = directoryPath;
	}

	public string BaseDirectoryPath { get; }

	public async Task SaveAuditAsync(CoinVerifyInfo verifyInfo)
	{
		var details = $"{string.Join(',', verifyInfo.ApiResponseItem.Cscore_section.Cscore_info.Select(x => x.Id).ToArray())};" +
			$"{string.Join(',', verifyInfo.ApiResponseItem.Cscore_section.Cscore_info.Select(x => x.Name))}";

		await SaveAuditAsync(verifyInfo.Coin, verifyInfo.ShouldBan, verifyInfo.Reason.ToString(), details).ConfigureAwait(false);
	}

	public async Task SaveAuditAsync(Coin coin, bool isBanned, string reason, string? details = null)
	{
		var currentDate = DateTimeOffset.UtcNow;

		string fileName = $"VerifierAudits.{currentDate:yyyy.MM}.txt";
		string filePath = Path.Combine(BaseDirectoryPath, fileName);
		IoHelpers.EnsureDirectoryExists(BaseDirectoryPath);

		await File.AppendAllLinesAsync(filePath, new[] { $"{DateTimeOffset.UtcNow.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff}:{coin.Outpoint}:{coin.ScriptPubKey.GetDestinationAddress(Network.Main)}:{isBanned}:{coin.Amount}:{reason}:{details ?? ""}" }).ConfigureAwait(false);
	}
}
