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
			var utxos = new List<ulong> { 10, 5, 8, 11, 2 };
			ulong target = 19;
			var tryLimit = 100000;

			var bnb = new Bnb(tryLimit, utxos);

			var wasSuccessful = bnb.TryGetExactMatch(target, out List<ulong> solution);

			Assert.True(wasSuccessful);
		}

		[Fact]
		public void BnBSimpleWithMoreUTXOTest()
		{
			var utxos = new List<ulong> { 1, 1, 2, 5, 10, 2, 5, 3, 11, 7, 4, 3, 8, 9 };
			ulong target = 33;
			var tryLimit = 100000;
			var bnb = new Bnb(tryLimit, utxos);

			List<ulong> solution = new();

			var wasSuccessful = bnb.TryGetExactMatch(target, out solution);

			Assert.True(wasSuccessful);
		}
	}
}
