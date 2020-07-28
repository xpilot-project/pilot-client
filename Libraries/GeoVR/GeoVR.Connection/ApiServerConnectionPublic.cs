using GeoVR.Shared;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GeoVR.Connection
{
    public class ApiServerConnectionPublic
    {
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly string address;

        public ApiServerConnectionPublic(string address)
        {
            this.address = address;
        }

        public async Task<List<CallsignDataDto>> GetOnlineCallsigns()
        {
            return await Get<List<CallsignDataDto>>("api/v1/network/online/callsigns");
        }

        public async Task<Dictionary<string, List<TxTransceiverRxCallsignsDto>>> GetOnlineCallsignsReceivingTransceivers()
        {
            return await Get<Dictionary<string, List<TxTransceiverRxCallsignsDto>>>("api/v1/network/online/callsignsReceivingTransceivers");
        }

        private async Task<TResponse> Get<TResponse>(string resource)
        {
            var watch = Stopwatch.StartNew();
            var client = new RestClient(address);
            var request = new RestRequest(resource, Method.GET);
            IRestResponse response = await client.ExecuteTaskAsync(request);
            watch.Stop();
            logger.Debug(resource + " (" + watch.ElapsedMilliseconds + "ms)");
            if (!response.IsSuccessful)
                throw new Exception(resource + " failed (" + response.StatusCode + " - " + response.Content + ")");

            var responseDto = JsonConvert.DeserializeObject<TResponse>(response.Content);
            return responseDto;
        }
    }
}
