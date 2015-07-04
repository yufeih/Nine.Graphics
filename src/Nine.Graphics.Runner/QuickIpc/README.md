# QuickIPC

## What is it?
This is a super-fast (low latency) IPC (Interprocess Comminication) library for .NET platform. There are many ways to do IPC in Windows/.NET system, WCF and 
COM-Interop being the most prominent. However, all of these IPC can be of high latency, especially when you are dealing with soft-real-time or low-latency system. 
This one deals with MemoryMappedFile (MMF) and Event. Event (synchronization Windows events) is used for cotrol signal and MemoryMappedFile (MMF) is used for 
fast-sharing of arbitrary amount of data.

Same IpcService can be run as Client or Server

Usage
-------------
* Create a IPC end-point "sender" as follows -

ipcS = new IpcService(Context.Server);

* Create another IPC end-point "receiver" as follows (some other app)

ipcR = new IpcService(Context.Client);
ipcR.Init();

* Register an event handler

ipcR.IpcEvent += new IpcEventHandler(Listener_IpcEvent);
void Listener_IpcEvent(object sender, TextualEventArgs eventArgs) 
{
}

* Send from the "sender" as follows -

ipcS.Poke(message)



