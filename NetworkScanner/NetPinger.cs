using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkScanner;

public class NetPinger {

    #region Initialize
    
    public string BaseIP = "192.168.0.";
    private readonly int StartIP = 1;
    private readonly int StopIP = 255;
    private string ip;
    private int timeout = 100;
    public int nFound = 0;

    private static readonly object lockeObj = new object();
    private readonly Stopwatch stopWatch = new Stopwatch();
    public TimeSpan ts;
    
    //Event Stuff
    public event EventHandler<string>? PingEvent;
    
    public IPHostEntry host;
    public List<HostData> hosts = new();
    
    #endregion
    
    #region PingRegion
    public async void RunPingSweep_Async() {
        // Pinging of each of the 255 IP addresses will be an individual task/thread, and those tasks will be stored in a List<Task>
        var tasks = new List<Task>();
        stopWatch.Restart();
            this.nFound = 0;
        for (int i = this.StartIP; i <= this.StopIP; i++) {
            //Construct the full IP address for each task to ping
            this.ip = this.BaseIP + i.ToString();
            
            //Make a new Ping object for each IP address to be pinged
            Ping p = new Ping();
            var task = this.PingAndUpdateAsync(p, this.ip);
            tasks.Add(task);
        }
        // when all tasks are completed do the following
        await Task.WhenAll(tasks).ContinueWith(t => {
            this.stopWatch.Stop();
            this.ts= this.stopWatch.Elapsed;
        });
        this.PingEvent?.Invoke(this, this.ts.ToString());
    }

    private async Task PingAndUpdateAsync(Ping ping, string ipInput) {
        //Do the actual pinging to each IP address using system.Net.Ping.dll
        //The "configureAwait(false)" allows any thread other than the main UI
        //thread to continue the method when the SendPingAsync is done. This frees
        //The UI thread
        var reply = await ping.SendPingAsync(ipInput, this.timeout).ConfigureAwait(false);

        if (reply.Status == IPStatus.Success) {
            // if a device ("host") was found, get its host properties (name, etc.)
            host = Dns.GetHostEntry(ipInput);
            this.hosts.Add(new HostData(this.host,ipInput));
            
            // Synchronize access to the private "nFound" field by locking
            // on a dedicated "lockObj" instance. this ensures that the nFound
            // field cannot be updated simultaneously bt two threads attempting to call
            // the Ping methods simultaneously. Instead, one will get access while the other
            // waits its turn

            lock (lockeObj) {
                // one can do it at a time 
                this.nFound++;
            }
        }
        
    }

    #endregion
    
}