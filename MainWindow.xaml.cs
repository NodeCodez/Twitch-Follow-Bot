using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Win32;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;

namespace ATWITCHBC
{
	// Token: 0x02000003 RID: 3
	public partial class MainWindow : Window
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000020C4 File Offset: 0x000002C4
		public MainWindow()
		{
			this.InitializeComponent();
			this.SpamThread = new Thread(new ThreadStart(this.StartMessaging));
			bool flag = File.Exists("stg.txt");
			if (flag)
			{
				try
				{
					string[] temp = File.ReadAllLines("stg.txt");
					this.OpenFileCB(temp[0]);
					this.NickName.Text = temp[1];
				}
				catch
				{
				}
			}
			string[] args = Environment.GetCommandLineArgs();
			bool flag2 = args.Count<string>() > 1;
			if (flag2)
			{
				this.need = Convert.ToInt32(args[2]);
				this.NickName.Text = args[1];
				bool flag3 = args[2] != null;
				if (flag3)
				{
					this.ToLow();
				}
				this.clients.Clear();
				this.BaseConnect = new Thread(new ThreadStart(this.ConnectionsThread));
				this.BaseConnect.Start();
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000224C File Offset: 0x0000044C
		private void Common_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
		{
			this.CBname.Content = "Name: " + this.usernames[this.Common.SelectedIndex];
			this.CBauth.Content = "OAUTH: " + this.tokens[this.Common.SelectedIndex];
			bool flag = this.status[this.Common.SelectedIndex] == "Sleep";
			if (flag)
			{
				this.CBstatus.Foreground = Brushes.Aqua;
			}
			else
			{
				bool flag2 = this.status[this.Common.SelectedIndex] == "Connected";
				if (flag2)
				{
					this.CBstatus.Foreground = Brushes.LightGreen;
				}
				else
				{
					this.CBstatus.Foreground = Brushes.Red;
				}
			}
			this.CBstatus.Content = this.status[this.Common.SelectedIndex];
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000235C File Offset: 0x0000055C
		private void test_MouseDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				base.DragMove();
			}
			catch
			{
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000238C File Offset: 0x0000058C
		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002398 File Offset: 0x00000598
		private void RfrStg()
		{
			File.WriteAllText("stg.txt", string.Concat(new string[]
			{
				this.pathtobots,
				"\r\n",
				this.lastnickname,
				"\r\n",
				this.pathtowords,
				"\r\n"
			}));
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000023F0 File Offset: 0x000005F0
		private void OpenFileCB(string path)
		{
			this.chatters.Clear();
			this.chatters.AddRange(File.ReadAllLines(path));
			this.ChatBotsCount.Text = this.chatters.Count.ToString();
			this.pathtobots = path;
			List<string> temp = (from c in this.chatters
			orderby this.rnd.Next()
			select c).ToList<string>();
			foreach (string item in temp)
			{
				string[] spl = Regex.Split(item, ":oauth:");
				this.usernames.Add(spl[0]);
				this.status.Add("Sleep");
				this.tokens.Add(spl[1]);
				this.Common.Items.Add(new ListBoxItem
				{
					Content = spl[0],
					Foreground = Brushes.White
				});
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002508 File Offset: 0x00000708
		private void LoadChatBots_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.chatbots.Filter = "txt files (*.txt)|*.txt";
				this.chatbots.ShowDialog();
				string temp = this.chatbots.FileName;
				this.OpenFileCB(temp);
				this.RfrStg();
			}
			catch
			{
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002568 File Offset: 0x00000768
		private void Log(string text)
		{
			try
			{
				this.log.Dispatcher.Invoke(delegate()
				{
					this.log.AppendText(text + "\r\n");
				});
				File.AppendAllText("log.txt", text + "\r\n");
			}
			catch
			{
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000025DC File Offset: 0x000007DC
		private void ValueUp_Click(object sender, RoutedEventArgs e)
		{
			int value = Convert.ToInt32(this.ChatBotsCount.Text);
			bool flag = value < this.chatters.Count;
			if (flag)
			{
				this.ChatBotsCount.Text = (Convert.ToInt32(this.ChatBotsCount.Text) + 1).ToString();
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002638 File Offset: 0x00000838
		private void Down_Click(object sender, RoutedEventArgs e)
		{
			int value = Convert.ToInt32(this.ChatBotsCount.Text);
			bool flag = value > 1;
			if (flag)
			{
				this.ChatBotsCount.Text = (Convert.ToInt32(this.ChatBotsCount.Text) - 1).ToString();
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002688 File Offset: 0x00000888
		private void ShowLogs_Click(object sender, RoutedEventArgs e)
		{
			bool flag = !this.logs;
			if (flag)
			{
				base.Width = 777.0;
				this.ShowLogs.Content = "Hide logs";
			}
			else
			{
				base.Width = 529.0;
				this.ShowLogs.Content = "Show logs";
			}
			this.logs = !this.logs;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000026FC File Offset: 0x000008FC
		private void ConnectBTN_Click(object sender, RoutedEventArgs e)
		{
			this.ProxyUsageCB.IsEnabled = false;
			this.ProxyLinkTB.IsEnabled = false;
			this.clients.Clear();
			this.BaseConnect = new Thread(new ThreadStart(this.ConnectionsThread));
			this.BaseConnect.Start();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002754 File Offset: 0x00000954
		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+,");
			e.Handled = regex.IsMatch(e.Text);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002780 File Offset: 0x00000980
		private void ConnectionsThread()
		{
			try
			{
				string nick = "";
				this.NickName.Dispatcher.Invoke(delegate()
				{
					nick = this.NickName.Text.ToLower();
				});
				base.Dispatcher.Invoke(delegate()
				{
					this.Title = nick;
				});
				this.pos = 0;
			}
			catch
			{
			}
			for (;;)
			{
				bool flag = this.pos < this.need;
				if (flag)
				{
					bool flag2 = this.status[this.pos] == "Sleep";
					if (flag2)
					{
						try
						{
							this.ConnectChatBot(this.pos, this.usernames[this.pos], this.tokens[this.pos]);
						}
						catch (Exception ex)
						{
							this.Log(ex.Message);
						}
						this.pos++;
					}
					else
					{
						this.pos++;
					}
					Thread.Sleep(this.rnd.Next(200, 500));
				}
				else
				{
					this.BaseConnect.Abort();
				}
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000028E0 File Offset: 0x00000AE0
		private void ConnectChatBot(int position, string login, string token)
		{
			try
			{
				bool temp = false;
				this.ProxyUsageCB.Dispatcher.Invoke(delegate()
				{
					bool flag = this.ProxyUsageCB.IsChecked == true;
					if (flag)
					{
						temp = true;
					}
					else
					{
						temp = false;
					}
				});
				bool temp2 = temp;
				if (temp2)
				{
					string[] temp3 = this.ips[this.rnd.Next(this.ips.Count)].Split(new char[]
					{
						':'
					});
					string ip = temp3[0];
					int port = Convert.ToInt32(temp3[1]);
					this.credentials = new ConnectionCredentials(login, token, "irc-ws.chat.twitch.tv", 80, false, ip, new int?(port));
				}
				else
				{
					this.credentials = new ConnectionCredentials(login, token, "irc-ws.chat.twitch.tv", 80, false, null, null);
				}
				string nick = "";
				this.NickName.Dispatcher.Invoke(delegate()
				{
					nick = this.NickName.Text.ToLower();
				});
				TwitchClient cl = new TwitchClient(this.credentials, nick, '!', '!', false, null, true);
				this.clients.Add(cl);
				cl.OnJoinedChannel += delegate(object sender, OnJoinedChannelArgs e)
				{
					this.onJoinedChannel(sender, e, position);
				};
				cl.OnConnectionError += delegate(object sender, OnConnectionErrorArgs e)
				{
					this.onJoinError(sender, e, position);
				};
				cl.OnIncorrectLogin += delegate(object sender, OnIncorrectLoginArgs e)
				{
					this.onLoginInc(sender, e, position);
				};
				cl.Connect();
			}
			catch (Exception ex)
			{
				this.Log(ex.Message);
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002A98 File Offset: 0x00000C98
		private void onJoinedChannel(object sender, OnJoinedChannelArgs e, int position)
		{
			try
			{
				this.Log(e.BotUsername + " Connected");
				(sender as TwitchClient).OnDisconnected += delegate(object sender1, OnDisconnectedArgs e1)
				{
					this.onLeaveChannel(sender1, e1, position);
				};
				this.connections++;
				this.currentconnected.Add(sender as TwitchClient);
				this.statuslbl.Dispatcher.Invoke(delegate()
				{
					this.statuslbl.Content = "Status: " + this.connections;
				});
				this.Common.Dispatcher.Invoke(delegate()
				{
					(this.Common.Items[position] as ListBoxItem).Foreground = Brushes.LightGreen;
				});
				this.status[position] = "Connected";
			}
			catch (Exception ex)
			{
				try
				{
					this.Log(ex.Message);
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002B98 File Offset: 0x00000D98
		private void onJoinError(object sender, OnConnectionErrorArgs e, int position)
		{
			try
			{
				this.Log(string.Concat(new object[]
				{
					e.BotUsername,
					" Error(",
					e.Error,
					")"
				}));
				this.Common.Dispatcher.Invoke(delegate()
				{
					(this.Common.Items[position] as ListBoxItem).Foreground = Brushes.Red;
				});
				this.status[position] = "Failed";
			}
			catch (Exception ex)
			{
				try
				{
					this.Log(ex.Message);
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002C60 File Offset: 0x00000E60
		private void onLoginInc(object sender, OnIncorrectLoginArgs e, int position)
		{
			try
			{
				this.Log((sender as TwitchClient).TwitchUsername + " Login failed");
				this.Common.Dispatcher.Invoke(delegate()
				{
					(this.Common.Items[position] as ListBoxItem).Foreground = Brushes.Red;
				});
				this.status[position] = "Failed";
			}
			catch (Exception ex)
			{
				try
				{
					this.Log(ex.Message);
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002D10 File Offset: 0x00000F10
		private void onLeaveChannel(object sender, OnDisconnectedArgs e, int position)
		{
			try
			{
				this.Log(e.BotUsername + " Disconnected");
				this.Common.Dispatcher.Invoke(delegate()
				{
					(this.Common.Items[position] as ListBoxItem).Foreground = Brushes.White;
				});
				this.status[position] = "Sleep";
				this.connections--;
				this.statuslbl.Dispatcher.Invoke(delegate()
				{
					this.statuslbl.Content = "Status: " + this.connections;
				});
			}
			catch (Exception ex)
			{
				try
				{
					this.Log(ex.Message);
				}
				catch
				{
				}
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002DE4 File Offset: 0x00000FE4
		private void ChatBotsCount_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				this.need = Convert.ToInt32(this.ChatBotsCount.Text);
			}
			catch
			{
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002E24 File Offset: 0x00001024
		private void CBname_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				Clipboard.SetText(this.usernames[this.Common.SelectedIndex] + ":oauth:" + this.tokens[this.Common.SelectedIndex]);
			}
			catch
			{
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002E88 File Offset: 0x00001088
		private void CBauth_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				Clipboard.SetText(this.usernames[this.Common.SelectedIndex] + ":oauth:" + this.tokens[this.Common.SelectedIndex]);
			}
			catch
			{
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002EEC File Offset: 0x000010EC
		private void ConnectBTN_Custom_Click(object sender, RoutedEventArgs e)
		{
			this.lastnickname = this.NickName.Text;
			this.RfrStg();
			try
			{
				this.credentials = new ConnectionCredentials(this.usernames[this.Common.SelectedIndex], this.tokens[this.Common.SelectedIndex], "irc-ws.chat.twitch.tv", 80, false, null, null);
				string nick = "";
				this.NickName.Dispatcher.Invoke(delegate()
				{
					nick = this.NickName.Text.ToLower();
				});
				int index = 0;
				this.Common.Dispatcher.Invoke(delegate()
				{
					index = this.Common.SelectedIndex;
				});
				TwitchClient cl = new TwitchClient(this.credentials, nick, '!', '!', false, null, true);
				cl.OnJoinedChannel += delegate(object sender1, OnJoinedChannelArgs e1)
				{
					this.onJoinedChannel(sender1, e1, index);
				};
				cl.OnConnectionError += delegate(object sender1, OnConnectionErrorArgs e1)
				{
					this.onJoinError(sender1, e1, index);
				};
				cl.OnIncorrectLogin += delegate(object sender1, OnIncorrectLoginArgs e1)
				{
					this.onLoginInc(sender1, e1, index);
				};
				cl.OnDisconnected += delegate(object sender1, OnDisconnectedArgs e1)
				{
					this.onLeaveChannel(sender1, e1, index);
				};
				cl.Connect();
			}
			catch (Exception ex)
			{
				try
				{
					this.Log(ex.Message);
					this.Log(this.usernames[this.Common.SelectedIndex] + " " + this.tokens[this.Common.SelectedIndex] + " неудача");
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000030B8 File Offset: 0x000012B8
		private void Disconnect_BTN_Click(object sender, RoutedEventArgs e)
		{
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000030BC File Offset: 0x000012BC
		private void LoadWords_Click(object sender, RoutedEventArgs e)
		{
			this.textforchat.Filter = "txt files (*.txt)|*.txt";
			this.textforchat.ShowDialog();
			try
			{
				this.Messages.Text = File.ReadAllText(this.textforchat.FileName);
			}
			catch
			{
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x0000311C File Offset: 0x0000131C
		private void StartMessaging()
		{
			int curline = 0;
			for (;;)
			{
				bool boo = true;
				this.RandomCheck.Dispatcher.Invoke(delegate()
				{
					bool flag2 = this.RandomCheck.IsChecked == true;
					if (flag2)
					{
						boo = true;
					}
					else
					{
						boo = false;
					}
				});
				bool boo2 = boo;
				if (boo2)
				{
					List<string> temp = new List<string>();
					this.Messages.Dispatcher.Invoke(delegate()
					{
						temp.AddRange(Regex.Split(this.Messages.Text, "\r\n|\r|\n"));
					});
					this.currentconnected[this.rnd.Next(this.connections)].SendMessage(temp[this.rnd.Next(temp.Count)], false);
				}
				else
				{
					List<string> temp = new List<string>();
					this.Messages.Dispatcher.Invoke(delegate()
					{
						temp.AddRange(Regex.Split(this.Messages.Text, "\r\n|\r|\n"));
					});
					this.currentconnected[this.rnd.Next(this.connections)].SendMessage(temp[curline], false);
					curline++;
					bool flag = curline == temp.Count;
					if (flag)
					{
						curline = 0;
					}
				}
				double pause = 0.0;
				this.Interval.Dispatcher.Invoke(delegate()
				{
					pause = Convert.ToDouble(this.Interval.Text);
				});
				Thread.Sleep(Convert.ToInt32(1000.0 * pause));
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000032C8 File Offset: 0x000014C8
		private string GetLine(int num)
		{
			string txt = "0";
			this.Messages.Dispatcher.Invoke(delegate()
			{
				txt = this.Messages.GetLineText(num);
			});
			return txt;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000331C File Offset: 0x0000151C
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://atwitch.ru");
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000332C File Offset: 0x0000152C
		private void SaveTextBTN_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				File.WriteAllText("ATWITCHnewtext.txt", this.Messages.Text);
			}
			catch
			{
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000336C File Offset: 0x0000156C
		private void StartSpam_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				bool flag = this.SpamThread.ThreadState == System.Threading.ThreadState.Unstarted;
				if (flag)
				{
					this.SpamThread.Start();
					this.StartSpam.Content = "Pause spam";
				}
				else
				{
					bool flag2 = this.StartSpam.Content == "Resume spam";
					if (flag2)
					{
						this.SpamThread.Resume();
						this.StartSpam.Content = "Pause spam";
					}
					else
					{
						bool flag3 = this.SpamThread.ThreadState > System.Threading.ThreadState.Running;
						if (flag3)
						{
							this.SpamThread.Suspend();
							this.StartSpam.Content = "Resume spam";
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.Log(ex.Message);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000343C File Offset: 0x0000163C
		private void SendCustomMSGBTN_Click(object sender, RoutedEventArgs e)
		{
			this.credentials = new ConnectionCredentials(this.usernames[this.Common.SelectedIndex], this.tokens[this.Common.SelectedIndex], "irc-ws.chat.twitch.tv", 80, false, null, null);
			string nick = "";
			this.NickName.Dispatcher.Invoke(delegate()
			{
				nick = this.NickName.Text.ToLower();
			});
			int index = 0;
			this.Common.Dispatcher.Invoke(delegate()
			{
				index = this.Common.SelectedIndex;
			});
			TwitchClient cl = new TwitchClient(this.credentials, nick, '!', '!', false, null, true);
			cl.OnConnected += this.SCMC;
			cl.Connect();
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003520 File Offset: 0x00001720
		private void SCMC(object sender, OnConnectedArgs e)
		{
			string msg = "";
			this.CustomMSGTXTBX.Dispatcher.Invoke(delegate()
			{
				msg = this.CustomMSGTXTBX.Text;
			});
			(sender as TwitchClient).SendMessage(msg, false);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003578 File Offset: 0x00001778
		private void ToLow()
		{
			bool flag = !this.low;
			if (flag)
			{
				Application.Current.MainWindow.Height = 40.0;
				Application.Current.MainWindow.Width = 50.0;
			}
			else
			{
				Application.Current.MainWindow.Height = 611.0;
				Application.Current.MainWindow.Width = 529.0;
			}
			this.low = !this.low;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x0000360C File Offset: 0x0000180C
		private void ToLowBTN_Click(object sender, RoutedEventArgs e)
		{
			this.ToLow();
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003618 File Offset: 0x00001818
		private void CloseBTN_Click(object sender, RoutedEventArgs e)
		{
			this.CloseBTN.IsEnabled = false;
			Thread t = new Thread(new ThreadStart(this.close));
			t.Start();
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000364C File Offset: 0x0000184C
		private void MinutesTB_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				this.leftminutes = Convert.ToInt32(this.MinutesTB.Text);
				this.RemainingLabel.Content = "Remaining: " + this.leftminutes;
			}
			catch
			{
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000036AC File Offset: 0x000018AC
		private void close()
		{
			for (;;)
			{
				Thread.Sleep(60000);
				this.leftminutes--;
				bool flag = this.leftminutes < 0;
				if (flag)
				{
					Environment.Exit(0);
				}
				this.RemainingLabel.Dispatcher.Invoke(delegate()
				{
					this.RemainingLabel.Content = "Remaining: " + this.leftminutes;
				});
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003710 File Offset: 0x00001910
		private void ProxyLinkTB_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				WebClient wc = new WebClient();
				string link = "";
				this.ProxyLinkTB.Dispatcher.Invoke(delegate()
				{
					link = this.ProxyLinkTB.Text;
				});
				string temp3 = wc.DownloadString(link);
				this.ips.Clear();
				this.ips.AddRange(Regex.Split(temp3, "\r\n|\r|\n"));
			}
			catch
			{
			}
		}

		// Token: 0x04000002 RID: 2
		private OpenFileDialog textforchat = new OpenFileDialog();

		// Token: 0x04000003 RID: 3
		private OpenFileDialog chatbots = new OpenFileDialog();

		// Token: 0x04000004 RID: 4
		private List<string> chatters = new List<string>();

		// Token: 0x04000005 RID: 5
		private List<string> usernames = new List<string>();

		// Token: 0x04000006 RID: 6
		private List<string> tokens = new List<string>();

		// Token: 0x04000007 RID: 7
		private List<TwitchClient> clients = new List<TwitchClient>();

		// Token: 0x04000008 RID: 8
		private List<TwitchClient> customclients = new List<TwitchClient>();

		// Token: 0x04000009 RID: 9
		private List<TwitchClient> currentconnected = new List<TwitchClient>();

		// Token: 0x0400000A RID: 10
		private List<string> status = new List<string>();

		// Token: 0x0400000B RID: 11
		private Random rnd = new Random();

		// Token: 0x0400000C RID: 12
		private bool logs = false;

		// Token: 0x0400000D RID: 13
		private bool low = false;

		// Token: 0x0400000E RID: 14
		private string pathtobots;

		// Token: 0x0400000F RID: 15
		private string pathtowords;

		// Token: 0x04000010 RID: 16
		private string lastnickname;

		// Token: 0x04000011 RID: 17
		private int connections;

		// Token: 0x04000012 RID: 18
		private int pos;

		// Token: 0x04000013 RID: 19
		private int pos1;

		// Token: 0x04000014 RID: 20
		private int need;

		// Token: 0x04000015 RID: 21
		private int leftminutes = 60;

		// Token: 0x04000016 RID: 22
		private ConnectionCredentials credentials;

		// Token: 0x04000017 RID: 23
		private Thread BaseConnect;

		// Token: 0x04000018 RID: 24
		private Thread SpamThread;

		// Token: 0x04000019 RID: 25
		private List<string> ips = new List<string>();
	}
}
