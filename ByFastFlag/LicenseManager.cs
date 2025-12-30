using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ByFastFlag
{
	// Token: 0x02000006 RID: 6
	[NullableContext(1)]
	[Nullable(0)]
	public class LicenseManager
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002BEC File Offset: 0x00000DEC
		static LicenseManager()
		{
			Task.Run(delegate()
			{
				LicenseManager.<>c.<<-cctor>b__17_0>d <<-cctor>b__17_0>d = new LicenseManager.<>c.<<-cctor>b__17_0>d();
				<<-cctor>b__17_0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
				<<-cctor>b__17_0>d.<>4__this = LicenseManager.<>c.<>9;
				<<-cctor>b__17_0>d.<>1__state = -1;
				<<-cctor>b__17_0>d.<>t__builder.Start<LicenseManager.<>c.<<-cctor>b__17_0>d>(ref <<-cctor>b__17_0>d);
				return <<-cctor>b__17_0>d.<>t__builder.Task;
			});
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002D08 File Offset: 0x00000F08
		private static void StartBackgroundRefreshTimer()
		{
			Task.Run(delegate()
			{
				LicenseManager.<>c.<<StartBackgroundRefreshTimer>b__18_0>d <<StartBackgroundRefreshTimer>b__18_0>d = new LicenseManager.<>c.<<StartBackgroundRefreshTimer>b__18_0>d();
				<<StartBackgroundRefreshTimer>b__18_0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
				<<StartBackgroundRefreshTimer>b__18_0>d.<>4__this = LicenseManager.<>c.<>9;
				<<StartBackgroundRefreshTimer>b__18_0>d.<>1__state = -1;
				<<StartBackgroundRefreshTimer>b__18_0>d.<>t__builder.Start<LicenseManager.<>c.<<StartBackgroundRefreshTimer>b__18_0>d>(ref <<StartBackgroundRefreshTimer>b__18_0>d);
				return <<StartBackgroundRefreshTimer>b__18_0>d.<>t__builder.Task;
			});
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002D30 File Offset: 0x00000F30
		[DebuggerStepThrough]
		private static Task<string> GetCurrentGistUrlAsync()
		{
			LicenseManager.<GetCurrentGistUrlAsync>d__19 <GetCurrentGistUrlAsync>d__ = new LicenseManager.<GetCurrentGistUrlAsync>d__19();
			<GetCurrentGistUrlAsync>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
			<GetCurrentGistUrlAsync>d__.<>1__state = -1;
			<GetCurrentGistUrlAsync>d__.<>t__builder.Start<LicenseManager.<GetCurrentGistUrlAsync>d__19>(ref <GetCurrentGistUrlAsync>d__);
			return <GetCurrentGistUrlAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002D70 File Offset: 0x00000F70
		[DebuggerStepThrough]
		private static Task<List<string>> LoadGlobalRevokedKeysAsync()
		{
			LicenseManager.<LoadGlobalRevokedKeysAsync>d__20 <LoadGlobalRevokedKeysAsync>d__ = new LicenseManager.<LoadGlobalRevokedKeysAsync>d__20();
			<LoadGlobalRevokedKeysAsync>d__.<>t__builder = AsyncTaskMethodBuilder<List<string>>.Create();
			<LoadGlobalRevokedKeysAsync>d__.<>1__state = -1;
			<LoadGlobalRevokedKeysAsync>d__.<>t__builder.Start<LicenseManager.<LoadGlobalRevokedKeysAsync>d__20>(ref <LoadGlobalRevokedKeysAsync>d__);
			return <LoadGlobalRevokedKeysAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002DB0 File Offset: 0x00000FB0
		private static List<string> ParseRevokedKeys(string jsonResponse)
		{
			List<string> result;
			try
			{
				Console.WriteLine("[LicenseManager] Parsing JSON response...");
				JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse, default(JsonDocumentOptions));
				List<string> keys = new List<string>();
				JsonElement keysElement;
				bool flag = jsonDoc.RootElement.TryGetProperty("revoked_keys", out keysElement);
				if (flag)
				{
					Console.WriteLine("[LicenseManager] Found 'revoked_keys' property");
					keys = (from x in keysElement.EnumerateArray()
					select x.GetString() ?? "" into x
					where !string.IsNullOrEmpty(x)
					select x).ToList<string>();
				}
				else
				{
					bool flag2 = jsonDoc.RootElement.ValueKind == JsonValueKind.Array;
					if (flag2)
					{
						Console.WriteLine("[LicenseManager] Found direct array");
						keys = (from x in jsonDoc.RootElement.EnumerateArray()
						select x.GetString() ?? "" into x
						where !string.IsNullOrEmpty(x)
						select x).ToList<string>();
					}
					else
					{
						JsonElement altKeysElement;
						bool flag3 = jsonDoc.RootElement.TryGetProperty("keys", out altKeysElement);
						if (flag3)
						{
							Console.WriteLine("[LicenseManager] Found 'keys' property");
							keys = (from x in altKeysElement.EnumerateArray()
							select x.GetString() ?? "" into x
							where !string.IsNullOrEmpty(x)
							select x).ToList<string>();
						}
						else
						{
							Console.WriteLine("[LicenseManager] WARNING: Unknown JSON format");
							Console.WriteLine("[LicenseManager] JSON starts with: " + jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length)));
						}
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[LicenseManager] Parsed ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(keys.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" revoked keys");
				Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
				bool flag4 = keys.Count > 0;
				if (flag4)
				{
					Console.WriteLine("[LicenseManager] First 3 keys: " + string.Join(", ", keys.Take(3)));
				}
				result = keys;
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Error parsing JSON: " + ex.Message);
				Console.WriteLine("[LicenseManager] JSON that failed: " + jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length)));
				result = new List<string>();
			}
			return result;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000308C File Offset: 0x0000128C
		[DebuggerStepThrough]
		private static Task SaveToCacheAsync(List<string> keys)
		{
			LicenseManager.<SaveToCacheAsync>d__22 <SaveToCacheAsync>d__ = new LicenseManager.<SaveToCacheAsync>d__22();
			<SaveToCacheAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<SaveToCacheAsync>d__.keys = keys;
			<SaveToCacheAsync>d__.<>1__state = -1;
			<SaveToCacheAsync>d__.<>t__builder.Start<LicenseManager.<SaveToCacheAsync>d__22>(ref <SaveToCacheAsync>d__);
			return <SaveToCacheAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000030D0 File Offset: 0x000012D0
		[DebuggerStepThrough]
		private static Task<List<string>> LoadFromCacheAsync()
		{
			LicenseManager.<LoadFromCacheAsync>d__23 <LoadFromCacheAsync>d__ = new LicenseManager.<LoadFromCacheAsync>d__23();
			<LoadFromCacheAsync>d__.<>t__builder = AsyncTaskMethodBuilder<List<string>>.Create();
			<LoadFromCacheAsync>d__.<>1__state = -1;
			<LoadFromCacheAsync>d__.<>t__builder.Start<LicenseManager.<LoadFromCacheAsync>d__23>(ref <LoadFromCacheAsync>d__);
			return <LoadFromCacheAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003110 File Offset: 0x00001310
		[DebuggerStepThrough]
		private static Task LoadRevokedKeysAsync()
		{
			LicenseManager.<LoadRevokedKeysAsync>d__24 <LoadRevokedKeysAsync>d__ = new LicenseManager.<LoadRevokedKeysAsync>d__24();
			<LoadRevokedKeysAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<LoadRevokedKeysAsync>d__.<>1__state = -1;
			<LoadRevokedKeysAsync>d__.<>t__builder.Start<LicenseManager.<LoadRevokedKeysAsync>d__24>(ref <LoadRevokedKeysAsync>d__);
			return <LoadRevokedKeysAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00003150 File Offset: 0x00001350
		private static void LoadRevokedKeys()
		{
			try
			{
				Task task = Task.Run(delegate()
				{
					LicenseManager.<>c.<<LoadRevokedKeys>b__25_0>d <<LoadRevokedKeys>b__25_0>d = new LicenseManager.<>c.<<LoadRevokedKeys>b__25_0>d();
					<<LoadRevokedKeys>b__25_0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
					<<LoadRevokedKeys>b__25_0>d.<>4__this = LicenseManager.<>c.<>9;
					<<LoadRevokedKeys>b__25_0>d.<>1__state = -1;
					<<LoadRevokedKeys>b__25_0>d.<>t__builder.Start<LicenseManager.<>c.<<LoadRevokedKeys>b__25_0>d>(ref <<LoadRevokedKeys>b__25_0>d);
					return <<LoadRevokedKeys>b__25_0>d.<>t__builder.Task;
				});
				task.Wait(TimeSpan.FromSeconds(5.0));
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Sync load error: " + ex.Message);
				LicenseManager._revokedKeys = new HashSet<string>();
				LicenseManager._lastRevokedKeysLoadTime = DateTime.Now;
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000031DC File Offset: 0x000013DC
		public static bool IsKeyRevoked(string key)
		{
			bool result;
			try
			{
				bool flag = LicenseManager._revokedKeys == null;
				if (flag)
				{
					LicenseManager.LoadRevokedKeys();
				}
				else
				{
					bool flag2 = LicenseManager._revokedKeys.Count == 0;
					if (flag2)
					{
						LicenseManager._lastRevokedKeysLoadTime = DateTime.MinValue;
						Task.Run(delegate()
						{
							LicenseManager.<>c.<<IsKeyRevoked>b__26_0>d <<IsKeyRevoked>b__26_0>d = new LicenseManager.<>c.<<IsKeyRevoked>b__26_0>d();
							<<IsKeyRevoked>b__26_0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
							<<IsKeyRevoked>b__26_0>d.<>4__this = LicenseManager.<>c.<>9;
							<<IsKeyRevoked>b__26_0>d.<>1__state = -1;
							<<IsKeyRevoked>b__26_0>d.<>t__builder.Start<LicenseManager.<>c.<<IsKeyRevoked>b__26_0>d>(ref <<IsKeyRevoked>b__26_0>d);
							return <<IsKeyRevoked>b__26_0>d.<>t__builder.Task;
						});
					}
				}
				bool flag3 = string.IsNullOrEmpty(key);
				if (flag3)
				{
					result = false;
				}
				else
				{
					string cleanKey = key.Replace("-", "").ToUpperInvariant();
					HashSet<string> revokedKeys = LicenseManager._revokedKeys;
					bool isRevoked = revokedKeys != null && revokedKeys.Contains(cleanKey);
					bool flag4 = isRevoked;
					if (flag4)
					{
						Console.WriteLine("[LicenseManager] ⚠️ KEY REVOKED: " + cleanKey.Substring(0, Math.Min(8, cleanKey.Length)) + "...");
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[LicenseManager] Total revoked keys in memory: ");
						HashSet<string> revokedKeys2 = LicenseManager._revokedKeys;
						defaultInterpolatedStringHandler.AppendFormatted<int>((revokedKeys2 != null) ? revokedKeys2.Count : 0);
						Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
						Application application = Application.Current;
						if (application != null)
						{
							Dispatcher dispatcher = application.Dispatcher;
							if (dispatcher != null)
							{
								dispatcher.Invoke(delegate()
								{
									MessageBox.Show("⚠️ This license has been REVOKED!\n\nYou cannot use this license anymore.\n\nPlease contact support for a new license.", "License Revoked", MessageBoxButton.OK, MessageBoxImage.Exclamation);
								});
							}
						}
					}
					result = isRevoked;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Error checking revocation: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003370 File Offset: 0x00001570
		public static bool IsLicenseValid()
		{
			bool result;
			try
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[LicenseManager] === LICENSE CHECK [");
				defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now, "HH:mm:ss");
				defaultInterpolatedStringHandler.AppendLiteral("] ===");
				Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
				bool flag = !File.Exists(LicenseManager.LicenseFilePath);
				if (flag)
				{
					Console.WriteLine("[LicenseManager] No license file");
					result = false;
				}
				else
				{
					LicenseManager.LicenseInfo license = LicenseManager.LoadLicense();
					bool flag2 = license == null;
					if (flag2)
					{
						Console.WriteLine("[LicenseManager] Failed to load license");
						result = false;
					}
					else
					{
						Console.WriteLine("[LicenseManager] Checking key: " + license.Key);
						bool flag3 = LicenseManager.IsKeyRevoked(license.Key);
						if (flag3)
						{
							Console.WriteLine("[LicenseManager] ⚠️ LICENSE REVOKED!");
							result = false;
						}
						else
						{
							LicenseManager.LicenseStatus status = LicenseManager.ValidateLicense(license);
							bool isValid = status == LicenseManager.LicenseStatus.Valid;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(29, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("[LicenseManager] Status: ");
							defaultInterpolatedStringHandler2.AppendFormatted<LicenseManager.LicenseStatus>(status);
							defaultInterpolatedStringHandler2.AppendLiteral(" -> ");
							defaultInterpolatedStringHandler2.AppendFormatted(isValid ? "VALID" : "INVALID");
							Console.WriteLine(defaultInterpolatedStringHandler2.ToStringAndClear());
							result = isValid;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Error in IsLicenseValid: " + ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000034E8 File Offset: 0x000016E8
		[NullableContext(2)]
		public static LicenseManager.LicenseInfo LoadLicense()
		{
			LicenseManager.LicenseInfo result;
			try
			{
				bool flag = !File.Exists(LicenseManager.LicenseFilePath);
				if (flag)
				{
					result = null;
				}
				else
				{
					string encryptedData = File.ReadAllText(LicenseManager.LicenseFilePath);
					bool flag2 = string.IsNullOrWhiteSpace(encryptedData);
					if (flag2)
					{
						result = null;
					}
					else
					{
						string jsonData = LicenseManager.Decrypt(encryptedData);
						bool flag3 = string.IsNullOrWhiteSpace(jsonData);
						if (flag3)
						{
							result = null;
						}
						else
						{
							result = JsonSerializer.Deserialize<LicenseManager.LicenseInfo>(jsonData, null);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Error loading license: " + ex.Message);
				result = null;
			}
			return result;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000357C File Offset: 0x0000177C
		public static bool SaveLicense(LicenseManager.LicenseInfo license)
		{
			bool result;
			try
			{
				bool flag = LicenseManager.IsKeyRevoked(license.Key);
				if (flag)
				{
					MessageBox.Show("This license key has been REVOKED and is no longer valid.", "Activation Failed", MessageBoxButton.OK, MessageBoxImage.Hand);
					result = false;
				}
				else
				{
					string jsonData = JsonSerializer.Serialize<LicenseManager.LicenseInfo>(license, null);
					string encryptedData = LicenseManager.Encrypt(jsonData);
					bool flag2 = !Directory.Exists(LicenseManager.LicenseDirectory);
					if (flag2)
					{
						Directory.CreateDirectory(LicenseManager.LicenseDirectory);
					}
					File.WriteAllText(LicenseManager.LicenseFilePath, encryptedData);
					Console.WriteLine("[LicenseManager] License saved");
					result = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to save license: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				result = false;
			}
			return result;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003634 File Offset: 0x00001834
		public static LicenseManager.LicenseStatus ValidateLicense(LicenseManager.LicenseInfo license)
		{
			bool flag = string.IsNullOrEmpty(license.Key);
			LicenseManager.LicenseStatus result;
			if (flag)
			{
				result = LicenseManager.LicenseStatus.Invalid;
			}
			else
			{
				bool flag2 = LicenseManager.IsKeyRevoked(license.Key);
				if (flag2)
				{
					result = LicenseManager.LicenseStatus.Revoked;
				}
				else
				{
					bool flag3 = !LicenseManager.ValidateKeyFormat(license.Key);
					if (flag3)
					{
						result = LicenseManager.LicenseStatus.Invalid;
					}
					else
					{
						bool flag4 = !LicenseManager.IsKeySignatureValid(license.Key);
						if (flag4)
						{
							result = LicenseManager.LicenseStatus.Invalid;
						}
						else
						{
							string currentMachineId = LicenseManager.GetMachineId();
							bool flag5 = license.MachineId != currentMachineId;
							if (flag5)
							{
								result = LicenseManager.LicenseStatus.MaxActivationsReached;
							}
							else
							{
								result = LicenseManager.LicenseStatus.Valid;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000036C0 File Offset: 0x000018C0
		public static bool ActivateLicense(string key)
		{
			bool result;
			try
			{
				Console.WriteLine("[LicenseManager] Activating key: " + key);
				bool flag = LicenseManager.IsKeyRevoked(key);
				if (flag)
				{
					MessageBox.Show("This license key has been REVOKED and is no longer valid.", "Activation Failed", MessageBoxButton.OK, MessageBoxImage.Hand);
					result = false;
				}
				else
				{
					bool flag2 = !LicenseManager.ValidateKeyFormat(key);
					if (flag2)
					{
						MessageBox.Show("Invalid license key format.", "Activation Failed", MessageBoxButton.OK, MessageBoxImage.Hand);
						result = false;
					}
					else
					{
						bool flag3 = !LicenseManager.IsKeySignatureValid(key);
						if (flag3)
						{
							MessageBox.Show("Invalid license key. Please check the key and try again.", "Activation Failed", MessageBoxButton.OK, MessageBoxImage.Hand);
							result = false;
						}
						else
						{
							string cleanKey = key.Replace("-", "", StringComparison.Ordinal);
							string licenseType = cleanKey.StartsWith("BYFAST") ? "standard" : "premium";
							LicenseManager.LicenseInfo license = new LicenseManager.LicenseInfo
							{
								Key = key,
								ActivationDate = DateTime.Now,
								MachineId = LicenseManager.GetMachineId(),
								CustomerName = "Customer",
								LicenseType = licenseType,
								AppVersion = "1.0"
							};
							Console.WriteLine("[LicenseManager] Creating " + licenseType + " license");
							result = LicenseManager.SaveLicense(license);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Activation error: " + ex.Message);
				MessageBox.Show("Activation failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				result = false;
			}
			return result;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x0000384C File Offset: 0x00001A4C
		private static bool ValidateKeyFormat(string key)
		{
			bool flag = string.IsNullOrEmpty(key);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				string pattern = "^[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}$";
				result = Regex.IsMatch(key, pattern);
			}
			return result;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000387C File Offset: 0x00001A7C
		private static bool IsKeySignatureValid(string key)
		{
			bool result;
			try
			{
				string cleanKey = key.Replace("-", "", StringComparison.Ordinal);
				bool flag = !cleanKey.StartsWith("BYFAST") && !cleanKey.StartsWith("PREMIUM");
				if (flag)
				{
					result = false;
				}
				else
				{
					result = LicenseManager.ValidateChecksum(cleanKey);
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000038E4 File Offset: 0x00001AE4
		private static bool ValidateChecksum(string key)
		{
			bool flag = key.Length != 25;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				int sum = 0;
				foreach (char c in key)
				{
					bool flag2 = char.IsDigit(c);
					if (flag2)
					{
						sum += int.Parse(c.ToString(), CultureInfo.InvariantCulture);
					}
					else
					{
						bool flag3 = char.IsLetter(c);
						if (flag3)
						{
							sum += (int)(char.ToUpperInvariant(c) - 'A' + '\u0001');
						}
					}
				}
				result = (sum % 7 == 0);
			}
			return result;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003978 File Offset: 0x00001B78
		private static string GetMachineId()
		{
			string result;
			try
			{
				bool flag = File.Exists(LicenseManager.MachineIdFilePath);
				if (flag)
				{
					result = File.ReadAllText(LicenseManager.MachineIdFilePath);
				}
				else
				{
					string generatedId = LicenseManager.GenerateMachineId();
					bool flag2 = !Directory.Exists(LicenseManager.LicenseDirectory);
					if (flag2)
					{
						Directory.CreateDirectory(LicenseManager.LicenseDirectory);
					}
					File.WriteAllText(LicenseManager.MachineIdFilePath, generatedId);
					result = generatedId;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[LicenseManager] Error getting machine ID: " + ex.Message);
				result = "MACHINE-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
			}
			return result;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003A30 File Offset: 0x00001C30
		private static string GenerateMachineId()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
			defaultInterpolatedStringHandler.AppendFormatted(Environment.MachineName);
			defaultInterpolatedStringHandler.AppendLiteral("-");
			defaultInterpolatedStringHandler.AppendFormatted(Environment.UserName);
			defaultInterpolatedStringHandler.AppendLiteral("-");
			defaultInterpolatedStringHandler.AppendFormatted<OperatingSystem>(Environment.OSVersion);
			string combined = defaultInterpolatedStringHandler.ToStringAndClear();
			byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
			return BitConverter.ToString(hash).Replace("-", "", StringComparison.Ordinal).Substring(0, 16).ToUpperInvariant();
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003AC8 File Offset: 0x00001CC8
		private static string Encrypt(string plainText)
		{
			string result;
			using (Aes aes = Aes.Create())
			{
				Rfc2898DeriveBytes key = new Rfc2898DeriveBytes("ByFastFlag-SecureKey-2024-ChangeThis!@#$", LicenseManager.Salt, 1000, HashAlgorithmName.SHA256);
				aes.Key = key.GetBytes(32);
				aes.IV = key.GetBytes(16);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
					{
						byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
						cs.Write(plainBytes, 0, plainBytes.Length);
						cs.FlushFinalBlock();
					}
					result = Convert.ToBase64String(ms.ToArray());
				}
			}
			return result;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003BA8 File Offset: 0x00001DA8
		private static string Decrypt(string cipherText)
		{
			string result;
			try
			{
				using (Aes aes = Aes.Create())
				{
					Rfc2898DeriveBytes key = new Rfc2898DeriveBytes("ByFastFlag-SecureKey-2024-ChangeThis!@#$", LicenseManager.Salt, 1000, HashAlgorithmName.SHA256);
					aes.Key = key.GetBytes(32);
					aes.IV = key.GetBytes(16);
					using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
					{
						using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
						{
							using (StreamReader sr = new StreamReader(cs))
							{
								result = sr.ReadToEnd();
							}
						}
					}
				}
			}
			catch
			{
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003CA0 File Offset: 0x00001EA0
		public static string GetLicenseInfo()
		{
			LicenseManager.LicenseInfo license = LicenseManager.LoadLicense();
			bool flag = license == null;
			string result;
			if (flag)
			{
				result = "No license found";
			}
			else
			{
				bool isRevoked = LicenseManager.IsKeyRevoked(license.Key);
				string status = isRevoked ? "REVOKED ⚠️" : "Valid";
				string warning = isRevoked ? "\n\n⚠️ WARNING: This license has been REVOKED!\nPlease contact support for a new license." : "";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Status: ");
				defaultInterpolatedStringHandler.AppendFormatted(status);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("Key: ");
				defaultInterpolatedStringHandler.AppendFormatted(license.Key);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("Type: ");
				defaultInterpolatedStringHandler.AppendFormatted(license.LicenseType);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("Activated: ");
				defaultInterpolatedStringHandler.AppendFormatted<DateTime>(license.ActivationDate, "yyyy-MM-dd");
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendLiteral("Machine ID: ");
				defaultInterpolatedStringHandler.AppendFormatted(license.MachineId);
				result = defaultInterpolatedStringHandler.ToStringAndClear() + warning;
			}
			return result;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003DD0 File Offset: 0x00001FD0
		public static void DeleteLicense()
		{
			try
			{
				bool flag = File.Exists(LicenseManager.LicenseFilePath);
				if (flag)
				{
					File.Delete(LicenseManager.LicenseFilePath);
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003E14 File Offset: 0x00002014
		public static void ForceReloadRevokedKeys()
		{
			LicenseManager._lastRevokedKeysLoadTime = DateTime.MinValue;
			LicenseManager.LoadRevokedKeys();
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003E28 File Offset: 0x00002028
		[DebuggerStepThrough]
		public static Task TestGlobalConnection()
		{
			LicenseManager.<TestGlobalConnection>d__42 <TestGlobalConnection>d__ = new LicenseManager.<TestGlobalConnection>d__42();
			<TestGlobalConnection>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<TestGlobalConnection>d__.<>1__state = -1;
			<TestGlobalConnection>d__.<>t__builder.Start<LicenseManager.<TestGlobalConnection>d__42>(ref <TestGlobalConnection>d__);
			return <TestGlobalConnection>d__.<>t__builder.Task;
		}

		// Token: 0x0400000C RID: 12
		private static readonly string LicenseFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ByFastFlag", "license.dat");

		// Token: 0x0400000D RID: 13
		private static readonly string LicenseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ByFastFlag");

		// Token: 0x0400000E RID: 14
		private static readonly string MachineIdFilePath = Path.Combine(LicenseManager.LicenseDirectory, "machine.id");

		// Token: 0x0400000F RID: 15
		private static readonly string RevokedKeysFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "revoked_keys.json");

		// Token: 0x04000010 RID: 16
		private static readonly string AppDataRevokedKeysFilePath = Path.Combine(LicenseManager.LicenseDirectory, "revoked_keys.json");

		// Token: 0x04000011 RID: 17
		private static readonly string GlobalConfigUrl = "https://xSaraap.github.io/byfastflag-config/config.json";

		// Token: 0x04000012 RID: 18
		private static readonly string FallbackGistUrl = "https://gist.githubusercontent.com/xSaraap/9169c22bd7959cc9f824955a78b9498a/raw/byfastflag_revoked_keys.json";

		// Token: 0x04000013 RID: 19
		private static readonly string GlobalCachePath = Path.Combine(LicenseManager.LicenseDirectory, "global_revoked_cache.json");

		// Token: 0x04000014 RID: 20
		private static readonly string LastCheckTimePath = Path.Combine(LicenseManager.LicenseDirectory, "last_check.txt");

		// Token: 0x04000015 RID: 21
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private static HashSet<string> _revokedKeys = null;

		// Token: 0x04000016 RID: 22
		private static DateTime _lastRevokedKeysLoadTime = DateTime.MinValue;

		// Token: 0x04000017 RID: 23
		private static readonly TimeSpan CacheRefreshInterval = TimeSpan.FromSeconds(30.0);

		// Token: 0x04000018 RID: 24
		private static readonly TimeSpan RemoteCacheExpiry = TimeSpan.FromHours(1.0);

		// Token: 0x04000019 RID: 25
		private const string EncryptionKey = "ByFastFlag-SecureKey-2024-ChangeThis!@#$";

		// Token: 0x0400001A RID: 26
		private static readonly byte[] Salt = Encoding.UTF8.GetBytes("ByFastFlag-Salt-2024");

		// Token: 0x02000011 RID: 17
		[NullableContext(0)]
		public enum LicenseStatus
		{
			// Token: 0x04000064 RID: 100
			Invalid,
			// Token: 0x04000065 RID: 101
			Valid,
			// Token: 0x04000066 RID: 102
			MaxActivationsReached,
			// Token: 0x04000067 RID: 103
			Revoked
		}

		// Token: 0x02000012 RID: 18
		[Nullable(0)]
		public class LicenseInfo
		{
			// Token: 0x17000004 RID: 4
			// (get) Token: 0x0600008B RID: 139 RVA: 0x00007B0B File Offset: 0x00005D0B
			// (set) Token: 0x0600008C RID: 140 RVA: 0x00007B13 File Offset: 0x00005D13
			public string Key { get; set; } = string.Empty;

			// Token: 0x17000005 RID: 5
			// (get) Token: 0x0600008D RID: 141 RVA: 0x00007B1C File Offset: 0x00005D1C
			// (set) Token: 0x0600008E RID: 142 RVA: 0x00007B24 File Offset: 0x00005D24
			public DateTime ActivationDate { get; set; }

			// Token: 0x17000006 RID: 6
			// (get) Token: 0x0600008F RID: 143 RVA: 0x00007B2D File Offset: 0x00005D2D
			// (set) Token: 0x06000090 RID: 144 RVA: 0x00007B35 File Offset: 0x00005D35
			public string MachineId { get; set; } = string.Empty;

			// Token: 0x17000007 RID: 7
			// (get) Token: 0x06000091 RID: 145 RVA: 0x00007B3E File Offset: 0x00005D3E
			// (set) Token: 0x06000092 RID: 146 RVA: 0x00007B46 File Offset: 0x00005D46
			public string CustomerName { get; set; } = string.Empty;

			// Token: 0x17000008 RID: 8
			// (get) Token: 0x06000093 RID: 147 RVA: 0x00007B4F File Offset: 0x00005D4F
			// (set) Token: 0x06000094 RID: 148 RVA: 0x00007B57 File Offset: 0x00005D57
			public string LicenseType { get; set; } = "standard";

			// Token: 0x17000009 RID: 9
			// (get) Token: 0x06000095 RID: 149 RVA: 0x00007B60 File Offset: 0x00005D60
			// (set) Token: 0x06000096 RID: 150 RVA: 0x00007B68 File Offset: 0x00005D68
			public string AppVersion { get; set; } = "1.0";
		}

		// Token: 0x02000013 RID: 19
		[Nullable(0)]
		private class GlobalConfig
		{
			// Token: 0x1700000A RID: 10
			// (get) Token: 0x06000098 RID: 152 RVA: 0x00007BB1 File Offset: 0x00005DB1
			// (set) Token: 0x06000099 RID: 153 RVA: 0x00007BB9 File Offset: 0x00005DB9
			public string gist_url { get; set; } = "";

			// Token: 0x1700000B RID: 11
			// (get) Token: 0x0600009A RID: 154 RVA: 0x00007BC2 File Offset: 0x00005DC2
			// (set) Token: 0x0600009B RID: 155 RVA: 0x00007BCA File Offset: 0x00005DCA
			public string updated { get; set; } = "";

			// Token: 0x1700000C RID: 12
			// (get) Token: 0x0600009C RID: 156 RVA: 0x00007BD3 File Offset: 0x00005DD3
			// (set) Token: 0x0600009D RID: 157 RVA: 0x00007BDB File Offset: 0x00005DDB
			public string version { get; set; } = "";

			// Token: 0x1700000D RID: 13
			// (get) Token: 0x0600009E RID: 158 RVA: 0x00007BE4 File Offset: 0x00005DE4
			// (set) Token: 0x0600009F RID: 159 RVA: 0x00007BEC File Offset: 0x00005DEC
			public int revoked_count { get; set; }

			// Token: 0x1700000E RID: 14
			// (get) Token: 0x060000A0 RID: 160 RVA: 0x00007BF5 File Offset: 0x00005DF5
			// (set) Token: 0x060000A1 RID: 161 RVA: 0x00007BFD File Offset: 0x00005DFD
			public string last_update { get; set; } = "";
		}

		// Token: 0x02000014 RID: 20
		[Nullable(0)]
		private class CacheData
		{
			// Token: 0x1700000F RID: 15
			// (get) Token: 0x060000A3 RID: 163 RVA: 0x00007C3B File Offset: 0x00005E3B
			// (set) Token: 0x060000A4 RID: 164 RVA: 0x00007C43 File Offset: 0x00005E43
			public DateTime timestamp { get; set; }

			// Token: 0x17000010 RID: 16
			// (get) Token: 0x060000A5 RID: 165 RVA: 0x00007C4C File Offset: 0x00005E4C
			// (set) Token: 0x060000A6 RID: 166 RVA: 0x00007C54 File Offset: 0x00005E54
			public List<string> keys { get; set; } = new List<string>();
		}
	}
}
