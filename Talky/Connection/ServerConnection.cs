﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talky.Client;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Talky.Message;
using Talky.Channel;

namespace Talky.Connection
{
    class ServerConnection
    {

        public ServerClient Client { get; private set; }

        public ServerConnection(ServerClient client)
        {
            Client = client;
            ClientRepository.Instance.Store(client);
        }

        public void HandleMessages()
        {
            StreamReader reader = new StreamReader(Client.TcpClient.GetStream());
            while (true)
            {
                string line = null;
                
                try
                {
                    line = reader.ReadLine().Replace("§", "");
                } catch
                {
                    line = null;
                }

                if (string.IsNullOrEmpty(line))
                {
                    if (Client.TcpClient.Connected)
                    {
                        Client.Disconnect();
                    }
                    return;
                }

                while (line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                while (line.EndsWith(" "))
                {
                    line = line.Substring(0, line.Length - 1);
                }

                ChatMessage chatMessage = new ChatMessage(Client, line);
                CommandMessage commandMessage = new CommandMessage(Client, line);
                StatMessage statMessage = new StatMessage(Client, line);

                Client.LastActivity = (int) (DateTime.UtcNow.Subtract(Program.EPOCH_START)).TotalSeconds;

                if (chatMessage.Valid())
                {
                    chatMessage.Handle();
                } else if (commandMessage.Valid())
                {
                    commandMessage.Handle();
                } else if (statMessage.Valid())
                {
                    statMessage.Handle();
                } else
                {
                    // ???
                    Client.Disconnect("§2What was that?");
                    return;
                }
            }
        }

    }
}
