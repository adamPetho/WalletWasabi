using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using WalletWasabi.Logging;

namespace WalletWasabi.Fluent.Helpers
{
	public static class StartupHelper
	{
		private const string KeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
		private static string ArgumentToAddWasabiToMacOsStartupSetting = $"-c \"osascript -e \' tell application \\\"System Events\\\" to make new login item at end of login items with properties {{name:\\\"WasabiWallet\\\", path:\\\"/Applications/WasabiWallet.app\\\",hidden:false}} \' \"";
		private static string ArgumentToDeleteWasabiFromMacOsStartupSetting = $"-c \"osascript -e \' tell application \\\"System Events\\\" to delete login item \\\"WasabiWallet\\\" \' \"";

		public static bool TryModifyStartupSetting(bool runOnSystemStartup)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Assembly assembly = Assembly.GetEntryAssembly() ?? throw new NullReferenceException();
				string pathToExeFile = assembly.Location[..^4] + ".exe";
				if (File.Exists(pathToExeFile))
				{
					return TryModifyRegistry(runOnSystemStartup, pathToExeFile);
				}
				else
				{
					Logger.LogError($"Path {pathToExeFile} does not exist.");
				}
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Method call here
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return TryMacOsModification(runOnSystemStartup);
			}

			return false;
		}

		[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Method can be called only on Windows.")]
		private static bool TryModifyRegistry(bool runOnSystemStartup, string pathToExeFile)
		{
			try
			{
				using RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true) ?? throw new NullReferenceException();
				if (runOnSystemStartup)
				{
					key.SetValue(nameof(WalletWasabi), pathToExeFile);
				}
				else
				{
					key.DeleteValue(nameof(WalletWasabi));
				}

				return true;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}

			return false;
		}

		private static bool TryMacOsModification(bool runOnSystemStartup)
		{
			try
			{
				if (runOnSystemStartup)
				{
					ProcessStartInfo processInfo = new()
					{
						UseShellExecute = true,
						WindowStyle = ProcessWindowStyle.Normal,
						FileName = "/bin/bash",
						Arguments = ArgumentToAddWasabiToMacOsStartupSetting,
						CreateNoWindow = false
					};

					Process.Start(processInfo);
				}
				else
				{
					ProcessStartInfo processInfo = new()
					{
						UseShellExecute = true,
						WindowStyle = ProcessWindowStyle.Normal,
						FileName = "/bin/bash",
						Arguments = ArgumentToDeleteWasabiFromMacOsStartupSetting,
						CreateNoWindow = false
					};

					Process.Start(processInfo);
				}

				return true;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}
			return false;
		}
	}
}
