﻿using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace vmPing.Views
{
    /// <summary>
    /// DialogWindow is used to display popup dialog boxes.
    /// </summary>
    public partial class DialogWindow : Window
    {
        public enum DialogIcon
        {
            Warning
        }

        public DialogWindow(DialogIcon icon, string title, string body, string confirmationText, bool isCancelButtonVisible)
        {
            InitializeComponent();

            MyTitle.Text = title;
            Body.Text = body;
            OK.Content = confirmationText;

            if (!isCancelButtonVisible)
                Cancel.Visibility = Visibility.Collapsed;

            switch (icon)
            {
                case DialogIcon.Warning:
                    MyIcon.Source = new BitmapImage(new Uri(@"/Resources/caution-40.png", UriKind.Relative));
                    break;
            }
        }

        public static DialogWindow ErrorWindow(string message)
        {
            return new DialogWindow(
                icon: DialogIcon.Warning,
                title: "Error",
                body: message,
                confirmationText: "OK",
                isCancelButtonVisible: false);
        }

        public static DialogWindow WarningWindow(string message, string confirmButtonText)
        {
            return new DialogWindow(
                icon: DialogIcon.Warning,
                title: "Warning",
                body: message,
                confirmationText: confirmButtonText,
                isCancelButtonVisible: true);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}