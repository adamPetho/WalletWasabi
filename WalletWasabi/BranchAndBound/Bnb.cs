using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWasabi.Logging;

namespace WalletWasabi.BranchAndBound
{
	public class Bnb
	{
		private readonly ulong _costOfHeader = 0;
		private readonly ulong _costPerOutput = 0;

		public Bnb(List<ulong> utxos)
		{
			AvaliableUTXOs = utxos;
		}

		private int BnbTryLimit { get; set; } = 10000;
		public List<ulong> AvaliableUTXOs { get; set; }

		public bool TryGetExactMatch(ulong target, out List<ulong> selectedCoins)
		{
			selectedCoins = new List<ulong>();
			try
			{
				selectedCoins = SolveX(depth: 0, currentSelection: new List<ulong>(), effValue: 0, target: target);
				return selectedCoins.Any();
			}
			catch (Exception ex)
			{
				Logger.LogError("Couldn't find the right pair. " + ex);
				return false;
			}
		}

		private List<ulong>? SolveX(int depth, List<ulong> currentSelection, ulong effValue, ulong target)
		{
			ulong targetForMatch = target + _costOfHeader + _costPerOutput;
			ulong matchRange = _costOfHeader + _costPerOutput;
			var utxoSorted = AvaliableUTXOs.OrderByDescending(x => x).ToArray();

			effValue = CalcEffectiveValue(currentSelection);

			BnbTryLimit--;

			if (effValue > targetForMatch + matchRange)
			{
				return null;        // Excessive funds: cut branch
			}
			else if (effValue >= targetForMatch)
			{
				return currentSelection;  // Match!
			}
			else if (BnbTryLimit <= 0)
			{
				return null;        // Search limit reached
			}
			else if (depth >= utxoSorted.Length)
			{
				return null;        // Leaf reached, no match
			}
			else
			{
				var random = new Random();
				var randomBool = random.Next(2) == 1;

				if (randomBool)
				{
					var clonedCurrentSelection = currentSelection.ToList();
					clonedCurrentSelection.Add(utxoSorted[depth]);

					var withThis = SolveX(depth + 1, clonedCurrentSelection, effValue, target);

					if (withThis is not null)
					{
						return withThis;        // Match found
					}
					else
					{
						var withoutThis = SolveX(depth + 1, currentSelection, effValue, target);
						if (withoutThis is not null)
						{
							return withoutThis;         // Match found
						}
					}

					return null;
				}
				else
				{
					var withoutThis = SolveX(depth + 1, currentSelection, effValue, target);
					if (withoutThis is not null)
					{
						return withoutThis;     // Match found
					}
					else
					{
						currentSelection.Add(utxoSorted[depth]);
						var withThis = SolveX(depth + 1, currentSelection, effValue, target);

						if (withThis is not null)
						{
							return withThis;        // Match found
						}
					}

					return null;
				}
			}
		}

		private ulong CalcEffectiveValue(List<ulong> list)
		{
			ulong sum = 0;

			foreach (var item in list)
			{
				sum += item;        // TODO effectiveValue = utxo.value − feePerByte × bytesPerInput
			}

			return sum;
		}
	}
}
