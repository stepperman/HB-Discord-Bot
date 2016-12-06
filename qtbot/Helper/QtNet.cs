using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace QtNetHelper
{
    class QtNet
    {
        public Dictionary<string, string> Query = new Dictionary<string, string>();
        public string BaseUrl { get; set; }

        public HttpClient _client = new HttpClient();
        private string completeUrl = "";

        public QtNet(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public void AddQuery(string key, string value)
        {
            Query[key] = value;
        }

        public async Task<string> GetStringAsync()
        {
            string url = GetUrl();
            return await _client.GetStringAsync(GetUrl());
        }


        public async Task<byte[]> GetByteArrayAsync()
        {
            return await _client.GetByteArrayAsync(GetUrl());
        }


        public async Task<HttpResponseMessage> PostAsync()
        {
            var content = new FormUrlEncodedContent(Query);
            var response = await _client.PostAsync(BaseUrl, content);
            return response;
        }

        public string GetUrl()
        {

            if (Query.Count == 0)
                return BaseUrl;

            string querys = "?";

            foreach (var query in Query)
            {
                querys += Uri.EscapeUriString(query.Key) + "=" + Uri.EscapeUriString(query.Value) + "&";
            }

            querys = querys.Remove(querys.Length - 1);

            return BaseUrl + querys;
        }
    }
}