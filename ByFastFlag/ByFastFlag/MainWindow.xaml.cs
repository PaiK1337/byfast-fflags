using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ByFastFlag.Services;
using Microsoft.Win32;

namespace ByFastFlag
{
	// Token: 0x02000007 RID: 7
	public partial class MainWindow : Window
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00003E70 File Offset: 0x00002070
		public MainWindow()
		{
			Console.WriteLine("[MainWindow] ===== CYBERPUNK EDITION INITIALIZING =====");
			MainWindow.SetupAssemblyResolution();
			this.InitializeComponent();
			this._settingsService = new SettingsService();
			this._statusTimer = new DispatcherTimer();
			this._clockTimer = new DispatcherTimer();
			this._injectionTimer = new DispatcherTimer();
			base.MouseDown += delegate(object s, MouseButtonEventArgs e)
			{
				FrameworkElement element;
				bool flag;
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					element = (e.OriginalSource as FrameworkElement);
					flag = (element != null);
				}
				else
				{
					flag = false;
				}
				bool flag2 = flag;
				if (flag2)
				{
					bool flag3 = element.Name != "MinimizeButton" && element.Name != "CloseButton" && element.Name != "LicenseInfoButton";
					if (flag3)
					{
						base.DragMove();
					}
				}
			};
			this.LoadFlagsBtn.Click += this.LoadFlagsBtn_Click;
			this.LoadOffsetsBtn.Click += this.LoadOffsetsBtn_Click;
			this.ClearLogBtn.Click += this.ClearLogBtn_Click;
			this.LicenseInfoButton.Click += this.LicenseInfoButton_Click;
			this.MinimizeButton.Click += this.Minimize_Click;
			this.CloseButton.Click += this.Close_Click;
			this.SetupStatusAnimation();
			this.SetupClock();
			this.SetupAutoInjectionTimer();
			this.UpdateStatus("INITIALIZING", Colors.Yellow);
			this.UpdateFileInfo("Not loaded", "Not loaded");
			this.UpdateCounts(0, 0, 0);
			this.ProgressBar.Value = 0.0;
			this.ProgressText.Text = "READY";
			this.Log("[SYSTEM] ByFastFlag Cyberpunk Edition v2.0");
			this.Log("[SYSTEM] Loading service modules...");
			this.InitializeService();
			this.LoadPreviousFiles();
			Console.WriteLine("[MainWindow] ===== INITIALIZATION COMPLETE =====");
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00004034 File Offset: 0x00002234
		private void InitializeService()
		{
			try
			{
				Console.WriteLine("[MainWindow] Initializing FFlagService...");
				string[] possibleDllPaths = new string[]
				{
					Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFlagTool.dll"),
					Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "FFlagTool.dll"),
					Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "FFlagTool.dll")
				};
				string dllPath = "";
				foreach (string path in possibleDllPaths)
				{
					bool flag = File.Exists(path);
					if (flag)
					{
						dllPath = path;
						this.Log("[SYSTEM] DLL located: " + Path.GetFileName(path));
						break;
					}
				}
				bool flag2 = string.IsNullOrEmpty(dllPath);
				if (flag2)
				{
					this.Log("[SYSTEM] FFlagTool.dll not found");
					this.Log("[SYSTEM] Please place FFlagTool.dll in the application directory");
					this.UpdateStatus("DLL NOT FOUND", Colors.Red);
					this.DisableButtons();
				}
				else
				{
					this._fflagService = new FFlagService(this);
					bool flag3 = this._fflagService != null && this._fflagService.IsDllLoaded();
					if (flag3)
					{
						this.Log("[SYSTEM] Service initialized successfully");
						this.UpdateStatus("READY", Colors.LimeGreen);
						this.EnableAllButtons();
						Console.WriteLine("[MainWindow] Service initialized successfully");
					}
					else
					{
						this.Log("[SYSTEM] Service failed to initialize");
						this.UpdateStatus("SERVICE ERROR", Colors.Red);
						this.DisableButtons();
						Console.WriteLine("[MainWindow] Service initialization failed");
					}
				}
			}
			catch (Exception ex)
			{
				this.Log("[SYSTEM] Error: " + ex.Message);
				this.UpdateStatus("ERROR", Colors.Red);
				this.DisableButtons();
				Console.WriteLine("[MainWindow] Error in InitializeService: " + ex.Message);
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x0000422C File Offset: 0x0000242C
		private void LoadPreviousFiles()
		{
			try
			{
				bool flag = !string.IsNullOrEmpty(this._settingsService.LastFlagsPath) && File.Exists(this._settingsService.LastFlagsPath);
				if (flag)
				{
					this._loadedFlagsPath = this._settingsService.LastFlagsPath;
					this.Log("[SYSTEM] Auto-loading previous flags file: " + Path.GetFileName(this._loadedFlagsPath));
					bool flag2 = this._fflagService != null && this._fflagService.IsDllLoaded();
					if (flag2)
					{
						this._fflagService.LoadFlags(this._loadedFlagsPath);
						ValueTuple<int, int> counts = this._fflagService.GetLoadedCounts();
						this._flagsCount = counts.Item1;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SUCCESS] Auto-loaded ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this._flagsCount);
						defaultInterpolatedStringHandler.AppendLiteral(" FFlags from previous session");
						this.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						this.UpdateFileInfo(this._loadedFlagsPath, this.OffsetsPathText.Text);
						this.UpdateCounts(this._flagsCount, this._offsetsCount, MainWindow.GetRobloxProcessCount());
					}
				}
				bool flag3 = !string.IsNullOrEmpty(this._settingsService.LastOffsetsPath) && File.Exists(this._settingsService.LastOffsetsPath);
				if (flag3)
				{
					this.Log("[SYSTEM] Auto-loading previous offsets file: " + Path.GetFileName(this._settingsService.LastOffsetsPath));
					bool flag4 = this._fflagService != null && this._fflagService.IsDllLoaded();
					if (flag4)
					{
						Task.Run(delegate()
						{
							MainWindow.<<LoadPreviousFiles>b__13_0>d <<LoadPreviousFiles>b__13_0>d = new MainWindow.<<LoadPreviousFiles>b__13_0>d();
							<<LoadPreviousFiles>b__13_0>d.<>t__builder = AsyncTaskMethodBuilder.Create();
							<<LoadPreviousFiles>b__13_0>d.<>4__this = this;
							<<LoadPreviousFiles>b__13_0>d.<>1__state = -1;
							<<LoadPreviousFiles>b__13_0>d.<>t__builder.Start<MainWindow.<<LoadPreviousFiles>b__13_0>d>(ref <<LoadPreviousFiles>b__13_0>d);
							return <<LoadPreviousFiles>b__13_0>d.<>t__builder.Task;
						});
					}
				}
				bool flag5 = this._settingsService.HasSavedData();
				if (flag5)
				{
					this.Log("[SYSTEM] Previous injection data found in memory");
				}
			}
			catch (Exception ex)
			{
				this.Log("[SYSTEM] Error loading previous files: " + ex.Message);
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00004428 File Offset: 0x00002628
		private static void SetupAssemblyResolution()
		{
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, [Nullable(1)] ResolveEventArgs args)
			{
				try
				{
					AssemblyName assemblyName = new AssemblyName(args.Name);
					string assemblyShortName = assemblyName.Name ?? "";
					Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == assemblyShortName);
					bool flag = loadedAssembly != null;
					if (flag)
					{
						return loadedAssembly;
					}
					string assemblyFile = assemblyShortName + ".dll";
					string[] paths = new string[]
					{
						Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFile),
						Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", assemblyFile),
						Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", assemblyFile)
					};
					foreach (string path in paths)
					{
						bool flag2 = File.Exists(path);
						if (flag2)
						{
							return Assembly.LoadFrom(path);
						}
					}
				}
				catch
				{
				}
				return null;
			};
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004458 File Offset: 0x00002658
		[NullableContext(1)]
		private void LoadFlagsBtn_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				bool flag = this._fflagService == null || !this._fflagService.IsDllLoaded();
				if (flag)
				{
					this.Log("[ERROR] Service not loaded");
					this.UpdateStatus("SERVICE ERROR", Colors.Red);
				}
				else
				{
					OpenFileDialog dialog = new OpenFileDialog
					{
						Title = "Select FFlags File",
						Filter = "All files (*.*)|*.*|JSON files (*.json)|*.json|Text files (*.txt)|*.txt",
						DefaultExt = ".*",
						Multiselect = false
					};
					bool valueOrDefault = dialog.ShowDialog().GetValueOrDefault();
					if (valueOrDefault)
					{
						this._loadedFlagsPath = dialog.FileName;
						this._settingsService.LastFlagsPath = this._loadedFlagsPath;
						this.UpdateStatus("LOADING", Colors.Yellow);
						this.Log("[FILE] Loading: " + Path.GetFileName(this._loadedFlagsPath));
						this._fflagService.LoadFlags(this._loadedFlagsPath);
						ValueTuple<int, int> counts = this._fflagService.GetLoadedCounts();
						this._flagsCount = counts.Item1;
						this.UpdateStatus("READY", Colors.LimeGreen);
						this.UpdateFileInfo(this._loadedFlagsPath, this.OffsetsPathText.Text);
						this.UpdateCounts(this._flagsCount, this._offsetsCount, MainWindow.GetRobloxProcessCount());
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
						defaultInterpolatedStringHandler.AppendLiteral("[SUCCESS] Loaded ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this._flagsCount);
						defaultInterpolatedStringHandler.AppendLiteral(" FFlags");
						this.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						this._hasInjectedCurrentSession = false;
					}
				}
			}
			catch (Exception ex)
			{
				this.UpdateStatus("ERROR", Colors.Red);
				this.Log("[ERROR] " + ex.Message);
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00004640 File Offset: 0x00002840
		[NullableContext(1)]
		[DebuggerStepThrough]
		private void LoadOffsetsBtn_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.<LoadOffsetsBtn_Click>d__16 <LoadOffsetsBtn_Click>d__ = new MainWindow.<LoadOffsetsBtn_Click>d__16();
			<LoadOffsetsBtn_Click>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<LoadOffsetsBtn_Click>d__.<>4__this = this;
			<LoadOffsetsBtn_Click>d__.sender = sender;
			<LoadOffsetsBtn_Click>d__.e = e;
			<LoadOffsetsBtn_Click>d__.<>1__state = -1;
			<LoadOffsetsBtn_Click>d__.<>t__builder.Start<MainWindow.<LoadOffsetsBtn_Click>d__16>(ref <LoadOffsetsBtn_Click>d__);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00004688 File Offset: 0x00002888
		private void SetupAutoInjectionTimer()
		{
			this._injectionTimer.Interval = TimeSpan.FromSeconds(2.0);
			this._injectionTimer.Tick += delegate([Nullable(2)] object s, EventArgs e)
			{
				MainWindow.<<SetupAutoInjectionTimer>b__17_0>d <<SetupAutoInjectionTimer>b__17_0>d = new MainWindow.<<SetupAutoInjectionTimer>b__17_0>d();
				<<SetupAutoInjectionTimer>b__17_0>d.<>t__builder = AsyncVoidMethodBuilder.Create();
				<<SetupAutoInjectionTimer>b__17_0>d.<>4__this = this;
				<<SetupAutoInjectionTimer>b__17_0>d.s = s;
				<<SetupAutoInjectionTimer>b__17_0>d.e = e;
				<<SetupAutoInjectionTimer>b__17_0>d.<>1__state = -1;
				<<SetupAutoInjectionTimer>b__17_0>d.<>t__builder.Start<MainWindow.<<SetupAutoInjectionTimer>b__17_0>d>(ref <<SetupAutoInjectionTimer>b__17_0>d);
			};
			this._injectionTimer.Start();
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000046D4 File Offset: 0x000028D4
		[NullableContext(1)]
		[DebuggerStepThrough]
		private Task CheckAndAutoInjectAsync()
		{
			MainWindow.<CheckAndAutoInjectAsync>d__18 <CheckAndAutoInjectAsync>d__ = new MainWindow.<CheckAndAutoInjectAsync>d__18();
			<CheckAndAutoInjectAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<CheckAndAutoInjectAsync>d__.<>4__this = this;
			<CheckAndAutoInjectAsync>d__.<>1__state = -1;
			<CheckAndAutoInjectAsync>d__.<>t__builder.Start<MainWindow.<CheckAndAutoInjectAsync>d__18>(ref <CheckAndAutoInjectAsync>d__);
			return <CheckAndAutoInjectAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00004718 File Offset: 0x00002918
		[NullableContext(1)]
		[DebuggerStepThrough]
		private Task PerformAutoInjectionAsync()
		{
			MainWindow.<PerformAutoInjectionAsync>d__19 <PerformAutoInjectionAsync>d__ = new MainWindow.<PerformAutoInjectionAsync>d__19();
			<PerformAutoInjectionAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<PerformAutoInjectionAsync>d__.<>4__this = this;
			<PerformAutoInjectionAsync>d__.<>1__state = -1;
			<PerformAutoInjectionAsync>d__.<>t__builder.Start<MainWindow.<PerformAutoInjectionAsync>d__19>(ref <PerformAutoInjectionAsync>d__);
			return <PerformAutoInjectionAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000475C File Offset: 0x0000295C
		[NullableContext(1)]
		private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
		{
			TextBox logTextBox = this.LogTextBox;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("[");
			defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now, "HH:mm:ss");
			defaultInterpolatedStringHandler.AppendLiteral("] LOG CLEARED\n");
			logTextBox.Text = defaultInterpolatedStringHandler.ToStringAndClear();
			this.Log("[SYSTEM] Log cleared");
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000047C0 File Offset: 0x000029C0
		[NullableContext(1)]
		private void LicenseInfoButton_Click(object sender, RoutedEventArgs e)
		{
			string licenseInfo = LicenseManager.GetLicenseInfo();
			MessageBox.Show(licenseInfo, "License Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000047E3 File Offset: 0x000029E3
		[NullableContext(1)]
		private void Minimize_Click(object sender, RoutedEventArgs e)
		{
			base.WindowState = WindowState.Minimized;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000047EE File Offset: 0x000029EE
		[NullableContext(1)]
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			DispatcherTimer statusTimer = this._statusTimer;
			if (statusTimer != null)
			{
				statusTimer.Stop();
			}
			DispatcherTimer clockTimer = this._clockTimer;
			if (clockTimer != null)
			{
				clockTimer.Stop();
			}
			DispatcherTimer injectionTimer = this._injectionTimer;
			if (injectionTimer != null)
			{
				injectionTimer.Stop();
			}
			base.Close();
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00004830 File Offset: 0x00002A30
		private static int GetRobloxProcessCount()
		{
			int result;
			try
			{
				result = Process.GetProcessesByName("RobloxPlayerBeta").Length;
			}
			catch
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00004868 File Offset: 0x00002A68
		[NullableContext(1)]
		private void Log(string message)
		{
			base.Dispatcher.Invoke(delegate()
			{
				TextBoxBase logTextBox = this.LogTextBox;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 2);
				defaultInterpolatedStringHandler.AppendLiteral("[");
				defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now, "HH:mm:ss");
				defaultInterpolatedStringHandler.AppendLiteral("] ");
				defaultInterpolatedStringHandler.AppendFormatted(message);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				logTextBox.AppendText(defaultInterpolatedStringHandler.ToStringAndClear());
				this.LogTextBox.ScrollToEnd();
			});
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000048A4 File Offset: 0x00002AA4
		[NullableContext(1)]
		private void UpdateStatus(string status, Color color)
		{
			base.Dispatcher.Invoke(delegate()
			{
				this.StatusText.Text = status;
				this.StatusDot.Fill = new SolidColorBrush(color);
				bool flag = color == Colors.LimeGreen;
				if (flag)
				{
					this.StatusDot.Width = 12.0;
					this.StatusDot.Height = 12.0;
					this.StatusDot.Opacity = 1.0;
				}
				else
				{
					this.StatusDot.Width = 10.0;
					this.StatusDot.Height = 10.0;
					this.StatusDot.Opacity = 0.8;
				}
			});
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000048E8 File Offset: 0x00002AE8
		[NullableContext(1)]
		private void UpdateFileInfo(string flagsPath, string offsetsPath)
		{
			base.Dispatcher.Invoke(delegate()
			{
				this.FlagsPathText.Text = (string.IsNullOrEmpty(flagsPath) ? "Not loaded" : Path.GetFileName(flagsPath));
				this.OffsetsPathText.Text = offsetsPath;
			});
		}

		// Token: 0x06000043 RID: 67 RVA: 0x0000492C File Offset: 0x00002B2C
		private void UpdateCounts(int flagsCount, int offsetsCount, int robloxCount)
		{
			base.Dispatcher.Invoke(delegate()
			{
				this.FlagsCountText.Text = flagsCount.ToString();
				this.OffsetsCountText.Text = offsetsCount.ToString();
				this.RobloxCountText.Text = robloxCount.ToString();
			});
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004974 File Offset: 0x00002B74
		private void SetupStatusAnimation()
		{
			this._statusTimer.Interval = TimeSpan.FromSeconds(0.5);
			this._statusTimer.Tick += delegate([Nullable(2)] object s, EventArgs e)
			{
				this._statusDotState = !this._statusDotState;
				this.StatusDot.Opacity = (this._statusDotState ? 1.0 : 0.3);
			};
			this._statusTimer.Start();
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000049C0 File Offset: 0x00002BC0
		private void SetupClock()
		{
			this._clockTimer.Interval = TimeSpan.FromSeconds(1.0);
			this._clockTimer.Tick += delegate([Nullable(2)] object s, EventArgs e)
			{
				this.TimeDisplay.Text = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
			};
			this._clockTimer.Start();
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00004A0C File Offset: 0x00002C0C
		private void DisableButtons()
		{
			this.LoadFlagsBtn.IsEnabled = false;
			this.LoadOffsetsBtn.IsEnabled = false;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00004A29 File Offset: 0x00002C29
		private void EnableAllButtons()
		{
			this.LoadFlagsBtn.IsEnabled = true;
			this.LoadOffsetsBtn.IsEnabled = true;
		}

		// Token: 0x0400001B RID: 27
		[Nullable(2)]
		private FFlagService _fflagService;

		// Token: 0x0400001C RID: 28
		[Nullable(1)]
		private SettingsService _settingsService;

		// Token: 0x0400001D RID: 29
		[Nullable(1)]
		private readonly DispatcherTimer _statusTimer;

		// Token: 0x0400001E RID: 30
		[Nullable(1)]
		private readonly DispatcherTimer _clockTimer;

		// Token: 0x0400001F RID: 31
		[Nullable(1)]
		private readonly DispatcherTimer _injectionTimer;

		// Token: 0x04000020 RID: 32
		private bool _statusDotState = true;

		// Token: 0x04000021 RID: 33
		private int _flagsCount = 0;

		// Token: 0x04000022 RID: 34
		private int _offsetsCount = 0;

		// Token: 0x04000023 RID: 35
		[Nullable(1)]
		private string _loadedFlagsPath = "";

		// Token: 0x04000024 RID: 36
		private bool _hasInjectedCurrentSession = false;

		// Token: 0x04000025 RID: 37
		private int _robuxProcessCheckCount = 0;
	}
}
