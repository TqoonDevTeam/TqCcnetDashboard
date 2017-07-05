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

        public bool IsHTTPS { get { return Port == 443; } }
        public bool HasSSL { get { return !string.IsNullOrEmpty(SSL); } }

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
                string ssl = SSL.Replace(" ", string.Empty);
                return Enumerable.Range(0, ssl.Length)
                    .Where(t => t % 2 == 0)
                    .Select(t => Convert.ToByte(ssl.Substring(t, 2), 16))
                    .ToArray();
            }
            else
            {
                return new byte[] { };
            }
        }

        public string GetCertificateStoreName()
        {
            if (HasSSL)
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