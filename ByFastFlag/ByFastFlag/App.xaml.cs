using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace ByFastFlag
{
	// Token: 0x02000003 RID: 3
	[NullableContext(1)]
	[Nullable(0)]
	public partial class App : Application
	{
		// Token: 0x0600000D RID: 13 RVA: 0x000024C7 File Offset: 0x000006C7
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Console.WriteLine("[App] ===== APPLICATION START =====");
			this.StartActivationFlowAsync();
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024DC File Offset: 0x000006DC
		[DebuggerStepThrough]
		private Task StartActivationFlowAsync()
		{
			App.<StartActivationFlowAsync>d__2 <StartActivationFlowAsync>d__ = new App.<StartActivationFlowAsync>d__2();
			<StartActivationFlowAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<StartActivationFlowAsync>d__.<>4__this = this;
			<StartActivationFlowAsync>d__.<>1__state = -1;
			<StartActivationFlowAsync>d__.<>t__builder.Start<App.<StartActivationFlowAsync>d__2>(ref <StartActivationFlowAsync>d__);
			return <StartActivationFlowAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002520 File Offset: 0x00000720
		[DebuggerStepThrough]
		private Task<bool> ShowActivationWindowAsync()
		{
			App.<ShowActivationWindowAsync>d__3 <ShowActivationWindowAsync>d__ = new App.<ShowActivationWindowAsync>d__3();
			<ShowActivationWindowAsync>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<ShowActivationWindowAsync>d__.<>4__this = this;
			<ShowActivationWindowAsync>d__.<>1__state = -1;
			<ShowActivationWindowAsync>d__.<>t__builder.Start<App.<ShowActivationWindowAsync>d__3>(ref <ShowActivationWindowAsync>d__);
			return <ShowActivationWindowAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002564 File Offset: 0x00000764
		private void ShowMainWindow()
		{
			Console.WriteLine("[App] Creating MainWindow...");
			try
			{
				base.MainWindow = new MainWindow();
				Console.WriteLine("[App] MainWindow created successfully");
				base.MainWindow.Show();
				Console.WriteLine("[App] MainWindow.Show() called successfully");
				base.MainWindow.Activate();
				base.MainWindow.Focus();
				Console.WriteLine("[App] MainWindow activated and focused");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[App] ERROR showing MainWindow: " + ex.Message);
				Console.WriteLine("[App] Stack trace: " + ex.StackTrace);
				MessageBox.Show("Failed to start application: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				base.Shutdown(1);
			}
		}

		// Token: 0x0400000A RID: 10
		[Nullable(2)]
		private ActivationWindow _activationWindow;
	}
}
