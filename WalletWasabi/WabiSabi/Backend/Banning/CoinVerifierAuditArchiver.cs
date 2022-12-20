using NBitcoin;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WalletWasabi.WabiSabi.Backend.Banning;

public class CoinVerifierAuditArchiver
{
	public CoinVerifierAuditArchiver(string directoryPath)
	{
		BaseDirectoryPath = directoryPath;
	}

	public string BaseDirectoryPath { get; }

	public async Task<string> SaveAuditAsync(CoinVerifyInfo verifyInfo)
	{
		var currentDate = DateTimeOffset.UtcNow;

		VerifierAudit audit = new(
			verifyInfo.Coin.Outpoint,
			verifyInfo.Coin.ScriptPubKey.GetDestinationAddress(Network.Main),
			verifyInfo.Coin.Amount,
			verifyInfo.ApiResponseItem.Cscore_section.Cscore_info.First().Name,
			verifyInfo.ApiResponseItem.Cscore_section.Cscore_info.Select(x => x.Id).ToArray()
			);

		// Use a date format in the file name to let the files be sorted by date by default.
		string fileName = $"VerifierAudits.{currentDate:yyyy.MM}.txt";
		string filePath = Path.Combine(BaseDirectoryPath, fileName);
		IoHelpers.EnsureDirectoryExists(BaseDirectoryPath);

		await File.AppendAllLinesAsync(filePath, new[] { audit.ToLine() }).ConfigureAwait(false);

		return filePath;
	}
}

public class VerifierAudit
{
	public VerifierAudit(OutPoint outPoint, BitcoinAddress address, Money amount, string riskCategory, int[] riskFlagIds)
	{
		OutPoint = outPoint;
		Address = address;
		Amount = amount;
		RiskCategory = riskCategory;
		RiskFlagIds = riskFlagIds;
	}

	public OutPoint OutPoint { get; }
	public BitcoinAddress Address { get; }
	public Money Amount { get; }
	public string RiskCategory { get; }
	public int[] RiskFlagIds { get; }

	public string ToLine()
	{
		return $"{OutPoint}:{Address}:{Amount}:{RiskCategory}:{RiskFlagIds}";
	}
}
