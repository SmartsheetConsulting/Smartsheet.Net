using Newtonsoft.Json;
using Smartsheet.Core.Entities;
using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using ProfessionalServices.Core.Responses;
using System.Threading.Tasks;
using System.Threading;
using Smartsheet.Core.Interfaces;
using Smartsheet.Core.Responses;
using System.Collections;

namespace Smartsheet.Core.Http
{
    public class SmartsheetHttpClient : ISmartsheetClient
    {
        private HttpClient _HttpClient = new HttpClient();
        private string _AccessToken = null;
        private string _ChangeAgent = null;
        private static int _AttemptLimit = 10;
        private int _WaitTime = 0;
        private int _RetryCount = 0;
        private bool _RetryRequest = true;

        public SmartsheetHttpClient(string token, string changeAgent = null)
        {
            this._AccessToken = token;
            this._ChangeAgent = changeAgent;
            this._HttpClient.BaseAddress = new Uri("https://api.smartsheet.com/2.0/");
            this._HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._AccessToken);
        }

        //
        //  Request Logic
        #region SmartsheetHttpClient Request Logic
        public async Task<TResult> ExecuteRequest<TResult, T>(HttpVerb verb, string url, T data)
        {
            this.ValidateRequestInjectedResult(typeof(TResult));

            //this.ValidateRequestInjectedType(typeof(T));

            this.ValidateClientParameters();

            this.InitiazeNewRequest();

            while (_RetryRequest && (_RetryCount < _AttemptLimit))
            {
                try
                {
                    if (_WaitTime > 0)
                    {
                        Thread.Sleep(_WaitTime);
                    }

                    HttpResponseMessage response;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var serializedData = JsonConvert.SerializeObject(data, Formatting.None, serializerSettings);

                    switch (verb)
                    {
                        default:
                        case HttpVerb.GET:
                            response = await this._HttpClient.GetAsync(url);
                            break;
                        case HttpVerb.PUT:
                            response = await this._HttpClient.PutAsync(url, new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json"));
                            break;
                        case HttpVerb.POST:
                            response = await this._HttpClient.PostAsync(url, new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json"));
                            break;
                        case HttpVerb.DELETE:
                            response = await this._HttpClient.DeleteAsync(url);
                            break;
                    }

                    var statusCode = response.StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();

                            var jsonReponseBody = JsonConvert.DeserializeObject(responseBody).ToString();

                            var resultResponse = JsonConvert.DeserializeObject<TResult>(jsonReponseBody);

                            return resultResponse;
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    }

                    if (statusCode.Equals(HttpStatusCode.InternalServerError) || statusCode.Equals(HttpStatusCode.ServiceUnavailable) || statusCode.Equals((HttpStatusCode)429)) // .NET doesn't have a const for this
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        dynamic result = JsonConvert.DeserializeObject(responseJson);

                        // did we hit an error that we should retry?
                        int code = result["errorCode"];

                        if (code == 4001)
                        {
                            // service may be down temporarily
                            _WaitTime = Backoff(_WaitTime, 60 * 1000);
                        }
                        else if (code == 4002 || code == 4004)
                        {
                            // internal error or simultaneous update.
                            _WaitTime = Backoff(_WaitTime, 1 * 1000);
                        }
                        else if (code == 4003)
                        {
                            // rate limit
                            _WaitTime = Backoff(_WaitTime, 2 * 1000);
                        }
                    }
                    else
                    {
                        _RetryRequest = false;
                        dynamic result;
                        try
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();

                            result = JsonConvert.DeserializeObject(responseJson);
                        }
                        catch (Exception)
                        {
                            throw new Exception(string.Format("HTTP Error {0}: url:[{1}]", statusCode, url));
                        }

                        var message = string.Format("Smartsheet error code {0}: {1} url:[{2}]", result["errorCode"], result["message"], url);

                        throw new Exception(message);
                    }
                }
                catch (Exception e)
                {
                    if (!_RetryRequest)
                    {
                        throw e;
                    }
                }

                _RetryCount += 1;
            }

