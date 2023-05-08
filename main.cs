using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;

namespace SpotifyPlaylistRandomizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientId = "your_client_id";
            var clientSecret = "your_client_secret";
            var redirectUri = "http://localhost:8888/callback";

            var auth = new AuthorizationCodeAuth(
                clientId,
                clientSecret,
                redirectUri,
                new[] { Scopes.PlaylistModifyPublic }
            );

            auth.AuthReceived += async (sender, payload) =>
            {
                auth.Stop();
                var token = await auth.ExchangeCode(payload.Code);
                var api = new SpotifyWebAPI()
                {
                    AccessToken = token.AccessToken,
                    TokenType = token.TokenType
                };

                var playlists = await api.GetUserPlaylists(api.GetPrivateProfile().Id);
                var randomPlaylist = playlists.Items[new Random().Next(playlists.Items.Count)];
                var playlist = await api.GetPlaylistTracks(randomPlaylist.Id);

                var random = new Random();
                var randomizedTracks = playlist.Tracks.Items.OrderBy(x => random.Next());

                await api.ReorderPlaylistTracks(
                    playlist.Id,
                    new ReorderPlaylistTracksRequest()
                    {
                        RangeStart = 0,
                        RangeLength = playlist.Tracks.Total,
                        InsertBefore = 0,
                        SnapshotId = playlist.SnapshotId,
                        TrackUris = randomizedTracks.Select(x => x.Track.Uri).ToList()
                    }
                );
            };

            auth.Start();
            auth.OpenBrowser();
            Console.ReadLine();
        }
    }
}
