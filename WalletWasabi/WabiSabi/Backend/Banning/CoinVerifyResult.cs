using NBitcoin;

namespace WalletWasabi.WabiSabi.Backend.Banning;

public record CoinVerifyResult(Coin Coin, bool ShouldBan, bool ShouldRemove, Reason Reason, ApiResponseItem? ApiResponseItem = null);

public enum Reason
{
	Remix,
	Whitelisted,
	OneHop,
	RemoteApiChecked,
	NotChecked,
	Inmature,
	Exception
}
