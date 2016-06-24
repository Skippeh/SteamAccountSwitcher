using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Win32;
using System.Reflection;
using Hardcodet.Wpf.TaskbarNotification;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using SteamAccountSwitcher.UserControls;

namespace SteamAccountSwitcher
{
    /// ****
    /// SteamAccountSwitcher
    /// Copyright by Christoph Wedenig
    /// ****
    
    public partial class MainWindow : Window
    {
        public AccountList AccountList {get { return AccountsListView.AccountList; } }
        public Steam Steam { get { return AccountsListView.Steam; } }

        public static List<ListView> AccountListViews { get; private set; } // Keep track of all account list views.
        
        public MainWindow()
        {
            AccountListViews = new List<ListView>();

            InitializeComponent();

            this.buttonInfo.ToolTip = "Build Version: " + Assembly.GetEntryAssembly().GetName().Version.ToString();

            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;

            if (Properties.Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        internal static void RefreshLists()
        {
            AccountListViews.ForEach(list => list.Items.Refresh());
        }

        public new void Show()
        {
            base.Show();
            WindowState = WindowState.Normal; // Unminimize the window.
        }

        private void buttonLogout_Click(object sender, RoutedEventArgs e)
        {
            Steam.LogoutSteam();
        }

        private void buttonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            AddAccount newAccWindow = new AddAccount();
            newAccWindow.Owner = this;
            newAccWindow.ShowDialog();

            if (newAccWindow.Account != null)
            {
                AccountList.Accounts.Add(newAccWindow.Account);
                RefreshLists();
            }
        }

        private void buttonEditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListView.SelectedItem != null)
            {
                AddAccount newAccWindow = new AddAccount((SteamAccount)AccountsListView.SelectedItem);
                newAccWindow.Owner = this;
                newAccWindow.ShowDialog();

                if (newAccWindow.Account.Username != "" && newAccWindow.Account.Password != "")
                {
                    AccountList.Accounts[AccountsListView.SelectedIndex] = newAccWindow.Account;
                    RefreshLists();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        private void NotifyIcon_ClickShow(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void NotifyIcon_ClickExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();

                if (!Properties.Settings.Default.HideBalloonTip)
                    notifyIcon.ShowBalloonTip("Steam Account Switched", "The window has been minimized to the tray.", BalloonIcon.Info);
            }
        }

        private void NotifyIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HideBalloonTip = true;
            Properties.Settings.Default.Save();
        }

        private void NotifyIcon_NotifyIconDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }
    }
}
