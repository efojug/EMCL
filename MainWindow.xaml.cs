using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Panuon.UI.Silver;
using System.IO;
using ProjBobcat.DefaultComponent.Launch;
using SquareMinecraftLauncher.Core.OAuth;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
                            await game.StartGame(versionCombo.Text, javaCombo.SelectedValue.ToString(), intCutMemoryCombo, Offline.IDText.Text);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "启动失败");
                        }
                    }
                    else
                    {
                        MessageBoxX.Show("请完整填写信息", "启动失败");
                    }
                    break;
                case 2:
                    if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty && Online.Email.Text != string.Empty && Online.Password.Password != string.Empty)
                    {
                        try {
                            await game.StartGame(versionCombo.Text, javaCombo.SelectedValue.ToString(), intCutMemoryCombo, Online.Email.Text, Online.Password.Password);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "启动失败");
                        }
                    }
                    else
                    {
                        MessageBoxX.Show("请完整填写信息", "启动失败");
                    }
                    break;
                case 3:
                    if (versionCombo.Text != string.Empty && javaCombo.Text != string.Empty)
                    {
                        //第一次启动时可以直接这样调用，可以直接获取Minecraft的Token
                        bool auto = false;//true是登录电脑设置里的微软账户，false是登录其他账户
                        MicrosoftLogin microsoftLogin = new MicrosoftLogin();
                        Xbox XboxLogin = new Xbox();
                        string Minecraft_Token = new MinecraftLogin().GetToken(XboxLogin.XSTSLogin(XboxLogin.GetToken(microsoftLogin.GetToken(microsoftLogin.Login(auto)).access_token)));
                        MinecraftLogin minecraftlogin = new MinecraftLogin();
                        var Minecraft = minecraftlogin.GetMincraftuuid(Minecraft_Token);
                        string uuid = Minecraft.uuid;
                        string name = Minecraft.name;
                        try
                        {
                            await game.StartGame(versionCombo.Text, javaCombo.SelectedValue.ToString(), intCutMemoryCombo, name, uuid, Minecraft_Token, "", "");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("资源文件不完整：\n" + ex.Message, "启动失败");
                        }
                        /*try
                        {
                            microsoft_launcher.MicrosoftAPIs microsoftAPIs = new microsoft_launcher.MicrosoftAPIs();
                            var v = Microsoft.wb.Source.ToString().Replace(microsoftAPIs.cutUri, string.Empty);
                            var t = Task.Run(() => {
                                return microsoftAPIs.GetAccessTokenAsync(v, false).Result;
                            });
                            await t;
                            var v1 = microsoftAPIs.GetAllThings(t.Result.access_token, false);
                            await game.StartGame(versionCombo.Text, javaCombo.SelectedValue.ToString(), intCutMemoryCombo, v1.name, v1.uuid, v1.mcToken, string.Empty, string.Empty);
                        }
                        catch (Exception ex)
                        {
                            MessageBoxX.Show("资源文件不完整：\n" + ex.Message, "启动失败");
                        }*/
                    }
                    else
                    {
                        MessageBoxX.Show("请完整填写信息", "启动失败");
                    }
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GameStart();
        }
        private void Offline_Login(object sender, RoutedEventArgs e)
        {
            LoginContent.Content = new Frame
            {Content = Offline};
            Color color = (Color)ColorConverter.ConvertFromString("#FF7DCDFF");
            Start.Background = new SolidColorBrush(color);
           launchMode = 1;
        }
        private void Online_Login(object sender, RoutedEventArgs e)
        {
            LoginContent.Content = new Frame
            { Content = Online };
            Color color = (Color)ColorConverter.ConvertFromString("#FF7DFFCD");
            Start.Background = new SolidColorBrush(color);
            launchMode = 2;
        }

        private void Microsoft_Login(object sender, RoutedEventArgs e)
        {
            LoginContent.Content = new Frame
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
            try
            {
                javaCombo.SelectedItem = javaCombo.Items[0];
            }
            catch
            {
                MessageBoxX.Show("刷新Java列表失败，请确认是否安装Java", "错误");
            }
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
                    MessageBoxX.Show("加载成功", "提示");
                }
                catch { }
            }
            else
            {
                MessageBoxX.Show("未找到配置文件，请先保存", "错误");
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

        private void clickme_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
            Process.GetCurrentProcess().Kill();
        }
    }
}
