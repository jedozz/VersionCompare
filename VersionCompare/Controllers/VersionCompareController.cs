using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VersionCompare.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;

namespace VersionCompare.Controllers
{
    [ApiController]
    [WebApiResultProcessFilter]
    [WebApiApplicationExceptionFilter]
    [Route("[controller]/[action]")]
    public class VersionCompareController : ControllerBase
    {
        private List<Project> _projects;
        private string _beyondCompareExePath;

        public VersionCompareController(IConfiguration configuration)
        {
            _beyondCompareExePath = configuration.GetSection("BeyondCompare").Get<string>();
            _projects = configuration.GetSection("Projects").Get<List<Project>>();
        }

        //public string Test()
        //{
        //    var r = "";
        //    var paths = _codesInStore.Select(m => m.Path);
        //    foreach (var path in paths)
        //    {
        //        r += path + "\r\n";
        //    }
        //    return r;
        //}

        public List<Project> CompareAll()
        {
            foreach (var project in _projects)
            {
                FillProject(project);
            }
            return _projects;
        }

        [HttpPost]
        public Project Compare([FromForm] int projectIndex)
        {
            var project = _projects[projectIndex];
            FillProject(project);
            return project;
        }

        [HttpPost]
        public void StartBeyondCompare([FromBody] CodeRelation cr)
        {
            try
            {
                Process.Start(_beyondCompareExePath, cr.CodeInStore.Path + " " + cr.CodeInProject.Path);
            }
            catch
            {
                throw new ApplicationException("BeyondCompare启动失败，检查启动路径配置，检查是否安装BeyondCompare");
            }
        }

        [HttpPost]
        public void OpenDirOfFile([FromForm] string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            OpenDir(dir);
        }

        [HttpPost]
        public void OpenProjectDir([FromForm] int projectIndex)
        {
            var project = _projects[projectIndex];
            var dir = project.Path;
            OpenDir(dir);
        }

        [HttpPost]
        public void Download([FromBody] CodeRelation cr)
        {
            System.IO.File.Copy(cr.CodeInStore.Path, cr.CodeInProject.Path, true);
        }

        [HttpPost]
        public void Upload([FromBody] CodeRelation cr)
        {
            System.IO.File.Copy(cr.CodeInProject.Path, cr.CodeInStore.Path, true);
        }


        [HttpPost]
        public void DownloadMore([FromForm] int projectIndex)
        {
            var project = _projects[projectIndex];
            FillProject(project);
            foreach (var cr in project.CodeRelations)
            {
                if (cr.CompareResult == CompareResults.VersionLow)
                {
                    System.IO.File.Copy(cr.CodeInStore.Path, cr.CodeInProject.Path, true);
                }
            }
        }

        [HttpPost]
        public void UploadMore([FromForm] int projectIndex)
        {
            var project = _projects[projectIndex];
            FillProject(project);
            foreach (var cr in project.CodeRelations)
            {
                if (cr.CompareResult == CompareResults.VersionHigh)
                {
                    System.IO.File.Copy(cr.CodeInProject.Path, cr.CodeInStore.Path, true);
                }
            }
        }

