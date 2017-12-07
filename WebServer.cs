
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;
using System.Diagnostics;
using CS_Web;
//sophicles, eurpides, art of courtley love, ovid,
namespace CS_Web
{

	public class WebServer// WEBSERVER CLASS
	{
		/*public static void Main()
		{
			WebServer.Start (1100, 100);
			WebServer.Stop ();
			while (true) {
			}
		}*/

		public WebServer ()// empty constructor
		{

		}

		static List<WebService> services = new List<WebService>();
		public static bool dispose;
		public static Thread TCP_Listening_Thread;// we have this so we can close the listening thread in stop.
		public static BlockingCollection<TcpClient> coll;
		public static Thread [] Tarray;
		private const int firstLBsize = 	2048;
		private const int doubleLBsize = 2048 * 1000;// these are the sizes for the various line break limits
		public delegate void tm();

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

		public static bool Start (Int32 port, Int32 threadMax)
		{


			if (threadMax < 0) {
				threadMax = 64;
			}
			coll = new BlockingCollection<TcpClient> ();
			Tarray = new Thread[threadMax];
			dispose = false;
            //services = new List<WebService>();

            IPAddress localAddr = IPAddress.Parse("127.0.0.1"); //IPAddress.Parse(GetLocalIPAddress());
			IPEndPoint pass_this = new IPEndPoint(localAddr,port);


			TcpListener j = new TcpListener (pass_this);
			//ThreadStart threadDelegate = new ThreadStart(WebServer.ThreadWorkFunc);
			// start the thread to handle all tcp requests
			//Thread t = new Thread(WebServer.ThreadWorkFunc);
			TCP_Listening_Thread = new Thread(() => WebServer.ThreadWorkFunc(j,threadMax));
			TCP_Listening_Thread.Start ();


			return true;

		}



		// build a clients request from and incoming message and respond.

