namespace NetMonTray;

internal static class Program
{
  /// <summary>
  ///  The main entry point for the application.
  /// </summary>
  [STAThread]
  static void Main()
  {
    // To customize application configuration such as set high DPI settings or default font,
    // see https://aka.ms/applicationconfiguration.
    ApplicationConfiguration.Initialize();

    try
    {
    using var icon = new AppNotifyIcon();

    Application.Run();
    GC.KeepAlive(icon);
    }
    catch (Exception ex)
    {
      _ = MessageBox.Show("Unhandled Exception:\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
      Application.Exit();
    }

  }
}