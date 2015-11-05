using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class StreamExtensions
{
    public static void Close(this Stream stream)
    {
        stream.Dispose();
    }
}

