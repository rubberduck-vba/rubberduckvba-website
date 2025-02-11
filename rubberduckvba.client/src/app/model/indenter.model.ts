export enum IndenterEmptyLineHandling {
  ignore = 0,
  remove = 1,
  indent = 2
}

export enum IndenterEndOfLineCommentStyle {
  absolute = 0,
  sameGap = 1,
  standardGap = 2,
  alignInColumn = 3
}


export interface IndenterViewModel {
    indenterVersion: string;
    code: string;
    indentedCode: string;

    // indent
    indentSpaces: number;
    emptyLineHandlingMethod: IndenterEmptyLineHandling;
    indentEntireProcedureBody: boolean;
    indentFirstDeclarationBlock: boolean;
    indentFirstCommentBlock: boolean;
    ignoreEmptyLinesInFirstBlock: boolean;
    indentEnumTypeAsProcedure: boolean;
    indentCase: boolean;

    // outdent
    indentCompilerDirectives: boolean;
    forceCompilerDirectivesInColumn1: boolean;
    forceDebugPrintInColumn1: boolean;
    forceStopInColumn1: boolean;
    forceDebugAssertInColumn1: boolean;
    forceDebugStatementsInColumn1: boolean;

    // alignment
    alignContinuations: boolean;
    ignoreOperatorsInContinuations: boolean;
    alignDims: boolean;
    alignDimColumn: number;


    // comments
    alignCommentsWithCode: boolean;
    endOfLineCommentStyle: IndenterEndOfLineCommentStyle;
    endOfLineCommentColumnSpaceAlignment: number;

    // vertical spacing
    groupRelatedProperties: boolean;
    verticallySpaceProcedures: boolean;
    linesBetweenProcedures: number;
}

export class IndenterVersionViewModelClass {
  public version: string;
  constructor(version: string){
    this.version = version;
  }
}

export class IndenterViewModelClass implements IndenterViewModel {
  indenterVersion: string;
  code: string;
  indentedCode: string;
  indentSpaces: number;
  emptyLineHandlingMethod: IndenterEmptyLineHandling;
  indentEntireProcedureBody: boolean;
  indentFirstDeclarationBlock: boolean;
  indentFirstCommentBlock: boolean;
  ignoreEmptyLinesInFirstBlock: boolean;
  indentEnumTypeAsProcedure: boolean;
  indentCase: boolean;
  indentCompilerDirectives: boolean;
  forceCompilerDirectivesInColumn1: boolean;
  forceStopInColumn1: boolean;
  forceDebugPrintInColumn1: boolean;
  forceDebugAssertInColumn1: boolean;
  forceDebugStatementsInColumn1: boolean;
  alignContinuations: boolean;
  ignoreOperatorsInContinuations: boolean;
  alignDims: boolean;
  alignDimColumn: number;
  alignCommentsWithCode: boolean;
  endOfLineCommentStyle: IndenterEndOfLineCommentStyle;
  endOfLineCommentColumnSpaceAlignment: number;
  groupRelatedProperties: boolean;
  verticallySpaceProcedures: boolean;
  linesBetweenProcedures: number;

  constructor(model: IndenterViewModel) {
    this.indenterVersion = model.indenterVersion;
    this.code = model.code;
    this.indentedCode = model.indentedCode;

    this.indentSpaces = model.indentSpaces;
    this.emptyLineHandlingMethod = model.emptyLineHandlingMethod;
    this.indentEntireProcedureBody = model.indentEntireProcedureBody;
    this.indentFirstDeclarationBlock = model.indentFirstDeclarationBlock;
    this.indentFirstCommentBlock = model.indentFirstCommentBlock;
    this.ignoreEmptyLinesInFirstBlock = model.ignoreEmptyLinesInFirstBlock;
    this.indentEnumTypeAsProcedure = model.indentEnumTypeAsProcedure;
    this.indentCase = model.indentCase;
    this.indentCompilerDirectives = model.indentCompilerDirectives;
    this.forceCompilerDirectivesInColumn1 = model.forceCompilerDirectivesInColumn1;
    this.forceStopInColumn1 = model.forceStopInColumn1;
    this.forceDebugPrintInColumn1 = model.forceDebugPrintInColumn1;
    this.forceDebugAssertInColumn1 = model.forceDebugAssertInColumn1;
    this.forceDebugStatementsInColumn1 = model.forceDebugStatementsInColumn1;
    this.alignContinuations = model.alignContinuations;
    this.ignoreOperatorsInContinuations = model.ignoreOperatorsInContinuations;
    this.alignDims = model.alignDims;
    this.alignDimColumn = model.alignDimColumn;
    this.alignCommentsWithCode = model.alignCommentsWithCode;
    this.endOfLineCommentStyle = model.endOfLineCommentStyle;
    this.endOfLineCommentColumnSpaceAlignment = model.endOfLineCommentColumnSpaceAlignment;
    this.groupRelatedProperties = model.groupRelatedProperties;
    this.verticallySpaceProcedures = model.verticallySpaceProcedures;
    this.linesBetweenProcedures = model.linesBetweenProcedures;
  }
}
