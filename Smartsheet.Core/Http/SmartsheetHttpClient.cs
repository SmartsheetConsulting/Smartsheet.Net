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
        private static string _BaseAddress = "https://api.smartsheet.com/2.0/";
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
            this._HttpClient.BaseAddress = new Uri(_BaseAddress);
            this._HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._AccessToken);
        }

        //
        //  Request Logic
        #region SmartsheetHttpClient Request Logic
        public async Task<TResult> ExecuteRequest<TResult, T>(HttpVerb verb, string url, T data, IList<Tuple<string, string>> headers = null, bool deserializeAsJson = true)
        {
            //this.ValidateRequestInjectedResult(typeof(TResult));

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

                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(_BaseAddress + url)
                    };

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers.Add(header.Item1, header.Item2);
                        }
                    }

                    switch (verb)
                    {
                        default:
                        case HttpVerb.GET:
                            request.Method = HttpMethod.Get;
                            break;
                        case HttpVerb.PUT:
                            request.Method = HttpMethod.Put;
                            request.Content = new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json");
                            break;
                        case HttpVerb.POST:
                            request.Method = HttpMethod.Post;
                            request.Content = new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json");
                            break;
                        case HttpVerb.DELETE:
                            request.Method = HttpMethod.Delete;
                            break;
                    }

                    response = await this._HttpClient.SendAsync(request);

                    var statusCode = response.StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();

                            if (deserializeAsJson)
                            {
                                var jsonReponseBody = JsonConvert.DeserializeObject(responseBody).ToString();

                                return JsonConvert.DeserializeObject<TResult>(jsonReponseBody);
                            }
                            else
                            {
                                return (TResult)Convert.ChangeType(responseBody, typeof(TResult)); ;
                            }
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
        //  Groups

        #region Groups
       
        public async Task<Group> GetGroup(long groupId)
        {
            var response = await this.ExecuteRequest<Group, Group>(HttpVerb.GET, string.Format("groups/{0}", groupId), null);

            return response;
        }

        public async Task<IEnumerable<Group>> GetGroups(bool includeGroupMembers = false)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Group>, Group>(HttpVerb.GET, "groups", null);

            IEnumerable<Group> groups = response.Data;

            if (includeGroupMembers)
            {
                foreach(Group g in groups)
                {
                    Group group = GetGroup(g.Id.Value).Result;
                    if(group != null)
                    {
                        g.Members = group.Members;
                    }
                }
            }

            return groups;
        }


        public async Task<Group> CreateGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new Exception("Group Name cannot be null or blank");
            }

            var group = new Group(groupName);

            var response = await this.ExecuteRequest<ResultResponse<Group>, Group>(HttpVerb.POST, "groups", group);

            return response.Result;
        }

        public async Task<bool>DeleteGroup(long groupId)
        {
            var response = await this.ExecuteRequest<ResultResponse<Group>, long>(HttpVerb.DELETE, string.Format("groups/{0}", groupId), groupId);

            return true;
        }

        public async Task<bool> DeleteGroupMember(long groupId, long userId)
        {
            var response = await this.ExecuteRequest<ResultResponse<GroupMember>, string>(HttpVerb.DELETE, string.Format("groups/{0}/members/{1}", groupId, userId), string.Format("g{0}:u{1}", groupId, userId));

            return true;
        }

        public async Task<IEnumerable<GroupMember>> CreateGroupMembers(long groupId, IEnumerable<GroupMember> groupMembers)
        {
            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<GroupMember>>, IEnumerable<GroupMember>>(HttpVerb.POST, string.Format("groups/{0}/members", groupId), groupMembers);

            return response.Result;
        }

        #endregion

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
        public async Task<Sheet> CreateSheet(string sheetName, IEnumerable<Column> columns, long? folderId = null, long? workspaceId = null)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new Exception("Sheet Name cannot be null or blank");
            }

            var sheet = new Sheet(sheetName, columns.ToList());

            var response = new ResultResponse<Sheet>();
               
            if (folderId == null && workspaceId == null)
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("sheets"), sheet);
            }
            else if (folderId != null && workspaceId == null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("folders/{0}/sheets", folderId), sheet);
            }
            else if (folderId == null && workspaceId != null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("workspaces/{0}/sheets", workspaceId), sheet);
            }

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<Sheet> CreateSheetFromTemplate(string sheetName, long? templateId, long? folderId = null, long? workspaceId = null)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new Exception("Sheet Name cannot be null or blank");
            }

            if (templateId == null)
            {
                throw new Exception("Template ID cannot be null or blank");
            }

            var sheet = new Sheet(sheetName, null);
            sheet.FromId = templateId;

            var response = new ResultResponse<Sheet>();

            if (folderId == null && workspaceId == null)
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("sheets"), sheet);
            }
            else if (folderId != null && workspaceId == null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("folders/{0}/sheets?include=data", folderId), sheet);
            }
            else if (folderId == null && workspaceId != null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("workspaces/{0}/sheets", workspaceId), sheet);
            }

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<Sheet> GetSheetById(long? sheetId, string include = "")
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            string includeClause = String.IsNullOrEmpty(include) ? String.Empty : $"?include={include}";
            var response = await this.ExecuteRequest<Sheet, Sheet>(HttpVerb.GET, string.Format("sheets/{0}{1}", sheetId, includeClause), null);

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
        public async Task<IEnumerable<Sheet>> GetOrgSheets() {
            var response = await this.ExecuteRequest<IndexResultResponse <Sheet>, Sheet>(HttpVerb.GET, "users/sheets", null);
            
            return response.Data;
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
                foreach (var row in rows)
                {
                    row.ToTop = toTop;
                    row.ToBottom = toBottom;
                    row.ParentId = parentId;
                    row.SiblingId = siblingId;

                    foreach (var cell in row.Cells)
                    {
                        cell.Build(false);
                    }
                }
            }

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.POST, string.Format("sheets/{0}/rows", sheetId), rows);

            return response.Result;
        }

        public async Task<CopyOrMoveRowResult> CopyRowsToSheet(long? sourceSheetId, long? destinationSheetId, IList<long?> rowIds)
        {
            if (sourceSheetId == null || destinationSheetId == null)
            {
                throw new Exception("Source or Destination Sheet ID cannot be null");
            }

            var copyOrMoveRowDirective = new CopyOrMoveRowDirective()
            {
                To = new CopyOrMoveRowDestination()
                {
                    SheetId = destinationSheetId
                },
                RowIds = rowIds
            };

            var response = await this.ExecuteRequest<CopyOrMoveRowResult, CopyOrMoveRowDirective>(HttpVerb.POST, string.Format("sheets/{0}/rows/copy?include=all", sourceSheetId), copyOrMoveRowDirective);

            return response;
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
        //  Columns
        public async Task<IEnumerable<Column>> CreateColumns(long? sheetId, IEnumerable<Column> columns, bool? toTop = null, bool? toBottom = null, long? parentId = null, long? siblingId = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (columns.Count() > 1)
            {
                foreach (var column in columns)
                {
                    if (column.Title == null || column.Type == null)
                    {
                        throw new Exception("Column is missing required attributes Title, or Type");
                    }
                }
            }

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Column>>, IEnumerable<Column>>(HttpVerb.POST, string.Format("sheets/{0}/columns", sheetId), columns);

            return response.Result;
        }

        //
        //  Folders
        #region Folders
        public async Task<Folder> CreateFolder(string folderName,  string workspaceId = null)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                throw new Exception("Folder Name cannot be null or blank");
            }

            var folder = new Folder(folderName);

            var response = new ResultResponse<Folder>();

            if (workspaceId == null)
            {
                response = await this.ExecuteRequest<ResultResponse<Folder>, Folder>(HttpVerb.POST, string.Format("home/folders"), folder);
            }
            else if (workspaceId != null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Folder>, Folder>(HttpVerb.POST, string.Format("workspaces/{0}/sheets", workspaceId), folder);
            }

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<IEnumerable<ISmartsheetObject>> GetFoldersForWorkspace(long? workspaceId)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null);

            return response.Folders;
        }

        public async Task<Folder> GetFolderById(long? folderId)
        {
            if (folderId == null)
            {
                throw new Exception("Folder ID cannot be null");
            }

            var response = await this.ExecuteRequest<Folder, Folder>(HttpVerb.GET, string.Format("folders/{0}", folderId), null);

            return response;
        }
        #endregion

        //
        //  Reports
        #region Reports
        public async Task<Report> GetReportById(long? reportId, int? pageSize = 500, int? page = 1)
        {
            if (reportId == null)
            {
                throw new Exception("Report ID cannot be null");
            }

            var response = await this.ExecuteRequest<Report, Report>(HttpVerb.GET, string.Format("reports/{0}?pageSize={1}&page={2}", reportId, pageSize, page), null);

            response._Client = this;

            return response;
        }

        public async Task<string> GetReportByIdAsCsv(long? reportId)
        {
            if (reportId == null)
            {
                throw new Exception("Report ID cannot be null");
            }

            var headers = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("Accept", "text/csv")
            };

            var response = await this.ExecuteRequest<string, string>(HttpVerb.GET, string.Format("reports/{0}", reportId), null, headers, false);

            return response;
        }

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

        public async Task<UpdateRequest> CreateUpdateRequest(long? sheetId, UpdateRequest updateRequest)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (updateRequest.RowIds.Count == 0)
            {
                throw new Exception("Must specifiy 1 or more rows to update");
            }

            if (updateRequest.SendTo.Count == 0)
            {
                throw new Exception("Must specifiy 1 or more recipients");
            }

            if (updateRequest.IncludeAttachments == null)
            {
                updateRequest.IncludeAttachments = false;
            }

            if (updateRequest.IncludeDiscussions == null)
            {
                updateRequest.IncludeDiscussions = false;
            }

            var result = await this.ExecuteRequest<ResultResponse<UpdateRequest>, UpdateRequest>(HttpVerb.POST, string.Format("sheets/{0}/updaterequests", sheetId), updateRequest);

            return result.Result;
        }
        #endregion
    }
}
