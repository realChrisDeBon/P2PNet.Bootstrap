#if DEBUG
global using static ConsoleDebugger.ConsoleDebugger;
#endif
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using P2PNet.Peers;
using P2PNet.NetworkPackets;
using System.Net;

namespace P2PNet.Bootstrap
    {

    /// <summary>
    /// A local bootstrap server instance that will broadcast peer connection info to new peer members, while adding them to the roster.
    /// </summary>
    public class BoostrapServer
        {
        /// <summary>
        /// Gets an all encompasing list of known peers as a serialized <see cref="CollectionSharePacket"/>
        /// </summary>
        /// <returns>JSON serialized string of a <see cref="CollectionSharePacket"/> with all known peers.</returns>
        private static string SerializedPeerList()
            {
            CollectionSharePacket newcollshare = new CollectionSharePacket();
            newcollshare.peers = PeerNetwork.KnownPeers;
            return Serialize<CollectionSharePacket>(newcollshare);
            } 

        public static async Task Startserver(string[] args)
            {
            var builder = WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });


            var app = builder.Build();
            

            app.MapPost("/attemptconnect", ([FromBody] IdentifierPacket request) =>
            {
                try
                    {
#if DEBUG
                    DebugMessage($"Packet received: {request.Message} from {request.ip}" + Environment.NewLine + $"\t\t\tSecret Port: {request.Data}");
#endif
                    IPeer newPeer = new GenericPeer(IPAddress.Parse(request.ip), request.Data);
                    PeerNetwork.AddPeer(newPeer);
                    return Results.Ok(SerializedPeerList());
                    }
                catch (Exception ex)
                    {
#if DEBUG
                    DebugMessage($"Packet received: {request.Message} from {request.ip}" + Environment.NewLine + $"\t\t\tSecret Port: {request.Data}");
#endif
                    return Results.BadRequest(new ErrorResponse(ex.Message));
                    }
            });

            // Start the Web API
            var apiTask = Task.Run(async () => app.Run());
            }
        }

    public record ErrorResponse (string Error);

    [JsonSerializable(typeof(ErrorResponse))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
        {

        }
    }
