using System;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;

namespace QuickIPC
{
    internal enum Context
    {
        /// <summary>
        /// Server context
        /// </summary>
        Server = 0,
        
        /// <summary>
        /// Client context
        /// </summary>
        Client
    }

    internal class IpcService
    {
        private const int BufferSizeInBytes = 8 * 1024 * 1024;
        public const int MaxMessageSizeInBytes = BufferSizeInBytes - 4;

        private Thread m_ListenerThread;
        private readonly string m_MappedMemoryName;
        private readonly string m_NamedEventName;
        private readonly string m_NamedEventBuddy;
        private readonly Lazy<byte[]> readBuffer = new Lazy<byte[]>(() => new byte[BufferSizeInBytes]);
        public event Action<byte[]> IpcEvent;
        private EventWaitHandle m_NamedEventHandle;
        private EventWaitHandle m_NamedEventBuddyHandle;
        private Context m_Context;
        private EventWaitHandle m_Terminated;
        private object m_SyncRoot;

        public IpcService(Context context)
            : this(context,
                    @"Global\QuickIPC-501E9DE7-3C8E-4AB9-85F6-14157B701164",
                    @"Global\QuickIPCEvent-2121A37B-EE5E-4DD3-88A6-4167E6FDB4E8",
                    @"Global\QuickIPCEvent-Buddy-0504DD04-A73A-4031-9F6B-7F5DFB1151F5")
        {}

        private IpcService(Context context, string pMappedMemoryName, string pNamedEventName, string pNamedEventBuddy)
        {
            m_Context = context;
            m_MappedMemoryName = pMappedMemoryName;
            m_NamedEventName = pNamedEventName;
            m_NamedEventBuddy = pNamedEventBuddy;
            m_SyncRoot = new object();

            if (m_Context == Context.Server)
            {
                m_NamedEventHandle = CreateEvent(m_NamedEventName);
                m_NamedEventBuddyHandle = CreateEvent(m_NamedEventBuddy);
            }
            else
            {
                m_NamedEventHandle = GetEvent(m_NamedEventName);
                m_NamedEventBuddyHandle = GetEvent(m_NamedEventBuddy);
            }
        }

        public void Init()
        {
            m_ListenerThread = new Thread(new ThreadStart(listenUsingNamedEventsAndMemoryMappedFiles));
            m_ListenerThread.Name = "IPCListener";
            m_ListenerThread.IsBackground = true;
            m_ListenerThread.Start();
        }
        
        public void UnInit()
        {
            if (m_Terminated != null)
            {
                m_Terminated.Set();
                m_ListenerThread.Join(5000);
            }
        }
        
        /// <summary>
        /// Creates an global event of specified name having empty DACL (again not NULL DACL)
        /// </summary>
        /// <param name="mNamedEventName"></param>
        /// <returns></returns>
        private EventWaitHandle CreateEvent(string mNamedEventName)
        {
           EventWaitHandle handle = null;
            
            try
            {
                //Create a security object that grants no access.
                EventWaitHandleSecurity mSec = new EventWaitHandleSecurity();                
                mSec.AddAccessRule(new EventWaitHandleAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                                                                 EventWaitHandleRights.FullControl,
                                                                 AccessControlType.Allow));

                bool createdNew;
                handle = new EventWaitHandle(false,
                                             EventResetMode.ManualReset,
                                             mNamedEventName,
                                             out createdNew,
                                             mSec);
            }
            catch(WaitHandleCannotBeOpenedException whcboe)
            {
                EventLog.WriteEntry("IpcService",
                                    string.Format("IpcService::CreateEvent: WaitHandle cannot be opened: {0}", whcboe),
                                    EventLogEntryType.Error);
            }
            catch(UnauthorizedAccessException uae)
            {
                EventLog.WriteEntry("IpcService",
                                    string.Format("IpcService::CreateEvent: Unauthorized Access: {0}", uae),
                                    EventLogEntryType.Error);
            }
            catch(Exception ex)
            {
                EventLog.WriteEntry("IpcService",
                                    string.Format("IpcService::CreateEvent: Error while creating event {0}", ex),
                                    EventLogEntryType.Error);
            }

            return handle;
        }

        private EventWaitHandle GetEvent(string mNamedEventName)
        {
            EventWaitHandle ewh = null;
            try
            {
#if !DEBUG
                ewh = EventWaitHandle.OpenExisting(mNamedEventName,
                                                   EventWaitHandleRights.FullControl);
#endif
            }
            catch (UnauthorizedAccessException ex)
            {
                EventLog.WriteEntry("IpcService",
                                    string.Format("IpcService::GetEvent: Querying event {0}", mNamedEventName), 
                                    EventLogEntryType.Error);
            }

            return ewh;
        }

