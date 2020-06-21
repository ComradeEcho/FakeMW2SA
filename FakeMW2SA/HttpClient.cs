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
                listener.Prefixes.Add("http://localhost:" + Program.port + "/");
                listener.Prefixes.Add("http://127.0.0.1:" + Program.port + "/");
                listener.Start();
                Console.WriteLine("Listening on http://localhost:" + Program.port + "/" + "and http://127.0.0.1:" + Program.port + "/");
                while (true)
                {
                    string responseString = Helper.GetLocalFileContent("FakeMWHtml.html");
                    responseString = responseString.Replace("[PORT]", Program.port.ToString());


                    responseString = responseString.Replace("[BootstrapCss]", Helper.GetLocalFileContent("BootstrapCss.txt"));
                    responseString = responseString.Replace("[JQUERY]", Helper.GetLocalFileContent("jQuery.txt"));
                    responseString = responseString.Replace("[JQUERYUI]", Helper.GetLocalFileContent("jQueryUI.txt"));
                    responseString = responseString.Replace("[POPPER]", Helper.GetLocalFileContent("Popper.txt"));
                    responseString = responseString.Replace("[BOOTSTRAP]", Helper.GetLocalFileContent("Bootstrap.txt"));
                    responseString = responseString.Replace("[CLIENTJS]", Helper.GetLocalFileContent("ClientJs.txt"));
                    responseString = responseString.Replace("[MOMENT]", Helper.GetLocalFileContent("MomentJs.txt"));
                    responseString = responseString.Replace("[CSS]", Helper.GetLocalFileContent("SiteCss.txt"));
                    responseString = responseString.Replace("[CSRF]", FakeMW2SA.Program.csrf.ToString());


                    //responseString = responseString.Replace("[BootstrapCss]", HtmlResource.BootstrapCss);
                    //responseString = responseString.Replace("[JQUERY]", HtmlResource.jQuery);
                    //responseString = responseString.Replace("[JQUERYUI]", HtmlResource.jQueryUI);
                    //responseString = responseString.Replace("[POPPER]", HtmlResource.Popper);
                    //responseString = responseString.Replace("[BOOTSTRAP]", HtmlResource.Bootstrap);
                    //responseString = responseString.Replace("[CLIENTJS]", HtmlResource.ClientJs);
                    //responseString = responseString.Replace("[MOMENT]", HtmlResource.MomentJs);
                    //responseString = responseString.Replace("[CSS]", HtmlResource.SiteCss);
                    //responseString = responseString.Replace("[CSRF]", FakeMW2SA.Program.csrf.ToString());



                    //responseString = responseString.Replace("[BootstrapCss]", Helper.LinkCdn("https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta.2/css/bootstrap.min.css"));
                    //responseString = responseString.Replace("[JQUERY]", Helper.LinkCdn("https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"));
                    //responseString = responseString.Replace("[JQUERYUI]", Helper.LinkCdn("http://ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js"));
                    //responseString = responseString.Replace("[POPPER]", Helper.LinkCdn("https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.3/umd/popper.min.js"));
                    //responseString = responseString.Replace("[BOOTSTRAP]", Helper.LinkCdn("https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-beta.2/js/bootstrap.min.js"));
                    //responseString = responseString.Replace("[CLIENTJS]", Helper.LinkCdn("http://mw2.adie.space/js/client.js"));
                    //responseString = responseString.Replace("[MOMENT]", Helper.LinkCdn("http://mw2.adie.space/js/moment.js"));
                    //responseString = responseString.Replace("[CSS]", Helper.LinkCdn("http://mw2.adie.space/css/site.css"));
                    //responseString = responseString.Replace("[CSRF]", FakeMW2SA.Program.csrf.ToString());

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
            catch (HttpListenerException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to open the application on port " + Program.port + "/" + ". Is the application already running?");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void Start()
        {
            Thread a = new Thread(Run);
            a.Start();
        }
    }
}
