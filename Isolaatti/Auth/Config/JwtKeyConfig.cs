using System;
using System.Text;

namespace Isolaatti.Auth.Config;

public class JwtKeyConfig
{
    public string JwtSigningKey { get; set; }

    public byte[] ToByteArray()
    {
        return Encoding.UTF8.GetBytes(JwtSigningKey);
    }
}