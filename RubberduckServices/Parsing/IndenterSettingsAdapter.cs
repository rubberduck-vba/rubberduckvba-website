namespace RubberduckServices.Internal;

internal class IndenterSettingsAdapter : IIndenterSettings, Rubberduck.SmartIndenter.IIndenterSettings
{
    public IndenterSettingsAdapter(IIndenterSettings settings)
    {
        IndenterVersion = typeof(Rubberduck.SmartIndenter.IIndenterSettings).Assembly.GetName().Version.ToString(3);

        Code = settings.Code;

        ForceDebugPrintInColumn1 = settings.ForceDebugPrintInColumn1;
        VerticallySpaceProcedures = settings.VerticallySpaceProcedures;
        IndentSpaces = settings.IndentSpaces;
        EndOfLineCommentColumnSpaceAlignment = settings.EndOfLineCommentColumnSpaceAlignment;
        EmptyLineHandlingMethod = settings.EmptyLineHandlingMethod;
        EndOfLineCommentStyle = settings.EndOfLineCommentStyle;
        AlignDimColumn = settings.AlignDimColumn;
        AlignDims = settings.AlignDims;
        IndentCompilerDirectives = settings.IndentCompilerDirectives;
        ForceCompilerDirectivesInColumn1 = settings.ForceCompilerDirectivesInColumn1;
        ForceStopInColumn1 = settings.ForceStopInColumn1;
        ForceDebugAssertInColumn1 = settings.ForceDebugAssertInColumn1;
        GroupRelatedProperties = settings.GroupRelatedProperties;
        ForceDebugStatementsInColumn1 = settings.ForceDebugStatementsInColumn1;
        IndentCase = settings.IndentCase;
        IgnoreOperatorsInContinuations = settings.IgnoreOperatorsInContinuations;
        AlignContinuations = settings.AlignContinuations;
        AlignCommentsWithCode = settings.AlignCommentsWithCode;
        IgnoreEmptyLinesInFirstBlocks = settings.IgnoreEmptyLinesInFirstBlocks;
        IndentFirstDeclarationBlock = settings.IndentFirstDeclarationBlock;
        IndentFirstCommentBlock = settings.IndentFirstCommentBlock;
        IndentEnumTypeAsProcedure = settings.IndentEnumTypeAsProcedure;
        IndentEntireProcedureBody = settings.IndentEntireProcedureBody;
        LinesBetweenProcedures = settings.LinesBetweenProcedures;
    }

    public string IndenterVersion { get; set; }

    public string Code { get; set; }

    public bool ForceDebugPrintInColumn1 { get; set; }
    public bool VerticallySpaceProcedures { get; set; }
    public int IndentSpaces { get; set; }
    public int EndOfLineCommentColumnSpaceAlignment { get; set; }
    public IndenterEmptyLineHandling EmptyLineHandlingMethod { get; set; }
    public IndenterEndOfLineCommentStyle EndOfLineCommentStyle { get; set; }
    public int AlignDimColumn { get; set; }
    public bool AlignDims { get; set; }
    public bool IndentCompilerDirectives { get; set; }
    public bool ForceCompilerDirectivesInColumn1 { get; set; }
    public bool ForceStopInColumn1 { get; set; }
    public bool ForceDebugAssertInColumn1 { get; set; }
    public bool GroupRelatedProperties { get; set; }
    public bool ForceDebugStatementsInColumn1 { get; set; }
    public bool IndentCase { get; set; }
    public bool IgnoreOperatorsInContinuations { get; set; }
    public bool AlignContinuations { get; set; }
    public bool AlignCommentsWithCode { get; set; }
    public bool IgnoreEmptyLinesInFirstBlocks { get; set; }
    public bool IndentFirstDeclarationBlock { get; set; }
    public bool IndentFirstCommentBlock { get; set; }
    public bool IndentEnumTypeAsProcedure { get; set; }
    public bool IndentEntireProcedureBody { get; set; }
    public int LinesBetweenProcedures { get; set; }