        private void OpenDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Process.Start("explorer.exe", dir);
                }
                else
                {
                    throw new ApplicationException("没有找到指定的文件夹");
                }
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                {
                    throw;
                }
                else
                {
                    throw new ApplicationException("打开文件夹失败");
                }
            }
        }

        private void FillProject(Project project)
        {
            project.CodeRelations = new List<CodeRelation>();
            var codesInStore = GetCodes(project.CodeStoreDirs).OrderBy(code => code.Path).ToList();
            foreach (var codeInStore in codesInStore)
            {
                var codeInProject = FindCode(codeInStore.FileName, project.Path);
                project.CodeRelations.Add(GetCodeRelation(codeInStore, codeInProject));
            }
        }

        private CodeRelation GetCodeRelation(Code codeInStore, Code codeInProject)
        {
            CompareResults cr;
            if (codeInProject == null)
            {
                cr = CompareResults.NoFoundInProject;
            }
            else if (!codeInProject.VersionDT.HasValue)
            {
                cr = CompareResults.VersionErrorInProject;
            }
            else if (!codeInStore.VersionDT.HasValue)
            {
                cr = CompareResults.VersionErrorInCodeStore;
            }
            else if (codeInStore.VersionDT > codeInProject.VersionDT)
            {
                cr = CompareResults.VersionLow;
            }
            else if (codeInStore.VersionDT < codeInProject.VersionDT)
            {
                cr = CompareResults.VersionHigh;
            }
            else if (codeInStore.VersionDT == codeInProject.VersionDT && codeInStore.Content == codeInProject.Content)
            {
                cr = CompareResults.Equal;
            }
            else
            {
                cr = CompareResults.VersionEqualContentNot;
            }
            var r = new CodeRelation() { CodeInStore = codeInStore, CodeInProject = codeInProject, CompareResult = cr };
            return r;
        }

        private Code FindCode(string fileName, string dir)
        {
            var filePath = FindFile(fileName, dir);
            var r = GetCode(filePath);
            return r;
        }

        private string FindFile(string fileName, string dir)
        {
            var filePaths = Directory.GetFiles(dir);
            foreach (var filePath in filePaths)
            {
                if (fileName == Path.GetFileName(filePath))
                {
                    return filePath;
                }
            }
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                var filePath = FindFile(fileName, subDir);
                if (!string.IsNullOrEmpty(filePath))
                {
                    return filePath;
                }
            }
            return null;
        }

        private List<Code> GetCodes(List<string> dirs)
        {
            var r = new List<Code>();
            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir))
                {
                    var filePaths = Directory.GetFiles(dir);
                    foreach (var filePath in filePaths)
                    {
                        r.Add(GetCode(filePath));
                    }
                    r.AddRange(GetCodes(Directory.GetDirectories(dir).ToList()));
                }
            }
            return r;
        }

        private Code GetCode(string filePath)
        {
            Code r;
            if (System.IO.File.Exists(filePath))
            {
                switch (Path.GetExtension(filePath).ToLower())
                {
                    case ".js":
                        r = GetCodeOfJavaScriptOrVueComponent(filePath, CodeTypes.JavaScript);
                        break;
                    case ".vue":
                        r = GetCodeOfJavaScriptOrVueComponent(filePath, CodeTypes.VueComponent);
                        break;
                    case ".css":
                        r = GetCodeOfCss(filePath);
                        break;
                    case ".json":
                        r = GetCodeOfJson(filePath);
                        break;
                    default:
                        r = null;
                        break;
                }
            }
            else
            {
                r = null;
            }
            return r;
        }

        private Code GetCodeOfJavaScriptOrVueComponent(string filePath, CodeTypes codeType)
        {
            var content = GetFileContent(filePath);
            var r = new Code() { Path = filePath, CodeType = codeType, Content = content };
            var versionLine = GetVersionLine(content);
            try
            {
                var versionStr = versionLine.Replace("//", "").Trim();
                r.VersionDT = DateTime.Parse(versionStr);
            }
            catch
            {
                r.VersionDT = null;
            }
            return r;
        }
        private Code GetCodeOfCss(string filePath)
        {
            var content = GetFileContent(filePath);
            var r = new Code() { Path = filePath, CodeType = CodeTypes.Css, Content = content };
            var versionLine = GetVersionLine(content);
            try
            {
                var versionStr = versionLine.Replace("/*", "").Replace("*/", "").Trim();
                r.VersionDT = DateTime.Parse(versionStr);
            }
            catch
            {
                r.VersionDT = null;
            }
            return r;
        }

        private Code GetCodeOfJson(string filePath)
        {
            var content = GetFileContent(filePath);
            var r = new Code() { Path = filePath, CodeType = CodeTypes.Json, Content = content };
            dynamic obj = JsonConvert.DeserializeObject(content);
            try
            {
                var versionStr = (string)obj.JsonVersion;
                r.VersionDT = DateTime.Parse(versionStr);
            }
            catch
            {
                r.VersionDT = null;
            }
            return r;
        }

        private string GetFileContent(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open);
            fs.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(fs);
            var r = sr.ReadToEnd();
            fs.Dispose();
            return r;
        }

        private string GetVersionLine(string content)
        {
            var lines = content.Split("\n");
            var r = lines.Length > 0 ? lines[0] : null;
            return r;
        }

        public class Code
        {
            public string FileName => System.IO.Path.GetFileName(Path);

            public string Path { get; set; }

            [JsonIgnore]
            public DateTime? VersionDT { get; set; }

            public string Version => VersionDT.HasValue ? VersionDT.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";

            public CodeTypes CodeType { get; set; }

            [JsonIgnore]
            public string Content { get; set; }
        }

        public enum CodeTypes
        {
            JavaScript,
            VueComponent,
            Css,
            Json
        }

        public class Project
        {
            public string Name { get; set; }

            [JsonIgnore]
            public string Path { get; set; }

            public List<CodeRelation> CodeRelations { get; set; }

            [JsonIgnore]
            public List<string> CodeStoreDirs { get; set; }
        }

        public class CodeRelation
        {
            public Code CodeInStore { get; set; }

            public Code CodeInProject { get; set; }

            public CompareResults CompareResult { get; set; }

            public string CompareResultDes
            {
                get
                {
                    string r = null;
                    switch (CompareResult)
                    {
                        case CompareResults.Equal:
                            r = "相同";
                            break;
                        case CompareResults.VersionLow:
                            r = "版本低";
                            break;
                        case CompareResults.VersionHigh:
                            r = "版本高";
                            break;
                        case CompareResults.VersionEqualContentNot:
                            r = "版本相同，代码不相同";
                            break;
                        case CompareResults.VersionErrorInProject:
                            r = "项目中的文件版本号有误";
                            break;
                        case CompareResults.NoFoundInProject:
                            r = "项目中无此文件";
                            break;
                        case CompareResults.VersionErrorInCodeStore:
                            r = "代码库中的文件版本号有误";
                            break;
                    }
                    return r;
                }
            }
        }

        public enum CompareResults
        {
            Equal,
            VersionLow,
            VersionHigh,
            VersionEqualContentNot,
            VersionErrorInProject,
            NoFoundInProject,
            VersionErrorInCodeStore
        }

        #region api配置相关
        public class WebApiResultProcessFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Exception == null)
                {
                    if (context.Result is ObjectResult)
                    {
                        var r = context.Result as ObjectResult;
                        context.Result = WebApiResult.CreateResult(0, null, r.Value);
                    }
                    else if (context.Result is EmptyResult)
                    {
                        context.Result = WebApiResult.CreateResult(0, null, null);
                    }
                }
            }
        }

        public class WebApiApplicationExceptionFilterAttribute : ExceptionFilterAttribute
        {
            public override void OnException(ExceptionContext context)
            {
                if (context.Exception is WebApiException)
                {
                    var webApiEx = (WebApiException)context.Exception;
                    context.Result = WebApiResult.CreateResult(webApiEx.ErrorCode, webApiEx.Message, null);
                }
                else if (context.Exception is ApplicationException)
                {
                    context.Result = WebApiResult.CreateResult(1, context.Exception.Message, null);
                }
                else
                {
                    ExceptionHandler.Current.Log(context.Exception);
                    context.Result = WebApiResult.CreateResult(2, "系统错误", null);
                }
            }
        }

        internal class WebApiResult
        {
            public bool Success => ErrorCode == 0;

            /// <summary>
            /// 0正常，1已处理的通用错误代码的，2 其他数字为指定的错误代码
            /// </summary>
            public int ErrorCode { get; set; }

            public string ErrorTypeDesc => ErrorType == null ? "" : ErrorType.ToString();

            public WebApiErrorTypes? ErrorType
            {
                get
                {
                    WebApiErrorTypes? r;
                    if (ErrorCode == 0)
                    {
                        r = null;
                    }
                    else if (ErrorCode == 1 || ErrorCode >= 1000)
                    {
                        r = WebApiErrorTypes.Handled;
                    }
                    else if (ErrorCode == 2)
                    {
                        r = WebApiErrorTypes.Unhandled;
                    }
                    else if (ErrorCode >= 100 && ErrorCode <= 600)
                    {
                        r = WebApiErrorTypes.HttpError;
                    }
                    else
                    {
                        r = WebApiErrorTypes.UnknownErrorCode;
                    }
                    return r;
                }
            }

            public string ErrorMessage { get; set; }

            public object Data { get; set; }

            internal static JsonResult CreateResult(int errorCode, string errorMsg, object data)
            {
                var result = new WebApiResult() { ErrorMessage = errorMsg, ErrorCode = errorCode, Data = data };
                return new JsonResult(result);
            }
        }

        public class WebApiException : ApplicationException
        {
            public WebApiException(int errorCode) : base("")
            {
                ErrorCode = errorCode;
            }

            public WebApiException(string message, int errorCode) : base(message)
            {
                ErrorCode = errorCode;
            }

            public int ErrorCode { get; private set; }
        }

        public enum WebApiErrorTypes
        {
            Handled,
            Unhandled,
            HttpError,
            UnknownErrorCode
        }
        #endregion
    }
}