        private void listenUsingNamedEventsAndMemoryMappedFiles()
        {
            if (m_NamedEventHandle == null)
            {
                EventLog.WriteEntry("Application",
                                       string.Format(
                                           "IpcService::listenUsingNamedEventsAndMemoryMappedFiles: NULL event"),
                                       EventLogEntryType.Error);
                return;
            }
            
            if (m_NamedEventBuddyHandle == null)
            {
                EventLog.WriteEntry("IpcService",
                                    string.Format("IpcService::listenUsingNamedEventsAndMemoryMappedFiles: NULL event (Buddy)"),
                                    EventLogEntryType.Error);
                return;
            }

            m_Terminated = new EventWaitHandle(false, EventResetMode.ManualReset);
            EventWaitHandle[] waitObjects = 
                new EventWaitHandle[] { m_Terminated, m_NamedEventHandle };
            
            try
            {
                while(true)
                {
                    //Waits on "Ready to read?"
                    int index = EventWaitHandle.WaitAny(waitObjects, 
                                                        Timeout.Infinite,false);
                    if (index == 0)
                    {
                        break;
                    }
                    
                    try
                    {
                        //Read data
                        IpcEvent?.Invoke(Peek());
                    }
                    catch(Exception ex)
                    {
                        EventLog.WriteEntry("IpcService",
                                    string.Format("IpcService::listenUsingNamedEventsAndMemoryMappedFiles: Error: {0}", ex),
                                    EventLogEntryType.Error);
                    }
                    finally
                    {
                        m_NamedEventHandle.Reset();

                        //Signals "Read done"
                        m_NamedEventBuddyHandle.Set();
                    }
                }
            }
            finally
            {
                if (m_NamedEventHandle != null)
                {
                    m_NamedEventHandle.Set();
                    m_NamedEventHandle.Close();
                }
                
                if (m_NamedEventBuddyHandle != null)
                {
                    m_NamedEventBuddyHandle.Set();
                    m_NamedEventBuddyHandle.Close();
                }

                m_Terminated.Close();
            }
        }

        public void Poke(string format, params object[] args)
        {
            Poke(string.Format(format, args));
        }

        public void Poke(byte[] somedata)
        {
            if (somedata == null) throw new ArgumentNullException(nameof(somedata));
            if (somedata.Length > MaxMessageSizeInBytes) throw new ArgumentOutOfRangeException(nameof(somedata));

            lock(m_SyncRoot)
            {
                using (MemoryMappedFileStream fs = new MemoryMappedFileStream(m_MappedMemoryName,
                                                                          BufferSizeInBytes,
                                                                          MemoryProtection.PageReadWrite))
                {
                    fs.MapViewToProcessMemory(0, somedata.Length);
                    fs.Write(BitConverter.GetBytes(somedata.Length), 0, 4);
                    fs.Write(somedata, 0, somedata.Length);
                }

                //
                //Signal the "Please Read" event and waits on "Read Done"
                EventWaitHandle.SignalAndWait(m_NamedEventHandle, 
                                              m_NamedEventBuddyHandle, 
                                              20000, 
                                              false);
                m_NamedEventBuddyHandle.Reset();
            }
        }

        public byte[] Peek()
        {
            var buffer = readBuffer.Value;

            using (MemoryMappedFileStream fs = new MemoryMappedFileStream(m_MappedMemoryName, 
                                                                          BufferSizeInBytes, 
                                                                          MemoryProtection.PageReadWrite))
            {
                fs.MapViewToProcessMemory(0, BufferSizeInBytes);
                if (fs.Read(buffer, 0, 4) != 4) return null;

                var length = BitConverter.ToInt32(buffer, 0);
                if (length <= 0) return null;

                var result = new byte[length];
                fs.Read(result, 0, length);
                return result;
            }
        }

        private bool mDisposed = false;

        public void Dispose()
        {
            if (!mDisposed)
            {
                try
                {
                    if (m_NamedEventHandle != null)
                    {
                        m_NamedEventHandle.Close();
                        m_NamedEventHandle = null;
                    }

                    if (m_ListenerThread != null)
                    {
                        m_ListenerThread.Abort();
                        m_ListenerThread = null;
                    }

                    mDisposed = true;
                    GC.SuppressFinalize(this);
                }
                catch
                {
                }
            }
        }

        ~IpcService()
        {
            Dispose();
        }

    }

}
