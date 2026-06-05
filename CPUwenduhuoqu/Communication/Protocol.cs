using System;
using System.Globalization;
using System.Text;

namespace CPUwenduhuoqu.Communication
{
    public enum FrameType
    {
        TempCpu,
        TempGpu,
        StatusQuery,
        StatusResponse,
        FcurveSet,
        FcurveQuery,
        FcurveResponse,
        Ack,
        Nack,
        SafetyReset,  // P0-6: remote safety override reset
        Unknown
    }

    public struct StatusData
    {
        public int Mode;
        public int DutyPercent;
        public int FreqHz;
        public float CpuTemp;
        public float GpuTemp;
        public bool CpuValid;
        public bool GpuValid;
    }

    public static class Protocol
    {
        private const string FrameStart = "$";
        private const string ChecksumSep = "*";
        private const string FrameEnd = "\n";

        public static byte ComputeChecksum(string data)
        {
            byte cs = 0;
            foreach (char c in data)
                cs ^= (byte)c;
            return cs;
        }

        private static string FinalizeFrame(string data)
        {
            byte cs = ComputeChecksum(data);
            return FrameStart + data + ChecksumSep + cs.ToString("X2") + FrameEnd;
        }

        // ---- 帧构建 ----

        public static string BuildTempFrame(bool isCpu, float temp)
        {
            string data = (isCpu ? "CPU" : "GPU") + "," + temp.ToString("F1", CultureInfo.InvariantCulture);
            return FinalizeFrame(data);
        }

        public static string BuildStatusQuery()
        {
            return FinalizeFrame("STA,?");
        }

        public static string BuildFcurveQuery()
        {
            return FinalizeFrame("FCQ,?");
        }

        public static string BuildFcurveSet(FanCurvePoint[] points)
        {
            if (points == null || points.Length < 2 || points.Length > 10)
                throw new ArgumentException("风扇曲线必须包含 2-10 个点");

            var sb = new StringBuilder("FCV,");
            sb.Append(points.Length);
            foreach (var p in points)
            {
                sb.Append(',');
                sb.Append(p.Temperature.ToString("F1", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(p.DutyPercent);
            }
            return FinalizeFrame(sb.ToString());
        }

        public static string BuildModeSet(int mode)
        {
            if (mode < 1 || mode > 4)
                throw new ArgumentException("Mode must be 1-4");
            return FinalizeFrame($"MOD,{mode}");
        }

        public static string BuildFreqSet(int freqHz)
        {
            if (freqHz < 1000 || freqHz > 40000)
                throw new ArgumentException("Frequency must be 1000-40000 Hz");
            return FinalizeFrame($"FRQ,{freqHz}");
        }

        public static string BuildDutySet(int dutyPercent)
        {
            if (dutyPercent < 20 || dutyPercent > 100)  // P0-4: enforce minimum safe duty
                throw new ArgumentException("Duty must be 20-100%");
            return FinalizeFrame($"DUT,{dutyPercent}");
        }

        public static string BuildSafetyReset()  // P0-6: remote safety override reset
        {
            return FinalizeFrame("SAF");
        }

        // ---- 帧解析 ----

        public static FrameType IdentifyFrame(string frame)
        {
            if (string.IsNullOrEmpty(frame) || !frame.StartsWith(FrameStart))
                return FrameType.Unknown;

            int starIdx = frame.IndexOf(ChecksumSep);
            if (starIdx < 0 || starIdx + 3 > frame.Length)
                return FrameType.Unknown;

            // 去除尾部 \r\n
            string cleanFrame = frame.TrimEnd('\r', '\n');

            string dataPart = cleanFrame.Substring(1, starIdx - 1);
            string csStr = cleanFrame.Substring(starIdx + 1, 2);
            byte expectedCs = ComputeChecksum(dataPart);
            byte actualCs;
            if (!byte.TryParse(csStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out actualCs))
                return FrameType.Unknown;
            if (expectedCs != actualCs)
                return FrameType.Unknown;

            if (dataPart.StartsWith("CPU,")) return FrameType.TempCpu;
            if (dataPart.StartsWith("GPU,")) return FrameType.TempGpu;
            if (dataPart.StartsWith("STA,")) return FrameType.StatusQuery;
            if (dataPart.StartsWith("STP,")) return FrameType.StatusResponse;
            if (dataPart.StartsWith("FCV,")) return FrameType.FcurveSet;
            if (dataPart.StartsWith("FCQ,")) return FrameType.FcurveQuery;
            if (dataPart.StartsWith("FCP,")) return FrameType.FcurveResponse;
            if (dataPart == "ACK") return FrameType.Ack;
            if (dataPart.StartsWith("NAK,")) return FrameType.Nack;
            if (dataPart == "SAF") return FrameType.SafetyReset;

            return FrameType.Unknown;
        }

        public static bool TryParseStatusResponse(string frame, out StatusData status)
        {
            status = new StatusData();

            if (IdentifyFrame(frame) != FrameType.StatusResponse)
                return false;

            int starIdx = frame.IndexOf(ChecksumSep);
            string dataPart = frame.Substring(1, starIdx - 1);
            // STP,M,D,F,CT,GT,CV,GV
            string[] parts = dataPart.Split(',');
            if (parts.Length != 8) return false;

            try
            {
                status.Mode = int.Parse(parts[1], CultureInfo.InvariantCulture);
                status.DutyPercent = int.Parse(parts[2], CultureInfo.InvariantCulture);
                status.FreqHz = int.Parse(parts[3], CultureInfo.InvariantCulture);
                status.CpuTemp = float.Parse(parts[4], CultureInfo.InvariantCulture);
                status.GpuTemp = float.Parse(parts[5], CultureInfo.InvariantCulture);
                status.CpuValid = parts[6] == "1";
                status.GpuValid = parts[7] == "1";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryParseFcurveResponse(string frame, out FanCurvePoint[] points)
        {
            points = null;

            if (IdentifyFrame(frame) != FrameType.FcurveResponse)
                return false;

            int starIdx = frame.IndexOf(ChecksumSep);
            string dataPart = frame.Substring(1, starIdx - 1);
            // FCP,N,t1,d1,...,tN,dN
            string[] parts = dataPart.Split(',');
            if (parts.Length < 3) return false;

            int count;
            if (!int.TryParse(parts[1], out count) || count < 2 || count > 10)
                return false;
            if (parts.Length != 2 + count * 2) return false;

            points = new FanCurvePoint[count];
            for (int i = 0; i < count; i++)
            {
                float temp;
                byte duty;
                if (!float.TryParse(parts[2 + i * 2], NumberStyles.Float, CultureInfo.InvariantCulture, out temp))
                    return false;
                if (!byte.TryParse(parts[3 + i * 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out duty))
                    return false;
                points[i] = new FanCurvePoint
                {
                    Temperature = temp,
                    DutyPercent = duty
                };
            }
            return true;
        }

        public static bool TryParseAck(string frame)
        {
            return IdentifyFrame(frame) == FrameType.Ack;
        }

        public static bool TryParseNack(string frame, out int errorCode)
        {
            errorCode = 0;
            if (IdentifyFrame(frame) != FrameType.Nack)
                return false;

            int starIdx = frame.IndexOf(ChecksumSep);
            string dataPart = frame.Substring(1, starIdx - 1);
            // NAK,CC
            string[] parts = dataPart.Split(',');
            if (parts.Length != 2) return false;
            return int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out errorCode);
        }
    }
}
