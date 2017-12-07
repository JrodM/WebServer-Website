using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;

namespace CS_Web
{
	public class test
	{


		public static Stream GenerateStreamFromString(string s)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

	}


	public class ConcatStream: Stream
	{ /* 	One	will	store	the	current	stream	position,
        and	the	other	the	stream	length,	in	bytes */
		private long pos;//position
		private long len; //length in bytes
		private bool canseek;
		private bool canread;
		private bool canwrite;

		private bool islength;
		private bool constructor;// false means the first constructor, true means the second.

		Stream one;
		Stream two;

		public ConcatStream(Stream first, Stream second)
		{
			one = first;
			two = second;

			if (first.CanRead && second.CanRead)
			{
				canread = true;
			} 
			else {

				canread = false;
			}
			// can we write?
			if (first.CanWrite && second.CanWrite)
			{
				canwrite = true;
			} 
			else {

				canwrite = false;
			}

			constructor = false;
			// dont need to anything with the contructor number since it instantiates to false.

			try
			{
				//islength= false;// so we dont get an exception in the throw when we try to access is length.

				if(first.Length >=0)// throw exception if non accesable
				{
					islength = true; // so we know that the first stream checks out.
				}

				if(second.CanSeek && first.CanSeek)
				{
					canseek = true;// default is read foward only
				}

				if(second.Length >=0){SetLength(second.Length+first.Length);}// throw exception but continue since the first stream has a length
				// but dont support length in this implementation

			}
			catch (Exception)// catch our exceptions if we can't access length
			{
				if (islength) 
				{
					islength = false;
				} 
				else // we know that if we reach this point the first stream doesn't have a length so we cant continue
				{
					throw;// only throw if first stream cant be read.
				}
			}
		}

		public ConcatStream(Stream first, Stream second, long fixedLength)
		{

			one = first;
			two = second;

			if (first.CanRead && second.CanRead)
			{
				canread = true;
			} 
			else {

				canread = false;
			}
			// can we write?
			if (first.CanWrite && second.CanWrite)
			{
				canwrite = true;
			} 
			else {

				canwrite = false;
			}
			constructor = true;

			try
			{

				if(first.Length >=0)// throw exception if non accesable
				{
					islength = true; // so we know that the first stream checks out.
				}

				if(second.CanSeek && first.CanSeek)
				{
					canseek = true;// default is read foward only
				}

				SetLength(fixedLength);// set the length

				if(second.Length >=0);// throw exception but continue since the first stream has a length
				// but dont support length in this implementation

			}
			catch (Exception)// catch our exceptions if we can't access length
			{
				if (!islength) 
				{
					throw;
				} 
			}
		}


		public override long Position
		{
			get
			{
				if (CanSeek)
					return pos;
				else
					throw new NotSupportedException ();

			}
			set {
				if (CanSeek) {
					if (value >= 0) {// if the stream isnt negative
						if (islength && value > len) {// if the value is greater// then the stream length
							Seek (len, SeekOrigin.Begin);
						} else {
							Seek (value, SeekOrigin.Begin);
						}
					} else {// if the stream is negative
						Seek (0, SeekOrigin.Begin);
					}
				} else {
					throw new NotSupportedException();
				}
			}// end of set

		}

		// get the length
		public override long Length
		{
			get
			{
				if (islength == false) {

					throw new NotSupportedException();
				}

				return len;
			}
		}


		// lets us know the this class can be read.
		public override bool CanRead
		{
			get { return canread; }
		}

		// lets us know the this class cannot write.
		public override bool CanWrite
		{
			get { return canwrite; }
		}

		public override bool CanSeek
		{
			get
			{
				return canseek;
			}

		}

		//procedurally generated read function
		public override int Read(byte[] buffer, int offset, int count)
		{
			bool start = false;// which stream to start at 0 is first 1 is second
			int start_index = offset;

			long bytesread = 0;
			long length = count;// the "length of the combined two streams" if there isnt length use the count as a max guidline
			//long index = offset;


			long i = 0;// keep track of # of bytes from each read


			if (!canread) 
			{
				throw new NotSupportedException();
			}

			if (islength) { 
				length = Length;
			}

			if (canseek) {
				long diff = pos - (one.Length);

				if (diff < 0) { // if we know the the position is in the first stream.
					one.Position = pos;
				} else {
					two.Position = diff;
					start = true;
				}
			}

			// go while the bytes read is less then count or the length of the stream.
			while (bytesread < count && bytesread < length  && bytesread < buffer.Length) {


				if (one.Length  == one.Position) {
					start = true;
				}

				// of we're still reading from the first stream
				if (start == false) {

					long how_much_2read = buffer.Length - start_index - bytesread;// space left in our buffer

					// check to see if theres less space in the buffer then is left in the pne.streaam
					if (how_much_2read > one.Length - one.Position) {

						how_much_2read = one.Length - one.Position;
					}


					// in case we want to read less then the full buffer
					if (how_much_2read > count-bytesread) {
						how_much_2read = count-bytesread;/// maybe minus bytes read
					}

					i = one.Read (buffer, (int)offset, (int)how_much_2read);



					offset += (int)i;
					bytesread += i;
					pos += i;

					//first
				} 
				else// read the second stream.
				{
					long how_much_2read = buffer.Length - start_index - bytesread;// space left in our buffer

					// check to see if theres less space in the buffer then is left in the pne.streaam
					if (islength && constructor == false && how_much_2read > two.Length - two.Position) {

						how_much_2read = two.Length - two.Position;
					}// if the second constructor was used
					else if (constructor && how_much_2read > Length - pos)
					{
						how_much_2read = Length - pos;
					}



					// in case we want to read less then the full buffer
					if (how_much_2read > count-bytesread) {
						how_much_2read = count-bytesread;/// maybe minus bytes read
					} 

					i = two.Read (buffer, (int)offset, (int)how_much_2read);
					//i = one.Read (buffer, 12, 10);

					offset += (int)i;
					bytesread += i;
					pos += i;


					if (i == 0) {
						break;
					}



				}




			}

			return (int)bytesread;
		}

