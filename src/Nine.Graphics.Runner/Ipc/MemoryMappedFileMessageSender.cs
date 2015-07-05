using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MemoryMessagePipe
{
    class MemoryMappedFileMessageSender : IDisposable
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

        public MemoryMappedFileMessageSender(string name)
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

        public void SendMessage(byte[] bytes)
        {
            SendMessage(stream => stream.Write(bytes, 0, bytes.Length));
        }

        public void SendMessage(Action<Stream> action)
        {
            SendMessage(action, new CancellationToken());
        }

        public void SendMessage(Action<Stream> action, CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (cancellationToken.IsCancellationRequested)
            {
                LastMessageWasCancelled = true;

                return;
            }

            LastMessageWasCancelled = false;
            _messageCancelledEvent.Reset();
            _messageSendingEvent.Set();

            using (var stream = new MemoryMappedOutputStream(this, cancellationToken))
            {
                try
                {
                    action(stream);
                }
                catch
                {
                    LastMessageWasCancelled = true;
                    _messageCancelledEvent.Set();

                    throw;
                }
            }

            var index = WaitHandle.WaitAny(new[] {_messageReadEvent, _messageCancelledEvent, cancellationToken.WaitHandle});

            if (index == 1 || index == 2)
            {
                LastMessageWasCancelled = true;
            }
        }

        public bool LastMessageWasCancelled { get; private set; }

        public void CancelMessage()
        {
            _messageCancelledEvent.Set();
        }

        private class MemoryMappedOutputStream : Stream
        {
            private readonly MemoryMappedFileMessageSender _sender;
            private readonly MemoryMappedViewAccessor _bytesWrittenAccessor;
            private readonly MemoryMappedViewAccessor _messageCompletedAccessor;
            private readonly MemoryMappedViewStream _stream;
            private readonly EventWaitHandle _messageCancelledEvent;
            private readonly EventWaitHandle _bytesWrittenEvent;
            private readonly EventWaitHandle _bytesReadEvent;
            private readonly CancellationToken _cancellationToken;

            public MemoryMappedOutputStream(MemoryMappedFileMessageSender sender, CancellationToken cancellationToken)
            {
                _sender = sender;
                _bytesWrittenAccessor = sender._bytesWrittenAccessor;
                _messageCompletedAccessor = sender._messageCompletedAccessor;
                _stream = sender._stream;
                _messageCancelledEvent = sender._messageCancelledEvent;
                _bytesWrittenEvent = sender._bytesWrittenEvent;
                _bytesReadEvent = sender._bytesReadEvent;
                _cancellationToken = cancellationToken;
            }

            private int _bytesWritten;

            public override void Close()
            {
                _messageCompletedAccessor.Write(0, true);

                if (_bytesWritten > 0)
                {
                    _bytesWrittenAccessor.Write(0, _bytesWritten);
                }

                _bytesWrittenEvent.Set();
                var index = WaitHandle.WaitAny(new[] {_bytesReadEvent, _messageCancelledEvent, _cancellationToken.WaitHandle});

                if (index == 1 || index == 2)
                {
                    _sender.LastMessageWasCancelled = true;
                }

                _bytesWrittenAccessor.Write(0, 0);
                _messageCompletedAccessor.Write(0, false);
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
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException("buffer", "Buffer cannot be null.");
                if (offset < 0)
                    throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
                if (buffer.Length - offset < count)
                    throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

                var bytesRemainingInStream = SizeOfStream - _bytesWritten;

                if (bytesRemainingInStream < count)
                {
                    var bytesRemainingToWrite = count;
                    var bytesWritten = 0;

                    while (bytesRemainingInStream < bytesRemainingToWrite)
                    {
                        _stream.Write(buffer, offset + bytesWritten, bytesRemainingInStream);
                        _bytesWrittenAccessor.Write(0, SizeOfStream);

                        _bytesWrittenEvent.Set();
                        var index = WaitHandle.WaitAny(new[] {_bytesReadEvent, _messageCancelledEvent, _cancellationToken.WaitHandle});

                        if (index == 1 || index == 2)
                        {
                            _sender.LastMessageWasCancelled = true;

                            return;
                        }

                        _stream.Seek(0, SeekOrigin.Begin);
                        bytesRemainingToWrite -= bytesRemainingInStream;
                        bytesWritten += bytesRemainingInStream;
                        bytesRemainingInStream = SizeOfStream;
                        _bytesWritten = 0;
                    }

                    if (bytesRemainingToWrite > 0)
                    {
                        _stream.Write(buffer, offset + bytesWritten, bytesRemainingToWrite);
                        _bytesWritten = bytesRemainingToWrite;
                    }
                }
                else
                {
                    _stream.Write(buffer, offset, count);
                    _bytesWritten += count;
                }
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
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