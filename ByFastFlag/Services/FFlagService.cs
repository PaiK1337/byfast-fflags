using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ByFastFlag.Services
{
	// Token: 0x02000008 RID: 8
	[NullableContext(1)]
	[Nullable(0)]
	public class FFlagService
	{
		// Token: 0x06000050 RID: 80
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		// Token: 0x06000051 RID: 81
		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		// Token: 0x06000052 RID: 82
		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

		// Token: 0x06000053 RID: 83
		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr hObject);

		// Token: 0x06000054 RID: 84
		[DllImport("kernel32.dll")]
		private static extern int GetLastError();

		// Token: 0x06000055 RID: 85
		[DllImport("psapi.dll")]
		private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [MarshalAs(UnmanagedType.U4)] [In] int nSize);

		// Token: 0x06000056 RID: 86
		[DllImport("psapi.dll")]
		private static extern int EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

		// Token: 0x06000057 RID: 87 RVA: 0x00004DE4 File Offset: 0x00002FE4
		public FFlagService(Window hostWindow)
		{
			Console.WriteLine("[FFlagService] Constructor called");
			this.LogToConsole("=== FFlagService ===");
			string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "FFlagTool.dll");
			Console.WriteLine("[FFlagService] DLL path: " + dllPath);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[FFlagService] DLL exists: ");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(File.Exists(dllPath));
			Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
			bool flag = !File.Exists(dllPath);
			if (flag)
			{
				this.LogToConsole("✗ ERROR: DLL NOT FOUND");
			}
			else
			{
				try
				{
					this.LogToConsole("Loading DLL...");
					Assembly assembly = Assembly.LoadFrom(dllPath);
					this.LogToConsole("✓ Assembly loaded: " + assembly.FullName);
					this.LoadOffsetsFromLocalFile();
					this.LoadFFlagAddressesFromLocalFile();
					this._isDllLoaded = true;
					this.LogToConsole("✓ Service initialized successfully");
				}
				catch (Exception ex)
				{
					this.LogToConsole("✗ ERROR: " + ex.GetType().Name + ": " + ex.Message);
				}
			}
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00004F60 File Offset: 0x00003160
		private void LoadOffsetsFromLocalFile()
		{
			try
			{
				string offsetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "offsets.hpp");
				bool flag = !File.Exists(offsetsPath);
				if (flag)
				{
					this.LogToConsole("[OFFSETS] offsets.hpp not found (optional)");
				}
				else
				{
					this.LogToConsole("[OFFSETS] Loading from: " + offsetsPath);
					string content = File.ReadAllText(offsetsPath);
					this._offsets = this.ParseOffsetsContent(content);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[OFFSETS] Successfully parsed ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this._offsets.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" offsets");
					this.LogToConsole(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			catch (Exception ex)
			{
				this.LogToConsole("[OFFSETS] Error: " + ex.Message);
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00005044 File Offset: 0x00003244
		private void LoadFFlagAddressesFromLocalFile()
		{
			try
			{
				string fflagsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "FFlags.hpp");
				bool flag = !File.Exists(fflagsPath);
				if (flag)
				{
					this.LogToConsole("[FFLAGS] FFlags.hpp not found in Libs folder");
					this.LogToConsole("[FFLAGS] Path checked: " + fflagsPath);
				}
				else
				{
					this.LogToConsole("[FFLAGS] Loading from: " + fflagsPath);
					string content = File.ReadAllText(fflagsPath);
					bool flag2 = string.IsNullOrWhiteSpace(content);
					if (flag2)
					{
						this.LogToConsole("[FFLAGS] ERROR: File is empty!");
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[FFLAGS] File content length: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(content.Length);
						defaultInterpolatedStringHandler.AppendLiteral(" chars");
						this.LogToConsole(defaultInterpolatedStringHandler.ToStringAndClear());
						this._fflagAddresses = this.ParseFFlagsContent(content);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(45, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("[FFLAGS] Successfully parsed ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(this._fflagAddresses.Count);
						defaultInterpolatedStringHandler2.AppendLiteral(" FFlag addresses");
						this.LogToConsole(defaultInterpolatedStringHandler2.ToStringAndClear());
						bool flag3 = this._fflagAddresses.Count == 0;
						if (flag3)
						{
							this.LogToConsole("[FFLAGS] WARNING: No FFlag addresses found in file!");
							this.LogToConsole("[FFLAGS] File might be in wrong format");
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.LogToConsole("[FFLAGS] Error: " + ex.Message);
				this.LogToConsole("[FFLAGS] Stack trace: " + ex.StackTrace);
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000051E8 File Offset: 0x000033E8
		private Dictionary<string, long> ParseOffsetsContent(string content)
		{
			Dictionary<string, long> offsets = new Dictionary<string, long>();
			string[] patterns = new string[]
			{
				"(\\w+)\\s*=\\s*(0x[0-9A-Fa-f]+);",
				"dword\\s+(\\w+)\\s*=\\s*(0x[0-9A-Fa-f]+);",
				"uintptr_t\\s+(\\w+)\\s*=\\s*(0x[0-9A-Fa-f]+);"
			};
			foreach (string pattern in patterns)
			{
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
				MatchCollection matches = regex.Matches(content);
				foreach (object obj in matches)
				{
					Match match = (Match)obj;
					bool flag = match.Groups.Count >= 3;
					if (flag)
					{
						string name = match.Groups[1].Value;
						string valueStr = match.Groups[2].Value;
						try
						{
							long value = Convert.ToInt64(valueStr, 16);
							bool flag2 = !offsets.ContainsKey(name);
							if (flag2)
							{
								offsets[name] = value;
							}
						}
						catch
						{
						}
					}
				}
			}
			return offsets;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00005328 File Offset: 0x00003528
		private Dictionary<string, long> ParseFFlagsContent(string content)
		{
			Dictionary<string, long> fflags = new Dictionary<string, long>();
			int totalFound = 0;
			this.LogToConsole("[PARSE] Starting to parse FFlags content...");
			Regex regex = new Regex("uintptr_t\\s+(\\w+)\\s*=\\s*(0x[0-9A-Fa-f]+);", RegexOptions.IgnoreCase);
			MatchCollection matches = regex.Matches(content);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[PARSE] Pattern 1 (uintptr_t): Found ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(matches.Count);
			defaultInterpolatedStringHandler.AppendLiteral(" matches");
			this.LogToConsole(defaultInterpolatedStringHandler.ToStringAndClear());
			foreach (object obj in matches)
			{
				Match match = (Match)obj;
				bool flag = match.Groups.Count >= 3;
				if (flag)
				{
					string name = match.Groups[1].Value.Trim();
					string valueStr = match.Groups[2].Value.Trim();
					try
					{
						long value = Convert.ToInt64(valueStr, 16);
						bool flag2 = !fflags.ContainsKey(name);
						if (flag2)
						{
							fflags[name] = value;
							totalFound++;
						}
					}
					catch (Exception ex)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(29, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("[PARSE] Failed to parse ");
						defaultInterpolatedStringHandler2.AppendFormatted(name);
						defaultInterpolatedStringHandler2.AppendLiteral(" = ");
						defaultInterpolatedStringHandler2.AppendFormatted(valueStr);
						defaultInterpolatedStringHandler2.AppendLiteral(": ");
						defaultInterpolatedStringHandler2.AppendFormatted(ex.Message);
						this.LogToConsole(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
				}
			}
			regex = new Regex("intptr_t\\s+(\\w+)\\s*=\\s*(0x[0-9A-Fa-f]+);", RegexOptions.IgnoreCase);
			matches = regex.Matches(content);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(44, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("[PARSE] Pattern 2 (intptr_t): Found ");
			defaultInterpolatedStringHandler3.AppendFormatted<int>(matches.Count);
			defaultInterpolatedStringHandler3.AppendLiteral(" matches");
			this.LogToConsole(defaultInterpolatedStringHandler3.ToStringAndClear());
			foreach (object obj2 in matches)
			{
				Match match2 = (Match)obj2;
				bool flag3 = match2.Groups.Count >= 3;
				if (flag3)
				{
					string name2 = match2.Groups[1].Value.Trim();
					string valueStr2 = match2.Groups[2].Value.Trim();
					try
					{
						long value2 = Convert.ToInt64(valueStr2, 16);
						bool flag4 = !fflags.ContainsKey(name2);
						if (flag4)
						{
							fflags[name2] = value2;
							totalFound++;
						}
					}
					catch (Exception ex2)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(29, 3);
						defaultInterpolatedStringHandler4.AppendLiteral("[PARSE] Failed to parse ");
						defaultInterpolatedStringHandler4.AppendFormatted(name2);
						defaultInterpolatedStringHandler4.AppendLiteral(" = ");
						defaultInterpolatedStringHandler4.AppendFormatted(valueStr2);
						defaultInterpolatedStringHandler4.AppendLiteral(": ");
						defaultInterpolatedStringHandler4.AppendFormatted(ex2.Message);
						this.LogToConsole(defaultInterpolatedStringHandler4.ToStringAndClear());
					}
				}
			}
			regex = new Regex("#define\\s+(\\w+)\\s+(0x[0-9A-Fa-f]+)", RegexOptions.IgnoreCase);
			matches = regex.Matches(content);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(43, 1);
			defaultInterpolatedStringHandler5.AppendLiteral("[PARSE] Pattern 3 (#define): Found ");
			defaultInterpolatedStringHandler5.AppendFormatted<int>(matches.Count);
			defaultInterpolatedStringHandler5.AppendLiteral(" matches");
			this.LogToConsole(defaultInterpolatedStringHandler5.ToStringAndClear());
			foreach (object obj3 in matches)
			{
				Match match3 = (Match)obj3;
				bool flag5 = match3.Groups.Count >= 3;
				if (flag5)
				{
					string name3 = match3.Groups[1].Value.Trim();
					string valueStr3 = match3.Groups[2].Value.Trim();
					try
					{
						long value3 = Convert.ToInt64(valueStr3, 16);
						bool flag6 = !fflags.ContainsKey(name3);
						if (flag6)
						{
							fflags[name3] = value3;
							totalFound++;
						}
					}
					catch (Exception ex3)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(29, 3);
						defaultInterpolatedStringHandler6.AppendLiteral("[PARSE] Failed to parse ");
						defaultInterpolatedStringHandler6.AppendFormatted(name3);
						defaultInterpolatedStringHandler6.AppendLiteral(" = ");
						defaultInterpolatedStringHandler6.AppendFormatted(valueStr3);
						defaultInterpolatedStringHandler6.AppendLiteral(": ");
						defaultInterpolatedStringHandler6.AppendFormatted(ex3.Message);
						this.LogToConsole(defaultInterpolatedStringHandler6.ToStringAndClear());
					}
				}
			}
			regex = new Regex("(\\w+)\\s*=\\s*(0x[0-9A-Fa-f]+);", RegexOptions.IgnoreCase);
			matches = regex.Matches(content);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(47, 1);
			defaultInterpolatedStringHandler7.AppendLiteral("[PARSE] Pattern 4 (any = 0x...): Found ");
			defaultInterpolatedStringHandler7.AppendFormatted<int>(matches.Count);
			defaultInterpolatedStringHandler7.AppendLiteral(" matches");
			this.LogToConsole(defaultInterpolatedStringHandler7.ToStringAndClear());
			foreach (object obj4 in matches)
			{
				Match match4 = (Match)obj4;
				bool flag7 = match4.Groups.Count >= 3;
				if (flag7)
				{
					string name4 = match4.Groups[1].Value.Trim();
					string valueStr4 = match4.Groups[2].Value.Trim();
					bool flag8 = fflags.ContainsKey(name4);
					if (!flag8)
					{
						try
						{
							long value4 = Convert.ToInt64(valueStr4, 16);
							fflags[name4] = value4;
							totalFound++;
						}
						catch (Exception ex4)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(37, 3);
							defaultInterpolatedStringHandler8.AppendLiteral("[PARSE] Failed to parse generic ");
							defaultInterpolatedStringHandler8.AppendFormatted(name4);
							defaultInterpolatedStringHandler8.AppendLiteral(" = ");
							defaultInterpolatedStringHandler8.AppendFormatted(valueStr4);
							defaultInterpolatedStringHandler8.AppendLiteral(": ");
							defaultInterpolatedStringHandler8.AppendFormatted(ex4.Message);
							this.LogToConsole(defaultInterpolatedStringHandler8.ToStringAndClear());
						}
					}
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(38, 1);
			defaultInterpolatedStringHandler9.AppendLiteral("[PARSE] Total FFlag addresses parsed: ");
			defaultInterpolatedStringHandler9.AppendFormatted<int>(totalFound);
			this.LogToConsole(defaultInterpolatedStringHandler9.ToStringAndClear());
			return fflags;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00005A3C File Offset: 0x00003C3C
		private void LogToConsole(string message)
		{
			this._logs.Add(message);
			Console.WriteLine(message);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00005A53 File Offset: 0x00003C53
		public bool IsDllLoaded()
		{
			return this._isDllLoaded;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00005A5B File Offset: 0x00003C5B
		public List<string> GetLogs()
		{
			return this._logs;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00005A64 File Offset: 0x00003C64
		public void LoadFlags(string jsonPath)
		{
			bool flag = !this._isDllLoaded;
			if (flag)
			{
				throw new InvalidOperationException("DLL not loaded.");
			}
			bool flag2 = !File.Exists(jsonPath);
			if (flag2)
			{
				throw new FileNotFoundException("File not found: " + jsonPath);
			}
			try
			{
				this.LogToConsole("[FFlagService] Loading flags from: " + jsonPath);
				bool flag3 = !this.TryLoadFlagsWithMultipleMethods(jsonPath);
				if (flag3)
				{
					throw new InvalidOperationException("All parsing methods failed");
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Failed to load flags: " + ex.Message);
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00005B04 File Offset: 0x00003D04
		private bool TryLoadFlagsWithMultipleMethods(string jsonPath)
		{
			string json = File.ReadAllText(jsonPath);
			bool result;
			try
			{
				result = this.TryParseJson(json, "Standard");
			}
			catch (JsonException ex)
			{
				this.LogToConsole("[PARSING] Standard JSON parsing failed: " + ex.Message);
				try
				{
					string cleaned = this.CleanJsonAggressively(json);
					result = this.TryParseJson(cleaned, "Cleaned");
				}
				catch (JsonException ex2)
				{
					this.LogToConsole("[PARSING] Cleaned JSON parsing failed: " + ex2.Message);
					try
					{
						result = this.ParseJsonLineByLine(json);
					}
					catch (Exception ex3)
					{
						this.LogToConsole("[PARSING] Line-by-line parsing failed: " + ex3.Message);
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00005BC8 File Offset: 0x00003DC8
		private bool TryParseJson(string json, string methodName)
		{
			this.LogToConsole("[PARSING] Trying " + methodName + " method...");
			JsonDocument doc = JsonDocument.Parse(json, default(JsonDocumentOptions));
			bool flag = doc.RootElement.ValueKind != JsonValueKind.Object;
			if (flag)
			{
				throw new Exception("Root JSON must be an object");
			}
			this._flags.Clear();
			this._cleanFlagNames.Clear();
			int count = 0;
			int errorCount = 0;
			foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
			{
				try
				{
					string originalName = prop.Name;
					string cleanName = this.CleanFlagName(originalName);
					bool flag2 = string.IsNullOrWhiteSpace(cleanName);
					if (flag2)
					{
						errorCount++;
					}
					else
					{
						this._flags[cleanName] = prop.Value;
						this._cleanFlagNames[cleanName] = originalName;
						count++;
						bool flag3 = count <= 5;
						if (flag3)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 2);
							defaultInterpolatedStringHandler.AppendLiteral("[FLAGS] ");
							defaultInterpolatedStringHandler.AppendFormatted(cleanName);
							defaultInterpolatedStringHandler.AppendLiteral(" = ");
							defaultInterpolatedStringHandler.AppendFormatted<JsonElement>(prop.Value);
							this.LogToConsole(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
				catch (Exception ex)
				{
					errorCount++;
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(41, 3);
			defaultInterpolatedStringHandler2.AppendLiteral("[PARSING] ");
			defaultInterpolatedStringHandler2.AppendFormatted(methodName);
			defaultInterpolatedStringHandler2.AppendLiteral(" method: Loaded ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(count);
			defaultInterpolatedStringHandler2.AppendLiteral(" flags, ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(errorCount);
			defaultInterpolatedStringHandler2.AppendLiteral(" errors");
			this.LogToConsole(defaultInterpolatedStringHandler2.ToStringAndClear());
			return count > 0;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00005DD0 File Offset: 0x00003FD0
		private bool ParseJsonLineByLine(string json)
		{
			this.LogToConsole("[PARSING] Trying line-by-line parsing...");
			this._flags.Clear();
			this._cleanFlagNames.Clear();
			string[] lines = json.Split('\n', StringSplitOptions.None);
			int count = 0;
			int errorCount = 0;
			Regex flagRegex = new Regex("\\s*\"([^\"]+)\"\\s*:\\s*(\"(?:[^\"\\\\]|\\\\.)*\"|[^,\\s\\}]+)\\s*,?");
			string[] array = lines;
			int i = 0;
			while (i < array.Length)
			{
				string line = array[i];
				try
				{
					Match match = flagRegex.Match(line);
					bool success = match.Success;
					if (success)
					{
						string key = match.Groups[1].Value;
						string valueStr = match.Groups[2].Value.Trim();
						string cleanName = this.CleanFlagName(key);
						bool flag = string.IsNullOrWhiteSpace(cleanName);
						if (flag)
						{
							errorCount++;
						}
						else
						{
							bool flag2 = valueStr.StartsWith("\"") && valueStr.EndsWith("\"");
							JsonElement value;
							if (flag2)
							{
								value = JsonDocument.Parse("{\"temp\": " + valueStr + "}", default(JsonDocumentOptions)).RootElement.GetProperty("temp");
							}
							else
							{
								bool flag3 = valueStr.Equals("true", StringComparison.OrdinalIgnoreCase) || valueStr.Equals("false", StringComparison.OrdinalIgnoreCase);
								if (flag3)
								{
									value = JsonDocument.Parse("{\"temp\": " + valueStr + "}", default(JsonDocumentOptions)).RootElement.GetProperty("temp");
								}
								else
								{
									int num;
									double num2;
									bool flag4 = int.TryParse(valueStr, out num) || double.TryParse(valueStr, out num2);
									if (!flag4)
									{
										errorCount++;
										goto IL_209;
									}
									value = JsonDocument.Parse("{\"temp\": " + valueStr + "}", default(JsonDocumentOptions)).RootElement.GetProperty("temp");
								}
							}
							this._flags[cleanName] = value;
							this._cleanFlagNames[cleanName] = key;
							count++;
						}
					}
				}
				catch (Exception ex)
				{
					errorCount++;
				}
				IL_209:
				i++;
				continue;
				goto IL_209;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
			defaultInterpolatedStringHandler.AppendLiteral("[PARSING] Line-by-line: Loaded ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(count);
			defaultInterpolatedStringHandler.AppendLiteral(" flags, ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(errorCount);
			defaultInterpolatedStringHandler.AppendLiteral(" errors");
			this.LogToConsole(defaultInterpolatedStringHandler.ToStringAndClear());
			return count > 0;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00006070 File Offset: 0x00004270
		private string CleanJsonAggressively(string json)
		{
			StringBuilder result = new StringBuilder();
			string[] lines = json.Split('\n', StringSplitOptions.None);
			foreach (string line in lines)
			{
				string cleanedLine = line;
				cleanedLine = cleanedLine.Replace("\\u0022", "\"");
				bool flag = cleanedLine.Contains("\": \"");
				if (flag)
				{
					int colonCount = Regex.Matches(cleanedLine, "\":\\s*\"").Count;
					bool flag2 = colonCount > 1;
					if (flag2)
					{
						int firstColon = cleanedLine.IndexOf("\": \"");
						int lastQuote = cleanedLine.LastIndexOf("\"");
						bool flag3 = firstColon > 0 && lastQuote > firstColon + 4;
						if (flag3)
						{
							string keyPart = cleanedLine.Substring(0, firstColon + 2);
							string middlePart = cleanedLine.Substring(firstColon + 2, lastQuote - firstColon - 1);
							string[] parts = middlePart.Split(new string[]
							{
								"\": \""
							}, StringSplitOptions.RemoveEmptyEntries);
							bool flag4 = parts.Length > 1;
							if (flag4)
							{
								string lastValue = parts[parts.Length - 1];
								cleanedLine = keyPart + "\"" + lastValue + "\"";
							}
						}
					}
				}
				result.AppendLine(cleanedLine);
			}
			return result.ToString();
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000061B0 File Offset: 0x000043B0
		private string CleanFlagName(string flagName)
		{
			string cleaned = flagName.Replace("\\u0022", "");
			cleaned = cleaned.Trim('"');
			int colonIndex = cleaned.IndexOf(": \"");
			bool flag = colonIndex > 0;
			if (flag)
			{
				cleaned = cleaned.Substring(0, colonIndex);
			}
			cleaned = cleaned.Replace("\"", "");
			return cleaned.Trim();
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00006214 File Offset: 0x00004414
		[DebuggerStepThrough]
		public Task LoadOffsetsAsync()
		{
			FFlagService.<LoadOffsetsAsync>d__34 <LoadOffsetsAsync>d__ = new FFlagService.<LoadOffsetsAsync>d__34();
			<LoadOffsetsAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<LoadOffsetsAsync>d__.<>4__this = this;
			<LoadOffsetsAsync>d__.<>1__state = -1;
			<LoadOffsetsAsync>d__.<>t__builder.Start<FFlagService.<LoadOffsetsAsync>d__34>(ref <LoadOffsetsAsync>d__);
			return <LoadOffsetsAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00006258 File Offset: 0x00004458
		public void Inject()
		{
			Console.WriteLine("[FFlagService] Inject() called");
			bool flag3 = !this._isDllLoaded;
			if (flag3)
			{
				throw new InvalidOperationException("DLL not loaded.");
			}
			bool flag4 = this._flags.Count == 0;
			if (flag4)
			{
				throw new InvalidOperationException("No flags loaded.");
			}
			bool flag5 = this._fflagAddresses.Count == 0;
			if (flag5)
			{
				throw new InvalidOperationException("No FFlag addresses loaded.");
			}
			try
			{
				this.LogToConsole("[INJECT] ===== STARTING INJECTION =====");
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[INJECT] Flags: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this._flags.Count);
				defaultInterpolatedStringHandler.AppendLiteral(", FFlag Addresses: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this._fflagAddresses.Count);
				this.LogToConsole(defaultInterpolatedStringHandler.ToStringAndClear());
				Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
				bool flag6 = processes.Length == 0;
				if (flag6)
				{
					throw new InvalidOperationException("Roblox not running!");
				}
				Process robloxProcess = processes[0];
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("[INJECT] Target: ");
				defaultInterpolatedStringHandler2.AppendFormatted(robloxProcess.ProcessName);
				defaultInterpolatedStringHandler2.AppendLiteral(" (PID: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(robloxProcess.Id);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				this.LogToConsole(defaultInterpolatedStringHandler2.ToStringAndClear());
				this._robloxBaseAddress = this.GetRobloxBaseAddress(robloxProcess.Id);
				bool flag7 = this._robloxBaseAddress == 0L;
				if (flag7)
				{
					throw new InvalidOperationException("Failed to get Roblox base address.");
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("[INJECT] Roblox base address: 0x");
				defaultInterpolatedStringHandler3.AppendFormatted<long>(this._robloxBaseAddress, "X");
				this.LogToConsole(defaultInterpolatedStringHandler3.ToStringAndClear());
				IntPtr processHandle = FFlagService.OpenProcess(1080, false, robloxProcess.Id);
				bool flag8 = processHandle == IntPtr.Zero;
				if (flag8)
				{
					int error = FFlagService.GetLastError();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(42, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("[INJECT] ✗ Failed to open process. Error: ");
					defaultInterpolatedStringHandler4.AppendFormatted<int>(error);
					this.LogToConsole(defaultInterpolatedStringHandler4.ToStringAndClear());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(31, 1);
					defaultInterpolatedStringHandler5.AppendLiteral("Failed to open process. Error: ");
					defaultInterpolatedStringHandler5.AppendFormatted<int>(error);
					throw new InvalidOperationException(defaultInterpolatedStringHandler5.ToStringAndClear());
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(38, 1);
				defaultInterpolatedStringHandler6.AppendLiteral("[INJECT] ✓ Process opened (Handle: 0x");
				defaultInterpolatedStringHandler6.AppendFormatted<long>(processHandle.ToInt64(), "X");
				defaultInterpolatedStringHandler6.AppendLiteral(")");
				this.LogToConsole(defaultInterpolatedStringHandler6.ToStringAndClear());
				try
				{
					int appliedCount = 0;
					int missingCount = 0;
					int skippedCount = 0;
					int errorCount = 0;
					this.LogToConsole("[INJECT] Starting FFlag injection with safety measures...");
					List<KeyValuePair<string, JsonElement>> flagsToApply = new List<KeyValuePair<string, JsonElement>>();
					foreach (KeyValuePair<string, JsonElement> flag in this._flags)
					{
						string cleanFlagName = flag.Key;
						bool isProblematic = false;
						foreach (string problematic in FFlagService.ProblematicFFlags)
						{
							bool flag9 = cleanFlagName.Contains(problematic, StringComparison.OrdinalIgnoreCase);
							if (flag9)
							{
								isProblematic = true;
								break;
							}
						}
						bool flag10 = isProblematic;
						if (flag10)
						{
							this.LogToConsole("[SAFETY] Skipping potentially problematic flag: " + cleanFlagName);
							skippedCount++;
						}
						else
						{
							flagsToApply.Add(flag);
						}
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(82, 2);
					defaultInterpolatedStringHandler7.AppendLiteral("[INJECT] Will attempt to apply ");
					defaultInterpolatedStringHandler7.AppendFormatted<int>(flagsToApply.Count);
					defaultInterpolatedStringHandler7.AppendLiteral(" safe flags (skipped ");
					defaultInterpolatedStringHandler7.AppendFormatted<int>(skippedCount);
					defaultInterpolatedStringHandler7.AppendLiteral(" potentially problematic ones)");
					this.LogToConsole(defaultInterpolatedStringHandler7.ToStringAndClear());
					foreach (KeyValuePair<string, JsonElement> flag2 in flagsToApply)
					{
						try
						{
							string cleanFlagName2 = flag2.Key;
							string[] nameVariations = this.GenerateNameVariations(cleanFlagName2);
							long fflagAddress = 0L;
							bool found = false;
							foreach (string variation in nameVariations)
							{
								bool flag11 = this._fflagAddresses.TryGetValue(variation, out fflagAddress);
								if (flag11)
								{
									found = true;
									break;
								}
							}
							bool flag12 = !found;
							if (flag12)
							{
								missingCount++;
							}
							else
							{
								long absoluteAddress = this._robloxBaseAddress + fflagAddress;
								bool flag13 = !this.IsAddressValid(processHandle, absoluteAddress);
								if (flag13)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(46, 2);
									defaultInterpolatedStringHandler8.AppendLiteral("[SAFETY] Skipping ");
									defaultInterpolatedStringHandler8.AppendFormatted(cleanFlagName2);
									defaultInterpolatedStringHandler8.AppendLiteral(": address 0x");
									defaultInterpolatedStringHandler8.AppendFormatted<long>(absoluteAddress, "X");
									defaultInterpolatedStringHandler8.AppendLiteral(" appears invalid");
									this.LogToConsole(defaultInterpolatedStringHandler8.ToStringAndClear());
									errorCount++;
								}
								else
								{
									Thread.Sleep(1);
									bool success = this.ApplyFlagValueSafely(processHandle, cleanFlagName2, flag2.Value, absoluteAddress);
									bool flag14 = success;
									if (flag14)
									{
										appliedCount++;
										bool flag15 = appliedCount <= 15;
										if (flag15)
										{
											this.LogToConsole("[✓] Applied " + cleanFlagName2);
										}
										else
										{
											bool flag16 = appliedCount == 16;
											if (flag16)
											{
												this.LogToConsole("[...] (more successes not shown)");
											}
										}
									}
									else
									{
										errorCount++;
										bool flag17 = errorCount <= 10;
										if (flag17)
										{
											this.LogToConsole("[✗] Failed to write " + cleanFlagName2);
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							errorCount++;
						}
					}
					this.LogToConsole("[INJECT] Results:");
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(22, 1);
					defaultInterpolatedStringHandler9.AppendLiteral("[INJECT]   ✓ Applied: ");
					defaultInterpolatedStringHandler9.AppendFormatted<int>(appliedCount);
					this.LogToConsole(defaultInterpolatedStringHandler9.ToStringAndClear());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler10 = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler10.AppendLiteral("[INJECT]   [MISS] Not found: ");
					defaultInterpolatedStringHandler10.AppendFormatted<int>(missingCount);
					this.LogToConsole(defaultInterpolatedStringHandler10.ToStringAndClear());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler11 = new DefaultInterpolatedStringHandler(31, 1);
					defaultInterpolatedStringHandler11.AppendLiteral("[INJECT]   [SKIP] Problematic: ");
					defaultInterpolatedStringHandler11.AppendFormatted<int>(skippedCount);
					this.LogToConsole(defaultInterpolatedStringHandler11.ToStringAndClear());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler12 = new DefaultInterpolatedStringHandler(33, 1);
					defaultInterpolatedStringHandler12.AppendLiteral("[INJECT]   [ERROR] Other errors: ");
					defaultInterpolatedStringHandler12.AppendFormatted<int>(errorCount);
					this.LogToConsole(defaultInterpolatedStringHandler12.ToStringAndClear());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler13 = new DefaultInterpolatedStringHandler(32, 1);
					defaultInterpolatedStringHandler13.AppendLiteral("[INJECT]   Total flags checked: ");
					defaultInterpolatedStringHandler13.AppendFormatted<int>(this._flags.Count);
					this.LogToConsole(defaultInterpolatedStringHandler13.ToStringAndClear());
					bool flag18 = appliedCount > 0;
					if (flag18)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler14 = new DefaultInterpolatedStringHandler(38, 1);
						defaultInterpolatedStringHandler14.AppendLiteral("[INJECT] ✓ Successfully applied ");
						defaultInterpolatedStringHandler14.AppendFormatted<int>(appliedCount);
						defaultInterpolatedStringHandler14.AppendLiteral(" flags");
						this.LogToConsole(defaultInterpolatedStringHandler14.ToStringAndClear());
					}
					else
					{
						this.LogToConsole("[INJECT] ⚠️ No flags were applied");
					}
					this.LogToConsole("[INJECT] ===== INJECTION COMPLETE =====");
				}
				finally
				{
					FFlagService.CloseHandle(processHandle);
				}
			}
			catch (Exception ex2)
			{
				this.LogToConsole("[INJECT] ERROR: " + ex2.GetType().Name + ": " + ex2.Message);
				throw new InvalidOperationException("Failed to inject: " + ex2.Message);
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00006A40 File Offset: 0x00004C40
		private bool IsAddressValid(IntPtr processHandle, long address)
		{
			bool result;
			try
			{
				byte[] testBuffer = new byte[1];
				int bytesRead;
				bool flag = FFlagService.ReadProcessMemory(processHandle, new IntPtr(address), testBuffer, 1, out bytesRead);
				if (flag)
				{
					result = (bytesRead == 1);
				}
				else
				{
					result = false;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00006A90 File Offset: 0x00004C90
		private bool ApplyFlagValueSafely(IntPtr processHandle, string flagName, JsonElement flagValue, long address)
		{
			bool result;
			try
			{
				bool flag = flagValue.ValueKind == JsonValueKind.True || flagValue.ValueKind == JsonValueKind.False;
				if (flag)
				{
					byte byteValue = flagValue.GetBoolean() ? 1 : 0;
					int written;
					result = FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), new byte[]
					{
						byteValue
					}, 1, out written);
				}
				else
				{
					bool flag2 = flagValue.ValueKind == JsonValueKind.Number;
					if (flag2)
					{
						int intValue;
						bool flag3 = flagValue.TryGetInt32(out intValue);
						if (flag3)
						{
							byte[] intBytes = BitConverter.GetBytes(intValue);
							int written2;
							return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), intBytes, 4, out written2);
						}
						double doubleValue;
						bool flag4 = flagValue.TryGetDouble(out doubleValue);
						if (flag4)
						{
							byte[] doubleBytes = BitConverter.GetBytes(doubleValue);
							int written3;
							return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), doubleBytes, 8, out written3);
						}
					}
					else
					{
						bool flag5 = flagValue.ValueKind == JsonValueKind.String;
						if (flag5)
						{
							string stringValue = flagValue.GetString() ?? "";
							bool flag6 = stringValue.Equals("True", StringComparison.OrdinalIgnoreCase) || stringValue.Equals("true", StringComparison.OrdinalIgnoreCase);
							if (flag6)
							{
								byte byteValue2 = 1;
								int written4;
								return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), new byte[]
								{
									byteValue2
								}, 1, out written4);
							}
							bool flag7 = stringValue.Equals("False", StringComparison.OrdinalIgnoreCase) || stringValue.Equals("false", StringComparison.OrdinalIgnoreCase);
							if (flag7)
							{
								byte byteValue3 = 0;
								int written5;
								return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), new byte[]
								{
									byteValue3
								}, 1, out written5);
							}
							int intValue2;
							bool flag8 = int.TryParse(stringValue, out intValue2);
							if (flag8)
							{
								byte[] intBytes2 = BitConverter.GetBytes(intValue2);
								int written6;
								return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), intBytes2, 4, out written6);
							}
							double doubleValue2;
							bool flag9 = double.TryParse(stringValue, out doubleValue2);
							if (flag9)
							{
								byte[] doubleBytes2 = BitConverter.GetBytes(doubleValue2);
								int written7;
								return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), doubleBytes2, 8, out written7);
							}
							bool flag10 = stringValue.Equals("null", StringComparison.OrdinalIgnoreCase);
							if (flag10)
							{
								byte byteValue4 = 0;
								int written8;
								return FFlagService.WriteProcessMemory(processHandle, new IntPtr(address), new byte[]
								{
									byteValue4
								}, 1, out written8);
							}
						}
					}
					result = false;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00006CE4 File Offset: 0x00004EE4
		private string[] GenerateNameVariations(string flagName)
		{
			List<string> variations = new List<string>();
			variations.Add(flagName);
			string[] prefixesToStrip = new string[]
			{
				"FFlag",
				"DFFlag",
				"FInt",
				"DFInt",
				"FString",
				"DFString",
				"FLog",
				"DFLog",
				"GoogleAnalytics"
			};
			foreach (string prefix in prefixesToStrip)
			{
				bool flag = flagName.StartsWith(prefix);
				if (flag)
				{
					string stripped = flagName.Substring(prefix.Length);
					variations.Add(stripped);
					bool flag2 = stripped.Length > 0;
					if (flag2)
					{
						char c = char.ToLower(stripped[0]);
						string camelCase = new ReadOnlySpan<char>(ref c) + stripped.Substring(1);
						variations.Add(camelCase);
					}
				}
			}
			return variations.Distinct<string>().ToArray<string>();
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00006DE8 File Offset: 0x00004FE8
		private long GetRobloxBaseAddress(int processId)
		{
			try
			{
				IntPtr processHandle = FFlagService.OpenProcess(1040, false, processId);
				bool flag = processHandle == IntPtr.Zero;
				if (flag)
				{
					return 0L;
				}
				try
				{
					IntPtr[] moduleHandles = new IntPtr[1024];
					uint bytesNeeded;
					bool flag2 = FFlagService.EnumProcessModules(processHandle, moduleHandles, (uint)(IntPtr.Size * moduleHandles.Length), out bytesNeeded) != 0;
					if (flag2)
					{
						uint moduleCount = bytesNeeded / (uint)IntPtr.Size;
						bool flag3 = moduleCount > 0U;
						if (flag3)
						{
							return (long)moduleHandles[0];
						}
					}
				}
				finally
				{
					FFlagService.CloseHandle(processHandle);
				}
			}
			catch
			{
			}
			return 0L;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00006E94 File Offset: 0x00005094
		[NullableContext(0)]
		[return: TupleElementNames(new string[]
		{
			"FlagsCount",
			"OffsetsCount"
		})]
		public ValueTuple<int, int> GetLoadedCounts()
		{
			return new ValueTuple<int, int>(this._flags.Count, this._fflagAddresses.Count);
		}

		// Token: 0x04000038 RID: 56
		private bool _isDllLoaded = false;

		// Token: 0x04000039 RID: 57
		private List<string> _logs = new List<string>();

		// Token: 0x0400003A RID: 58
		private Dictionary<string, long> _offsets = new Dictionary<string, long>();

		// Token: 0x0400003B RID: 59
		private Dictionary<string, long> _fflagAddresses = new Dictionary<string, long>();

		// Token: 0x0400003C RID: 60
		private Dictionary<string, JsonElement> _flags = new Dictionary<string, JsonElement>();

		// Token: 0x0400003D RID: 61
		private Dictionary<string, string> _cleanFlagNames = new Dictionary<string, string>();

		// Token: 0x0400003E RID: 62
		private long _robloxBaseAddress = 0L;

		// Token: 0x0400003F RID: 63
		private const int PROCESS_ALL_ACCESS = 2035711;

		// Token: 0x04000040 RID: 64
		private const int PROCESS_VM_READ = 16;

		// Token: 0x04000041 RID: 65
		private const int PROCESS_VM_WRITE = 32;

		// Token: 0x04000042 RID: 66
		private const int PROCESS_VM_OPERATION = 8;

		// Token: 0x04000043 RID: 67
		private const int PROCESS_QUERY_INFORMATION = 1024;

		// Token: 0x04000044 RID: 68
		private static readonly HashSet<string> ProblematicFFlags = new HashSet<string>
		{
			"DebugCrashReports",
			"DebugGraphicsCrashOnLeaks",
			"DebugForceDisable3DRendering",
			"DebugForceChatDisabled",
			"DebugDisablePhysicsLOD",
			"DebugDisableParticles",
			"DebugDisableRenderingPostEffects",
			"DebugDisableLODFoliage",
			"DebugDisableGuiEffects",
			"DebugForceDisableShadows",
			"DebugGraphicsDisableOpenGL",
			"DebugGraphicsDisableVulkan",
			"DebugGraphicsDisableVulkan11",
			"DebugGraphicsDisableDirect3D11",
			"TrackPlaceIdForCrashEnabled",
			"CrashUploadToBacktracePercentage",
			"WriteFullDmpPercent",
			"DetectCrashEarlyPercentage",
			"CrashReportingHundredthsPercentage"
		};
	}
}
