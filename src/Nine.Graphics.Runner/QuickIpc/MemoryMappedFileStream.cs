using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Text;
using System.Runtime.InteropServices; 
using System.Security.Permissions;

namespace QuickIPC
{
	/// <summary>
	///	This class provides methods to create, provide file
	///	access, write, read and remove a memory mapped file
	///	from the store.
	/// </summary>
	[Serializable]
    internal class MemoryMappedFileStream : Stream, IDisposable
	{
		#region Field & constant declaration

		private static ResourceManager rm = new ResourceManager( 
			typeof(MemoryMappedFileStream).Namespace + ".MemoryMappedFileText", 
			Assembly.GetExecutingAssembly());

		private MemoryProtection memProtection;
		private string objectName = "";
		private uint mapLength = 0;
		private uint dataPosition = 0;
		private int viewOffSet = 0;
		private int viewLength = 0;
		
		private bool isReadable = false;
		private bool isWritable = false;
		private bool isSeekable = false;
        
		private IntPtr mapHandle;
		private IntPtr mapViewPointer;

		#endregion

		#region Constructors

		private MemoryMappedFileStream(IntPtr mapHandler, MemoryProtection protection)
		{
			if(mapHandler == IntPtr.Zero)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_MapHandler"));
			} 
			isReadable = true;
			isWritable = true;
			isSeekable = true;
			mapHandle = mapHandler;
			mapLength = 0;
			memProtection = protection;
		}

		public MemoryMappedFileStream(string name, MemoryProtection protection): 
		    this (name,0,protection)
		{}

		public MemoryMappedFileStream(string name, uint maxLength, MemoryProtection protection)
		{
			if(string.IsNullOrEmpty(name))
			{
				throw(new ArgumentException("Argument_Name"));
			}
		    
			if(maxLength < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_MaxLength"));
			}
		    
			objectName = name;

			if (protection < MemoryProtection.PageNoAccess || 
				protection > MemoryProtection.SecReserve)
			{
				throw new ArgumentOutOfRangeException("ArgumentOutOfRange_Enum");
			}
            
			if( maxLength == 0 )
			{
                mapLength = 5120;
			}
			else
			{
				mapLength = maxLength;
			}

			memProtection = protection;

			isReadable = true;
			isWritable = true;
			isSeekable = true;

			// Initialize mapViewPointer
			mapViewPointer = new IntPtr(-1);

		    SECURITY_ATTRIBUTES sa = SECURITY_ATTRIBUTES.GetNullDacl();
		    try
		    {
                mapHandle = MemoryMappedFileHelper.CreateFileMapping(
                                                    IntPtr.Zero,
                                                    sa,
                                                    memProtection,
                                                    0,
                                                    mapLength,
                                                    objectName);
		    }
			finally
		    {
                Marshal.FreeHGlobal(sa.lpSecurityDescriptor);
                sa.lpSecurityDescriptor = IntPtr.Zero;
		    }

			if(mapHandle == IntPtr.Zero )
			{
				uint lastError = MemoryMappedFileHelper.GetLastError();
				throw new IOException( 
					MemoryMappedFileHelper.GetWin32ErrorMessage( lastError ) ); 
			}
		}

		#endregion

		#region Stream class implementation

		/// <summary>
		///	Gets the status whether the memory mapped 
		///	file has read access.
		/// </summary>
        public override bool CanRead
		{
			get
			{ 
				return ( mapViewPointer != IntPtr.Zero && isReadable );
			} 
		}

		/// <summary>
		///	Gets the status whether the memory mapped 
		///	file has seek access.
		/// </summary>
		public override bool CanSeek
		{
			get
			{ 
				return ( mapViewPointer != IntPtr.Zero && isSeekable ); 
			}
		}

		/// <summary>
		///	Gets the status whether the memory mapped 
		///	file has write access.
		/// </summary>
		public override bool CanWrite
		{
			get
			{ 
				return ( mapViewPointer != IntPtr.Zero && isWritable ); 
			}
		}

		/// <summary>
		///	Removes all the memory mapped files from the store.
		/// </summary>
		public override void Flush()
		{
			MemoryMappedFileHelper.FlushViewOfFile( mapViewPointer,
				(uint)viewLength ); 
		}

		/// <summary>
		///	Gets the length of the memory mapped file.
		/// </summary>
		public override long Length
		{
			get
			{ 
				return viewLength; 
			}
		}

		/// <summary>
		///	Gets the position of the memory mapped file.
		/// </summary>
		public override long Position
		{
			get
			{ 
				return dataPosition; 
			}
			set
			{
				dataPosition = (uint)value;
			}
		}

