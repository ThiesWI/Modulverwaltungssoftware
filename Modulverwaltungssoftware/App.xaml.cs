using Modulverwaltungssoftware.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Modulverwaltungssoftware
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

            DatabaseInitializationService.InitializeDatabase();
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Ein unerwarteter Fehler ist aufgetreten:\n\n{e.Exception.Message}\n\n(Der Fehler wurde abgefangen, Sie können versuchen, ihre Arbeit fortzusetzen.)",
                            "Unerwarteter Fehler",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

            e.Handled = true;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            MessageBox.Show($"Kritischer Systemfehler:\n\n{ex?.Message}\n\nDas Programm wird sicherheitshalber beendet.",
                            "Fataler Fehler",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }

}
