using System;
using System.Windows;

namespace UpdateWPF
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			// 获取命令行参数
			string[] args = Environment.GetCommandLineArgs();
			//MessageBox.Show(str);
			string startMes =args.Length>1? args[1] : "";
			var MainWindow = new MainWindow(startMes);
			MainWindow.Show();
		}
	}
}