using System;
using System.IO;
using System.Collections;
using  System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace CS_Web
{

	public abstract class Dir_Jrod
	{

		public abstract string Name{ get;}
		//directory name
		public abstract IList<File_Jrod> GetFiles();
		//get files
		public abstract Dir_Jrod Parent{get;}

		public abstract IList<Dir_Jrod> GetDirs();
		/* o Gets a list of all the directories contained within this one. This is NOT a recursive search.
        It only returns the directories that are directly inside this one.
        */
		public abstract bool ContainsFile (string fileName, bool recursive);
		/* 
         * o Searches for a file with the specified name within this directory, optionally recursively
searching subdirectories if requested. Returns true if the file is found within the scope of
the search, false if it is not found.
o Must reject file names with path characters. So if the file name string contains the /or \*/

		// does it contain a dir?
		public abstract bool ContainsDir(string dirName, bool recursive);

		// return the the dir in respect to our filesystem
		public abstract Dir_Jrod GetDir(string dirName);


		public abstract File_Jrod GetFile (string fileName);

		/*o Analogous to GetDir, only for a file. Must return null if the file name string contains path
            characters (/ or \).
            public abstract File_Jrod CreateFile(string fileName); */

		/*
		o First validates the file name to ensure that it does not have any invalid characters. For
		our file systems we consider the only invalid characters to be the path characters (/ and
			\). If the file name has these characters, or is null or empty, then null is returned.
		o Otherwise, if the name is ok, the file is created with a size of 0 in this directory. Should
			the file already exist, it is truncated back to a length of 0, erasing all prior content.
			o A File_Jrod object is returned on success, null on failure.*/
			public abstract Dir_Jrod CreateDir(string fileName);
		/*
            o First validates the file name to ensure that it does not have any invalid characters. For
            our file systems we consider the only invalid characters to be the path characters (/ and
                \). If the file name has these characters, or is null or empty, then null is returned.
        o Unlike CreateFile, this will NOT delete contents if the directory already exists. Instead it
            will just return the Dir_Jrod object for the existing directory.
                o If the directory does not exist, it is created and returned.
                o Null is returned on failure.*/
		public abstract File_Jrod CreateFile(string fileName);

	}

	public abstract class File_Jrod
	{
		public abstract string Name{get;}
		public abstract Dir_Jrod Parent{get;}
		public abstract Stream OpenReadOnly();
		public abstract Stream OpenReadWrite();

	}


	public abstract class FileSys_Jrod
	{

		public abstract Dir_Jrod GetRoot();

		public virtual bool Contains(File_Jrod file)
		{
			return Contains(file.Parent);
		}

		public virtual bool Contains(Dir_Jrod dir)
		{
			if (dir == null) {
				return false;
			}

			if (dir == GetRoot ()) {
				return true;
			}


			return Contains (dir.Parent);

		}


	}




	public class StdFSDir: Dir_Jrod
	{
		public StdFSDir(string names, Dir_Jrod parents)
		{

			name = names;
			parent = parents;

		}

		private string name;
		private Dir_Jrod parent;
		public List<StdFSDir> dirs = new List<StdFSDir>();
		public List<StdFSFile> files = new List<StdFSFile> ();

		public override string Name{ get{ return name; }}
		//get file list from this directory
		public override IList<File_Jrod>  GetFiles()
		{
			IList<File_Jrod>  temp = new List<File_Jrod>  ();

			foreach (File_Jrod x in files) {

				temp.Add (x);

			}

			return temp;

		}

		//directory name
		public override IList<Dir_Jrod>  GetDirs()
		{
			IList<Dir_Jrod>  temp = new List<Dir_Jrod>  ();

			foreach (Dir_Jrod x in dirs) {

				temp.Add (x);

			}

			return temp;

		}

		//get files
		public override Dir_Jrod Parent{get{return parent;}}

		public override bool ContainsFile (string fileName, bool recursive)
		{
			if (recursive == true)
			{

				if (dirs.Count == 0)
				{

					foreach (File_Jrod j in files)
					{

						if (j.Name == fileName)
						{
							return true;
						}

					}
					return false;
				}
				else
				{// else if theres still directorys to traverse.

					// first we check the files in this dir

					foreach (File_Jrod j in files)
					{

						if (j.Name==fileName)
						{
							return true;
						}

					}
					// now we check lowerlevels of directorys
					foreach (Dir_Jrod j in dirs)
					{
						if (j.ContainsFile(fileName, true))
						{
							return true;
						} // if its found in a lower depth return true.
					}
					return false;
				}// end of else statement



			}
			else// no recursion
			{
				foreach (File_Jrod j in files)
				{

					if (j.Name==fileName)
					{
						return true;
					}

				}

				return false;
			}

		}

		// check to see if we can recursively *or not* find a directory with dirname as its Name property

		public override bool ContainsDir(string dirName, bool recursive)
		{
			if (recursive == true)
			{

				if (dirs.Count == 0)// if there arent any dirs below this one
				{


					return false;

				}
				else
				{// else if theres still directorys to traverse.
					// check this dir first

					// now we check lowerlevels of directorys
					foreach (Dir_Jrod j in dirs)
					{                    
						if (j.Name == dirName)
						{
							return true;
						}
						if (j.ContainsDir(dirName, true))
						{
							return true;
						} // if its found in a lower depth return true.
					}
					return false;
				}// end of else statement



			}
			else// no recursion
			{

				// now we check lowerlevels of directorys
				foreach (Dir_Jrod j in dirs)
				{
					if (j.Name == dirName)
					{
						return true;
					} // if its found in a lower depth return true.
				}
				return false;

			}

		}



		// get the directory within the given directory no recursion.
		public override Dir_Jrod GetDir(string dirName)
		{
			if (dirName.Contains ("/") || dirName.Contains ("\\")) {
				return null;
			}
				

			foreach (Dir_Jrod x in dirs)
			{
				if (x.Name == dirName)
				{
					return x;
				}
			}

			return null;

		}

		// get the file within the given directory no recursion.
		public override File_Jrod GetFile (string fileName)
		{
			if (fileName.Contains ("/") || fileName.Contains ("\\")) {
				return null;
			}

			foreach (File_Jrod x in files)
			{
				if (x.Name == fileName)
				{
					return x;
				}
			}

			return null;

		}

		// create a directory
		public override Dir_Jrod CreateDir(string fileName)
		{
			if (fileName.Contains ("/") || fileName.Contains ("\\")) {
				return null;
			}

			// check their arent any files or dirs with the same name
			foreach (StdFSDir x in dirs)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			foreach (StdFSFile x in files)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			StdFSDir dir = new StdFSDir (fileName, this);
			string m = dir.GetPath();
			// m = m + "/" + dir.name;
			dirs.Add (dir);
			string cd = Directory.GetCurrentDirectory();
			Directory.CreateDirectory(m);
			return dir;
		}

		// since we write to disk we actually need to make a file.
		public override File_Jrod CreateFile(string fileName)
		{

			if (fileName.Contains ("/") || fileName.Contains ("\\")) {
				return null;
			}

			// check their arent any files or dirs with the same name
			foreach (StdFSDir x in dirs)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			foreach (StdFSFile x in files)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			StdFSFile file1 = new StdFSFile (fileName, this);
			string m = file1.GetPath();
			//m = m + "/" + file1.Name;
			files.Add(file1);

			File.Create(m);
			return file1;
		}

		public string GetPath()
		{
			StringBuilder n = new StringBuilder();
			List<string> h = new List<string>();
			h.Add(this.name);

			Dir_Jrod temp = this.parent;

			while (temp != null)
			{
				h.Add(temp.Name);
				temp = temp.Parent;
			}
			// now the weve pieced it together reverse it
			h.Reverse();
			string removepath = Directory.GetCurrentDirectory();
			string [] remove_this = removepath.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);

			h.Remove(remove_this[remove_this.Length-1]);

            for(int i = 0;i<h.Count;i++)
            {

                n.Append(h[i]);

                if (i == h.Count-1)
                {
                    break;
                }

                n.Append("/");
            }
			return n.ToString(); 

		}
	}




	public class StdFSFile: File_Jrod
	{
		public StdFSFile (string fileName, Dir_Jrod par)
		{
			parent = par;
			name = fileName;
		}

		public string GetPath()
		{
			StringBuilder n = new StringBuilder();
			List<string> h = new List<string>();
			h.Add(this.name);

			Dir_Jrod temp = this.parent;

			while (temp != null)
			{
				h.Add(temp.Name);
				temp = temp.Parent;
			}
			// now the weve pieced it together reverse it
			h.Reverse();
			string removepath = Directory.GetCurrentDirectory();
			string [] remove_this = removepath.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);

			h.Remove(remove_this[remove_this.Length-1]);

            for(int i = 0;i<h.Count;i++)
            {

                n.Append(h[i]);

                if (i == h.Count-1)
                {
                    break;
                }

                n.Append("/");
            }
			return n.ToString(); 

		}




		private string name;
		private Dir_Jrod parent;



		public override string Name{get {return name;}}
		public override Dir_Jrod Parent{get{ return parent; }}

		// open the disk storage file
		public override Stream OpenReadOnly()
		{
			try
			{
                FileStream one = File.Open(GetPath(), FileMode.Open, FileAccess.Read,FileShare.Read);
				return one;
			}
			catch
			{
				return  null;
			}
		}

		public override Stream OpenReadWrite()
		{
			try
			{
                FileStream one = File.Open(GetPath(), FileMode.Open, FileAccess.ReadWrite);
				return one;
			}
			catch
			{
				return  null;
			}
		}
	}

	public class StandardFileSystem: FileSys_Jrod
	{


		public StdFSDir root = null;

		// this sets the root dir 
		public StandardFileSystem(StdFSDir new_root,string rootDir)
		{
			root = new_root;
			Directory.SetCurrentDirectory(rootDir);
		}

		public override Dir_Jrod GetRoot()
		{
			return root;
		}
		// set up our file system recusively look down the file system and log all of them with or fs
		public static StandardFileSystem Create(string rootDir)
		{
			/// need to see how much protection for malcious code.
			string[] dirs = rootDir.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);


			if (!Directory.Exists(rootDir))
			{
				return null;
			}


			//set the root 


			// this is the new filesystem we are creaating and returning
			StdFSDir root_dir =  new StdFSDir(dirs[dirs.Length - 1], null);
			StandardFileSystem creation = new StandardFileSystem(root_dir,rootDir); 

			// now we creation the filesystem structure internally with STDFSDIR AND STDFSFILE
			List<string> direc = new List<string>();
			foreach (string x in Directory.EnumerateDirectories(rootDir))
			{
				string[] list = x.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);
				direc.Add(list[list.Length-1]);
			}

			List<string> files = new List<string>();
			foreach (string x in Directory.EnumerateFiles(rootDir))
			{
				string[] list = x.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);
				// break the path up  so we can get the last name aka the name of the dir
				files.Add(list[list.Length-1]);
			}

			creation.recursiveAdd( rootDir,root_dir,direc,files);


			return creation;




		}
		// this is used in the start function
		private void recursiveAdd(String path, StdFSDir start, List<string> dir_, List<string> files_)
		{


			// add all of the files to the directory
			foreach (string x in files_)
			{ 

				start.files.Add(new StdFSFile(x, start));


			}
			// if there arent anymore dirs we exit.
			if (dir_.Count == 0)
			{
				return;
			}
			// add all of the 
			foreach (string x in dir_)
			{ 

				start.dirs.Add(new StdFSDir(x,start));

			}


			StringBuilder newpath = new StringBuilder();


			// This is where we recursively call the function, we hand it each new dir path we extracted and look for
			// dirs and files in those paths.
			foreach (StdFSDir x in start.dirs)
			{

				newpath.Append(path+"/"+x.Name);

				List<string> direc = new List<string>();
				foreach (string y in Directory.EnumerateDirectories(newpath.ToString()))
				{
					string[] list = y.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);
					// break the path up  so we can get the last name aka the name of the dir
					direc.Add(list[list.Length-1]);
				}

				List<string> files = new List<string>();
				foreach (string y in Directory.EnumerateFiles(newpath.ToString()))
				{
					string[] list = y.Split(new[] { '/' },StringSplitOptions.RemoveEmptyEntries);
					// break the path up  so we can get the last name aka the name of the dir
					files.Add(list[list.Length-1]);
				}


				recursiveAdd(newpath.ToString(), x, direc, files);
				newpath.Clear();

			}

			// recursiveAdd( newpath.ToString(), x,Directory.EnumerateDirectories(rootDir),Directory.EnumerateFiles(rootDir));

		}






	}


	public class MemFSDir: Dir_Jrod
	{
		public MemFSDir(string names, Dir_Jrod parents)
		{

			name = names;
			parent = parents;

		}

		private string name;
		private Dir_Jrod parent;
		public List<MemFSDir> dirs = new List<MemFSDir>();
		public List<MemFSFile> files = new List<MemFSFile>();

		public override string Name{ get{ return name; }}
		//directory name
		public override IList<File_Jrod>  GetFiles()
		{
			IList<File_Jrod> temp = new List<File_Jrod>  ();

			foreach (File_Jrod x in files) {

				temp.Add (x);

			}

			return temp;

		}


		public override IList<Dir_Jrod>  GetDirs()
		{
			IList<Dir_Jrod> temp = new List<Dir_Jrod>  ();

			foreach (Dir_Jrod x in dirs) {

				temp.Add (x);

			}

			return temp;

		}


		//get files
		public override Dir_Jrod Parent{get{return parent;}}

		public override bool ContainsFile (string fileName, bool recursive)
		{
			if (recursive == true)
			{

				if (dirs.Count == 0)
				{

					foreach (File_Jrod j in files)
					{

						if (j.Name == fileName)
						{
							return true;
						}

					}
					return false;
				}
				else
				{// else if theres still directorys to traverse.

					// first we check the files in this dir

					foreach (File_Jrod j in files)
					{

						if (j.Name==fileName)
						{
							return true;
						}

					}
					// now we check lowerlevels of directorys
					foreach (Dir_Jrod j in dirs)
					{
						if (j.ContainsFile(fileName, true))
						{
							return true;
						} // if its found in a lower depth return true.
					}
					return false;
				}// end of else statement



			}
			else// no recursion
			{
				foreach (File_Jrod j in files)
				{

					if (j.Name==fileName)
					{
						return true;
					}

				}

				return false;
			}

		}

		// check to see if we can recursively *or not* find a directory with dirname as its Name property

		public override bool ContainsDir(string dirName, bool recursive)
		{
			if (recursive == true)
			{

				if (dirs.Count == 0)// if there arent any dirs below this one
				{

					return false;

				}
				else
				{// else if theres still directorys to traverse.
					// now we check lowerlevels of directorys
					foreach (Dir_Jrod j in dirs)
					{
						if (j.Name == dirName)
						{
							return true;
						}

						if (j.ContainsDir(dirName, true))
						{
							return true;
						} // if its found in a lower depth return true.
					}
					return false;
				}// end of else statement



			}
			else// no recursion
			{
				foreach (Dir_Jrod j in dirs)
				{

					if (j.Name==dirName)
					{
						return true;
					}

				}

				return false;
			}

		}



		// get the directory within the given directory no recursion.
		public override Dir_Jrod GetDir(string dirName)
		{
			if (dirName.Contains ("/") || dirName.Contains ("\\")) {
				return null;
			}

			foreach (Dir_Jrod x in dirs)
			{
				if (x.Name == dirName)
				{
					return x;
				}
			}

			return null;

		}

		// get the file within the given directory no recursion.
		public override File_Jrod GetFile (string fileName)
		{
			if (fileName.Contains ("/") || fileName.Contains ("\\")) {
				return null;
			}

			foreach (File_Jrod x in files)
			{
				if (x.Name == fileName)
				{
					return x;
				}
			}

			return null;

		}


		public override Dir_Jrod CreateDir(string fileName)
		{
			if (fileName.Contains ("/") || fileName.Contains ("\\")) {
				return null;
			}

			// check their arent any files or dirs with the same name
			foreach (MemFSDir x in dirs)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			foreach (MemFSFile x in files)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			MemFSDir dir = new MemFSDir (fileName, this);
			dirs.Add (dir);
			return dir;
		}

		public override File_Jrod CreateFile(string fileName)
		{
			if (fileName.Contains ("/") || fileName.Contains ("\\")) {
				return null;
			}

			// check their arent any files or dirs with the same name
			foreach (MemFSDir x in dirs)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			foreach (MemFSFile x in files)
			{
				if (x.Name == fileName)
				{
					return null;
				}
			}

			MemFSFile file1 = new MemFSFile (fileName, this);
			files.Add (file1);
			return file1;
		}


	} 


	public class MemFSFile: File_Jrod
	{
		class jaredstream:MemoryStream
		{
			public event EventHandler SomethingHappened;
			// so we can keep track of whats closing
			public string fileName;

			public jaredstream():base()
			{

			}

			public override void Close()
			{
				SomethingHappened(this, EventArgs.Empty);
				base.Close();
				return;
			}

		}


		public MemoryStream storage = new MemoryStream();
		// the filecontents
		private string name;
		private Dir_Jrod parent;


		public override string Name{get {return name;}}
		public override Dir_Jrod Parent{get{ return parent; }}
		// keep track of how many files and the type of stream, the first value in the tuple is read the second is readwrite
		static public ConcurrentDictionary<string,Tuple<int,int>> open_File_List = new ConcurrentDictionary<string, Tuple<int,int>>();


		public void DecrimateWrite(object sender, EventArgs args)
		{
			// decrimate the second number in the tuple create a new tuple and assign it to the concurrent dictionary
			open_File_List[(sender as jaredstream).fileName] = Tuple.Create(open_File_List[(sender as jaredstream).fileName].Item1,
				open_File_List[(sender as jaredstream).fileName].Item2 - 1);


		}

		//decrimate the number of open read type of file streams
		public void DecrimateRead(object sender, EventArgs args)
		{
			open_File_List[(sender as jaredstream).fileName] = Tuple.Create(open_File_List[(sender as jaredstream).fileName].Item1-1,
				open_File_List[(sender as jaredstream).fileName].Item2);
		}

		public MemFSFile (string fileName, Dir_Jrod par)
		{


			parent = par;
			name = fileName;

			//if this file isnt being accounted for addd it to the concurrent dicionary
			if (!open_File_List.ContainsKey(name))
			{
				open_File_List[name] = Tuple.Create(0, 0);
			}

		}

	


		// open the disk storage file
		public override Stream OpenReadOnly()
		{
			lock(open_File_List)
			{// if there arent any read writes open


				if (open_File_List[name].Item2 == 0)
				{

					jaredstream temp = new jaredstream();
					temp.fileName = name;

					storage.CopyTo(temp);// create a copy for the user to edit
					temp.SomethingHappened+=DecrimateRead;

					open_File_List[name] = Tuple.Create(open_File_List[name].Item1+1,
						open_File_List[name].Item2);

					return temp;
				}
				else
				{
					return  null;
				}

			}

		}



		//open a file for read write 
		public override Stream OpenReadWrite()
		{
			lock(open_File_List)
			{// check too see if we can open and



				if (open_File_List[name].Item1 == 0 && open_File_List[name].Item2 == 0)
				{

					jaredstream temp = new jaredstream();
					temp.fileName = name;

					storage.CopyTo(temp);// create a copy for the user to edit
					temp.SomethingHappened+=DecrimateWrite;

					open_File_List[name] = Tuple.Create(open_File_List[name].Item1,
						open_File_List[name].Item2+1);

					return temp;
				}
				else
				{
					return  null;
				}

			}

		}
	}


	public class MemoryFileSystem: FileSys_Jrod
	{


		public MemFSDir root = null;

		// this sets the root dir 
		public MemoryFileSystem(string rootDir)
		{
			root = new MemFSDir(rootDir,null);

		}

		public MemoryFileSystem()
		{
			root = new MemFSDir("files",null);

		}

		public override Dir_Jrod GetRoot()
		{
			return root;
		}
		// set up our file system recusively look down the file system and log all of them with or fs





	}
}
