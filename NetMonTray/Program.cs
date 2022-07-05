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
      Application.Run(new AppNotifyIcon());
    }
    catch (Exception ex)
    {
      _ = MessageBox.Show("Unhandled Exception:\n" + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
      Application.Exit();
    }

  }
}