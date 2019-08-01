using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IntegracaoManagerZoho
{
    public enum Method
    {
        Post,
        Put,
        Get,
        Delete
    }

    public class Zoho
    {
        //Refresh token
        private string RefreshToken { get; set; } = Environment.GetEnvironmentVariable("ZohoRefreshToken");

        //Token de acesso
        private string AccessToken { get; set; }

        //Expiração
        private DateTime Expires { get; set; }

        //Executa uma webRequest 
        public string Request(Method method, string url, string data = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //Verifica se ha algum acess token a ser passado 
                    if (AccessToken != null)
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Zoho-oauthtoken", AccessToken);

                    //Envia a requisição
                    HttpResponseMessage resp = null;

                    switch (method)
                    {
                        case Method.Post:
                            if (data != null)
                                resp = client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                            else
                                resp = client.PostAsync(url, null).GetAwaiter().GetResult();
                            break;
                        case Method.Put:
                            resp = client.PutAsync(url, new StringContent(data, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                            break;
                        case Method.Get:
                            resp = client.GetAsync(url).GetAwaiter().GetResult();
                            break;
                        case Method.Delete:
                            resp = client.DeleteAsync(url).GetAwaiter().GetResult();
                            break;
                    }

                    return resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //Atualiza o token de acesso se necessário

        public void GerarAcessToken()
        {
            if (AccessToken == null || Expires < DateTime.UtcNow)
            {
                string url = $"https://accounts.zoho.com/oauth/v2/token?refresh_token={RefreshToken}&grant_type=refresh_token&client_id=1000.9BZ1A9P4QQ198265013M6TT97YYOOH&client_secret=f850cc5441573cbdf7eda606c32e6b94c2fc9803f5";

                string resp = Request(Method.Post, url);

                dynamic obj = JsonConvert.DeserializeObject(resp);

                AccessToken = obj?.access_token;
            }
        }

        //Inicializa as funções do Zoho
        public Zoho()
        {
            GerarAcessToken();
        }

        //Insere dados no módulo
        public string Create(string module, string data)
        {
            //Validar se expirou
            GerarAcessToken();

            return Request(Method.Post, $"https://www.zohoapis.com/crm/v2/{module}", data);

        }

        //Atualiza os dados no módulo
        public string Update(string module, string data)
        {
            //Validar se expirou
            GerarAcessToken();

            return Request(Method.Put, $"https://www.zohoapis.com/crm/v2/{module}", data);
        }

        //Pesquisa dados no módulo
        public string Get(string module, string data, string criteria = null)
        {
            //Validar se expirou
            GerarAcessToken();

            string url = $"https://www.zohoapis.com/crm/v2/{module}/";

            if (criteria != null)
                url += $"search?criteria={criteria}";

            return Request(Method.Get,url);
        }

    }
}