		//read only implementation
		public override void Write(byte[] buffer, int offset, int count)
		{
			//while bytes read < stream one.length. POSITIONING

			bool start = false;// which stream to start at 0 is first 1 is second
			int start_index = offset;

			long byteswritten = 0;
			long length = count;// the "length of the combined two streams" if there isnt length use the count as a max guidline
			//long index = offset;

			long i = 0;// keep track of # of bytes from each read


			if (!canwrite) 
			{

				throw new NotSupportedException();
			}

			if (islength) { 
				length = Length;
			}

			long diff = pos - (one.Length);// a metric used to measure which stream we're in

			if (canseek) {
				//long diff = Position - (one.Length);

				if (diff < 0) { // if we know the the position is in the first stream.
					one.Position = pos;
				} else {

					two.Position = diff;
					start = true;
				}
			} 
			else if(CanSeek && diff>= 0 && pos != one.Length + two.Position)// take not t0 whether or not we can write. This is only when we cant seek
			{

				throw new NotSupportedException();

			}


			while (byteswritten < count && byteswritten + start_index < buffer.Length) 
			{

				if (one.Length  == one.Position) {
					start = true;
				}

				// test too see if were trying to expand.
				if (/*islength &&*/ constructor && pos == len) {
					return;
				}


				if (start == false) {
					long how_much_2read = buffer.Length - start_index - byteswritten;// space left in our buffer

					// check to see if theres less space in the buffer then is left in the pne.streaam
					if (how_much_2read > one.Length - one.Position) {

						how_much_2read = one.Length - one.Position;
					}


					// in case we want to read less then the full buffer
					if (how_much_2read > count - byteswritten) {
						how_much_2read = count - byteswritten;/// maybe minus bytes read
					}

					one.Write(buffer, offset, (int)how_much_2read);

					offset += (int)how_much_2read;
					byteswritten += how_much_2read;
					pos += how_much_2read;

				} 
				else 
				{// this is the case in which we start writing to the second stream


					long how_much_2read = buffer.Length - start_index - byteswritten;// space left in our buffer

					// check to see if theres less space in the buffer then is left in the pne.streaam
					// the constructor variable tells us if we used a fixed length/ second constructor
					// which means we cant expand the second stream
					/*	if (islength && constructor == false && how_much_2read > two.Length - two.Position)
					{

						how_much_2read = two.Length - two.Position;

					} // second constructor
					else */if (constructor && how_much_2read > Length - pos)
					{
						how_much_2read = Length - pos;
					}

					// in case we want to read less then the full buffer
					if (how_much_2read > count - byteswritten) {
						how_much_2read = count - byteswritten;/// maybe minus bytes read
					}

					two.Write (buffer, offset, (int)how_much_2read);

					offset += (int)how_much_2read;
					byteswritten += how_much_2read;

					pos += how_much_2read;

					// for the case when we have the first constructor and want to expand the buffer
					if (!constructor && pos > len)
						len = pos;

				}

			}

		}


		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek) 
			{

				throw new NotSupportedException();;

			}

			long newOffset = 0;





			if (origin == SeekOrigin.Begin) {
				long diff = offset - (one.Length);

				if (diff < 0) { // if we know the the position is in the first stream.
					one.Position = pos;
					two.Position = 0;
				} else {
					one.Position = one.Length;
					two.Position = diff;

				}

				newOffset = offset;

			} else if (origin == SeekOrigin.Current) {

				long diff = (pos + offset) - (one.Length);

				if (diff < 0) { // if we know the the position is in the first stream.
					one.Position = pos;
					two.Position = 0;
				} else {
					one.Position = one.Length;
					two.Position = diff;

				}

				newOffset = pos + offset;

			} else if ( origin == SeekOrigin.End) 
			{
				one.Position = one.Length;
				two.Position = two.Length+offset;
				newOffset = Length + offset;
			}
			//long newoffset
			//origin += offset;
			pos = newOffset;
			return newOffset;
		}


		public override void Flush()
		{
			return;

		}


		public override void SetLength(long value) {

			if (islength == false) {
				throw new NotSupportedException();
			}

			if (value >= 0)
			{
				len = value;
			} else
			{
				len = 0;
			}

		}
	}

}
