using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWasabi.BranchAndBound;
using Xunit;

namespace WalletWasabi.Tests.UnitTests
{
	public class BranchAndBoundTests
	{
		[Fact]
		public void BnBSimpleTest()
		{
			var bnb = new Bnb();
			var utxos = new List<Money> { Money.Satoshis(12), Money.Satoshis(10), Money.Satoshis(10), Money.Satoshis(5), Money.Satoshis(4) };
			var expectedCoins = new List<Money> { Money.Satoshis(10), Money.Satoshis(5), Money.Satoshis(4) };
			Money target = Money.Satoshis(19);

			var wasSuccessful = bnb.TryGetExactMatch(target, utxos, out List<Money> selectedCoins);

			Assert.True(wasSuccessful);
			Assert.Equal(expectedCoins, selectedCoins);
		}

		[Fact]
		public void BnBRandomTest()
		{
			var bnb = new Bnb();
			var utxos = GenerateRandomCoinList();
			Money target = Money.Satoshis(100000);

			var successful = bnb.TryGetExactMatch(target, utxos, out List<Money> selectedCoins);

			Assert.True(successful);
		}

		private List<Money> GenerateRandomCoinList()
		{
			Random random = new();
			List<Money> availableCoins = new();
			for (int i = 0; i < 100; i++)
			{
				availableCoins.Add(random.Next((int)Money.Satoshis(250), (int)Money.Satoshis(100001)));
			}
			return availableCoins;
		}
	}
}
