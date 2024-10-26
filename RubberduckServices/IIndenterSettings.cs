namespace RubberduckServices;
public enum IndenterEndOfLineCommentStyle
{
    Absolute = 0, // Rubberduck.SmartIndenter.EndOfLineCommentStyle.Absolute,
    SameGap = 1, // Rubberduck.SmartIndenter.EndOfLineCommentStyle.SameGap,
    StandardGap = 2, // Rubberduck.SmartIndenter.EndOfLineCommentStyle.StandardGap,
    AlignInColumn = 3, // Rubberduck.SmartIndenter.EndOfLineCommentStyle.AlignInColumn
}

public enum IndenterEmptyLineHandling
{
    Ignore = 0, // Rubberduck.SmartIndenter.EmptyLineHandling.Ignore,
    Remove = 1, // Rubberduck.SmartIndenter.EmptyLineHandling.Remove,
    Indent = 2, // Rubberduck.SmartIndenter.EmptyLineHandling.Indent
}

public interface IIndenterSettings
{
    string IndenterVersion { get; set; }
    string Code { get; set; }

    bool ForceDebugPrintInColumn1 { get; set; }
    bool VerticallySpaceProcedures { get; set; }
    int IndentSpaces { get; set; }
    int EndOfLineCommentColumnSpaceAlignment { get; set; }
    IndenterEmptyLineHandling EmptyLineHandlingMethod { get; set; }
    IndenterEndOfLineCommentStyle EndOfLineCommentStyle { get; set; }
    int AlignDimColumn { get; set; }
    bool AlignDims { get; set; }
    bool IndentCompilerDirectives { get; set; }
    bool ForceCompilerDirectivesInColumn1 { get; set; }
    bool ForceStopInColumn1 { get; set; }
    bool ForceDebugAssertInColumn1 { get; set; }
    bool GroupRelatedProperties { get; set; }
    bool ForceDebugStatementsInColumn1 { get; set; }
    bool IndentCase { get; set; }
    bool IgnoreOperatorsInContinuations { get; set; }
    bool AlignContinuations { get; set; }
    bool AlignCommentsWithCode { get; set; }
    bool IgnoreEmptyLinesInFirstBlocks { get; set; }
    bool IndentFirstDeclarationBlock { get; set; }
    bool IndentFirstCommentBlock { get; set; }
    bool IndentEnumTypeAsProcedure { get; set; }
    bool IndentEntireProcedureBody { get; set; }
    int LinesBetweenProcedures { get; set; }

}
