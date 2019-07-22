namespace Kingdee.Vincent.Generator
{
    using System.Collections.Generic;

    public class ClassPropertyDefinition
    {
        public ClassPropertyDefinition()
        {
            this.Attributes = new HashSet<string>();
        }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public string Annotation { get; set; }

        public object DefaultValue { get; set; }

        public HashSet<string> Attributes { get; set; }
    }
}