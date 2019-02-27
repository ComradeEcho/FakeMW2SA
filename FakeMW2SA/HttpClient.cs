using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;

namespace FakeMW2SA
{
    class HttpClient
    {
        private Object thisLock = new Object();

        public static void Run()
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:28961/");
                listener.Prefixes.Add("http://127.0.0.1:28961/");
                listener.Start();
                Console.WriteLine("Listening on http://localhost:28961 and http://127.0.0.1:28961/");
                while (true)
                {
                    string responseString = String.Format("" + @"
                        <!DOCTYPE html>
                        <html lang='en'>
                            <meta charset='utf-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>
                            <head>
                                <link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta.2/css/bootstrap.min.css' integrity='sha384-PsH8R72JQ3SOdhVi3uxftmaW6Vc51MKb0q5P2rRUpPvrszuE4W1povHYgTpBfshb' crossorigin='anonymous'>
                                <script>var csrf = {0}</script>
                            </head>
                            <body>
                                <div class='wrapper' style='margin:auto;'>
                                  <nav class='navbar navbar-expand-lg navbar-light bg-light'>
                                    <a class='navbar-brand' href='#'>FakeMW2SA</a>
                                    <button class='navbar-toggler' type='button' data-toggle='collapse' data-target='#navbarSupportedContent' aria-controls='navbarSupportedContent' aria-expanded='false' aria-label='Toggle navigation'>
                                      <span class='navbar-toggler-icon'></span>
                                    </button>

                                    <div class='collapse navbar-collapse' id='navbarSupportedContent'>
                                      <ul class='navbar-nav mr-auto'>
                                        <li class='nav-item active'>
                                          <a class='nav-link' href='#'>Home <span class='sr-only'>(current)</span></a>
                                        </li>
                                        <li class='nav-item'>
                                          <a class='nav-link' href='#' onclick=""memberjointoggle()""> Memberjoin</a>
                                        </li>
                                        <li class='nav-item dropdown'>
                                          <a class='nav-link dropdown-toggle' href='#' id='navbarDropdown' role='button' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>
                                            IPs
                                          </a>
                                          <div id = 'iplist' class='dropdown-menu' aria-labelledby='navbarDropdown'>

                                          </div>
                                        </li>
                                        <li class='nav-item dropdown'>
                                          <a class='nav-link dropdown-toggle' href='#' id='navbarDropdown' role='button' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>
                                            Stats
                                          </a>
                                          <div class='dropdown-menu' aria-labelledby='navbarDropdown'>
                                            <a class='dropdown-item' id='apicalls' href='#'>API calls</a>
                                            <a class='dropdown-item' id='partystatepackets' href='#'>partystate packets</a>
                                            <a class='dropdown-item' id='memberjoinpackets' href='#'>Memberjoin packets</a>
                                          </div>
                                        </li>
                                        <li class='nav-item'>
                                          <a class='nav-link' href='#'>About</a>
                                        </li>
                                          <li class='nav-item'>

                                          <a class='nav-link' href='http://localhost:28961/?action=players&csrf=" + FakeMW2SA.Program.csrf + @"'>JSON</a>
                                        </li>
                                      </ul>
                                      <span class='navbar-text'>Host =&nbsp;</span><span class='navbar-text' id='host'>0.0.0.0</span>
                                    </div>
                                  </nav>
                                  <div id='playertable'></div>
                                </div>
                                <script src='https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js'></script>
                                <script src='http://ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js'></script>
                                <script src='https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.3/umd/popper.min.js' integrity='sha384-vFJXuSJphROIrBnz7yo7oB41mKfc8JzQZiCq4NCceLEaO4IHwicKwpJf9c9IpFgh' crossorigin='anonymous'></script>
                                <script src='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta.2/js/bootstrap.min.js' integrity='sha384-alpBpkh1PFOepccYVYDB4do5UnbKysX5WZXm3XxPqe5iKTfUKjNkCk9SaVuEZflJ' crossorigin='anonymous'></script>
                                <script src='http://mw2.adie.space/js/client.js'></script>
                                <script src='http://mw2.adie.space/js/moment.js'></script>
                                <link rel='stylesheet' type='text/css' href='http://mw2.adie.space/css/site.css'>
                            </body>
                        </html>
                        ", FakeMW2SA.Program.csrf);
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    string clientIP = context.Request.RemoteEndPoint.ToString();
                    
                    response.AppendHeader("Access-Control-Allow-Origin", "*");
                    if (request.QueryString.GetValues("action") != null && request.QueryString.GetValues("csrf") != null && request.QueryString.GetValues("csrf")[0] == FakeMW2SA.Program.csrf.ToString())
                    {
                        switch (request.QueryString.GetValues("action")[0])
                        {
                            case "players":
                                response.ContentType = "application/json";
                                responseString = JsonConvert.SerializeObject(new FakeMW2SA.JsonOutput());
                                break;
                            case "ban":
                                FakeMW2SA.Utils.Ban(request.QueryString.GetValues("ip")[0]);
                                break;
                            case "unban":
                                FakeMW2SA.Utils.Unban(request.QueryString.GetValues("ip")[0]);
                                break;
                            case "clearbans":
                                FakeMW2SA.Utils.Clearfirewall();
                                break;
                            case "host":
                                    responseString = JsonConvert.SerializeObject(new FakeMW2SA.JsonOutput());
                                break;
                            default:
                                break;
                        }
                    }
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }
        public static void Start()
        {
            Thread a = new Thread(Run);
            a.Start();
        }
    }
}