		private static WebRequest BuildRequest (TcpClient client)
		{

			// NOW we process the streamand make a webrequest.
			NetworkStream stream = client.GetStream ();

			bool http = false;
			bool GET = false;// confirms that theres a valid get request. 1==GET && input = "GE
			int i = 0;//bytes read
			Byte[] bytes = new Byte[256];// to read in... this is the buffer
			StringBuilder data = new StringBuilder (); // post byte conversion data
			string newdata;
			Stopwatch timeout = new Stopwatch ();

			//break the loop when we have invalid data


			/* TIME OUTS */
			//stream.CanTimeout = true;
			stream.ReadTimeout = 1699;
			/// expire if we dont read the body fast enough.

			timeout.Start();
			while (true) { 

				// catch any timeouts

				try{
                   // System.Text.ASCIIEnc

					if ((i = stream.Read (bytes, 0, bytes.Length)) != 0) {
                        
                        newdata = System.Text.Encoding.ASCII.GetString (bytes, 0, i);


						data.Append (newdata);
						//DO CHECKS TO VALIDATE AND ENSURE WE'VE READ THE ENTIRE MESSAGE

						// see if timeout

						if (timeout.ElapsedMilliseconds > 10000) {
							stream.Close ();
							return null;
						}


                        ///data liimits if you read too much data before reaching a line break, then break.
						//Single LB
						if (!data.ToString ().Contains ("\r\n") == false && data.Length > firstLBsize) {
							stream.Close ();
							return null;
						}
						//double LB
						if (data.ToString ().Contains ("\r\n\r\n") == false && data.Length > doubleLBsize) {
							stream.Close ();
							return null;
						}




						string[] request = data.ToString ().Split (' ');

						// validate the various parts of the header
						if (ValidateGET (request [0], stream, ref GET) == false ||
							(request.Length > 2 && ValidateHttp (request [2], stream, ref http) == false)) {
							stream.Dispose ();
							return null;
						}

						if (http && GET && data.ToString ().Contains ("\r\n\r\n")) {

							//body timer
							timeout.Stop();


							WebRequest req = new WebRequest ();
							int sIndex = data.ToString ().IndexOf ("\r\n\r\n");
							string head = data.ToString ().Substring (0, sIndex);
							string[] headers = head.Split (new[]{ "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

							// if there are any headers add
							if (headers.Length > 1) {
								for(int x = 1; x< headers.Length; x++) {

									string[] temp = headers[x].Split (':');
									req.addHeader (temp [0], temp [1]);

								}
							}

							req.method = request [0];
                            //we only get a / if we're just trying to enter the webpage
                            if (request[1] ==  "/")
                            {
                                req.URI = "/files";
                            }
                            else
                            {
                                req.URI = Uri.UnescapeDataString(request[1]);
                            }

                            // remove the line break.
							req.version = request [2].Split (new[]{ "\r\n" }, StringSplitOptions.RemoveEmptyEntries) [0];
							req.Response = stream;


							// first check to see if we need one stream for the body or two
							if (sIndex == data.Length - 4) {
								req.body = stream;
							} else { // we need concat stream?

								byte[] forStream = Encoding.ASCII.GetBytes (data.ToString ().Substring (sIndex + 4));
								var memStream = new MemoryStream (forStream);

								// do we have
								if (req.headers.ContainsKey ("Content-Length")) {
									long temp;
									long.TryParse (req.headers ["Content-Length"], out temp);
									req.body = new ConcatStream (memStream, stream, temp);
								}

								ConcatStream body = new ConcatStream (memStream, stream);
								req.body = body;
							}							

							return req;
						}// IF BODY FOR CREATING WEBREQUEST


						// Send back a response.
					}	

				}
				catch(IOException e)
				{
					continue;

				}


			}/// end of while loop



		}


		// hold all of the threads and delegates a new request
		static void ThreadWorkFunc(TcpListener lis, Int32 threads) {


            lis.Start ();

			for(int i=0; i < threads; i++)
			{
				Thread t = new Thread(() => WebServer.ThreadWork());/// this is where each tcpclient is handled
				t.Start ();

				Tarray[i] = t;
			}


			lis.Start();
			// go until done
			while (!dispose) {
				//TcpClient j = coll.Take();

				TcpClient client = lis.AcceptTcpClient();

				NetworkStream stream = client.GetStream();//the data stream to send & receive\

				coll.Add (client);// add the new client to a thread and wait to process.


			}


			/* remove everything below and put in stop */


		}


		//hanlde an individual tcp request
		static void ThreadWork()
		{
			while (true) {
				TcpClient j = coll.Take();

				if (dispose == true) {
					return;
				}

				WebRequest req = BuildRequest(j);

				if (req == null)
				{
					j.Close ();
				} 
				else 
				{
					bool torf = false;// tell me if  HANDLED IT

					foreach(WebService x in services)
					{
						if (req.URI.StartsWith(x.ServiceURI)) {
							torf = true;
							x.Handler(req);
						}
					}

					if (!torf) {
						req.WriteNotFoundResponse ("");
					}
	
				}

				if (dispose == true) {
					return;
				}

			}
		}


		public static bool  ValidateGET(string data, NetworkStream stream,ref bool GET)
		{

			if (data.Length > 3 || ("GET").Substring (0, data.Length)
				!= data) {
				stream.Dispose ();
				return false;
			} 
			else 
			{
				if (data.Length == 3 && "GET" == data) 
				{
					GET = true;
				}
				return true;
			}


		}

		// this function tells us if the http request is valid, returns false otherwise.
		public static bool  ValidateHttp(string data, NetworkStream stream,ref bool http)
		{
			if (data.Length <= ("HTTP/1.1\r\n").Length) 
			{
				if(("HTTP/1.1\r\n").Substring (0, data.Length) != data)
				{
					stream.Dispose ();
					return false;
				}
				else
				{
					// if the we dont need to check the http version anymore
					if (data.Length == ("HTTP/1.1\r\n").Length) {
						http = true;
					}
					return true;
				}
			} 
			else // if the length is bigger (might be the start of body) 
			{
				string[] stringSeparators = new string[] {"\r\n"};
				string[] cleandata = data.Split(stringSeparators, StringSplitOptions.None);

				if (cleandata[0].Length > ("HTTP/1.1").Length || "HTTP/1.1" != cleandata[0] ) 
				{
					stream.Dispose ();
					return false;
				}
				else
				{
					if (cleandata[0] == "HTTP/1.1") {
						http = true;
					}
					return true;
				}
			}

			return true;
		}



		public static void AddService(WebService service)
		{
			services.Add(service);
		}

		public static void Stop()
		{
			dispose = true;// clean up

			TCP_Listening_Thread.Abort ();
			for (int i = 0; i < Tarray.Length; i++) {
				coll.Add(null);
			}
		}

       
		private static void BodyTimeout(NetworkStream l)
		{
			l.Close ();
			throw new TimeoutException ();
		}


	}//END OF CLASS

}//END OF NAMESPACE
