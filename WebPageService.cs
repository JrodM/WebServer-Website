using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace CS_Web
{
    internal class WebPageService: WebService
    {
        public WebPageService()
        {
            
        }

        public override void Handler(WebRequest req)
        {
            string[] first_split = req.URI.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            string[] pieces = new string[first_split.Length - 1];

            //Dir422 dir = r_sys.GetRoot();

            // check the make sure there wasnt garbage after home 
            if (first_split[0] != "home")
            {
                req.WriteNotFoundResponse(req.URI);
                return;
            }

         }


        public override string ServiceURI
        {
                get { return "/home"; }
        }
        
        }
    }

