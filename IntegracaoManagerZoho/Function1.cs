using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace IntegracaoManagerZoho
{
   
    public class Produto
    {
        public long id { get; set; }
        public string Product_Name { get; set; }
        public int QtdLicencas { get; set; }
    }

    public class ClienteZoho
    {
        public long id { get; set; }
        public string C_digo_do_Cliente { get; set; }
        public string Rescisao { get; set; }
        public List<Produto> Produtos { get; set; }
        public string Produtos_Atendidos { get; set; }
        public int CRC { get; set; }
    }


    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", Route = null)] HttpRequest req,
            ILogger log)
        {
            string module = "Accounts";
            Zoho zoho = new Zoho();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var clienteZohos = JsonConvert.DeserializeObject<List<ClienteZoho>>(requestBody);

            //Recuperando os produtos distintos de todos os clientes
            var produtosDistintos = clienteZohos.SelectMany(c => c.Produtos.Select(p => new { p.Product_Name, p.id })).Distinct().ToList();

            //foreach (var produto in produtosDistintos)
            //{
            //    dynamic obj = JsonConvert.DeserializeObject(zoho.Get("Products", null, $"(Product_Name:starts_with:{produto.Product_Name})"));

            //    /*APOS IMPLEMENTAR O MÓDULO DEALS
            //     * dynamic obj = JsonConvert.DeserializeObject(zoho.Get("Products", null, $"(Product_Name:equals:{produto.Product_Name})"));*/

            //    //Atribuir id do produto no cliente produtos
            //    //clienteZohos.FindAll(c => c.Produtos.Find(p => p.Product_Name == produto));
            //    //.id = obj?.data[0]?.id);
            //}

            //Atualizando as características do cliente 
            foreach (var cliente in clienteZohos)
            {
                //Tratamento do código de cliente
                cliente.C_digo_do_Cliente = Convert.ToInt32(cliente.C_digo_do_Cliente).ToString("00000");
                
                //Serializa a resposta (transforma o JSON em Objeto)
                dynamic obj = JsonConvert.DeserializeObject(zoho.Get(module, null, $"(C_digo_do_Cliente:equals:{cliente.C_digo_do_Cliente})"));

                //Recupera a ID do Zoho
                cliente.id = obj?.data[0]?.id;
                var prods = cliente.Produtos.Select(p => p.Product_Name).ToArray();
                cliente.Produtos_Atendidos = string.Join(", ", prods);

                ////Inserir produto
                //foreach (var produto in cliente.Produtos)
                //{
                //    //Passar o id cliente como numero 
                //}

                log.LogInformation($"Atualizando o cliente {cliente.id}");
            }

            object reqObj = new { data = clienteZohos };
            string strReqObj = JsonConvert.SerializeObject(reqObj);

            log.LogInformation("Atualizando os clintes em massa");
            string resp = zoho.Update(module, strReqObj);

            //Resposta da webRequest
            log.LogInformation($"Respota zoho Update {resp}");

            return new OkObjectResult("");
        }
    }
}
