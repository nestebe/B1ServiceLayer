using B1ServiceLayer.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace B1ServiceLayer
{
    public class B1Service
    {
        private static CookieContainer cookies;
        private static String SessionId;
        private static String SessionVersion;
        private static int SessionTimeout;
        private static string SLServer = null;



        public bool IsConnected { get; set; }
        public bool HasNextPage { get; set; }
        public String NextLink { get; set; }
        public bool ContainValues;
        private String ServerBaseUrl;

        public string ContentResult;
        public bool IsSuccessful;

        public string LastErrorMessage = "";

        /// <summary>
        /// Constructeur Service Layer
        /// </summary>
        public B1Service()
        {
        }

        /// <summary>
        /// Constructeur Service Layer
        /// </summary>
        public B1Service(string server, string companyDB, string username, string password)
        {
            Connect(server, companyDB, username, password);
        }

        /// <summary>
        /// Connexion au service layer
        /// </summary>
        /// <returns></returns>
        public bool Connect(string server, string companyDB, string username, string password)
        {
            SLServer = server + "/b1s/v1/";
            ServerBaseUrl = server;
            var client = new RestClient(SLServer);
            var request = new RestRequest("Login", Method.POST);


            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new { UserName = username, Password = password, CompanyDB = companyDB });

            IRestResponse response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                Console.WriteLine("Error Connecting to Service Layer");
                IsConnected = false;
                return false;
            }

            dynamic finalResult = JsonConvert.DeserializeObject(response.Content);
            if (finalResult != null)
            {
                SessionId = finalResult.SessionId;
                SessionTimeout = finalResult.SessionTimeout;
                SessionVersion = finalResult.Version;
                IsConnected = true;
            }
            else
            {
                IsConnected = false;
                SessionId = "Not Connected to SL";
                return false;
            }

            return true;
        }

        public string GetSessionId()
        {
            return SessionId;
        }

        public string GetSessionVersion()
        {
            return SessionVersion;
        }

        public int GetSessionTimeout()
        {
            return SessionTimeout;
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="endPoint">Sl enpoint</param>
        /// <returns></returns>
        public bool Logout()
        {
            try
            {


                string buildRequest = string.Format("Logout");
                var client = new RestClient(SLServer);
                var request = new RestRequest(buildRequest, Method.POST);
                request.AddCookie("B1SESSION", SessionId);
                request.AddHeader("Content-type", "application/json");

                IRestResponse response = client.Execute(request);
                if (!response.IsSuccessful)
                {
                    //TODO: ajouter logs
                    Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                    return false;
                }

                var responseString = response.Content;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// FIND Method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Odata<T> Find<T>(string endPoint, string query = "")
        {
            string buildRequest = string.Format("{0}{1}", endPoint, query);
            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.GET);
            request.AddCookie("B1SESSION", SessionId);

            request.AddHeader("Content-type", "application/json");

            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return null;
            }
            var responseString = response.Content;
            Odata<T> result = JsonConvert.DeserializeObject<Odata<T>>(response.Content);

            if (result.nextLink == null)
            {
                HasNextPage = false;
            }
            else
            {

                NextLink = System.Web.HttpUtility.UrlDecode(result.nextLink.Replace("/b1s/v1/", ""));
                HasNextPage = true;
            }

            return result;
        }

        public Odata<T> Find<T>(string endPoint, Dictionary<string, string> queryParams)
        {
            string buildRequest = string.Format("{0}", endPoint);
            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.GET);
            request.AddCookie("B1SESSION", SessionId);

            request.AddHeader("Content-type", "application/json");

            foreach (var param in queryParams)
            {
                request.AddParameter(param.Key, param.Value);

            }

            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return null;
            }
            var responseString = response.Content;
            Odata<T> result = JsonConvert.DeserializeObject<Odata<T>>(response.Content);

            if (result.nextLink == null)
            {
                HasNextPage = false;
            }
            else
            {

                NextLink = System.Web.HttpUtility.UrlDecode(result.nextLink.Replace("/b1s/v1/", ""));
                HasNextPage = true;
            }

            return result;
        }

        /// <summary>
        /// GET Method (Get Object)
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="endPoint">Sl enpoint</param>
        /// <returns></returns>
        public T Get<T>(string endPoint)
        {
            string buildRequest = string.Format("{0}", endPoint);
            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.GET);
            request.AddCookie("B1SESSION", SessionId);

            //request.AddHeader("Content-type", "application/json");

            IRestResponse response = client.Execute(request);

            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return default(T);
            }

            var responseString = response.Content;
            T result = JsonConvert.DeserializeObject<T>(response.Content);
            return result;
        }

        /// <summary>
        /// PATCH Method (Update Object)
        /// The difference between PATCH and PUT is that PATCH ignores (keeps the value) those properties that are not
        /// given in the request, while PUT sets them to the default value or to null.
        /// </summary>
        /// <param name="endPoint">Sl enpoint</param>
        /// <param name="objectId">object Id</param>
        /// <param name="updatedObject">updated object</param>
        /// <returns></returns>
        public string Patch(string endPoint, string objectId, object updatedObject)
        {
            string buildRequest = string.Format("{0}('{1}')", endPoint, objectId);

            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.PATCH);
            request.AddCookie("B1SESSION", SessionId);
            request.AddHeader("Content-type", "application/json");

            string formatJson = JsonHelpers.ConvertToJson(updatedObject);
            request.AddJsonBody(formatJson);

            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return "";
            }
            var responseString = response.Content;
            return responseString;
        }

        public string Patch(string endPoint, object updatedObject)
        {
            string buildRequest = endPoint;

            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.PATCH);
            request.AddCookie("B1SESSION", SessionId);
            request.AddHeader("Content-type", "application/json");

            string formatJson = JsonHelpers.ConvertToJson(updatedObject);
            request.AddJsonBody(formatJson);

            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return "";
            }
            var responseString = response.Content;
            return responseString;
        }

        /// <summary>
        /// PUT Method (Update Object)
        /// The difference between PATCH and PUT is that PATCH ignores (keeps the value) those properties that are not
        /// given in the request, while PUT sets them to the default value or to null.
        /// </summary>
        /// <param name="endPoint">Sl enpoint</param>
        /// <param name="objectId">object Id</param>
        /// <param name="updatedObject">updated object</param>
        /// <returns></returns>
        public string Put(string endPoint, string objectId, object updatedObject)
        {
            string buildRequest = string.Format("{0}('{1}')", endPoint, objectId);

            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.PUT);
            request.AddCookie("B1SESSION", SessionId);
            request.AddHeader("Content-type", "application/json");

            string formatJson = JsonHelpers.ConvertToJson(updatedObject);
            request.AddJsonBody(formatJson);

            request.AddJsonBody(updatedObject);


            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return "";
            }
            var responseString = response.Content;
            return responseString;
        }

        public string Put(string endPoint,  object updatedObject)
        {
            string buildRequest = endPoint;

            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.PUT);
            request.AddCookie("B1SESSION", SessionId);
            request.AddHeader("Content-type", "application/json");

            string formatJson = JsonHelpers.ConvertToJson(updatedObject);
            request.AddJsonBody(formatJson);

            request.AddJsonBody(updatedObject);


            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return "";
            }
            var responseString = response.Content;
            return responseString;
        }

        /// <summary>
        /// POST Method (Create object)
        /// </summary>
        /// <param name="endPoint">Sl enpoint</param>
        /// <param name="objectId">object Id</param>
        /// <param name="objectToCreate">Object to create</param>
        /// <returns></returns>
        public T Post<T>(string endPoint, object objectToCreate)
        {
            LastErrorMessage = "";
            string buildRequest = string.Format("{0}", endPoint);

            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.POST);
            request.AddCookie("B1SESSION", SessionId);
            request.AddHeader("Content-type", "application/json");

            string formatJson = JsonHelpers.ConvertToJson(objectToCreate);
            request.AddJsonBody(formatJson);

            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                LastErrorMessage = response.ErrorMessage;
       
                this.ContentResult = response.Content;
                return default(T);
            }

            T result = JsonConvert.DeserializeObject<T>(response.Content);
            return result;
        }

        /// <summary>
        /// DELETE Method (Delete object)
        /// </summary>
        /// <param name="endPoint">Sl enpoint<</param>
        /// <param name="objectId">object Id</param>
        /// <returns></returns>
        public string Delete(string endPoint, string objectId)
        {
            string buildRequest = string.Format("{0}('{1})'", endPoint, objectId);

            var client = new RestClient(SLServer);
            var request = new RestRequest(buildRequest, Method.DELETE);
            request.AddCookie("B1SESSION", SessionId);
            request.AddHeader("Content-type", "application/json");


            IRestResponse response = client.Execute(request);
            this.IsSuccessful = response.IsSuccessful;
            if (!response.IsSuccessful)
            {
                //TODO: ajouter logs
                Console.WriteLine("Service Layer Error {0}", response.ErrorMessage);
                this.ContentResult = response.Content;
                return "";
            }
            var responseString = response.Content;
            return responseString;
        }


    }


}
