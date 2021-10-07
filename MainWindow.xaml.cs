using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Panuon.UI.Silver;
using System.IO;
using ProjBobcat.DefaultComponent.Launch;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MCLauncher
{
    public partial class MainWindow : WindowX
    {
        public static void InitLauncherCore()
        {
            var clientToken = new Guid("88888888-8888-8888-8888-888888888888");
            var rootPath = ".minecraft\\";
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
        SquareMinecraftLauncher.Minecraft.Game game = new SquareMinecraftLauncher.Minecraft.Game();
        public static DefaultGameCore core;
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        public MainWindow()
        {
            InitializeComponent();
            InitLauncherCore();
            List<string> memoryList = new List<string>();
            int memorynum = 512;
            for (int i = 0; i < 7; i++)
            {
                memoryList.Add(Convert.ToString(memorynum) + "M");
                memorynum *= 2;
            }
            memoryCombo.ItemsSource = memoryList;
            memoryCombo.SelectedItem = memoryCombo.Items[2];
            javaCombo.ItemsSource = tools.GetJavaPath();
            var versions = tools.GetAllTheExistingVersion(); 
            versionCombo.ItemsSource = versions;
            try
            {
                versionCombo.SelectedItem = versionCombo.Items[0];
            }
            catch { }
            try
            {
                javaCombo.SelectedItem = javaCombo.Items[0];
            }
            catch { }
            string path = @"emcl.config";
            if (File.Exists(path))
            {
                try
                {
                    string[] contents = File.ReadAllLines(path);
                    memoryCombo.SelectedItem = contents[0];
                    versionCombo.Text = contents[1];
                    Offline.IDText.Text = contents[2];
                    Online.Email.Text = contents[3];
                    Online.Password.Password = contents[4];
                    javaCombo.SelectedItem = contents[5];
                }
                catch { }
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
        public async void GameStart()
        {
            string cutMemoryCombo = memoryCombo.Text.Substring(0, memoryCombo.Text.Length - 1);
            cutMemoryCombo = memoryCombo.Text.Remove(memoryCombo.Text.Length - 1, 1);
            int intCutMemoryCombo = Convert.ToInt32(cutMemoryCombo);
            switch (launchMode)
            {
                case 1:
                    if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty && Offline.IDText.Text != string.Empty)
                    {
                        try
                        {
                            await game.StartGame(versionCombo.Text, javaCombo.Text, intCutMemoryCombo, Offline.IDText.Text);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxX.Show("资源文件不完整：\n" + ex.Message,"游戏启动失败");
                        }
                    }
                    else
                    {
                        MessageBoxX.Show("请完整填写游戏信息", "游戏启动失败");
                    }
                    break;
                case 2:
                    if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty && Online.Email.Text != string.Empty && Online.Password.Password != string.Empty)
                    {
                        try {
                            await game.StartGame(versionCombo.Text, javaCombo.Text, intCutMemoryCombo, Online.Email.Text, Online.Password.Password);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "游戏启动失败");
                        }
                    }
                    else
                    {
                        MessageBoxX.Show("请完整填写游戏信息", "游戏启动失败");
                    }
                    break;
                case 3:
                    if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty)
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
                            await game.StartGame(versionCombo.Text, javaCombo.Text, intCutMemoryCombo, v1.name, v1.uuid, v1.mcToken, string.Empty, string.Empty);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "游戏启动失败");
                        }
                    }
                    else
                    {
                        MessageBoxX.Show("请完整填写游戏信息", "游戏启动失败");
                    }
                    break;
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
            var versions = tools.GetAllTheExistingVersion();
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
            javaCombo.ItemsSource = tools.GetJavaPath();
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
                    versionCombo.Text = contents[1];
                    Offline.IDText.Text = contents[2];
                    Online.Email.Text = contents[3];
                    Online.Password.Password = contents[4];
                    javaCombo.SelectedItem = contents[5];
                }
                catch { }
            }
        }
        private void saveconfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamWriter config = new StreamWriter("emcl.config"))
                {
                    config.WriteLine(memoryCombo.SelectedItem);
                    config.WriteLine(versionCombo.Text);
                    config.WriteLine(Offline.IDText.Text);
                    config.WriteLine(Online.Email.Text);
                    config.WriteLine(Online.Password.Password);
                    config.WriteLine(javaCombo.SelectedItem);
                    config.Close();
                    MessageBoxX.Show("保存成功","提示");
                }
            }
            catch (Exception ex)
            {
                MessageBoxX.Show(ex.Message, "错误");
            }
        }

        private void wtf_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxX.Show("这个命令是给一些高级玩家或整合包作者准备的，如果您需要正常的启动游戏，那么这个框不需要填写，您可以通过一些参数自定义启动方法，比如说：\n emcl -version[这里是要启动的版本名，如：1.15.2] \n -memory[这里是启动游戏所分配的内存，如：8192M] \n -offlinename[这里是离线模式用户名，如：abc123] \n -onlineemail[这里是正版模式邮箱(注意：输入了离线模式用户名后请勿输入正版模式信息，否则将会按照离线模式启动)，如：abc123@163.com] \n -onlinepassword[这里是正版模式密码(注意：不会自动加密为*，请在确保信息安全的情况下输入，如果发生盗号，本程序不承担任何法律责任)，如：abcabcabc] \n -[microsoftlogin](该参数没有任何后缀，只是用于标记为微软登录，注意：微软登录并不会自动登录，您需要在窗口中完成登录后才会自动启动游戏！) \n 下面是一个离线启动示例： \n emcl -version[1.15.2] -memory[2048M] -offlinename[efojug] \n 下面是一个正版启动示例： \n emcl -version[1.12.2] -memory[4096M] -onlineemail[efojug@efojug.com] -onlinepassword[123456] \n 下面是一个微软启动示例： \n emcl -version[1.14.4] -memory[16384M] -[microsoftlogin] \n 请注意，快速启动命令必须以emcl + 版本 + 内存 + 登录模式启动，如果不按此规则填写，可能会导致启动器崩溃", "提示");
        }

        private async void fastrun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string source = tb.Text;
                string[] temp = source.Split(']');
                List<string> list = new List<string>();
                foreach (string str in temp)
                {
                    if (str.Contains("["))
                    {
                        int index = str.IndexOf("[");
                        list.Add(str.Substring(index + 1, str.Length - index - 1));
                    }
                }
                string cutMemoryCombo = list[1].Substring(0, list[1].Length - 1);
                cutMemoryCombo = memoryCombo.Text.Remove(list[1].Length - 1, 1);
                int intCutMemoryCombo = Convert.ToInt32(cutMemoryCombo);
                if (!(list[2].Contains("@")) && !(list[2].Contains("microsoftlogin")))
                {
                    MessageBoxX.Show("Offline离线模式");
                    try
                    {
                        await game.StartGame(list[0], javaCombo.Text, intCutMemoryCombo, list[2]);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "游戏启动失败");
                    }
                }
                else if (list[2].Contains("@"))
                {
                    MessageBoxX.Show("Online正版验证");
                    try
                    {
                        await game.StartGame(list[0], javaCombo.Text, intCutMemoryCombo, list[2], list[3]);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "游戏启动失败");
                    }
                }
                else if (list[2].Contains("microsoftlogin"))
                {
                    MessageBoxX.Show("Microsoft微软登录");
                    try
                    {
                        microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
                        var v = Microsoft.wb.Source.ToString().Replace(microsoftAPIs.cutUri, string.Empty);
                        var t = Task.Run(() => {
                            return microsoftAPIs.GetAccessTokenAsync(v, false).Result;
                        });
                        await t;
                        var v1 = microsoftAPIs.GetAllThings(t.Result.access_token, false);
                        await game.StartGame(list[0], javaCombo.Text, intCutMemoryCombo, v1.name, v1.uuid, v1.mcToken, string.Empty, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "游戏启动失败");
                    }
                }
                for (int i = 0; i < temp.ToArray().Length - 1; i++)
                {
                    MessageBoxX.Show(list[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBoxX.Show("启动器崩溃了！错误等级II 错误信息：" + ex.Message);
            }
        }
    }
}
