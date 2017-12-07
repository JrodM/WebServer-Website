# WebServer-Website
I plan on expanding on how everything works once I finish my finals next week. Currently this project
accepts TCP connections and assigns them a thread from a thread pool to handle all subsequent http requests from the tcp client.

I failed to find the final version of this project so there is a known bug for sending (maybe) binary data files and definitly when they try to POST them. When I handle the webrequest I make the mistake of changing the binary body into a string before uploading into the filesystem.

