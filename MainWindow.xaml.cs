using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Panuon.UI.Silver;
using KMCCC.Launcher;
using KMCCC.Authentication;
using System.IO;
using ProjBobcat.DefaultComponent.Launch;

namespace MCLauncher
{
    public partial class MainWindow : WindowX
    {
        public static void InitLauncherCore()
        {
            var clientToken = new Guid("88888888-8888-8888-8888-888888888888");
            var rootPath = ".minecraft";
            core = new DefaultGameCore
            {
                ClientToken = clientToken,
                RootPath = rootPath,
                VersionLocator = new DefaultVersionLocator(rootPath, clientToken)
                {
                    LauncherProfileParser = new DefaultLauncherProfileParser(rootPath, clientToken)
                }
            };
        }
        LoginUI.Offline Offline = new LoginUI.Offline();
        LoginUI.Online Online = new LoginUI.Online();
        LoginUI.Microsoft Microsoft = new LoginUI.Microsoft();
        public int launchMode = 1;
        public static DefaultGameCore core;
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        public static LauncherCore Core = LauncherCore.Create();
        public MainWindow()
        {
            InitializeComponent();
            List<string> javaList = new List<string>();
            List<string> memoryList = new List<string>();
            int memorynum = 512;
            for(int i = 0; i < 7; i++)
            {
                memoryList.Add(Convert.ToString(memorynum) + "M");
                memorynum *= 2;
            }
            memoryCombo.ItemsSource = memoryList;
            memoryCombo.SelectedItem = memoryCombo.Items[2];
            string path = @"emcl.config";
            if (File.Exists(path))
            {
                try
                {
                    string[] contents = File.ReadAllLines(path);
                    memoryCombo.SelectedItem = contents[0];
                    versionCombo.SelectedItem = contents[1];
                }
                catch (Exception ex)
                {
                    MessageBoxX.Show(ex.Message, "错误");
                }
            }
            else
            {
                try
                {
                    FileStream newConfig = new FileStream(path, FileMode.Create);
                    newConfig.Close();
                }
                catch (Exception ex)
                {
                    MessageBoxX.Show(ex.Message, "错误");
                }
            }
            foreach (string i in KMCCC.Tools.SystemTools.FindJava())
            {
                javaList.Add(i);
            }
            foreach (string i in ProjBobcat.Class.Helper.SystemInfoHelper.FindJava())
            {
                javaList.Add(i);
            }
            javaList.Add(tools.GetJavaPath());
            javaCombo.ItemsSource = javaList;
            try
            {
                versionCombo.SelectedItem = versionCombo.Items[0];
            }
            catch
            {
                MessageBoxX.Show("找不到游戏版本，请下载后再打开启动器","提示");
            }
            try
            {
                javaCombo.SelectedItem = javaCombo.Items[0];
            }
            catch
            {
                MessageBoxX.Show("找不到Java8或Java16，请去java.com重新安装Java\n请不要使用“绿色版Java等非正常手动安装的Java，这会导致启动器无法识别", "提示");
            }
        }
        public async void GameStart()
        {
            try
            {
                var ver = (KMCCC.Launcher.Version)versionCombo.SelectedItem;
                using (StreamWriter config = new StreamWriter("emcl.config"))
                {
                    config.WriteLine(memoryCombo.SelectedItem);
                    config.WriteLine(ver);
                    config.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBoxX.Show(ex.Message, "错误");
            }
            LaunchOptions launchOptions = new LaunchOptions();
            switch (launchMode)
            {
                case 1:
                    launchOptions.Authenticator = new OfflineAuthenticator(Offline.IDText.Text);
                    break;
                case 2:
                    launchOptions.Authenticator = new YggdrasilLogin(Online.Email.Text,Online.Password.Password, false);
                    break;
            }
            launchOptions.MaxMemory = Convert.ToInt32(memoryCombo.SelectedItem);
            if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty && (Offline.IDText.Text != string.Empty || (Online.Email.Text != string.Empty && Online.Password.Password != string.Empty) && memoryCombo.Text != string.Empty))
            {
                try
                {
                    if(launchMode!=3)
                    {
                        Core.JavaPath = javaCombo.Text;
                        var ver = (KMCCC.Launcher.Version)versionCombo.SelectedItem;
                        launchOptions.Version = ver;
                        var result = Core.Launch(launchOptions);
                        if (!result.Success)
                        {
                            switch (result.ErrorType)
                            {
                                case ErrorType.NoJAVA:
                                    MessageBoxX.Show("找不到Java8或Java16，请去java.com重新安装Java\n请不要使用“绿色版Java等非正常手动安装的Java，这会导致启动器无法识别：" + result.ErrorMessage, "游戏启动失败");
                                    break;
                                case ErrorType.AuthenticationFailed:
                                    MessageBoxX.Show("登录失败，请重试：" + result.ErrorMessage, "游戏启动失败");
                                    break;
                                case ErrorType.UncompressingFailed:
                                    MessageBoxX.Show("找不到资源文件：" + result.ErrorMessage, "游戏启动失败");
                                    break;
                                default:
                                    MessageBoxX.Show("未知的错误：" + result.ErrorMessage, "游戏启动失败");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
                            var v = Microsoft.wb.Source.ToString().Replace(microsoftAPIs.cutUri, string.Empty);
                            var t = Task.Run(() => {
                                return microsoftAPIs.GetAccessTokenAsync(v, false).Result;
                            });
                            await t;
                            var v1 = microsoftAPIs.GetAllThings(t.Result.access_token, false);
                            SquareMinecraftLauncher.Minecraft.Game game = new SquareMinecraftLauncher.Minecraft.Game();
                            await game.StartGame(versionCombo.Text, javaCombo.Text, Convert.ToInt32(memoryCombo.Text), v1.name, v1.uuid, v1.mcToken, string.Empty, string.Empty);
                        }
                        catch (Exception e)
                        {
                            MessageBoxX.Show("未知的错误：" + e, "游戏启动失败");
                        }
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("程序出现严重故障，请将此问题反馈作者。错误等级：III，详细信息：\n"+e.Message, "警告");
                }
            }
            else
            {
                MessageBoxX.Show("信息未填写完整", "错误");
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GameStart();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {Content = Offline};
            Color color = (Color)ColorConverter.ConvertFromString("#FF7DCDFF");
            Start.Background = new SolidColorBrush(color);
           launchMode = 1;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            { Content = Online };
            Color color = (Color)ColorConverter.ConvertFromString("#FF7DFFCD");
            Start.Background = new SolidColorBrush(color);
            launchMode = 2;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            { Content = Microsoft };
            Color color = (Color)ColorConverter.ConvertFromString("#FFFF7D7D");
            Start.Background = new SolidColorBrush(color);
            launchMode = 3;
        }

        private void reloadver_Click(object sender, RoutedEventArgs e)
        {
            var versions = Core.GetVersions().ToArray();
            versionCombo.ItemsSource = versions;
            try
            {
                versionCombo.SelectedItem = versionCombo.Items[0];
            }
            catch
            {
                MessageBoxX.Show("找不到游戏版本，请重试", "提示");
            }
        }

        private void reloadjava_Click(object sender, RoutedEventArgs e)
        {
            List<string> javaList = new List<string>(); ;
            foreach (string i in KMCCC.Tools.SystemTools.FindJava())
            {
                javaList.Add(i);
            }
            javaList.Add(tools.GetJavaPath());
            javaCombo.ItemsSource = javaList;
            javaCombo.SelectedItem = javaCombo.Items[0];
        }

        private void loadconfig_Click(object sender, RoutedEventArgs e)
        {
            string path = @"emcl.config";
            if (File.Exists(path))
            {
                try
                {
                    string[] contents = File.ReadAllLines(path);
                    memoryCombo.SelectedItem = contents[0];
                }
                catch (Exception ex) {
                    MessageBoxX.Show(ex.Message, "错误");
                }
                try
                {
                    string[] contents = File.ReadAllLines(path);
                    versionCombo.SelectedItem = contents[1];
                }
                catch (Exception ex)
                {
                    MessageBoxX.Show(ex.Message, "错误");
                }
            }
            else
            {
                try
                {
                    FileStream newConfig = new FileStream(path, FileMode.Create);
                    newConfig.Close();
                }
                catch (Exception ex)
                {
                    MessageBoxX.Show(ex.Message, "错误");
                }
            }
        }

        private void saveconfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ver = (KMCCC.Launcher.Version)versionCombo.SelectedItem;
                using (StreamWriter config = new StreamWriter("emcl.config"))
                {
                    config.WriteLine(memoryCombo.SelectedItem);
                    config.WriteLine(ver);
                    config.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBoxX.Show(ex.Message, "错误");
            }
        }
    }
}