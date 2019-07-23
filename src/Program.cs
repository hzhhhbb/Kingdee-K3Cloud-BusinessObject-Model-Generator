using System.IO;

namespace Kingdee.Vincent.Generator
{
    using System;
    using System.Configuration;
    using System.Threading;

    using Kingdee.BOS.Authentication;
    using Kingdee.BOS.Core.Metadata;
    using Kingdee.BOS.ServiceFacade.KDServiceClient.Metadata;
    using Kingdee.BOS.ServiceFacade.KDServiceClient.User;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string hostUrl = appSettings["hostUrl"];
            string userName = appSettings["userName"];
            string password = appSettings["password"];
            string acctId = appSettings["dataCenterId"];

            UserServiceProxy proxy = new UserServiceProxy();
            LoginInfo loginInfo = new LoginInfo
                                      {
                                          Username = userName,
                                          Password = password,
                                          Lcid = 2052,
                                          AcctID = acctId,
                                          LoginType = LoginType.NormalERPLogin
                                      };

            proxy.HostURL = hostUrl;
            proxy.ValidateUser(
                hostUrl,
                loginInfo,
                result =>
                    {
                        Console.WriteLine(result.IsSuccessByAPI ? "登录成功" : throw new Exception("登录失败" + result.Message));
                    });
            Thread.Sleep(2000);

            MetadataServiceProxy metadataService = new MetadataServiceProxy { HostURL = hostUrl };
            string objectId;
            bool isExist;
            do
            {
                Console.WriteLine("输入业务对象标识并回车");

                objectId = Console.ReadLine();

                isExist = metadataService.IsExistMetaObjectType(objectId);
                if (!isExist)
                {
                    Console.WriteLine($"标识为{objectId}的业务对象不存在，请重新输入");
                }
            }
            while (!isExist);

            FormMetadata formMetadata = metadataService.GetFormMetadata(objectId);
            string outputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, formMetadata.SubSystemId + Path.DirectorySeparatorChar);
            Console.WriteLine($"开始生成业务对象{formMetadata.Name}的实体类");
            ClassFileGenerator.GenerateClassFiles(formMetadata, outputPath);
            Console.WriteLine($"生成结束，文件路径：{outputPath}");
            Console.ReadKey();
        }
    }
}