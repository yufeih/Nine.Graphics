using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace QuickIPC
{
    #region Enumeration for Win32 macros

    [Flags]
	internal enum MemoryProtection : uint
	{ 
		PageNoAccess = 0x01, 
		PageReadOnly = 0x02, 
		PageReadWrite = 0x04, 
		PageWriteCopy = 0x08,
		SecImage = 0x1000000,
		SecReserve = 0x4000000,
		SecCommit = 0x8000000,
		SecNoCache = 0x10000000
	}

	[Flags]
	internal enum Win32FileMapAccess: uint
	{
		FILE_MAP_COPY = 0x0001,
		FILE_MAP_WRITE = 0x0002,
		FILE_MAP_READ = 0x0004,
		FILE_MAP_ALL_ACCESS = 0x000F0000 
			| FILE_MAP_COPY
			| FILE_MAP_WRITE 
			| FILE_MAP_READ 
			| 0x0008 
			| 0x0010
	}

	[Flags]
	internal enum Win32FileAccess : uint 
	{
		GENERIC_READ = 0x80000000,
		GENERIC_WRITE = 0x40000000,
		GENERIC_EXECUTE = 0x20000000,
		GENERIC_ALL = 0x10000000
	}

	[Flags]
	internal enum Win32FileMode : uint
	{
		CREATE_NEW = 1,
		CREATE_ALWAYS = 2,
		OPEN_EXISTING = 3,
		OPEN_ALWAYS = 4,
		TRUNCATE_EXISTING = 5
	}

	[Flags]
	internal enum Win32FileShare : uint
	{
		FILE_SHARE_READ = 0x00000001,
		FILE_SHARE_WRITE = 0x00000002,
		FILE_SHARE_DELETE = 0x00000004,
	}

	[Flags]
	internal enum Win32FileAttributes : uint
	{
		FILE_ATTRIBUTE_READONLY = 0x00000001,
		FILE_ATTRIBUTE_HIDDEN = 0x00000002,
		FILE_ATTRIBUTE_SYSTEM = 0x00000004,
		FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
		FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
		FILE_ATTRIBUTE_DEVICE = 0x00000040,
		FILE_ATTRIBUTE_NORMAL = 0x00000080,
		FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
		FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
		FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
		FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
		FILE_ATTRIBUTE_OFFLINE = 0x00001000,
		FILE_ATTRIBUTE_NOTCONTENTINDEXED = 0x00002000,
		FILE_ATTRIBUTE_ENCRYPTED = 0x00004000
	}

	#endregion

    [StructLayout(LayoutKind.Sequential)]
    internal class SECURITY_ATTRIBUTES : IDisposable
    {
        internal int nLength;
        internal IntPtr lpSecurityDescriptor;
        internal int bInheritHandle;

        /// <summary>
        /// NULL SA and Empty SA are two different things. NULL SA writes the current user's security
        /// attributes (SID) to the object being secured. Empty SA allows literally everybody to access the 
        /// object.
        /// </summary>
        /// <returns></returns>
        public static SECURITY_ATTRIBUTES GetNullDacl()
        {
            // Construct SECURITY_ATTRIBUTES structure
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.bInheritHandle = 1;

            // Build NULL DACL (Allow everyone full access)
            RawSecurityDescriptor gsd = new RawSecurityDescriptor(ControlFlags.DiscretionaryAclPresent, 
                                                                  null, 
                                                                  null, 
                                                                  null, 
                                                                  null);

            // Get binary form of the security descriptor and copy it into place
            byte[] desc = new byte[gsd.BinaryLength];
            gsd.GetBinaryForm(desc, 0);
            sa.lpSecurityDescriptor = Marshal.AllocHGlobal(desc.Length);
            Marshal.Copy(desc, 
                         0, 
                         sa.lpSecurityDescriptor, 
                         desc.Length);

            return sa;
        }
        
        public void Dispose()
        {
            lock (this)
            {
                if (lpSecurityDescriptor != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(lpSecurityDescriptor);
                    lpSecurityDescriptor = IntPtr.Zero;
                }
            }
        }
        
        ~SECURITY_ATTRIBUTES()
        {
            Dispose();
        }
    }

    

	#region Win32 functions and conversion methods
	/// <summary>
	///	Memory mapped file helper class that provides the Win32 functions and 
	///	the conversion methods.
	/// </summary>
	internal class MemoryMappedFileHelper
	{
		private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
		
		/// <summary>
		///	Private constructor prevents class from getting created.
		/// </summary>
		private MemoryMappedFileHelper()
		{}	

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern IntPtr CreateFileMapping(
			IntPtr hFile, // Handle to file
            SECURITY_ATTRIBUTES lpAttributes, // Security
			MemoryProtection flProtect, // protection
			uint dwMaximumSizeHigh, // High-order DWORD of size
			uint dwMaximumSizeLow, // Low-order DWORD of size
			string lpName // Object name
			);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]     
		public static extern IntPtr CreateFile(
			string lpFileName, // File name
			Win32FileAccess dwDesiredAccess, // Access mode
			Win32FileShare dwShareMode, // Share mode
			IntPtr lpSecurityAttributes, // SD
			Win32FileMode dwCreationDisposition, // How to create
			Win32FileAttributes dwFlagsAndAttributes, // File attributes
			IntPtr hTemplateFile // Handle to template file
			);

		[DllImport("kernel32")]
		public static extern IntPtr OpenFileMapping(
			Win32FileMapAccess dwDesiredAccess, // Access mode
			bool isInheritHandle, // Inherit flag
			string lpName // Object name
			);

		[DllImport("kernel32")]
		public static extern IntPtr MapViewOfFile(
			IntPtr hFileMappingObject, // handle to file-mapping object
			Win32FileMapAccess dwDesiredAccess, // Access mode
			uint dwFileOffsetHigh, // High-order DWORD of offset
			uint dwFileOffsetLow, // Low-order DWORD of offset
			uint dwNumberOfBytesToMap // Number of bytes to map
			);

		[DllImport("kernel32")]
		public static extern bool FlushViewOfFile(
			IntPtr lpBaseAddress, // Starting address
			uint dwNumberOfBytesToFlush	// Number of bytes in range
			);

		[DllImport("kernel32")]
		public static extern bool UnmapViewOfFile(
			IntPtr lpBaseAddress // Starting address
			);

		[DllImport("kernel32")]
		public static extern uint GetLastError();

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hFile);

		[DllImport("kernel32")]
		public static extern uint FormatMessage(
			uint dwFlags, // Source and processing options
			IntPtr lpSource, // Message source
			uint dwMessageId, // Message identifier
			uint dwLanguageId, // Language identifier
			StringBuilder lpBuffer, // Message buffer
			uint nSize, // Maximum size of message buffer
			IntPtr Arguments  // Array of message inserts
			);

		public static Win32FileMapAccess GetWin32FileMapAccess( 
			MemoryProtection protection )
		{
			switch ( protection )
			{
				case MemoryProtection.PageReadOnly:
					return Win32FileMapAccess.FILE_MAP_READ;
				case MemoryProtection.PageWriteCopy:
					return Win32FileMapAccess.FILE_MAP_WRITE;
				default:
					return Win32FileMapAccess.FILE_MAP_ALL_ACCESS;
			}
		}

		public static Win32FileAccess GetWin32FileAccess( FileAccess access )
		{
			switch( access )
			{
				case FileAccess.Read:
					return Win32FileAccess.GENERIC_READ;
				case FileAccess.Write:
					return Win32FileAccess.GENERIC_WRITE;
				case FileAccess.ReadWrite:
					return Win32FileAccess.GENERIC_READ | 
						Win32FileAccess.GENERIC_WRITE;
				default:
					return Win32FileAccess.GENERIC_READ;
			}
		}

		public static Win32FileMode GetWin32FileMode( FileMode mode )
		{
			switch( mode )
			{
				case FileMode.Append:
					return Win32FileMode.OPEN_ALWAYS;
				case FileMode.Create:
					return Win32FileMode.CREATE_ALWAYS;
				case FileMode.CreateNew:
					return Win32FileMode.CREATE_NEW;
				case FileMode.Open:
					return Win32FileMode.OPEN_EXISTING;
				case FileMode.OpenOrCreate:
					return Win32FileMode.OPEN_ALWAYS;
				case FileMode.Truncate:
					return Win32FileMode.TRUNCATE_EXISTING;
				default:
					return Win32FileMode.OPEN_ALWAYS;
			}
		}

		public static Win32FileShare GetWin32FileShare( FileShare share )
		{
			switch( share )
			{
				case FileShare.None:
					return 0;
				case FileShare.Write:
					return Win32FileShare.FILE_SHARE_WRITE;
				case FileShare.Read:
					return Win32FileShare.FILE_SHARE_READ;
				case FileShare.ReadWrite:
					return (Win32FileShare.FILE_SHARE_READ | 
						Win32FileShare.FILE_SHARE_WRITE);
				default:
					return 0;
			}
		}

		public static string GetWin32ErrorMessage( uint error )
		{
			StringBuilder buff = new StringBuilder( 1024 );
			uint len = FormatMessage( FORMAT_MESSAGE_FROM_SYSTEM,
				IntPtr.Zero, 
				error,
				0,
				buff,
				1024,
				IntPtr.Zero );
			return buff.ToString( 0, (int)len );
		}        
	}

	#endregion
}
