using System;

namespace SLiNgshoT.Core {
    public class OutputFormatAttribute : Attribute {
        public OutputFormatAttribute(string name) {
            this.name = name;
        }

        private string name;

        public string Name {
            get {
                return name;
            }
        }
    }

    public class OutputParameterAttribute : Attribute {
        public OutputParameterAttribute(string name, bool required, string description) {
            this.name = name;
            this.required = required;
            this.description = description;
        }

        private string name;
        bool required;
        string description;

        public string Name {
            get {
                return name;
            }
        }

        public bool Required {
            get {
                return required;
            }
        }

        public string Description {
            get {
                return description;
            }
        }
    }
}
