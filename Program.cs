using System;
using CS_Web;

namespace Webserver
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            StandardFileSystem t = StandardFileSystem.Create("/Users/jared/Webserver_files");

            WebServer.AddService(new FilesWebService(t));
            WebServer.AddService(new WebPageService());
            WebServer.Start(1130, 100);


            while (true)
            {
            }


        }
    }
}
