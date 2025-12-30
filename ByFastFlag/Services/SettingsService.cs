using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ByFastFlag.Services
{
	// Token: 0x02000009 RID: 9
	[NullableContext(1)]
	[Nullable(0)]
	public class SettingsService
	{
		// Token: 0x0600006E RID: 110 RVA: 0x00007093 File Offset: 0x00005293
		public SettingsService()
		{
			this.LoadSettings();
		}

		// Token: 0x0600006F RID: 111 RVA: 0x000070B0 File Offset: 0x000052B0
		private void LoadSettings()
		{
			try
			{
				bool flag = File.Exists(SettingsService.SettingsPath);
				if (flag)
				{
					string json = File.ReadAllText(SettingsService.SettingsPath);
					this._settings = (JsonSerializer.Deserialize<SettingsService.AppSettings>(json, null) ?? new SettingsService.AppSettings());
				}
				else
				{
					this._settings = new SettingsService.AppSettings();
				}
			}
			catch
			{
				this._settings = new SettingsService.AppSettings();
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00007124 File Offset: 0x00005324
		private void SaveSettings()
		{
			try
			{
				string json = JsonSerializer.Serialize<SettingsService.AppSettings>(this._settings, new JsonSerializerOptions
				{
					WriteIndented = true
				});
				File.WriteAllText(SettingsService.SettingsPath, json);
			}
			catch
			{
			}
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000071 RID: 113 RVA: 0x00007170 File Offset: 0x00005370
		// (set) Token: 0x06000072 RID: 114 RVA: 0x0000717D File Offset: 0x0000537D
		public string LastFlagsPath
		{
			get
			{
				return this._settings.LastFlagsPath;
			}
			set
			{
				this._settings.LastFlagsPath = value;
				this.SaveSettings();
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00007194 File Offset: 0x00005394
		// (set) Token: 0x06000074 RID: 116 RVA: 0x000071A1 File Offset: 0x000053A1
		public string LastOffsetsPath
		{
			get
			{
				return this._settings.LastOffsetsPath;
			}
			set
			{
				this._settings.LastOffsetsPath = value;
				this.SaveSettings();
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000075 RID: 117 RVA: 0x000071B8 File Offset: 0x000053B8
		// (set) Token: 0x06000076 RID: 118 RVA: 0x000071C5 File Offset: 0x000053C5
		public bool HasInjectedOnce
		{
			get
			{
				return this._settings.HasInjectedOnce;
			}
			set
			{
				this._settings.HasInjectedOnce = value;
				this.SaveSettings();
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x000071DC File Offset: 0x000053DC
		public void SaveLoadedData(Dictionary<string, long> offsets, Dictionary<string, JsonElement> flags)
		{
			this._settings.SavedOffsets = new Dictionary<string, long>(offsets);
			this._settings.SavedFlags = new Dictionary<string, JsonElement>(flags);
			this.SaveSettings();
		}

		// Token: 0x06000078 RID: 120 RVA: 0x0000720C File Offset: 0x0000540C
		[return: TupleElementNames(new string[]
		{
			"Offsets",
			"Flags"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1,
			1,
			1
		})]
		public ValueTuple<Dictionary<string, long>, Dictionary<string, JsonElement>> GetSavedData()
		{
			return new ValueTuple<Dictionary<string, long>, Dictionary<string, JsonElement>>(this._settings.SavedOffsets, this._settings.SavedFlags);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x0000723C File Offset: 0x0000543C
		public bool HasSavedData()
		{
			return this._settings.SavedOffsets.Count > 0 || this._settings.SavedFlags.Count > 0;
		}

		// Token: 0x04000045 RID: 69
		private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

		// Token: 0x04000046 RID: 70
		private SettingsService.AppSettings _settings = new SettingsService.AppSettings();

		// Token: 0x02000028 RID: 40
		[Nullable(0)]
		private class AppSettings
		{
			// Token: 0x17000011 RID: 17
			// (get) Token: 0x060000E6 RID: 230 RVA: 0x0000A3CE File Offset: 0x000085CE
			// (set) Token: 0x060000E7 RID: 231 RVA: 0x0000A3D6 File Offset: 0x000085D6
			public string LastFlagsPath { get; set; } = "";

			// Token: 0x17000012 RID: 18
			// (get) Token: 0x060000E8 RID: 232 RVA: 0x0000A3DF File Offset: 0x000085DF
			// (set) Token: 0x060000E9 RID: 233 RVA: 0x0000A3E7 File Offset: 0x000085E7
			public string LastOffsetsPath { get; set; } = "";

			// Token: 0x17000013 RID: 19
			// (get) Token: 0x060000EA RID: 234 RVA: 0x0000A3F0 File Offset: 0x000085F0
			// (set) Token: 0x060000EB RID: 235 RVA: 0x0000A3F8 File Offset: 0x000085F8
			public Dictionary<string, long> SavedOffsets { get; set; } = new Dictionary<string, long>();

			// Token: 0x17000014 RID: 20
			// (get) Token: 0x060000EC RID: 236 RVA: 0x0000A401 File Offset: 0x00008601
			// (set) Token: 0x060000ED RID: 237 RVA: 0x0000A409 File Offset: 0x00008609
			public Dictionary<string, JsonElement> SavedFlags { get; set; } = new Dictionary<string, JsonElement>();

			// Token: 0x17000015 RID: 21
			// (get) Token: 0x060000EE RID: 238 RVA: 0x0000A412 File Offset: 0x00008612
			// (set) Token: 0x060000EF RID: 239 RVA: 0x0000A41A File Offset: 0x0000861A
			public bool HasInjectedOnce { get; set; } = false;
		}
	}
}
