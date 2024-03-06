using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace UpdateWPF
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow(string arg)
		{
			InitializeComponent();
			StartUpdate(arg);
		}
		async void StartUpdate(string arg)
		{
			try
			{
				var paths = arg.Split('|');
				var path = await LoadFile(paths[0], progressBar1);
				await ExtractZipFileAsync(path, paths[1], progressBar1);
				label1.Content = "更新成功。。。";
				if (File.Exists(paths[2]))
				{
					Process.Start(paths[2]);
				}
				File.Delete(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				Application.Current.Shutdown();
			}
		}
		private async Task<string> LoadFile(string fileUrl, ProgressBar progressBar)
		{
			string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyApp", "Cache"); // 缓存目录
																																			   // 确保缓存目录存在
			if (!Directory.Exists(cacheDirectory))
			{
				Directory.CreateDirectory(cacheDirectory);
			}
			string fileName = Path.GetFileName(fileUrl); // 从 URL 中获取文件名
			string filePath = Path.Combine(cacheDirectory, fileName); // 构建文件保存路径
			var webClient = new WebClient();
			webClient.DownloadProgressChanged += (s, e) =>
			{
				progressBar.Value = e.ProgressPercentage;
				label1.Content = $"下载中{e.ProgressPercentage}%...";
			};
			await webClient.DownloadFileTaskAsync(fileUrl, filePath);
			return filePath;
		}
		private async Task ExtractZipFileAsync(string zipFilePath, string extractToDirectory, ProgressBar progressBar)
		{
			try
			{
				// 创建目标目录
				Directory.CreateDirectory(extractToDirectory);
				using (var fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
				using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
				{
					int totalFiles = archive.Entries.Count;
					int extractedFiles = 0;
					foreach (var entry in archive.Entries)
					{
						string entryFullName = System.IO.Path.Combine(extractToDirectory, entry.FullName);
						// 如果是目录，则创建目录
						if (entry.FullName.EndsWith("/"))
						{
							Directory.CreateDirectory(entryFullName);
							continue;
						}
						// 如果是文件，则解压缩文件
						using (var entryStream = entry.Open())
						using (var outputStream = new FileStream(entryFullName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
						{
							await entryStream.CopyToAsync(outputStream);
							extractedFiles++;
							int progressPercentage = (int)((double)extractedFiles / totalFiles * 100);
							progressBar.Value = progressPercentage;
							label1.Content = $"更新中{progressPercentage}%";
						}
					}
				}
				Console.WriteLine("解压缩完成。");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"解压缩文件时发生错误：{ex.Message}");
			}
		}
	}
}
