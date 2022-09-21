using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Battlegrounds.Networking.Communication.Golang;

/// <summary>
/// Static utility class for handling marshalling between the server and client.
/// </summary>
public static class GoMarshal {

    /// <summary>
    /// The fixed size of the message header.
    /// </summary>
    public const int MSG_FIXED_SIZE = 1 + 4 + 16 + 2;

    /// <summary>
    /// Marshal <paramref name="obj"/> into the binary UTF-8 json representation.
    /// </summary>
    /// <param name="obj">The object to marshal.</param>
    /// <returns>The binary json representation of <paramref name="obj"/>.</returns>
    /// <exception cref="NotSupportedException"/>
    public static byte[] JsonMarshal(object obj) {
        try {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        } catch (Exception e) {
            Trace.WriteLine(e, "Networking.json");
            throw;
        }
    }

    /// <summary>
    /// Unmarshal <paramref name="input"/> into its .net representation.
    /// </summary>
    /// <typeparam name="T">The type to unmarshal to.</typeparam>
    /// <param name="input">The binary representation of the marshalled object.</param>
    /// <returns>The unmarshalled object.</returns>
    /// <exception cref="NotSupportedException"/>
    /// <exception cref="JsonException"/>
    public static T? JsonUnmarshal<T>(byte[] input) {
        if (input.Length is 0) {
            return default;
        }
        if (input[0] is 0) {
            File.WriteAllBytes("errpackage.json.dat", input);
            Trace.WriteLine("Halting unmarshal process before attempt to parse opening '0x0' byte.", "Networking.json");
        }
        try {
            return JsonSerializer.Deserialize<T>(input);
        } catch (Exception e) {
            File.WriteAllBytes("errpackage.json.dat", input);
            Trace.WriteLine(e, "Networking.json");
            throw;
        }
    }

    /// <summary>
    /// Unmarshal the binary representation of a <see cref="Message"/> instance.
    /// </summary>
    /// <param name="input">The binary input to unmarshal.</param>
    /// <param name="remaining">The binary data that was not read.</param>
    /// <returns>The first <see cref="Message"/> instance contained within the <paramref name="input"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static Message BytesUnmarshal(byte[] input, out byte[] remaining) {
        if (input.Length >= MSG_FIXED_SIZE) {
            var span = input.AsSpan();
            byte mode = input[0];
            uint cid = BitConverter.ToUInt32(span[1..5]);
            ulong target = BitConverter.ToUInt64(span[5..13]);
            ulong sender = BitConverter.ToUInt64(span[13..21]);
            ushort len = BitConverter.ToUInt16(span[21..23]);
            byte[] content = len == 0 ? Array.Empty<byte>() : input[23..(23 + len)];
            remaining = input[(23 + len)..];
            return new Message() {
                CID = cid,
                Mode = (MessageMode)mode,
                Target = target,
                Sender = sender,
                Content = content
            };
        } else {
            throw new ArgumentOutOfRangeException(nameof(input), input.Length, $"Invalid binary input; expected at least {MSG_FIXED_SIZE} bytes but received {input.Length}.");
        }
    }

}
