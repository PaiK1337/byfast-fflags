using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace ByFastFlag
{
	// Token: 0x02000002 RID: 2
	public partial class ActivationWindow : Window
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		// (remove) Token: 0x06000002 RID: 2 RVA: 0x00002080 File Offset: 0x00000280
		[Nullable(2)]
		[method: NullableContext(2)]
		[Nullable(2)]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler ActivationSuccessful;

		// Token: 0x06000003 RID: 3 RVA: 0x000020B8 File Offset: 0x000002B8
		public ActivationWindow()
		{
			this.InitializeComponent();
			base.MouseDown += delegate(object s, MouseButtonEventArgs e)
			{
				bool flag = e.LeftButton == MouseButtonState.Pressed;
				if (flag)
				{
					base.DragMove();
				}
			};
			this.LicenseKeyTextBox.TextChanged += this.LicenseKeyTextBox_TextChanged;
			this.ActivateButton.Click += this.ActivateButton_Click;
			this.BuyButton.Click += this.BuyButton_Click;
			this.CloseButton.Click += delegate(object s, RoutedEventArgs e)
			{
				base.Close();
			};
			this.UpdateActivateButton();
			this.LicenseKeyTextBox.Focus();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000215C File Offset: 0x0000035C
		[NullableContext(1)]
		private void LicenseKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = this.LicenseKeyTextBox.Text.ToUpper();
			text = Regex.Replace(text, "[^A-Z0-9]", "");
			bool flag = text.Length > 25;
			if (flag)
			{
				text = text.Substring(0, 25);
			}
			bool flag2 = text.Length > 0;
			if (flag2)
			{
				string formatted = "";
				for (int i = 0; i < text.Length; i++)
				{
					bool flag3 = i > 0 && i % 5 == 0;
					if (flag3)
					{
						formatted += "-";
					}
					ReadOnlySpan<char> str = formatted;
					char c = text[i];
					formatted = str + new ReadOnlySpan<char>(ref c);
				}
				bool flag4 = this.LicenseKeyTextBox.Text != formatted;
				if (flag4)
				{
					this.LicenseKeyTextBox.Text = formatted;
					this.LicenseKeyTextBox.CaretIndex = formatted.Length;
				}
			}
			this.UpdateActivateButton();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002258 File Offset: 0x00000458
		private void UpdateActivateButton()
		{
			string key = this.LicenseKeyTextBox.Text.Trim();
			bool isValidFormat = key.Length == 29 && key[5] == '-' && key[11] == '-' && key[17] == '-' && key[23] == '-';
			this.ActivateButton.IsEnabled = (isValidFormat && !string.IsNullOrWhiteSpace(key));
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000022D0 File Offset: 0x000004D0
		[NullableContext(1)]
		[DebuggerStepThrough]
		private void ActivateButton_Click(object sender, RoutedEventArgs e)
		{
			ActivationWindow.<ActivateButton_Click>d__6 <ActivateButton_Click>d__ = new ActivationWindow.<ActivateButton_Click>d__6();
			<ActivateButton_Click>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<ActivateButton_Click>d__.<>4__this = this;
			<ActivateButton_Click>d__.sender = sender;
			<ActivateButton_Click>d__.e = e;
			<ActivateButton_Click>d__.<>1__state = -1;
			<ActivateButton_Click>d__.<>t__builder.Start<ActivationWindow.<ActivateButton_Click>d__6>(ref <ActivateButton_Click>d__);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002318 File Offset: 0x00000518
		[NullableContext(1)]
		private void BuyButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "https://discord.gg/H4X7FjuC36",
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to open Discord: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000237C File Offset: 0x0000057C
		[NullableContext(1)]
		private void ShowStatus(string message, string color)
		{
			base.Dispatcher.Invoke(delegate()
			{
				this.StatusText.Text = message;
				this.StatusMessage.Background = (Brush)new BrushConverter().ConvertFromString(color);
				this.StatusMessage.Visibility = Visibility.Visible;
			});
		}
	}
}
