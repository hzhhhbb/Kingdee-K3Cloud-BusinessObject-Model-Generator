# 金蝶K3Cloud业务对象实体类生成器
# 简介
使用使用此项目需要具备一定的K3Cloud开发经验,理解K3Cloud业务对象的基本概念<br/>
通过此项目，你可以把一个业务对象中关于数据库定义的部分，生成一个实体类，如下图<br/>
--------picture 1----------<br/>
此项目通过读取业务对象的元数据并解析，把其中的单据体、字段等元素映射为类定义、属性定义，从而生成cs文件<br/>
从事K3Cloud开发的人员都可以使用此项目<br/>
此项目可以帮助开发人员节省定义业务对象的Model时间（如果在开发过程中会定义的话）<br/>
# 如何使用？
1. 把项目中使用到的Kingdee.BOS、Kingdee.BOS.Core、Kingdee.BOS.DataEntity、Kingdee.BOS.ServiceFacade.KDServiceClient、Kingdee.BOS.ServiceFacade.KDServiceClientFx这5个程序集的引用指向于K3Cloud的安装目录bin下的对应程序集文件
2. 修改项目中App.config文件中的配置
3. 编译并启动项目，按照提示输入业务对象标识即可
# License
see [License](/LICENSE.txe)