		/// <summary>
		///	Read the number of bytes from the buffer.
		/// </summary>
		/// <remarks>
		///	int tmpCount = Read( buffer, offset, count );
		/// </remarks>
		/// <param name="buffer">
		///	The data to be written
		/// </param>
		/// <param name="offset">
		///	The start position
		/// </param>
		/// <param name="count">
		///	The length of the binary stream to be written
		/// </param>
		public override int Read(byte[] buffer, int offSet, int count)
		{
			if(Object.Equals(buffer, null))
			{
				throw(new ArgumentNullException("ArgumentNull_Buffer"));
			}
		    
			if(offSet < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_OffSet"));
			}
		    
			if(count < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_Count"));
			}

			int tmpCount = count;

			if( (dataPosition + count) > viewLength )
			{
				tmpCount = viewLength - (int)dataPosition;
			}

			Marshal.Copy( 
				new IntPtr( mapViewPointer.ToInt32() + dataPosition ), 
				buffer, offSet, tmpCount );

			dataPosition += (uint)tmpCount;
			return tmpCount;

		}

		/// <summary>
		///	Sets a position within the stream of bytes.
		/// </summary>
		///	long seekPosition = Seek( offset, origin );
		/// <param name="offset">
		///	The offset position
		/// </param>
		/// <param name="origin">
		///	Origin position of the file
		/// </param>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if(offset < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_OffSet"));
			}
		    
			switch( origin )
			{
				case SeekOrigin.Begin:
					dataPosition = (uint)offset;
					break;
				case SeekOrigin.Current:
					dataPosition += (uint)offset;
					break;
				case SeekOrigin.End:
					dataPosition = ( (uint)viewLength - (uint)offset );
					break;
			}
			return dataPosition;
		}

		/// <summary>
		///	Set the length of the memory mapped file.
		/// </summary>
		/// <remarks>
		///	SetLength( memoryLength );
		/// </remarks>
		/// <param name="memoryLength">
		///	The length of the file
		/// </param>
		public override void SetLength( long memoryLength )
		{
			if(memoryLength < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_MemoryLength"));
			}
			MapViewToProcessMemory( viewOffSet, (int)memoryLength );
		}

		/// <summary>
		///	Write the binary data into the memory mapped file.
		/// </summary>
		/// <remarks>
		///	Write( buffer, offset, count );
		/// </remarks>
		/// <param name="buffer">
		///	The data to be written
		/// </param>
		/// <param name="offset">
		///	The start position
		/// </param>
		/// <param name="count">
		///	The length of the binary stream to be written
		/// </param>
		public override void Write(byte[] buffer, int offSet, int count)
		{
			if(Object.Equals(buffer, null))
			{
				throw(new ArgumentNullException("ArgumentNull_Buffer"));
			}
		    
			if(offSet < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_OffSet"));
			}
		    
			if(count < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_Count"));
			}
		    
			if( (dataPosition + count) > viewLength )
			{
				throw new IOException("Cant_write_end_stream");
			}

			Marshal.Copy( buffer, offSet, 
				new IntPtr(mapViewPointer.ToInt32() + dataPosition), count );

			dataPosition += (uint)count;
		}

		/// <summary>
		///	Close the memory mapped file.
		/// </summary>
		/// <remarks>
		///	Close();
		/// </remarks>
		public override void Close()
		{
			if( mapViewPointer != IntPtr.Zero )
			{
				MemoryMappedFileHelper.UnmapViewOfFile( mapViewPointer );
			}
			base.Close(); 
			System.GC.SuppressFinalize(this);
		}

		/// <summary>
		///	Close the map handle of the memory map.
		/// </summary>
		/// <remarks>
		///	CloseMapHandle();
		/// </remarks>
		public void CloseMapHandle()
		{
			if( mapHandle != IntPtr.Zero )
			{
				MemoryMappedFileHelper.CloseHandle( mapHandle );
			}
		}

		#endregion
		
		#region Custom methods

		#region MapViewToProcessMemory
		/// <summary>
		///	View the length and the offset of the memory
		///	mapped file.
		/// </summary>
		/// <remarks>
		///	MapViewToProcessMemory( offSet, count );
		/// </remarks>
		/// <param name="offSet">
		///	The start position
		/// </param>
		/// <param name="count">
		///	The length of the binary stream
		/// </param>
		public void MapViewToProcessMemory(int offSet, int count)
		{
			const int MEMORY_UNAVAILABLE = 5;
			if(offSet < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_OffSet"));
			}
		    
			if(count < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_Count"));
			}
		    
			if( mapViewPointer != IntPtr.Zero )
			{
				MemoryMappedFileHelper.UnmapViewOfFile( mapViewPointer );
			}
		    
 			mapViewPointer = MemoryMappedFileHelper.MapViewOfFile(
				mapHandle,
				MemoryMappedFileHelper.GetWin32FileMapAccess(memProtection),
				(uint)offSet,
				0,
				(uint)count);

			if( mapViewPointer == IntPtr.Zero )
			{
				// If GetLastError returns 5 throw specific exception
				if(MemoryMappedFileHelper.GetLastError() == MEMORY_UNAVAILABLE)
				{
					throw new Exception("RES_MemoryUnavailable");
				}
				else
				{
					throw new IOException( 
						MemoryMappedFileHelper.GetWin32ErrorMessage( 
						MemoryMappedFileHelper.GetLastError() ) );
				}
			}

			viewOffSet = offSet;
			viewLength = count;
		}
		#endregion

		#region SetMaxLength
		/// <summary>
		///	Set the maximum length of the file.
		/// </summary>
		/// <remarks>
		///	SetMaxLength( maxLength );
		/// </remarks>
		/// <param name="maxLength">
		///	Specifies the length to be set
		/// </param>
		public void SetMaxLength(long maxLength)
		{
			if(maxLength < 0)
			{
				throw(new ArgumentOutOfRangeException("ArgumentOutOfRange_MaxLength"));
			}
		    
			if( mapViewPointer != IntPtr.Zero )
			{
				MemoryMappedFileHelper.UnmapViewOfFile( mapViewPointer );
			}

			MemoryMappedFileHelper.CloseHandle( mapHandle );

			mapLength = (uint)maxLength;

            SECURITY_ATTRIBUTES sa = SECURITY_ATTRIBUTES.GetNullDacl();
            try
            {
                mapHandle = MemoryMappedFileHelper.CreateFileMapping(
                                                    IntPtr.Zero,
                                                    sa,
                                                    memProtection,
                                                    mapLength,
                                                    0,
                                                    objectName);
            }
            finally
            {
                Marshal.FreeHGlobal(sa.lpSecurityDescriptor);
                sa.lpSecurityDescriptor = IntPtr.Zero;
            }
		    
			if( mapHandle == IntPtr.Zero )
			{
				throw new IOException(  
					MemoryMappedFileHelper.GetWin32ErrorMessage( 
					MemoryMappedFileHelper.GetLastError() ) ); 
			}

			this.MapViewToProcessMemory( viewOffSet, viewLength );
		}
		#endregion
        
		#region OpenExisting
		/// <summary>
		///	Open the existing memory mapped file
		/// </summary>
		/// <remarks>
		///	MemoryMappedFileStream mmfs = OpenExisting( name, 
		///	protection  );
		/// </remarks>
		/// <param name="name">
		///	The name of the file
		/// </param>
		/// <param name="protection">
		///	The protection rights of the file
		/// </param>
		/// <returns>
		///	An instance of MemoryMappedFileStream class 
		/// </returns>
		public static MemoryMappedFileStream OpenExisting(string name,
			MemoryProtection protection)
		{
			if(Object.Equals(name, null) || name.Length == 0)
			{
				throw(new ArgumentException("name"));
			}
		    
			IntPtr mapHandler = MemoryMappedFileHelper.OpenFileMapping(
				MemoryMappedFileHelper.GetWin32FileMapAccess(protection),
				false,
				name);

			if( mapHandler == IntPtr.Zero )
			{
				throw new IOException( 
					MemoryMappedFileHelper.GetWin32ErrorMessage( 
					MemoryMappedFileHelper.GetLastError()));
			} 

			return new MemoryMappedFileStream( mapHandler, protection );
		}

		/// <summary>
		///	Open the dictionary object if it is available. This method is
		///	specifically used in the MmfStorage to find out the existance of
		///	the HybridDictionary object.
		/// </summary>
		/// <remarks>
		///	IntPtr dictPtr = OpenDictionary( name, protection );
		/// </remarks>
		/// <param name="name">
		///	The name of the file
		/// </param>
		/// <param name="protection">
		///	The protection rights of the file
		/// </param>
		/// <returns>
		///	An instance of IntPtr
		/// </returns>
		public static IntPtr OpenDictionary (string name,
			MemoryProtection protection)
		{

			IntPtr mapHandler = MemoryMappedFileHelper.OpenFileMapping(
				MemoryMappedFileHelper.GetWin32FileMapAccess( protection ),
				false,
				name );

			return mapHandler;
		}

		#endregion

		#endregion

		#region Custom properties

		/// <summary>
		///	Gets the maximum length of the file.
		/// </summary>
		public uint MaxLength
		{
			get{ return mapLength; }
		}

		#endregion
	
		#region IDisposable implementation

		/// <summary>
		///	Dispose the instance of the memory mapped
		///	file.
		/// </summary>
		public void Dispose()
		{
			Close();			
		}

		#endregion
	}
}
