using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TqLib.ccnet.Core.Util
{
    public class TqBinding
    {
        public string Ip { get; set; } = "*";

        public int Port { get; set; } = 80;

        public string Host { get; set; }

        public string SSL { get; set; } = string.Empty;

        public bool IsHTTPS { get { return !string.IsNullOrEmpty(SSL); } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Ip);
            sb.Append(":");
            sb.Append(Port);
            sb.Append(":");
            sb.Append(Host);
            if (!string.IsNullOrEmpty(SSL))
            {
                sb.Append(":");
                sb.Append(SSL);
            }
            return sb.ToString();
        }

        public string GetBindingInfomation()
        {
            return $"{Ip}:{Port}:{Host}";
        }

        public string GetBindingProtocol()
        {
            if (IsHTTPS)
            {
                return "https";
            }
            else
            {
                return "http";
            }
        }

        public byte[] GetCertificateHash()
        {
            if (IsHTTPS)
            {
                var ssl = SSL.Replace(" ", string.Empty);
                IList<byte> bytes = new List<byte>();
                for (var i = 1; i < ssl.Length; i += 2)
                {
                    bytes.Add(Convert.ToByte(Convert.ToInt32(ssl.Substring(i, 2), 16)));
                }

                return bytes.ToArray();
            }
            else
            {
                return new byte[] { };
            }
        }

        public string GetCertificateStoreName()
        {
            if (IsHTTPS)
            {
                return "My";
            }
            else
            {
                return "My";
            }
        }
    }
}