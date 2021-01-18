/*
Copyright(c) 2020-2021 Przemysław Łukawski

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DotnetWebApiBench.Helpers
{
    public class HostUrlGenerator
    {
        private const int NUMBER_OF_TRIALS = 100;
        private static readonly ConcurrentBag<int> UsedPorts = new ConcurrentBag<int>();
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        public static string GetLocalHttpUrl()
        {
            int port = GetFreeRandomPort();
            return $"http://localhost:{port}";
        }

        public static string GetLocalHttpsUrl()
        {
            int port = GetFreeRandomPort();
            return $"https://localhost:{port}";
        }

        public static int GetFreeRandomPort(int beginPort = 5200, int endPort = 10000)
        {
            for (var i = 0; i < NUMBER_OF_TRIALS; i++)
            {
                var randomPort = random.Next(beginPort, endPort);

                if (!PortInUse(randomPort))
                {
                    try
                    {
                        return UsePort(randomPort);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            throw new Exception("Cannot find available port to bind to.");
        }

        /// <summary>
        /// Tries to use the port - some ports may be free but reserved in the OS. We must ensure the port is usable.
        /// </summary>
        /// <param name="randomPort"></param>
        /// <returns></returns>
        private static int UsePort(int randomPort)
        {
            UsedPorts.Add(randomPort);

            var ipe = new IPEndPoint(IPAddress.Loopback, randomPort);

            using (var socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(ipe);
                socket.Close();
                return randomPort;
            }
        }

        private static bool PortInUse(int randomPort)
        {
            return UsedPorts.Any(p => p == randomPort);
        }
    }
}
