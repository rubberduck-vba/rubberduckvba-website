using System.ComponentModel.DataAnnotations;

namespace rubberduckvba.Server.Model;

public enum ExampleModuleType
{
    None = 0,
    [Display(Name = "(Any)")] Any,
    [Display(Name = "Class Module")] ClassModule,
    [Display(Name = "Document Module")] DocumentModule,
    [Display(Name = "Interface Module")] InterfaceModule,
    [Display(Name = "Predeclared Class")] PredeclaredClass,
    [Display(Name = "Standard Module")] StandardModule,
    [Display(Name = "UserForm Module")] UserFormModule
}
