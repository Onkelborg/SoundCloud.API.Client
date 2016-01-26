using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoundCloud.API.Client.Internal.Client;
using SoundCloud.API.Client.Internal.Converters;
using SoundCloud.API.Client.Internal.Infrastructure.Objects;
using SoundCloud.API.Client.Internal.Objects;
using SoundCloud.API.Client.Objects;

namespace SoundCloud.API.Client.Subresources
{
    public class PlaylistApi : IPlaylistApi
    {
        private readonly ISoundCloudRawClient soundCloudRawClient;
        private readonly IPlaylistConverter playlistConverter;
        private readonly string prefix;
        private readonly string playlistId;

        internal PlaylistApi(string playlistId, ISoundCloudRawClient soundCloudRawClient, IPlaylistConverter playlistConverter)
        {
            this.playlistId = playlistId;
            this.soundCloudRawClient = soundCloudRawClient;
            this.playlistConverter = playlistConverter;
            prefix = string.Format("playlists/{0}", playlistId);
        }

        public SCPlaylist GetPlaylist()
        {
            var playlist = soundCloudRawClient.Request<Playlist>(prefix, string.Empty, HttpMethod.Get);
            return playlistConverter.Convert(playlist);
        }

        public void UpdatePlaylist(SCPlaylist playlist)
        {
            if(playlist.Id != playlistId)
            {
                throw new SoundCloudApiException(string.Format("Context set for playlistId = {0}. Create new context for update another playlist.", playlistId));
            }

            var currentPlaylist = GetInternalPlaylist();

            var diff = currentPlaylist.GetDiff(playlistConverter.Convert(playlist));

            var diffo = new Dictionary<string, object>();
            diffo.Add("playlist", diff);
            soundCloudRawClient.Request(prefix, string.Empty, HttpMethod.Put, diffo);
        }

        private Playlist GetInternalPlaylist()
        {
            return soundCloudRawClient.Request<Playlist>(prefix, string.Empty, HttpMethod.Get);
        }
    }
}