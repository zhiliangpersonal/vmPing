﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using vmPing.Classes;

namespace vmPing.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Probe> _ProbeCollection = new ObservableCollection<Probe>();
        private Dictionary<string, string> _Aliases = new Dictionary<string, string>();

        public static RoutedCommand ProbeOptionsCommand = new RoutedCommand();
        public static RoutedCommand StartStopCommand = new RoutedCommand();
        public static RoutedCommand HelpCommand = new RoutedCommand();
        public static RoutedCommand NewInstanceCommand = new RoutedCommand();
        public static RoutedCommand TraceRouteCommand = new RoutedCommand();
        public static RoutedCommand FloodHostCommand = new RoutedCommand();
        public static RoutedCommand AddMonitorCommand = new RoutedCommand();


        public MainWindow()
        {
            InitializeComponent();
            InitializeAplication();
        }


        private void InitializeAplication()
        {
            InitializeCommandBindings();
            Configuration.UpgradeConfigurationFile();
            LoadFavorites();
            LoadAliases();
            Configuration.Load();

            List<string> hosts = CommandLine.ParseArguments();
            if (hosts.Count > 0)
            {
                AddProbe(hosts.Count);
                for (int i = 0; i < hosts.Count; ++i)
                    Probe.StartStop(_ProbeCollection[i]);
            }
            else
            {
                AddProbe(2);
            }

            ColumnCount.Value = _ProbeCollection.Count;
            ProbeItemsControl.ItemsSource = _ProbeCollection;
        }


        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ProbeOptionsCommand, ProbeOptionsExecute));
            CommandBindings.Add(new CommandBinding(StartStopCommand, StartStopExecute));
            CommandBindings.Add(new CommandBinding(HelpCommand, HelpExecute));
            CommandBindings.Add(new CommandBinding(NewInstanceCommand, NewInstanceExecute));
            CommandBindings.Add(new CommandBinding(TraceRouteCommand, TraceRouteExecute));
            CommandBindings.Add(new CommandBinding(FloodHostCommand, FloodHostExecute));
            CommandBindings.Add(new CommandBinding(AddMonitorCommand, AddMonitorExecute));

            var kgProbeOptions = new KeyGesture(Key.F10);
            var kgStartStop = new KeyGesture(Key.F5);
            var kgHelp = new KeyGesture(Key.F1);
            var kgNewInstance = new KeyGesture(Key.N, ModifierKeys.Control);
            var kgTraceRoute = new KeyGesture(Key.T, ModifierKeys.Control);
            var kgFloodHost = new KeyGesture(Key.F, ModifierKeys.Control);
            var kgAddMonitor = new KeyGesture(Key.A, ModifierKeys.Control);
            InputBindings.Add(new InputBinding(ProbeOptionsCommand, kgProbeOptions));
            InputBindings.Add(new InputBinding(StartStopCommand, kgStartStop));
            InputBindings.Add(new InputBinding(HelpCommand, kgHelp));
            InputBindings.Add(new InputBinding(NewInstanceCommand, kgNewInstance));
            InputBindings.Add(new InputBinding(TraceRouteCommand, kgTraceRoute));
            InputBindings.Add(new InputBinding(FloodHostCommand, kgFloodHost));
            InputBindings.Add(new InputBinding(AddMonitorCommand, kgAddMonitor));

            StartStopMenu.Command = StartStopCommand;
            HelpMenu.Command = HelpCommand;
            NewInstanceMenu.Command = NewInstanceCommand;
            TraceRouteMenu.Command = TraceRouteCommand;
            FloodHostMenu.Command = FloodHostCommand;
            AddMonitorMenu.Command = AddMonitorCommand;
        }


        public void AddProbe(int numberOfProbes = 1)
        {
            for (; numberOfProbes > 0; --numberOfProbes)
                _ProbeCollection.Add(new Probe());
        }


        public void ProbeStartStop_Click(object sender, EventArgs e)
        {
            Probe.StartStop((Probe)((Button)sender).DataContext);
        }


        private void ColumnCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ColumnCount.Value > _ProbeCollection.Count)
                ColumnCount.Value = _ProbeCollection.Count;
        }


        private void Hostname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var probe = (sender as TextBox).DataContext as Probe;
                Probe.StartStop(probe);

                if (_ProbeCollection.IndexOf(probe) < _ProbeCollection.Count - 1)
                {
                    var cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(_ProbeCollection.IndexOf(probe) + 1) as ContentPresenter;
                    var tb = (TextBox)cp.ContentTemplate.FindName("Hostname", cp);

                    if (tb != null)
                        tb.Focus();
                }
            }
        }


        private void RemoveProbe_Click(object sender, RoutedEventArgs e)
        {
            if (_ProbeCollection.Count <= 1)
                return;

            var probe = (sender as Button).DataContext as Probe;
            if (probe.Thread != null)
                probe.Thread.CancelAsync();
            _ProbeCollection.Remove(probe);
            if (ColumnCount.Value > _ProbeCollection.Count)
                ColumnCount.Value = _ProbeCollection.Count;
        }


        private void ProbeOptionsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            DisplayOptionsWindow();
        }


        private void StartStopExecute(object sender, ExecutedRoutedEventArgs e)
        {
            string toggleStatus = StartStopMenuHeader.Text;

            foreach (var pingItem in _ProbeCollection)
            {
                if (toggleStatus == "_Stop All (F5)" && pingItem.IsActive)
                    Probe.StartStop(pingItem);
                else if (toggleStatus == "_Start All (F5)" && !pingItem.IsActive)
                    Probe.StartStop(pingItem);
            }
        }


        private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (HelpWindow.openWindow == null)
            {
                new HelpWindow().Show();
            }
            else
            {
                HelpWindow.openWindow.Activate();
            }
        }


        private void NewInstanceExecute(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName =
                    System.Reflection.Assembly.GetExecutingAssembly().Location;
                p.Start();
            }

            catch (Exception ex)
            {
                var errorWindow = DialogWindow.ErrorWindow($"Failed to launch a new instance of vmPing. {ex.Message}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }


        private void TraceRouteExecute(object sender, ExecutedRoutedEventArgs e)
        {
            new TraceRouteWindow().Show();
        }


        private void FloodHostExecute(object sender, ExecutedRoutedEventArgs e)
        {
            new FloodHostWindow().Show();
        }


        private void AddMonitorExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _ProbeCollection.Add(new Probe());
        }

        
        private void mnuProbeOptions_Click(object sender, RoutedEventArgs e)
        {
            DisplayOptionsWindow();
        }


        private void DisplayOptionsWindow()
        {
            if (OptionsWindow.openWindow == null)
            {
                // Open the options window.
                new OptionsWindow().Show();
            }
            else
            {
                // Options window is already open.  Activate it.
                OptionsWindow.openWindow.Activate();
            }
        }


        private void RemoveAllProbes()
        {
            foreach (var probe in _ProbeCollection)
            {
                if (probe.Thread != null)
                    probe.Thread.CancelAsync();
            }
            _ProbeCollection.Clear();
        }

        private void LoadFavorites()
        {
            // Clear existing favorites menu.
            for (int i = mnuFavorites.Items.Count - 1; i > 2; --i)
                mnuFavorites.Items.RemoveAt(i);

            // Load favorites.
            foreach (var fav in Favorite.GetTitles())
            {
                var menuItem = new MenuItem();
                menuItem.Header = fav;
                menuItem.Click += (s, r) =>
                {
                    RemoveAllProbes();

                    var selectedFavorite = s as MenuItem;
                    var favorite = Favorite.GetContents(selectedFavorite.Header.ToString());
                    if (favorite.Hostnames.Count < 1)
                        AddProbe();
                    else
                    {
                        AddProbe(numberOfProbes: favorite.Hostnames.Count);
                        for (int i = 0; i < favorite.Hostnames.Count; ++i)
                        {
                            _ProbeCollection[i].Hostname = favorite.Hostnames[i].ToUpper();
                            Probe.StartStop(_ProbeCollection[i]);
                        }
                    }

                    ColumnCount.Value = favorite.ColumnCount;
                };

                mnuFavorites.Items.Add(menuItem);
            }
        }


        private void LoadAliases()
        {
            _Aliases = Alias.GetAliases();
            var aliasList = _Aliases.ToList();
            aliasList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            
            // Clear existing aliases menu.
            for (int i = mnuAliases.Items.Count - 1; i > 1; --i)
                mnuAliases.Items.RemoveAt(i);

            // Load aliases.
            foreach (var alias in aliasList)
            {
                mnuAliases.Items.Add(BuildAliasMenuItem(alias, false));
            }

            foreach (var probe in _ProbeCollection)
            {
                probe.Alias = probe.Hostname != null && _Aliases.ContainsKey(probe.Hostname)
                    ? _Aliases[probe.Hostname]
                    : string.Empty;
            }
        }

        private MenuItem BuildAliasMenuItem(KeyValuePair<string, string> alias, bool isContextMenu)
        {
            var menuItem = new MenuItem();
            menuItem.Header = alias.Value;

            if (isContextMenu)
            {
                menuItem.Click += (s, r) =>
                {
                    var selectedMenuItem = s as MenuItem;
                    var selectedAlias = (Probe)selectedMenuItem.DataContext;
                    selectedAlias.Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedMenuItem.Header.ToString()).Key;
                    Probe.StartStop(selectedAlias);
                };
            }
            else
            {
                menuItem.Click += (s, r) =>
                {
                    var selectedAlias = s as MenuItem;

                    var didFindEmptyHost = false;
                    for (int i = 0; i < _ProbeCollection.Count; ++i)
                    {
                        if (string.IsNullOrWhiteSpace(_ProbeCollection[i].Hostname))
                        {
                            _ProbeCollection[i].Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                            Probe.StartStop(_ProbeCollection[i]);
                            didFindEmptyHost = true;
                            break;
                        }
                    }

                    if (!didFindEmptyHost)
                    {
                        AddProbe();
                        _ProbeCollection[_ProbeCollection.Count - 1].Hostname = _Aliases.FirstOrDefault(x => x.Value == selectedAlias.Header.ToString()).Key;
                        Probe.StartStop(_ProbeCollection[_ProbeCollection.Count - 1]);
                    }
                };
            }

            return menuItem;
        }


        private void mnuAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            // Display add to favorites window.
            var currentHostList = new List<string>();
            var haveAnyHostnamesBeenEntered = false;

            for (int i = 0; i < _ProbeCollection.Count; ++i)
            {
                currentHostList.Add(_ProbeCollection[i].Hostname);
                if (!string.IsNullOrWhiteSpace(_ProbeCollection[i].Hostname))
                    haveAnyHostnamesBeenEntered = true;
            }

            if (!haveAnyHostnamesBeenEntered)
            {
                var errorWindow = DialogWindow.ErrorWindow(
                    $"You have not entered any hostnames.  Please setup vmPing with the hosts you would like to save as a favorite set.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            var addToFavoritesWindow = new NewFavoriteWindow(currentHostList, (int)ColumnCount.Value);
            addToFavoritesWindow.Owner = this;
            if (addToFavoritesWindow.ShowDialog() == true)
            {
                LoadFavorites();
            }
        }

        private void mnuManageFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (ManageFavoritesWindow.openWindow == null)
            {
                // Open the favorites window.
                var manageFavoritesWindow = new ManageFavoritesWindow();
                manageFavoritesWindow.Owner = this;
                manageFavoritesWindow.ShowDialog();
                LoadFavorites();
            }
            else
            {
                // Favorites window is already open.  Activate it.
                ManageFavoritesWindow.openWindow.Activate();
            }
        }

        private void mnuManageAliases_Click(object sender, RoutedEventArgs e)
        {
            if (ManageAliasesWindow.openWindow == null)
            {
                // Open the aliases window.
                var manageAliasesWindow = new ManageAliasesWindow();
                manageAliasesWindow.Owner = this;
                manageAliasesWindow.ShowDialog();
                LoadAliases();
            }
            else
            {
                // Aliases window is already open.  Activate it.
                ManageAliasesWindow.openWindow.Activate();
            }
        }

        private void mnuPopupNotification_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            mnuPopupAlways.IsChecked = false;
            mnuPopupNever.IsChecked = false;
            mnuPopupWhenMinimized.IsChecked = false;

            menuItem.IsChecked = true;

            switch (menuItem.Header.ToString())
            {
                case "Always":
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Always;
                    break;
                case "Never":
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.Never;
                    break;
                case "When Minimized":
                    ApplicationOptions.PopupOption = ApplicationOptions.PopupNotificationOption.WhenMinimized;
                    break;
            }
        }

        private void IsolatedView_Click(object sender, RoutedEventArgs e)
        {
            var probe = (sender as Button).DataContext as Probe;
            if (probe.IsolatedWindow == null || probe.IsolatedWindow.IsLoaded == false)
            {
                new IsolatedPingWindow(probe).Show();
            }
            else if (probe.IsolatedWindow.IsLoaded)
            {
                probe.IsolatedWindow.Focus();
            }
        }

        private void EditAlias_Click(object sender, RoutedEventArgs e)
        {
            var probe = (sender as Button).DataContext as Probe;

            if (string.IsNullOrEmpty(probe.Hostname))
                return;

            if (_Aliases.ContainsKey(probe.Hostname))
                probe.Alias = _Aliases[probe.Hostname];
            else
                probe.Alias = string.Empty;

            var wnd = new EditAliasWindow(probe);
            wnd.Owner = this;

            if (wnd.ShowDialog() == true)
            {
                LoadAliases();
            }
        }

        private void mnuStatusHistory_Click(object sender, RoutedEventArgs e)
        {
            if (Probe.StatusWindow == null || Probe.StatusWindow.IsLoaded == false)
            {
                var wnd = new StatusHistoryWindow(Probe.StatusChangeLog);
                Probe.StatusWindow = wnd;
                wnd.Show();
            }
            else if (Probe.StatusWindow.IsLoaded)
            {
                Probe.StatusWindow.Focus();
            }
        }

        private void Hostname_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to textbox on newly added monitors.  If the hostname field is blank for any existing monitors, do not change focus.
            for (int i = 0; i < _ProbeCollection.Count - 1; ++i)
            {
                if (string.IsNullOrEmpty(_ProbeCollection[i].Hostname))
                    return;
            }
            ((TextBox)sender).Focus();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Set initial focus first text box.
            if (_ProbeCollection.Count > 0)
            {
                var cp = ProbeItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                var tb = (TextBox)cp.ContentTemplate.FindName("Hostname", cp);

                if (tb != null)
                    tb.Focus();
            }
        }
    }
}