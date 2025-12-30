using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ByFastFlag
{
	// Token: 0x02000005 RID: 5
	public static class DllTester
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002818 File Offset: 0x00000A18
		public static void TestFFlagToolDLL()
		{
			Console.WriteLine("=== TESTING FFlagTool.dll ===");
			string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", "FFlagTool.dll");
			Console.WriteLine("DLL Path: " + dllPath);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Exists: ");
			defaultInterpolatedStringHandler.AppendFormatted<bool>(File.Exists(dllPath));
			Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
			bool flag = !File.Exists(dllPath);
			if (flag)
			{
				Console.WriteLine("✗ ERROR: DLL file not found!");
				Console.WriteLine("Make sure FFlagTool.dll is in: " + Path.GetDirectoryName(dllPath));
			}
			else
			{
				try
				{
					Console.WriteLine("\n[1] Loading assembly...");
					Assembly assembly = Assembly.LoadFrom(dllPath);
					Console.WriteLine("✓ Loaded: " + assembly.FullName);
					Console.WriteLine("\n[2] Looking for types...");
					try
					{
						Type[] types = assembly.GetTypes();
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("✓ Found ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(types.Length);
						defaultInterpolatedStringHandler2.AppendLiteral(" types:");
						Console.WriteLine(defaultInterpolatedStringHandler2.ToStringAndClear());
						foreach (Type type in types)
						{
							Console.WriteLine("\n  Type: " + type.FullName);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(13, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("    IsClass: ");
							defaultInterpolatedStringHandler3.AppendFormatted<bool>(type.IsClass);
							Console.WriteLine(defaultInterpolatedStringHandler3.ToStringAndClear());
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(14, 1);
							defaultInterpolatedStringHandler4.AppendLiteral("    IsPublic: ");
							defaultInterpolatedStringHandler4.AppendFormatted<bool>(type.IsPublic);
							Console.WriteLine(defaultInterpolatedStringHandler4.ToStringAndClear());
							bool isClass = type.IsClass;
							if (isClass)
							{
								ConstructorInfo[] constructors = type.GetConstructors();
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(18, 1);
								defaultInterpolatedStringHandler5.AppendLiteral("    Constructors: ");
								defaultInterpolatedStringHandler5.AppendFormatted<int>(constructors.Length);
								Console.WriteLine(defaultInterpolatedStringHandler5.ToStringAndClear());
								MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(20, 1);
								defaultInterpolatedStringHandler6.AppendLiteral("    Public Methods: ");
								defaultInterpolatedStringHandler6.AppendFormatted<int>(methods.Length);
								Console.WriteLine(defaultInterpolatedStringHandler6.ToStringAndClear());
								foreach (MethodInfo method in methods)
								{
									Console.WriteLine("      - " + method.Name);
								}
							}
						}
					}
					catch (ReflectionTypeLoadException ex)
					{
						Console.WriteLine("✗ Error loading all types: " + ex.Message);
						Console.WriteLine("Loader exceptions:");
						foreach (Exception loaderEx in ex.LoaderExceptions)
						{
							bool flag2 = loaderEx != null;
							if (flag2)
							{
								Console.WriteLine("  - " + loaderEx.Message);
							}
						}
						bool flag3 = ex.Types != null;
						if (flag3)
						{
							Console.WriteLine("\nPartially loaded types:");
							foreach (Type type2 in ex.Types)
							{
								bool flag4 = type2 != null;
								if (flag4)
								{
									Console.WriteLine("  - " + type2.FullName);
								}
							}
						}
					}
				}
				catch (Exception ex2)
				{
					Console.WriteLine("✗ FATAL ERROR: " + ex2.Message);
					Console.WriteLine("Stack Trace: " + ex2.StackTrace);
				}
			}
		}
	}
}
