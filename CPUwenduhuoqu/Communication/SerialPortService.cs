using System;
using System.IO.Ports;
using System.Text;

namespace CPUwenduhuoqu.Communication
{
    public class SerialPortService : IDisposable
    {
        private readonly object _lock = new object();
        private SerialPort _serialPort;
        private readonly StringBuilder _receiveBuffer = new StringBuilder();

        public string PortName { get; private set; }
        public bool IsOpen
        {
            get
            {
                lock (_lock)
                    return _serialPort != null && _serialPort.IsOpen;
            }
        }
        public int BaudRate { get; private set; } = 115200;

        public event EventHandler<string> DataReceived;
        public event EventHandler<bool> ConnectionChanged;

        public SerialPortService(string portName, int baudRate = 115200)
        {
            PortName = portName;
            BaudRate = baudRate;
        }

        public bool Open(string portName = null)
        {
            if (portName != null)
                PortName = portName;

            // 先在锁外关闭旧连接，避免与 DataReceived 竞争死锁
            SerialPort oldPort;
            lock (_lock)
            {
                oldPort = _serialPort;
                _serialPort = null;
            }
            ClosePortInternal(oldPort);

            lock (_lock)
            {
                try
                {
                    _serialPort = new SerialPort(PortName, BaudRate)
                    {
                        WriteTimeout = 5000,
                        ReadTimeout = 5000,
                        NewLine = "\n"
                    };
                    _serialPort.DataReceived += OnDataReceived;
                    _serialPort.Open();
                }
                catch (Exception)
                {
                    _serialPort?.Dispose();
                    _serialPort = null;
                    ConnectionChanged?.Invoke(this, false);
                    return false;
                }
            }

            ConnectionChanged?.Invoke(this, true);
            return true;
        }

        public void Close()
        {
            SerialPort oldPort;
            lock (_lock)
            {
                oldPort = _serialPort;
                _serialPort = null;
            }
            ClosePortInternal(oldPort);
            ConnectionChanged?.Invoke(this, false);
        }

        private void ClosePortInternal(SerialPort sp)
        {
            if (sp == null) return;
            try
            {
                sp.DataReceived -= OnDataReceived;
                if (sp.IsOpen)
                    sp.Close();
            }
            catch { }
            try { sp.Dispose(); } catch { }
        }

        public void Send(string frame)
        {
            SerialPort sp;
            lock (_lock)
            {
                sp = _serialPort;
            }

            if (sp != null && sp.IsOpen)
            {
                try
                {
                    sp.Write(frame);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SerialPortService send error: {ex.Message}");
                }
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp;
            lock (_lock)
            {
                sp = _serialPort;
            }

            if (sp == null || !sp.IsOpen) return;

            try
            {
                string data = sp.ReadExisting();

                string bufferStr;
                lock (_receiveBuffer)
                {
                    _receiveBuffer.Append(data);
                    bufferStr = _receiveBuffer.ToString();
                    _receiveBuffer.Clear();
                }

                int lastNewline = -1;
                for (int i = 0; i < bufferStr.Length; i++)
                {
                    if (bufferStr[i] == '\n')
                    {
                        string frame = bufferStr.Substring(lastNewline + 1, i - lastNewline - 1);
                        if (!string.IsNullOrWhiteSpace(frame))
                        {
                            DataReceived?.Invoke(this, frame.TrimEnd('\r'));
                        }
                        lastNewline = i;
                    }
                }

                if (lastNewline < bufferStr.Length - 1)
                {
                    lock (_receiveBuffer)
                    {
                        _receiveBuffer.Append(bufferStr.Substring(lastNewline + 1));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SerialPortService receive error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
