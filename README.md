# WebServer-Website 
 Currently this project accepts TCP connections and assigns them a thread from a thread pool to handle all subsequent http requests from the tcp client.

I failed to find the final version of this project so there is a known bug for sending (maybe) binary data files and definitly when they try to POST them. When I handle the webrequest I make the mistake of changing the binary body into a string before uploading into the filesystem.

#NOTE# After playing with Node.js I realized that I'm reinventing the wheel and probably will end up using the MERN stack to build my website. 
#Going to do a better job annotating code, also moved to .NET Core MVC
