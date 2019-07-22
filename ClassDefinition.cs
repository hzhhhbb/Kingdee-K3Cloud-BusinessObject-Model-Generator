namespace Kingdee.Vincent.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ClassDefinition
    {
        public ClassDefinition()
        {
            this.UsingNamespaces = new HashSet<string>();
            this.Properties = new List<ClassPropertyDefinition>();
            this.Attributes = new HashSet<string>();
        }

        public string Annotation { get; private set; }

        public HashSet<string> Attributes { get; private set; }

        public string InheritClassName { get; private set; }

        public string Name { get; private set; }

        public string Namespace { get; private set; }

        public List<ClassPropertyDefinition> Properties { get; private set; }

        public HashSet<string> UsingNamespaces { get; private set; }

        public ClassDefinition AddAttribute(string attribute)
        {
            if (this.Attributes == null)
            {
                this.Attributes = new HashSet<string>();
            }

            this.Attributes.Add(attribute);
            return this;
        }

        public ClassDefinition AddAttribute(IList<string> attributes)
        {
            if (this.Attributes == null)
            {
                this.Attributes = new HashSet<string>();
            }

            foreach (string attribute in attributes)
            {
                this.AddAttribute(attribute);
            }

            return this;
        }

        public ClassDefinition SetAnnotation(string annotation)
        {
            if (string.IsNullOrWhiteSpace(annotation))
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            this.Annotation = annotation;
            return this;
        }

        public ClassDefinition SetInheritClassName(string inheritClassName)
        {
            if (string.IsNullOrWhiteSpace(inheritClassName))
            {
                throw new ArgumentNullException(nameof(inheritClassName));
            }

            this.InheritClassName = inheritClassName;
            return this;
        }

        public ClassDefinition SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            return this;
        }

        public ClassDefinition SetNamespace(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
            {
                throw new ArgumentNullException(nameof(ns));
            }

            this.Namespace = ns;
            return this;
        }

        public ClassDefinition SetProperties(List<ClassPropertyDefinition> properties)
        {
            if (!properties.Any())
            {
                throw new ArgumentNullException(nameof(properties));
            }

            this.Properties = properties;
            return this;
        }

        public ClassDefinition SetUsingNamespaces(HashSet<string> usingNamespaces)
        {
            this.UsingNamespaces = usingNamespaces;
            return this;
        }

        public ClassDefinition AddUsingNamespaces(string usingNamespace)
        {
            this.UsingNamespaces.Add(usingNamespace);
            return this;
        }
    }
}