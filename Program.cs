using System.IO;

namespace Kingdee.Vincent.Generator
{
    using System;
    using System.Threading;

    using Kingdee.BOS.Authentication;
    using Kingdee.BOS.Core.Metadata;
    using Kingdee.BOS.ServiceFacade.KDServiceClient.Metadata;
    using Kingdee.BOS.ServiceFacade.KDServiceClient.User;

    internal class Program
    {
        private static void Main(string[] args)
        {
            const string HostUrl = "http://localhost/k3cloud/";

            UserServiceProxy proxy = new UserServiceProxy();
            LoginInfo loginInfo = new LoginInfo
                                      {
                                          Username = "administrator",
                                          Password = "888888",
                                          Lcid = 2052,
                                          AcctID = "5d148e1449825f",
                                          LoginType = LoginType.NormalERPLogin
                                      };

            proxy.HostURL = HostUrl;
            proxy.ValidateUser(
                HostUrl,
                loginInfo,
                result =>
                    {
                        Console.WriteLine(result.IsSuccessByAPI ? "登录成功" : throw new Exception("登录失败" + result.Message));
                    });
            Thread.Sleep(1500);
            MetadataServiceProxy metadataService = new MetadataServiceProxy
                                                       {
                                                           HostURL = HostUrl
                                                       };
            string objectId = "BD_MATERIAL";
            FormMetadata formMetadata = metadataService.GetFormMetadata(objectId);

            string outputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BD" + Path.DirectorySeparatorChar);

            ClassFileGenerator.GenerateClassFiles(formMetadata, outputPath);
            Console.WriteLine("生成结束");
            Console.ReadKey();
        }
    }
}