            throw new Exception(string.Format("Retries exceeded.  url:[{0}]", url));
        }

        private static int Backoff(int current, int min_wait)
        {
            if (current > 0)
            {
                return current * 2;
            }
            return min_wait;
        }

        private void ValidateRequestInjectedResult(Type type)
        {
            if (!type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISmartsheetResult)))
            {
                throw new Exception("Injected type must implement interface ISmartsheetResult");
            }
        }

        private void ValidateRequestInjectedType(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.GetGenericArguments()[0] != typeof(ISmartsheetObject))
                {
                    throw new Exception("Injected type must implement interface ISmartsheetObject");
                }
            }
            else
            {
                if (!type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISmartsheetObject)))
                {
                    throw new Exception("Injected type must implement interface ISmartsheetObject");
                }
            }
        }

        private void ValidateClientParameters()
        {
            if (this._AccessToken == null || string.IsNullOrWhiteSpace(this._AccessToken))
            {
                throw new ArgumentException("Access Token must be provided");
            }
        }

        private void InitiazeNewRequest()
        {
            this._WaitTime = 0;
            this._RetryCount = 0;
            this._RetryRequest = true;
        }
        #endregion

        //
        //  Users
        #region Users

        public class Users
        {

        }

        #endregion

        //
        //  Workspaces
        #region Workspaces

        public async Task<ISmartsheetObject> CreateWorkspace(string workspaceName)
        {
            if (string.IsNullOrWhiteSpace(workspaceName))
            {
                throw new Exception("Workspace Name cannot be null or blank");
            }

            var workspace = new Workspace(workspaceName);

            var response = await this.ExecuteRequest<ResultResponse<Workspace>, Workspace>(HttpVerb.POST, string.Format("workspaces"), workspace);

            return response.Result;
        }

        public async Task<ISmartsheetObject> GetWorkspaceById(long? workspaceId)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null);

            return response;
        }

        #endregion

        //
        //  Sheets
        #region Sheets
        public async Task<Sheet> CreateSheet(string sheetName, IEnumerable<Column> columns, string folderId = null, string workspaceId = null)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new Exception("Sheet Name cannot be null or blank");
            }

            var sheet = new Sheet(sheetName, columns.ToList());

            var response = await this.ExecuteRequest<ResultResponse<Sheet> ,Sheet>(HttpVerb.POST, string.Format("sheets"), sheet);

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<Sheet> GetSheetById(long? sheetId)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            var response = await this.ExecuteRequest<Sheet, Sheet>(HttpVerb.GET, string.Format("sheets/{0}", sheetId), null);

            response._Client = this;

            return response;
        }

        public async Task<IEnumerable<Sheet>> GetSheetsForWorkspace(long? workspaceId)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null);

            response.Sheets.FirstOrDefault()._Client = this;

            return response.Sheets;
        }
        #endregion

        //
        //  Rows
        #region Rows
        public async Task<IEnumerable<Row>> CreateRows(long? sheetId, IEnumerable<Row> rows, bool? toTop = null, bool? toBottom = null, long? parentId = null, long? siblingId = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rows.Count() > 1)
            {
                foreach(var row in rows)
                {
                    row.ToTop = toTop;
                    row.ToBottom = toBottom;
                    row.ParentId = parentId;
                    row.SiblingId = siblingId;

                    foreach(var cell in row.Cells)
                    {
                        cell.Build();
                    }
                }
            }

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.POST, string.Format("sheets/{0}/rows", sheetId), rows);

            return response.Result;
        }

        public async Task<MultiRowEmail> SendRows(long? sheetId, IEnumerable<long> rowIds, IEnumerable<Recipient> sendTo, IEnumerable<long> columnIds, string subject = null, string message = null, bool ccMe = false, bool includeDiscussions = true, bool includeAttachments = true)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rowIds.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more rows to update");
            }

            if (sendTo.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more recipients");
            }

            var multiRowEmail = new MultiRowEmail();
            multiRowEmail.RowIds = rowIds.ToList();
            multiRowEmail.ColumnIds = columnIds.ToList();
            multiRowEmail.CcMe = ccMe;
            multiRowEmail.IncludeAttachments = includeAttachments;
            multiRowEmail.IncludeDiscussions = includeDiscussions;
            multiRowEmail.Subject = subject;
            multiRowEmail.Message = message;
            multiRowEmail.SendTo = sendTo.ToList();

            var result = await this.ExecuteRequest<ResultResponse<MultiRowEmail>, MultiRowEmail>(HttpVerb.POST, string.Format("sheets/{0}/rows/emails", sheetId), multiRowEmail);

            return result.Result;
        }
        #endregion

        //
        //  Folders
        #region Folders

        public async Task<IEnumerable<ISmartsheetObject>> GetFoldersForWorkspace(long? workspaceId)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null);

            return response.Folders;
        }

        #endregion

        //
        //  Reports
        #region Reports
        public async Task<IEnumerable<ISmartsheetObject>> GetReportsForWorkspace(long? workspaceId)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null);

            return response.Reports;
        }
        #endregion

        //
        //  Templates
        #region Templates
        public async Task<IEnumerable<ISmartsheetObject>> GetTemplatesForWorkspace(long? workspaceId)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null);

            return response.Templates;
        }
        #endregion

        //
        //  Update Requests
        #region Update Requests
        public async Task<UpdateRequest> CreateUpdateRequest(long? sheetId, IEnumerable<long> rowIds, IEnumerable<Recipient> sendTo, IEnumerable<long> columnIds, string subject = null, string message = null, bool ccMe = false, bool includeDiscussions = true, bool includeAttachments = true)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rowIds.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more rows to update");
            }

            if (sendTo.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more recipients");
            }

            var request = new UpdateRequest()
            {
                SendTo = sendTo.ToList(),
                Subject = subject,
                Message = message,
                CcMe = ccMe,
                RowIds = rowIds.ToList(),
                ColumnIds = columnIds.ToList(),
                IncludeAttachments = includeAttachments,
                IncludeDiscussions = includeDiscussions
            };

            var result = await this.ExecuteRequest<ResultResponse<UpdateRequest>, UpdateRequest>(HttpVerb.POST, string.Format("sheets/{0}/updaterequests", sheetId), request);

            return result.Result;
        }
        #endregion
    }
}
