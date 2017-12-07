
﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace CS_Web
{
	public class WebRequest
	{
		public string method;
		public string URI;
		public string version;

		public Stream body;
		private NetworkStream responseStream;

		public Dictionary<string, string> headers = new Dictionary<string, string>();	

		public WebRequest()
		{
			headers = new Dictionary<string, string> ();
		}
			

		public void addHeader(string key, string value)
		{
			headers.Add(key, value);
		}



		public NetworkStream Response
		{
			get
			{
				return responseStream;
			}

			set
			{
				responseStream = value;
			}
		}
			

		public void WriteNotFoundResponse(string pageHTML)
		{
			
			StringBuilder j = new StringBuilder ();

			j.Append( String.Format ("{0} 404 Not Found\r\nContent-Type:text/html\r\n", version));
			/*if (headers.ContainsKey ("Content-Length")) {

				j.Append (String.Format("Content-Length:{0}\r\n\r\n", headers ["Content-Length"]));

			} else {
				j.Append ("\r\n");
			}*/

			j.Append (pageHTML);
            j.Append("\r\n");
			byte[] response = Encoding.ASCII.GetBytes(j.ToString());
			responseStream.Write(response, 0, j.Length);
		}


//		public bool WriteHTMLResponse(string htmlString)
//		{ 
//			StringBuilder j = new StringBuilder ();
//			j.Append( String.Format ("{0} 200 OK\r\nContent-Type:text/html\r\n", version));
//			if (headers.ContainsKey ("Content-Length")) {
//
//				j.Append (String.Format("Content-Length:{0}\r\n\r\n", headers ["Content-Length"]));
//			} else {
//				j.Append ("\r\n");
//			}
//
//			j.Append (htmlString);
//
//			byte[] response = Encoding.ASCII.GetBytes(j.ToString());
//			responseStream.Write(response, 0, response.Length);
//
//			return true;
//		}

        public bool WriteHTMLResponse(string htmlString)
        { 
            StringBuilder j = new StringBuilder ();
            j.Append( String.Format ("{0} 200 OK\r\nContent-Type:text/html\r\nContent-Length:{1}\r\n"
                ,version,Encoding.ASCII.GetBytes(htmlString).Length));

            //if (headers.ContainsKey("Content-Length"))
            //{

             //   j.Append(String.Format("Content-Length:{0}\r\n\r\n", headers["Content-Length"]));
            //}
            //else
            //{
                j.Append("\r\n");
            //}

            j.Append (htmlString);

            byte[] response = Encoding.ASCII.GetBytes(j.ToString());
            responseStream.Write(response, 0, response.Length);

            return true;
        }


	}
}