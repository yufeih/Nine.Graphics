using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MemoryMessagePipe
{
    class MemoryMappedFileMessageReceiver : IDisposable
    {
        private static readonly int SizeOfFile = Environment.SystemPageSize;
        private const int SizeOfInt32 = sizeof(int);
        private const int SizeOfBool = sizeof(bool);
        private static readonly int SizeOfStream = SizeOfFile - SizeOfInt32 - SizeOfBool - SizeOfBool;

        private readonly MemoryMappedFile _file;
        private readonly MemoryMappedViewAccessor _bytesWrittenAccessor;
        private readonly MemoryMappedViewAccessor _messageCompletedAccessor;
        private readonly MemoryMappedViewStream _stream;
        private readonly EventWaitHandle _messageSendingEvent;
        private readonly EventWaitHandle _messageReadEvent;
        private readonly EventWaitHandle _messageCancelledEvent;
        private readonly EventWaitHandle _bytesWrittenEvent;
        private readonly EventWaitHandle _bytesReadEvent;

        private bool _isDisposed;
        
        public MemoryMappedFileMessageReceiver(string name)
        {
            _file = MemoryMappedFile.CreateOrOpen(name, SizeOfFile);
            _bytesWrittenAccessor = _file.CreateViewAccessor(0, SizeOfInt32);
            _messageCompletedAccessor = _file.CreateViewAccessor(SizeOfInt32, SizeOfBool);
            _stream = _file.CreateViewStream(SizeOfInt32 + SizeOfBool + SizeOfBool, SizeOfStream);
            _messageSendingEvent = new EventWaitHandle(false, EventResetMode.AutoReset, name + "_MessageSending");
            _messageReadEvent = new EventWaitHandle(false, EventResetMode.AutoReset, name + "_MessageRead");
            _messageCancelledEvent = new EventWaitHandle(false, EventResetMode.ManualReset, name + "_MessageCancelled");
            _bytesWrittenEvent = new EventWaitHandle(false, EventResetMode.AutoReset, name + "_BytesWritten");
            _bytesReadEvent = new EventWaitHandle(false, EventResetMode.AutoReset, name + "_BytesRead");
        }

        public void Dispose()
        {
            _isDisposed = true;
            _messageCancelledEvent.Set();

            _bytesWrittenAccessor.Dispose();
            _messageCompletedAccessor.Dispose();
            _stream.Dispose();
            _messageSendingEvent.Dispose();
            _messageReadEvent.Dispose();
            _messageCancelledEvent.Dispose();
            _bytesWrittenEvent.Dispose();
            _bytesReadEvent.Dispose();
        }

        public void ReceiveMessage(Action<byte[], int> messageReceived)
        {
            new Thread(() =>
            {
                var buffer = new byte[SizeOfFile];
                while (true)
                {
                    ReceiveMessage(stream =>
                    {
                        var bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            messageReceived?.Invoke(buffer, bytesRead);
                        }
                        return 0;
                    });
                }
            }).Start();
        }

        public T ReceiveMessage<T>(Func<Stream, T> action)
        {
            return ReceiveMessage(action, new CancellationToken());
        }

        public T ReceiveMessage<T>(Func<Stream, T> action, CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LastMessageWasCancelled = false;
            _messageCancelledEvent.Reset();
            var index = WaitHandle.WaitAny(new[] {_messageSendingEvent, _messageCancelledEvent, cancellationToken.WaitHandle});

            if (index == 1 || index == 2)
            {
                LastMessageWasCancelled = true;

                return default(T);
            }

            T result;

            using (var stream = new MemoryMappedInputStream(this, cancellationToken))
            {
                try
                {
                    result = action(stream);
                }
                catch
                {
                    LastMessageWasCancelled = true;
                    _messageCancelledEvent.Set();

                    throw;
                }
            }

            _messageReadEvent.Set();

            return LastMessageWasCancelled ? default(T) : result;
        }

        public bool LastMessageWasCancelled { get; private set; }

        public void CancelMessage()
        {
            _messageCancelledEvent.Set();
        }

        private class MemoryMappedInputStream : Stream
        {
            private readonly MemoryMappedFileMessageReceiver _receiver;
            private readonly MemoryMappedViewAccessor _bytesWrittenAccessor;
            private readonly MemoryMappedViewAccessor _messageCompletedAccessor;
            private readonly MemoryMappedViewStream _stream;
            private readonly EventWaitHandle _messageCancelledEvent;
            private readonly EventWaitHandle _bytesWrittenEvent;
            private readonly EventWaitHandle _bytesReadEvent;
            private readonly CancellationToken _cancellationToken;

            public MemoryMappedInputStream(MemoryMappedFileMessageReceiver receiver, CancellationToken cancellationToken)
            {
                _receiver = receiver;
                _bytesWrittenAccessor = receiver._bytesWrittenAccessor;
                _messageCompletedAccessor = receiver._messageCompletedAccessor;
                _stream = receiver._stream;
                _messageCancelledEvent = receiver._messageCancelledEvent;
                _bytesWrittenEvent = receiver._bytesWrittenEvent;
                _bytesReadEvent = receiver._bytesReadEvent;
                _cancellationToken = cancellationToken;
            }

            private int _bytesRemainingToBeRead;
            private int _offset;
            private bool _shouldWait = true;
            private bool _messageCompleted;

            public override void Close()
            {
                _stream.Seek(0, SeekOrigin.Begin);

                base.Close();
            }

            public override void Flush() { }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException("buffer", "Buffer cannot be null.");
                if (offset < 0)
                    throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
                if (buffer.Length - offset < count)
                    throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

                if (_messageCompleted && _bytesRemainingToBeRead == 0)
                    return 0;

                if (_shouldWait)
                {
                    var index = WaitHandle.WaitAny(new[] {_bytesWrittenEvent, _messageCancelledEvent, _cancellationToken.WaitHandle});

                    if (index == 1 || index == 2)
                    {
                        _receiver.LastMessageWasCancelled = true;

                        return 0;
                    }

                    _stream.Seek(0, SeekOrigin.Begin);
                    _bytesRemainingToBeRead = _bytesWrittenAccessor.ReadInt32(0);
                    _offset = 0;
                    _shouldWait = false;
                    _messageCompleted = _messageCompletedAccessor.ReadBoolean(0);
                }

                var numberOfBytesToRead = count >= _bytesRemainingToBeRead ? _bytesRemainingToBeRead : count;

                if (_offset == 0)
                {
                    _stream.Read(buffer, offset, numberOfBytesToRead);
                }
                else
                {
                    var readBuffer = new byte[_offset + numberOfBytesToRead];
                    _stream.Read(readBuffer, _offset, numberOfBytesToRead);
                    Buffer.BlockCopy(readBuffer, _offset, buffer, offset, numberOfBytesToRead);
                }

                _offset += numberOfBytesToRead;
                _bytesRemainingToBeRead -= numberOfBytesToRead;

                if (_bytesRemainingToBeRead == 0)
                {
                    _shouldWait = true;
                    _bytesReadEvent.Set();
                }

                return numberOfBytesToRead;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }
        }
    }
}