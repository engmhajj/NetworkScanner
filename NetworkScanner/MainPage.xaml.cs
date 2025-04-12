namespace NetworkScanner;

public partial class MainPage : ContentPage {
    private readonly NetPinger _netPing = new();
    public MainPage() {
        InitializeComponent();
        this.TextBox1.TextColor = Colors.Red;
        this._netPing.PingEvent += this.NetPing_PingEvent;
    }

   

    private void PrintResults() {
        foreach (var host in this._netPing.hosts) {
            string[] hostNameStr = host.hostName.Split('.');
            this.TextBox1.Text += host.ipAddress + "   " + hostNameStr[0] + "\r\n";
        }

        string tSpanSec = $"{this._netPing.ts.TotalSeconds:F3} ";
        this.TextBox1.Text +="\r\n"+ this._netPing.nFound + " devices found... Elapsed time: "+ tSpanSec +" Seconds"+ "\r\n\r\n";
        this._netPing.hosts.Clear();
    }

    #region Button Events Handlers

    private void NetPing_PingEvent(object sender, string e) {
        this.PrintResults();
    }
    private void ExitBtn_OnClicked(object? sender, EventArgs e) {
        Application.Current?.Quit();
    }

    private void ScanBtn_OnClicked(object? sender, EventArgs e) {
        this.TextBox1.Text = "Scanning " + this._netPing.BaseIP + "  ... "+ "\r\n\r\n";
        this._netPing.RunPingSweep_Async();
    }

    #endregion
  
}