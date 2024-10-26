namespace rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Schema;

public static class XmlDocSchema
{
    public static class Inspection
    {
        public static class Config
        {
            public static readonly string ElementName = "CodeInspection";


            public static readonly string NameAttribute = "Name";
            public static readonly string InspectionTypeAttribute = "InspectionType";
            public static readonly string SeverityAttribute = "Severity";
        }

        public static class Summary
        {
            public static readonly string ElementName = "summary";
            public static readonly string IsHiddenAttribute = "hidden";
        }

        public static class Reference
        {
            public static readonly string ElementName = "reference";
            public static readonly string NameAttribute = "name";
        }

        public static class HostApp
        {
            public static readonly string ElementName = "hostapp";
            public static readonly string NameAttribute = "name";
        }

        public static class Reasoning
        {
            public static readonly string ElementName = "why";
        }

        public static class Remarks
        {
            public static readonly string ElementName = "remarks";
        }

        public static class Example
        {
            public static readonly string ElementName = "example";
            public static readonly string HasResultAttribute = "hasresult";

            public static class Module
            {
                public static readonly string ElementName = "module";
                public static readonly string ModuleNameAttribute = "name";
                public static readonly string ModuleTypeAttribute = "type";
            }
        }
    }

    public static class QuickFix
    {
        public static class Summary
        {
            public static readonly string ElementName = "summary";
        }

        public static class Remarks
        {
            public static readonly string ElementName = "remarks";
        }

        public static class CanFix
        {
            public static readonly string ElementName = "canfix";
            public static readonly string ProcedureAttribute = "procedure";
            public static readonly string ModuleAttribute = "module";
            public static readonly string ProjectAttribute = "project";
        }

        public static class Inspections
        {
            public static readonly string ElementName = "inspections";

            public static class Inspection
            {
                public static readonly string ElementName = "inspection";
                public static readonly string NameAttribute = "name";
            }
        }

        public static class Example
        {
            public static readonly string ElementName = "example";

            public static class Before
            {
                public static readonly string ElementName = "before";

                public static class Module
                {
                    public static readonly string ElementName = "module";
                    public static readonly string ModuleNameAttribute = "name";
                    public static readonly string ModuleTypeAttribute = "type";
                }
            }

            public static class After
            {
                public static readonly string ElementName = "after";

                public static class Module
                {
                    public static readonly string ElementName = "module";
                    public static readonly string ModuleNameAttribute = "name";
                    public static readonly string ModuleTypeAttribute = "type";
                }
            }
        }
    }

    public static class Annotation
    {
        public static class Summary
        {
            public static readonly string ElementName = "summary";
        }

        public static class Remarks
        {
            public static readonly string ElementName = "remarks";
        }

        public static class Parameter
        {
            public static readonly string ElementName = "parameter";
            public static readonly string NameAttribute = "name";
            public static readonly string TypeAttribute = "type";
        }

        public static class Example
        {
            public static readonly string ElementName = "example";

            public static class Module
            {
                public static readonly string ElementName = "module";
                public static readonly string ModuleNameAttribute = "name";
                public static readonly string ModuleTypeAttribute = "type";

                public static class Before
                {
                    public static readonly string ElementName = "before";
                }

                public static class After
                {
                    public static readonly string ElementName = "after";
                }
            }

            public static class Before
            {
                public static readonly string ElementName = "before";

                public static class Module
                {
                    public static readonly string ElementName = "module";
                    public static readonly string ModuleNameAttribute = "name";
                    public static readonly string ModuleTypeAttribute = "type";
                }
            }

            public static class After
            {
                public static readonly string ElementName = "after";

                public static class Module
                {
                    public static readonly string ElementName = "module";
                    public static readonly string ModuleNameAttribute = "name";
                    public static readonly string ModuleTypeAttribute = "type";
                }
            }
        }
    }
}
