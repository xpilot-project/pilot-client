using GeoVR.Shared;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Connection
{
    public class ApiServerConnection
    {
        private const int timeout = 500000;
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly string address;
        private string jwt;
        private string username;
        private string password;
        private Guid networkVersion;
        private string client;
        private DateTime expiryLocalUtc;
        private TimeSpan serverToUserOffset;

        public bool Authenticated { get; private set; }

        public ApiServerConnection(string address)
        {
            this.address = address;

            //Define TLS1.2 rather than rely on the default protocols enabled on the client (Win 7 example)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.UseNagleAlgorithm = false;

            logger.Debug(nameof(ApiServerConnection) + " instantiated");
        }

        public async Task Connect(string username, string password, Guid networkVersion, string client)     //Client is something like "vPilot 2.2.3"
        {
            this.username = username;
            this.password = password;
            this.networkVersion = networkVersion;
            this.client = client;
            var watch = Stopwatch.StartNew();
            var restClient = new RestClient(address) { Timeout = timeout };
            var request = new RestRequest("api/v1/auth", Method.POST, DataFormat.Json);
            request.AddJsonBody(new PostUserRequestDto() { Username = username, Password = password, NetworkVersion = networkVersion, Client = client });
            IRestResponse response = await restClient.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("POST api/v1/auth (" + watch.ElapsedMilliseconds + "ms)");
            if (response.ErrorException != null)
                throw response.ErrorException;
            if (!response.IsSuccessful)
                throw new Exception(nameof(Connect) + " failed (" + response.StatusCode + " - " + response.Content + ")");

            jwt = response.Content.TrimStart('"').TrimEnd('"');
            var jwtToken = new JwtSecurityToken(jwt);
            serverToUserOffset = jwtToken.ValidFrom - DateTime.UtcNow;      //A positive value means the server UTC time is ahead of the local UTC time
            expiryLocalUtc = jwtToken.ValidTo.Subtract(serverToUserOffset);
            Authenticated = true;
        }

        public async Task<PostCallsignResponseDto> AddCallsign(string callsign)
        {
            return await PostNoRequest<PostCallsignResponseDto>("api/v1/users/" + username + "/callsigns/" + callsign);
        }

        public async Task RemoveCallsign(string callsign)
        {
            await Delete("api/v1/users/" + username + "/callsigns/" + callsign);
        }

        public async Task UpdateTransceivers(string callsign, List<TransceiverDto> transceivers)
        {
            await PostNoResponse("api/v1/users/" + username + "/callsigns/" + callsign + "/transceivers", transceivers);
        }

        public async Task UpdateCrossCoupleGroups(string callsign, List<CrossCoupleGroupDto> crossCoupleGroups)
        {
            await PostNoResponse("api/v1/users/" + username + "/callsigns/" + callsign + "/crossCoupleGroups", crossCoupleGroups);
        }

        public async Task UpdateVoiceRooms(string callsign, List<VoiceRoomDto> voiceRooms)
        {
            await PostNoResponse("api/v1/users/" + username + "/callsigns/" + callsign + "/voiceRooms", voiceRooms);
        }

        public async Task UpdateDirectConfiguration(string callsign, DirectConfigurationDto directConfiguration)
        {
            await PostNoResponse("api/v1/users/" + username + "/callsigns/" + callsign + "/directConfiguration", directConfiguration);
        }

        public void ForceDisconnect()
        {
            Authenticated = false;
            jwt = "";
            logger.Debug("ForceDisconnect");
        }

        public async Task AddOrUpdateBot(string callsign, PutBotRequestDto addBotRequestDto)
        {
            await PutNoResponse("api/v1/bots/" + callsign, addBotRequestDto);
        }

        public async Task RemoveBot(string callsign)
        {
            await Delete("api/v1/bots/" + callsign);
        }

        public async Task<StationDto> GetStation(Guid stationID)
        {
            return await Get<StationDto>("api/v1/stations/" + stationID);
        }

        public async Task<StationDto> GetStation(string stationName)
        {
            return await Get<StationDto>("api/v1/stations/byName/" + stationName);
        }

        public async Task<StationTreeDto> GetStationTree(Guid stationID)
        {
            return await Get<StationTreeDto>("api/v1/stations/" + stationID + "/tree");
        }

        public async Task<StationTreeDto> GetStationTree(string stationName)
        {
            return await Get<StationTreeDto>("api/v1/stations/byName/" + stationName + "/tree");
        }

        public async Task<List<StationTransceiverDto>> GetStationTransceiversAllDistinctObeyExclusions(string stationName)
        {
            return await Get<List<StationTransceiverDto>>("api/v1/stations/byName/" + stationName + "/transceivers/allDistinctObeyExclusions");
        }

        public async Task<List<StationTransceiverDto>> GetStationTransceiversAllDistinctObeyExclusions(Guid stationID)
        {
            return await Get<List<StationTransceiverDto>>("api/v1/stations/" + stationID.ToString() + "/transceivers/allDistinctObeyExclusions");
        }

        public async Task<PostStationSearchResponseDto> SearchStations(string search, int take, int skip)
        {
            return await Post<PostStationSearchRequestDto, PostStationSearchResponseDto>("api/v1/stations/search", new PostStationSearchRequestDto() { SearchText = search, Take = take, Skip = skip });

        }

        public async Task<StationTreeDto> PostStation(PostStationRequestDto dto)
        {
            return await Post<PostStationRequestDto, StationTreeDto>("api/v1/stations", dto);
        }

        public async Task<StationTreeDto> PutStation(PutStationRequestDto dto)
        {
            return await Put<PutStationRequestDto, StationTreeDto>("api/v1/stations", dto);
        }

        public async Task DeleteStation(Guid stationID)
        {
            await Delete("api/v1/stations/" + stationID.ToString());
        }

        public async Task<StationTransceiverDto> GetStationTransceiver(string name)
        {
            return await Get<StationTransceiverDto>("api/v1/stations/transceivers/byName/" + name);
        }

        public async Task<StationTransceiverDto> GetStationTransceiver(Guid transceiverID)
        {
            return await Get<StationTransceiverDto>("api/v1/stations/transceivers/" + transceiverID);
        }

        public async Task<PostTransceiverSearchResponseDto> SearchStationTransceivers(string search, int take, int skip)
        {
            return await Post<PostStationTransceiverSearchRequestDto, PostTransceiverSearchResponseDto>("api/v1/stations/transceivers/search", new PostStationTransceiverSearchRequestDto() { SearchText = search, Take = take, Skip = skip });
        }

        public async Task<StationTransceiverDto> PostStationTransceiver(PostStationTransceiverRequestDto dto)
        {
            return await Post<PostStationTransceiverRequestDto, StationTransceiverDto>("api/v1/stations/transceivers", dto);
        }

        public async Task<StationTransceiverDto> PutStationTransceiver(PutStationTransceiverRequestDto dto)
        {
            return await Put<PutStationTransceiverRequestDto, StationTransceiverDto>("api/v1/stations/transceivers", dto);
        }

        public async Task DeleteStationTransceiver(Guid stationTransceiverID)
        {
            await Delete("api/v1/stations/transceivers/" + stationTransceiverID.ToString());
        }

        public async Task<IEnumerable<StationTransceiverDto>> GetStationTransceiverExclusions(Guid stationID)
        {
            return await Get<IEnumerable<StationTransceiverDto>>("api/v1/stations/" + stationID + "/transceivers/exclusions");
        }

        public async Task PutStationTransceiverExclusions(Guid stationID, List<Guid> transceiverIDs)
        {
            await PutNoResponse("api/v1/stations/" + stationID.ToString() + "/transceivers/exclusions", new PutStationTransceiverExclusionsRequestDto() { TransceiverIDs = transceiverIDs });
        }

        public async Task BanUsername(string username)
        {
            await GetNoResponse("api/v1/users/" + username + "/ban");
        }

        public async Task<IEnumerable<StationDto>> GetVccsStations(Guid stationID)
        {
            return await Get<IEnumerable<StationDto>>("api/v1/stations/" + stationID.ToString() + "/vccsStations");
        }

        public async Task<IEnumerable<StationDto>> GetVccsStations(string stationName)
        {
            return await Get<IEnumerable<StationDto>>("api/v1/stations/byName/" + stationName + "/vccsStations");
        }

        public async Task PutVccsStations(Guid stationID, List<Guid> stationIDs)
        {
            await PutNoResponse("api/v1/stations/" + stationID.ToString() + "/vccsStations", new PutVccsStationsRequestDto() { StationIDs = stationIDs });
        }

        public async Task<IEnumerable<StationDto>> GetAllAliasedStations()
        {
            return await Get<IEnumerable<StationDto>>("api/v1/stations/aliased");
        }

        public async Task AddFsdStation(string name)
        {
            await PostNoRequestNoResponse("api/v1/fsd/stations/" + name);
        }

        public async Task UpdateFsdStation(string name, uint frequency, double latDeg, double lonDeg)
        {
            await PostNoResponse("api/v1/fsd/stations/" + name + "/update", new PostFsdStationUpdateRequestDto() { Frequency = frequency, LatDeg = latDeg, LonDeg = lonDeg });
        }

        public async Task DeleteFsdStation(string name)
        {
            await Delete("api/v1/fsd/stations/" + name);
        }

        public async Task<IEnumerable<string>> GetStationsInRange(string callsign)
        {
            return await Get<IEnumerable<string>>("api/v1/users/" + username + "/callsigns/" + callsign + "/stationsInRange");
        }

        private async Task CheckExpiry()
        {
            if (DateTime.UtcNow > expiryLocalUtc.AddMinutes(-5))
            {
                await Connect(username, password, networkVersion, client);
            }
        }

        private async Task<TResponse> Get<TResponse>(string resource)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.GET);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("GET " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("GET " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");

            var responseDto = JsonConvert.DeserializeObject<TResponse>(response.Content);
            return responseDto;
        }

        private async Task GetNoResponse(string resource)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.GET);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("GET " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("GET " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");
        }

        private async Task<TResponse> Post<TRequest, TResponse>(string resource, TRequest requestDto)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.POST, DataFormat.Json);
            var json = JsonConvert.SerializeObject(requestDto);
            request.AddParameter("application/json", json, null, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("POST " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("POST " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");

            var responseDto = JsonConvert.DeserializeObject<TResponse>(response.Content);
            return responseDto;
        }

        private async Task<TResponse> Put<TRequest, TResponse>(string resource, TRequest dto)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.PUT, DataFormat.Json);
            var json = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", json, null, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("PUT " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("PUT " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");

            var responseDto = JsonConvert.DeserializeObject<TResponse>(response.Content);
            return responseDto;
        }

        private async Task Delete(string resource)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.DELETE);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("DELETE " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("DELETE " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");
        }

        private async Task PostNoResponse<TRequest>(string resource, TRequest dto)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.POST, DataFormat.Json);
            var json = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", json, null, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("POST " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("POST " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");
        }

        private async Task<TResponse> PostNoRequest<TResponse>(string resource)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.POST);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("POST " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("POST " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");

            var responseDto = JsonConvert.DeserializeObject<TResponse>(response.Content);
            return responseDto;
        }

        private async Task PutNoResponse<TRequest>(string resource, TRequest dto)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.PUT, DataFormat.Json);
            var json = JsonConvert.SerializeObject(dto);
            request.AddParameter("application/json", json, null, ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("PUT " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("PUT " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");
        }

        private async Task PutNoRequestNoResponse(string resource)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.PUT);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("PUT " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("PUT " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");
        }

        private async Task PostNoRequestNoResponse(string resource)
        {
            if (!Authenticated)
                throw new Exception("Not authenticated");

            await CheckExpiry();

            var watch = Stopwatch.StartNew();
            var client = new RestClient(address) { Timeout = timeout };
            client.AddDefaultParameter("Authorization", string.Format("Bearer " + jwt), ParameterType.HttpHeader);
            var request = new RestRequest(resource, Method.POST);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug("POST " + resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception("POST " + resource + " failed (" + response.StatusCode + " - " + response.Content + ")");
        }
    }
}
