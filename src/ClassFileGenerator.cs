using System.Globalization;
using Kingdee.BOS.Core.CommonFilter.ConditionVariableAnalysis;
using Kingdee.BOS.Core.Metadata.FormElement;

namespace Kingdee.Vincent.Generator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Kingdee.BOS.Core.Metadata;
    using Kingdee.BOS.Core.Metadata.EntityElement;
    using Kingdee.BOS.Core.Metadata.FieldElement;
    using Kingdee.BOS.Orm.Metadata.DataEntity;

    public static class ClassFileGenerator
    {
        public static void GenerateClassFiles(FormMetadata formMetadata, string directoryPath)
        {
            if (formMetadata == null)
            {
                throw new ArgumentNullException(nameof(formMetadata));
            }

            List<Entity> entities = formMetadata.BusinessInfo.Entrys;
            string formId = formMetadata.BusinessInfo.GetForm().Id;
            string ns = $"Kingdee.Vincent.Core.BD.{formId}";
            List<ClassDefinition> classDefinitions = new List<ClassDefinition>();
            foreach (Entity entity in entities)
            {
                classDefinitions.AddRange(BuildClassDefinitions(formMetadata.BusinessInfo,entity, ns, formId));
            }

            classDefinitions = FilterClassDefinition(classDefinitions);

            foreach (ClassDefinition definition in classDefinitions)
            {
                string filePath = Path.Combine(directoryPath + definition.Name + ".cs");
                GenerateClassFile(definition, filePath);
            }
        }

        private static List<ClassDefinition> FilterClassDefinition(List<ClassDefinition> classDefinitions)
        {
            // TODO 更精准地筛选重复类定义
            // 去除重复的类定义(名称重复即认为是重复)
           var filteredClassDefinitions = classDefinitions.GroupBy(u => u.Name)
                .Select(u => u.OrderByDescending(t => t.Properties.Count).FirstOrDefault()).ToList();
            return filteredClassDefinitions;
        }

        private static List<ClassDefinition> BuildClassDefinitions(BusinessInfo businessInfo,Entity entity, string ns, string formId)
        {
            List<ClassDefinition> results = new List<ClassDefinition>();

            ClassDefinition definition = new ClassDefinition();
            List<ClassPropertyDefinition> classProperties = new List<ClassPropertyDefinition>(entity.DynamicObjectType.Properties.Count);
            HashSet<string> usingNamespaces = new HashSet<string>();

            List<Field> fields = entity.Fields;
            foreach (DynamicProperty property in entity.DynamicObjectType.Properties)
            {
                Field field = fields.FirstOrDefault(u => u.PropertyName.Equals(property.Name));
                if (property.Name.EndsWith("_Id", true, CultureInfo.CurrentCulture))
                {
                    continue;
                }
                string usingNamespace = GetNamespaceName(property);
                if (!string.IsNullOrWhiteSpace(usingNamespace))
                {
                    usingNamespaces.Add(usingNamespace);
                }

                string annotation = field?.Name;
                if (string.IsNullOrWhiteSpace(annotation))
                {
                    annotation = property.Name;
                }

                string type = GetPropertyTypeName(property, field);
                string name = property.Name;

                // 基础资料类型属性、单选辅助资料属性
                // if (property is IComplexProperty complexProperty)
                // {
                //    string lookUpObjectId = GetFieldLookUpObjectId(field);
                //
                //    type = lookUpObjectId;
                //    results.Add(BuildClassDefinition(complexProperty, ns, lookUpObjectId));
                // }

                var singlePropertyDefinition = new ClassPropertyDefinition()
                                                   {
                                                       Name = name, Annotation = annotation, TypeName = type
                                                   };
                //给字段加上数据表列的特性
                if (field != null)
                {
                    singlePropertyDefinition.Attributes.Add($"[Column(\"{field.FieldName}\")]");
                }
                else
                {
                    if (property == entity.DynamicObjectType.PrimaryKey)
                    {
                        singlePropertyDefinition.Attributes.Add($"[Column(\"{entity.EntryPkFieldName}\")]");
                    }
                    else if(property == businessInfo.GetForm().MasterIdDynamicProperty)
                    {
                        singlePropertyDefinition.Attributes.Add($"[Column(\"{businessInfo.GetForm().MasterPKFieldName}\")]");
                    }
                    else if(entity.SeqDynamicProperty == property)
                    {
                        singlePropertyDefinition.Attributes.Add($"[Column(\"{entity.SeqFieldKey}\")]");
                    }
                }


                classProperties.Add(singlePropertyDefinition);
            }

            string className = GetClassName(entity, formId);
            var classDefinition = definition.SetAnnotation(entity.Name).SetName(className).SetNamespace(ns)
                .SetProperties(classProperties).SetUsingNamespaces(usingNamespaces);
            classDefinition.AddAttribute($"[BusinessObject(\"{businessInfo.GetForm().Id}\")]");
            if (!string.IsNullOrWhiteSpace(entity.TableName))
            {
                classDefinition.AddAttribute($"[Table(\"{entity.TableName}\")]");
#warning 需自行定义Table特性所在命名空间（.NET 4.0不提供Table特性定义）ref:https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.tableattribute?view=netframework-4.8&viewFallbackFrom=netframework-4.0
                classDefinition.AddUsingNamespaces("Kingdee.Vincent.Core.Domain.Entities");
            }

            results.Add(classDefinition);
            return results;
        }

        private static string GetFieldLookUpObjectId(Field field)
        {
            string lookUpObjectId = string.Empty;
            switch (field)
            {
                case ILookUpField lookUpField:
                    lookUpObjectId = lookUpField.LookUpObject.FormId;
                    break;
                case IRelatedFlexGroupField flexGroupField:
                    return flexGroupField.BDFlexType.FormId;
            }

            return lookUpObjectId;
        }

        private static string GetBusinessObjectPkTypeName(Field field)
        {
            EnumPkFieldType pkFieldType = EnumPkFieldType.INT;
            switch (field)
            {
                case ILookUpField lookUpField:
                    pkFieldType = lookUpField.LookUpObject.PkFieldType;
                    break;
                case IRelatedFlexGroupField flexGroupField:
                    pkFieldType = flexGroupField.RelateFlexBusinessInfo.GetForm().PkFieldType;
                    break;
            }

            return pkFieldType.ToString().ToLower();
        }

        private static ClassDefinition BuildClassDefinition(IComplexProperty property, string ns, string className)
        {
            ClassDefinition definition = new ClassDefinition();
            List<ClassPropertyDefinition> classProperties = new List<ClassPropertyDefinition>();
            HashSet<string> usingNamespaces = new HashSet<string>();

            foreach (var dataEntityProperty in property.ComplexPropertyType.Properties)
            {
                var entityProperty = (DynamicProperty)dataEntityProperty;
                usingNamespaces.Add(entityProperty.PropertyType.Namespace);
                string annotation = entityProperty.Name;
                if (string.IsNullOrWhiteSpace(annotation))
                {
                    annotation = entityProperty.Name;
                }

                classProperties.Add(
                new ClassPropertyDefinition()
                    {
                        Name = entityProperty.Name,
                        Annotation = annotation,
                        TypeName = GetBaseTypeName(entityProperty.PropertyType)
                    });
            }

            return definition.SetAnnotation(property.Name).SetName(className).SetNamespace(ns).SetProperties(classProperties).SetUsingNamespaces(usingNamespaces);
        }

        private static string GetPropertyTypeName(DynamicProperty property, Field field)
        {
            string typeName = string.Empty;

            switch (property)
            {
                // 基础资料类型属性、单选辅助资料属性
                case IComplexProperty _:
                {
                    typeName = GetBaseTypeName(property.ReflectedType.PrimaryKey.PropertyType);
                    break;
                }

                // 单据体
                case ICollectionProperty collectionProperty:
                    typeName = GetICollectionPropertyTypeName(collectionProperty);
                    break;

                // 普通属性
                case ISimpleProperty _:
                    typeName = GetBaseTypeName(property.PropertyType);
                    break;
            }

            return typeName;
        }

        private static string GetICollectionPropertyTypeName(ICollectionProperty property)
        {
           string typeName = property.PropertyType.Name.Equals("LocalDynamicObjectCollection") ?
                GetBaseTypeName(property.PropertyType) : $"List<{property.Name}>";

            return typeName;
        }

        private static string GenerateClassFile(ClassDefinition definition, string filePath)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            StringBuilder sb = new StringBuilder();

            // 文件头注释
            sb.AppendLine("// ================================================================================");
            sb.AppendLine("//  描述: 业务对象[" + definition.Name + "]的实体类");
            sb.AppendLine("//  此实体类通过代码生成工具自动生成，如需修改，请通过代码生成工具重新生成");
            sb.AppendLine("//  生成日期: " + DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒"));
            sb.AppendLine("// ================================================================================");

            // Using
            foreach (string ns in definition.UsingNamespaces)
            {
                sb.AppendLine($"using {ns};");
            }

            sb.AppendLine();

            // namespace
            sb.AppendLine("namespace " + definition.Namespace);
            sb.AppendLine("{");

            // class desc
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"   /// {definition.Annotation}");
            sb.AppendLine("    /// </summary>");

            // class attributes
            foreach (string attribute in definition.Attributes)
            {
                sb.AppendLine("    " + attribute);
            }

            // class
            sb.AppendLine($"    public class {definition.Name}");
            sb.AppendLine("    {");

            // field
            foreach (ClassPropertyDefinition property in definition.Properties)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {property.Annotation}");
                sb.AppendLine("        /// </summary>");
                foreach (string propertyAttribute in property.Attributes)
                {
                    sb.AppendLine("        " + propertyAttribute);
                }

                sb.AppendLine($"        public {property.TypeName} {property.Name} {{ get; set; }}");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            SaveFile(sb.ToString(), filePath);
            return filePath;
        }

        private static string GetBaseTypeName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Dictionary<string, string> typeDictionary = new Dictionary<string, string>();
            typeDictionary.Add("OrmLocaleValue", "string");
            typeDictionary.Add("String", "string");
            typeDictionary.Add("Int64", "long");
            if (typeDictionary.ContainsKey(type.Name))
            {
                return typeDictionary[type.Name];
            }

            string assemblyQualifiedName = type.AssemblyQualifiedName;
            if (!string.IsNullOrWhiteSpace(assemblyQualifiedName))
            {
                if (assemblyQualifiedName.Contains("Nullable") && assemblyQualifiedName.Contains("System.DateTime"))
                {
                    return "DateTime?";
                }
            } 

            return type.Name;
        }

        private static void SaveFile(string str, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            StreamWriter stream = null;

            // 保存
            try
            {
                stream = new StreamWriter(filePath, false, Encoding.UTF8);
                stream.Write(str);
            }
            finally
            {
                stream?.Close();
            }
        }

        private static string GetClassName(Entity entity, string formId)
        {
            string className = string.Empty;

            // 34:单据头，38:子单据头，35:单据体
            switch (entity.ElementType)
            {
                case 34:
                    className = formId;
                    break;
               default:
                    className = entity.EntryName;
                    break;
            }

            return className;
        }

        private static string GetNamespaceName(DynamicProperty property)
        {
            string ns = string.Empty;

            switch (property)
            {
                // 单据体
                case ICollectionProperty _:
                    ns = "System.Collections.Generic";
                    break;

                // 普通属性
                case ISimpleProperty _:
                    ns = property.PropertyType.Namespace;
                    break;
            }

            return ns;
        }
    }
}
