using System.Net;
using System.Net.NetworkInformation;

namespace NetMonTray;

internal class AppNotifyIcon : IDisposable
{
  private bool disposedValue;

  private static IPAddress PingTarget { get; } = new IPAddress(new byte[] { 1, 1, 1, 1 });

  private NotifyIcon Icon { get; }

  private System.Windows.Forms.Timer PingTimer { get; }

  private bool _timerIsBusy = false;

  private StateOption _state = StateOption.Ok;


  public AppNotifyIcon()
  {
    Icon = CreateNotifyIcon();
    PingTimer = CreateTimer();
    PingTimer.Tick += PingTimer_Tick;
  }

  private static System.Windows.Forms.Timer CreateTimer() => new()
  {
    Interval = IntervalStateMachine(StateOption.Ok),
    Enabled = true
  };


  private static NotifyIcon CreateNotifyIcon() => new()
  {
    Icon = Properties.Resources.trafficlight_green_256,
    ContextMenuStrip = CreateMenu(),
    Visible = true,
  };


  private static ContextMenuStrip CreateMenu()
  {
    var exitMenu = new ToolStripButton("Exit", null, (o, e) => { Application.Exit(); })
    {
      DisplayStyle = ToolStripItemDisplayStyle.Text
    };

    var popupMenu = new ContextMenuStrip { AutoClose = true };

    popupMenu.Items.Add(exitMenu);
    return popupMenu;
  }



  private StateOption State
  {
    get => _state;

    set
    {
      if (_state == value) return;
      _state = value;
      SetTrayIcon();
      PingTimer.Interval = IntervalStateMachine(_state);
    }

  }

  private async void PingTimer_Tick(object? sender, EventArgs e)
  {
    if (_timerIsBusy) return;
    _timerIsBusy = true;

    await CheckState();
    _timerIsBusy = false;
  }

  private async Task CheckState()
  {
    State = StateMachine(State, await PerformPingTest());
  }

  private static StateOption StateMachine(StateOption current, bool pingOk) => (current, pingOk) switch
  {
    (StateOption.Ok, true) => StateOption.Ok,
    (StateOption.Ok, false) => StateOption.Warn,
    (StateOption.Warn, true) => StateOption.Ok,
    (StateOption.Warn, false) => StateOption.Fail,
    (StateOption.Fail, true) => StateOption.Ok,
    (StateOption.Fail, false) => StateOption.Fail,
    _ => throw new ArgumentOutOfRangeException(nameof(current))

  };

  private static int IntervalStateMachine(StateOption state)
    => state switch
    {
      StateOption.Ok => 5000,
      StateOption.Warn => 1000,
      StateOption.Fail => 500,
      _ => throw new InvalidOperationException("Unsupported state: " + state.ToString())
    };


  private void SetTrayIcon()
  {
    Icon.Icon = _state switch
    {
      StateOption.Ok => Properties.Resources.trafficlight_green_256,
      StateOption.Warn => Properties.Resources.tennisball_256,
      StateOption.Fail => Properties.Resources.trafficlight_red_256,
      _ => throw new InvalidOperationException("Unsupported state: " + _state.ToString())
    };
  }

  private static async Task<bool> PerformPingTest()
  {
    using var p = new Ping();

    var r = await p.SendPingAsync(PingTarget);

    return r.Status == IPStatus.Success;

  }

  #region IDisposable

  protected virtual void Dispose(bool disposing)
  {
    if (!disposedValue)
    {
      if (disposing)
      {
        PingTimer.Tick -= PingTimer_Tick;
        PingTimer.Enabled = false;
        Icon.Visible = false;
        PingTimer.Dispose();
        Icon.ContextMenuStrip.Dispose();
        Icon.Dispose();
      }
      disposedValue = true;
    }
  }


  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
  #endregion
}