    bool Rubberduck.SmartIndenter.IIndenterSettings.IndentEntireProcedureBody { get => IndentEntireProcedureBody; set => IndentEntireProcedureBody = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IndentEnumTypeAsProcedure { get => IndentEnumTypeAsProcedure; set => IndentEnumTypeAsProcedure = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IndentFirstCommentBlock { get => IndentFirstCommentBlock; set => IndentFirstCommentBlock = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IndentFirstDeclarationBlock { get => IndentFirstDeclarationBlock; set => IndentFirstDeclarationBlock = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IgnoreEmptyLinesInFirstBlocks { get => IgnoreEmptyLinesInFirstBlocks; set => IgnoreEmptyLinesInFirstBlocks = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.AlignCommentsWithCode { get => AlignCommentsWithCode; set => AlignCommentsWithCode = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.AlignContinuations { get => AlignContinuations; set => AlignContinuations = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IgnoreOperatorsInContinuations { get => IgnoreOperatorsInContinuations; set => IgnoreOperatorsInContinuations = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IndentCase { get => IndentCase; set => IndentCase = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.ForceDebugStatementsInColumn1 { get => ForceDebugStatementsInColumn1; set => ForceDebugStatementsInColumn1 = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.ForceDebugPrintInColumn1 { get => ForceDebugPrintInColumn1; set => ForceDebugPrintInColumn1 = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.ForceDebugAssertInColumn1 { get => ForceDebugAssertInColumn1; set => ForceDebugAssertInColumn1 = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.ForceStopInColumn1 { get => ForceStopInColumn1; set => ForceStopInColumn1 = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.ForceCompilerDirectivesInColumn1 { get => ForceCompilerDirectivesInColumn1; set => ForceCompilerDirectivesInColumn1 = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.IndentCompilerDirectives { get => IndentCompilerDirectives; set => IndentCompilerDirectives = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.AlignDims { get => AlignDims; set => AlignDims = value; }
    int Rubberduck.SmartIndenter.IIndenterSettings.AlignDimColumn { get => AlignDimColumn; set => AlignDimColumn = value; }
    Rubberduck.SmartIndenter.EndOfLineCommentStyle Rubberduck.SmartIndenter.IIndenterSettings.EndOfLineCommentStyle 
    { 
        get => (Rubberduck.SmartIndenter.EndOfLineCommentStyle)EndOfLineCommentStyle; 
        set => EndOfLineCommentStyle = (IndenterEndOfLineCommentStyle)value;
    }
    Rubberduck.SmartIndenter.EmptyLineHandling Rubberduck.SmartIndenter.IIndenterSettings.EmptyLineHandlingMethod
    {
        get => (Rubberduck.SmartIndenter.EmptyLineHandling)EmptyLineHandlingMethod;
        set => EmptyLineHandlingMethod = (IndenterEmptyLineHandling)value;
    }
    int Rubberduck.SmartIndenter.IIndenterSettings.EndOfLineCommentColumnSpaceAlignment { get => EndOfLineCommentColumnSpaceAlignment; set => EndOfLineCommentColumnSpaceAlignment = value; } 
    int Rubberduck.SmartIndenter.IIndenterSettings.IndentSpaces { get => IndentSpaces; set => IndentSpaces = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.VerticallySpaceProcedures { get => VerticallySpaceProcedures; set => VerticallySpaceProcedures = value; }
    int Rubberduck.SmartIndenter.IIndenterSettings.LinesBetweenProcedures { get => LinesBetweenProcedures; set => LinesBetweenProcedures = value; }
    bool Rubberduck.SmartIndenter.IIndenterSettings.GroupRelatedProperties { get => GroupRelatedProperties; set => GroupRelatedProperties = value; }

    bool Rubberduck.SmartIndenter.IIndenterSettings.LegacySettingsExist() => throw new NotSupportedException();

    void Rubberduck.SmartIndenter.IIndenterSettings.LoadLegacyFromRegistry() => throw new NotSupportedException();
}
