
//uses blitzbit for networking
//https://github.com/harroo/blitzbit

using UnityEngine;

using System.Threading;
using System.Collections.Generic;

using Drifture;
using BlitzBit;

public static class Program {

    [RuntimeInitializeOnLoadMethod]
    public static void Main () {

        Debug.Log("start");

        //declare servers
        UBlitServer userver = new UBlitServer();
        BlitServer server = new BlitServer();

        //on recv player pos update from client
        server.AddPacket(0, (int sender, byte[] rb)=>{

            Submanager.UpdatePlayerPos(rb);
        });

        List<int> clients = new List<int>();

        //on client join
        server.AddPacket(1, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32();
            clients.Add(id);

            foreach (var client in clients) {

                BlitPacket joinPacket = new BlitPacket();
                joinPacket.Append(client);

                server.RelayTo(1, sender, joinPacket.ToArray());
            }

            server.RelayExclude(1, rb, sender);

            Debug.Log("client joned: " + id.ToString());
        });

        //on client leave
        server.AddPacket(2, (int sender, byte[] rb)=>{BlitPacket packet = new BlitPacket(rb);

            int id = packet.GetInt32();
            clients.Remove(id);

            server.RelayExclude(2, rb, sender);

            Debug.Log("client leaves: " + id.ToString());
        });

        server.onUnknownPacket = (int sender, int packetId, byte[] data) => {

            server.RelayExclude(packetId, data, sender);
        };

        //start servers
        server.Start(12369);
        userver.Start(12368);

        //declare loop of submanaging thing
        Thread thread = new Thread(()=>{

            if (Submanager.SendCount() != 0) {

                byte[] sendData = Submanager.PopSendQueue();

                server.RelayAll(0, sendData);
            }

            Submanager.RunChecks();

            Thread.Sleep(1024);
        });
        thread.Start();


        //coinfigure shutdown thing
        Application.quitting += () => {

            userver.Stop();
            server.Stop();
            thread.Abort();

            Debug.Log("stopped");
        };
    }
}
