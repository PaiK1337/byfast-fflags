using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ByFastFlag
{
	// Token: 0x02000004 RID: 4
	public static class DependencyLoader
	{
		// Token: 0x06000014 RID: 20 RVA: 0x000026B3 File Offset: 0x000008B3
		public static void Initialize()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			ResolveEventHandler value;
			if ((value = DependencyLoader.<>O.<0>__OnAssemblyResolve) == null)
			{
				value = (DependencyLoader.<>O.<0>__OnAssemblyResolve = new ResolveEventHandler(DependencyLoader.OnAssemblyResolve));
			}
			currentDomain.AssemblyResolve += value;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000026DC File Offset: 0x000008DC
		[NullableContext(2)]
		private static Assembly OnAssemblyResolve(object sender, [Nullable(1)] ResolveEventArgs args)
		{
			try
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				string assemblyShortName = assemblyName.Name ?? string.Empty;
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
					Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty, assemblyFile)
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
			catch (Exception)
			{
			}
			return null;
		}

		// Token: 0x0200000F RID: 15
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04000061 RID: 97
			public static ResolveEventHandler <0>__OnAssemblyResolve;
		}
	}
}
