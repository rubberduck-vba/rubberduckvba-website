export interface FeatureSummary {
  /**
   * The internal name of the feature.
   */
  name: string;
  /**
   * The display name of the feature.
   */
  title: string;
  /**
   * A short (1-2 sentences) summary description of the feature.
   */
  shortDescription: string;
  /**
   * Whether the feature should have a 'new!' marker, indicating it is currently only available in pre-release builds.
   */
  isNew: boolean;

  hasImage: boolean;
}

export interface XmlDocExampleModule {
  sortOrder: number;
  moduleName: string;
  moduleType: number; // TODO enum
  htmlContent: string;
  properties: string; // JSON string ~> TODO deserialize
}

export interface XmlDocExample {
  sortOrder: number;
  properties: string; // JSON string ~> TODO deserialize
  modules: XmlDocExampleModule[];
}

export interface FeatureItem {
  id: number;
  dateTimeInserted: Date;
  dateTimeUpdated: Date | null;

  featureId: number;
  featureName: string;
  featureTitle: string;

  name: string;
  title: string;
  summary: string;

  isNew: boolean;
  isDiscontinued: boolean;
  isHidden: boolean;

  tagAssetId: number;
  sourceUrl: string;
  serialized: string;
}

export interface InspectionInfo {
  Reasoning: string;
  Summary: string;
  Remarks: string;
  HostApp: string;
  DefaultSeverity: string;
  InspectionType: string;
  QuickFixes: string[];
  References: string[];
  Examples: any[];
}

export class InspectionInfoViewModel {
  id: number;
  dateTimeInserted: Date;
  dateTimeUpdated: Date | null;
  featureId: number;
  featureName: string;
  featureTitle: string;
  name: string;
  title: string;
  summary: string;
  isNew: boolean;
  isDiscontinued: boolean;
  isHidden: boolean;
  tagAssetId: number;
  sourceUrl: string;
  examples: InspectionExampleViewModel[] = [];
  reasoning: string = '';
  remarks: string = '';
  defaultSeverity: string = '';
  inspectionType: string = '';
  hostApp: string = '';
  references: string[] = [];
  quickFixes: string[] = [];

  constructor(model: any) {
    this.id = model.id;
    this.dateTimeInserted = model.dateTimeInserted;
    this.dateTimeUpdated = model.dateTimeUpdated;
    this.featureId = model.featureId;
    this.featureName = model.featureName;
    this.featureTitle = model.featureTitle;
    this.name = model.Name;
    this.title = model.Title;
    this.summary = model.Summary;
    this.isNew = model.IsNew;
    this.isDiscontinued = model.IsDiscontinued;
    this.isHidden = model.IsHidden;
    this.tagAssetId = model.TagAssetId;
    this.sourceUrl = model.SourceUrl;

    const info = JSON.parse(model.serialized);
    if (info) {
      this.reasoning = info.Reasoning;
      this.remarks = info.Remarks;
      this.defaultSeverity = info.DefaultSeverity;
      this.inspectionType = info.InspectionType;
      this.hostApp = info.HostApp;
      this.references = info.References;
      this.quickFixes = info.QuickFixes;

      if (info.Examples) {
        this.examples = info.Examples.map((e: any) => new InspectionExampleViewModel(e));
      }
    }
  }
}

export class InspectionExampleViewModel {
  hasResult: boolean;
  sortOrder: number;
  modules: ExampleModuleViewModel[];

  isCollapsed: boolean = false;

  constructor(model: any) {
    this.sortOrder = model.SortOrder;
    this.hasResult = model.Properties != undefined && (JSON.parse(model.Properties).HasResult == 'True');
    this.modules = model.Modules.map((module: any) => new ExampleModuleViewModel(module));
    this.isCollapsed = this.sortOrder > 1;
  }
}

export class ExampleModuleViewModel {
  sortOrder: number;
  moduleName: string;
  moduleType: number;
  moduleTypeName: string;
  htmlContent: string;

  constructor(model: any) {
    this.sortOrder = model.SortOrder;
    this.htmlContent = model.HtmlContent;
    this.moduleName = model.ModuleName;
    this.moduleType = model.ModuleType;
    this.moduleTypeName = model.ModuleTypeName;
  }

  public get moduleTypeIconClass(): string {
    switch (this.moduleType) {
      case 2:
        return 'icon-module-class';
      case 3:
        return 'icon-module-document';
      case 4:
        return 'icon-module-interface';
      case 5:
        return 'icon-module-predeclared';
      case 6:
        return 'icon-module-standard';
      case 7:
        return 'icon-module-userform';
      default:
        return 'icon-project';
    }
  }
}

export class QuickFixExampleViewModel {
  isBefore: boolean;
  sortOrder: number;
  modules: ExampleModuleViewModel[];

  constructor(model: any) {
    console.log(model);
    this.sortOrder = model.sortOrder;
    this.isBefore = JSON.parse(model.properties).isBefore === true;
    this.modules = model.modules.map((module: any) => new ExampleModuleViewModel(module));
  }
}

export interface QuickFixInfo {
    Remarks: string;
    CanFixInProcedure: boolean;
    CanFixInModule: boolean;
    CanFixInProject: boolean;
    Inspections: string[];
    Examples: any[];
}

export class QuickFixViewModel {
  id: number;
  dateTimeInserted: Date;
  dateTimeUpdated: Date | null;
  featureId: number;
  featureName: string;
  featureTitle: string;
  name: string;
  title: string;
  summary: string;
  isNew: boolean;
  isDiscontinued: boolean;
  isHidden: boolean;
  tagAssetId: number;
  sourceUrl: string;

  examples: QuickFixExampleViewModel[];

  remarks: string;
  canFixInProcedure: boolean;
  canFixInModule: boolean;
  canFixInProject: boolean;
  inspections: string[];

  constructor(model: FeatureItem) {
    const quickfix: QuickFixInfo = JSON.parse(model.serialized);

    this.id = model.id;
    this.dateTimeInserted = model.dateTimeInserted;
    this.dateTimeUpdated = model.dateTimeUpdated;
    this.featureId = model.featureId;
    this.featureName = model.featureName;
    this.featureTitle = model.featureTitle;
    this.name = model.name;
    this.title = model.title;
    this.summary = model.summary;
    this.isNew = model.isNew;
    this.isDiscontinued = model.isDiscontinued;
    this.isHidden = model.isHidden;
    this.tagAssetId = model.tagAssetId;
    this.sourceUrl = model.sourceUrl;

    this.remarks = quickfix.Remarks;
    this.canFixInProcedure = quickfix.CanFixInProcedure;
    this.canFixInModule = quickfix.CanFixInModule;
    this.canFixInProject = quickfix.CanFixInProject;
    this.inspections = quickfix.Inspections;

    console.log(model.serialized);
    console.log(quickfix);
    this.examples = quickfix.Examples.map(e => new QuickFixExampleViewModel(e));
  }
}

export interface AnnotationInfo {
  parameters: AnnotationParameterInfo[];
}

export interface AnnotationParameterInfo {
  name: string;
  type: string;
  description: string;
}

export interface AnnotationFeatureItem extends FeatureItem {
  parameters: AnnotationParameterInfo[];
}

export interface Feature extends FeatureSummary {
  /**
   * A markdown document with the user documentation of the feature.
   */
  description: string;

  /**
   * Summaries of child features, if any.
   */
  features: Feature[];
  items: FeatureItem[];

  inspections: InspectionInfo[];

  isHidden: boolean;
  sortOrder: number;
}

export interface PaginatedFeature {
  feature: Feature;
  pagination: unknown;
}

export class FeatureViewModel {
  description: string;
  features: FeatureViewModel[] = [];

  inspections: InspectionInfo[] = [];

  items: FeatureItemViewModel[] = [];
  isHidden: boolean;
  sortOrder: number;
  name: string;
  title: string;
  shortDescription: string;
  isNew: boolean;
  hasImage: boolean;

  isCollapsed: boolean;
  isDetailsCollapsed: boolean;

  constructor(model: any) {
    this.description = model.description;
      this.isHidden = model.isHidden;
      this.sortOrder = model.sortOrder;

      if (model.features) {
        this.features = model.features.map((e: any) => new FeatureViewModel(e));
      }
      if (model.inspections) {
        this.inspections = model.inspections.map((e: any) => new InspectionInfoViewModel(e));
      }
      if (model.items) {
        this.items = model.items.map((e: any) => new FeatureItemViewModel(e));
      }

    this.name = model.name;
    this.title = model.title;
    this.shortDescription = model.shortDescription;
    this.isNew = model.isNew;
    this.hasImage = model.hasImage;

    this.isCollapsed = !this.hasImage || this.items.length > 0;
    this.isDetailsCollapsed = true;
  }
}

export class FeatureItemViewModel {
  id: number;
  dateInserted: Date;
  dateUpdated: Date | undefined;
  featureId: number;
  featureName: string;
  featureTitle: string;
  name: string;
  title: string;
  summary: string;
  reasoning: string;
  remarks: string;
  isNew: boolean;
  isDiscontinued: boolean;
  isHidden: boolean;
  tagAssetId: number;
  sourceUrl: string;
  serialized: string;
  tagName: string;

  isCollapsed: boolean;
  isDetailsCollapsed: boolean;

  info: any; // QuickFixInfo | AnnotationInfo | InspectionInfo;
  examples: any[];

  constructor(model: any) {
    const info: any = JSON.parse(model.serialized);
    info.Id = model.id;
    info.FeatureName = model.featureName;
    info.FeatureTitle = model.featureTitle;

    if (model.featureName == 'Inspections') {
      this.info = new InspectionInfoViewModel(model);
    }
    else if (model.featureName == 'QuickFixes') {
      //this.info = new QuickFixInfoViewModel(JSON.parse(model.serialized));
    }
    else if (model.featureName == 'Annotations') {
      //this.info = new AnnotationInfoViewModel(JSON.parse(model.serialized));
    }

    this.id = model.id;
    this.dateInserted = model.dateTimeInserted;
    this.dateUpdated = model.dateTimeUpdated;
    this.featureId = model.featureId;
    this.featureName = model.featureName;
    this.featureTitle = model.featureTitle;
    this.name = model.name;
    this.title = model.title;
    this.summary = model.summary;

    this.reasoning = this.info.Reasoning;
    this.remarks = this.info.Remarks;

    this.isNew = model.isNew;
    this.isDiscontinued = model.isDiscontinued;
    this.isHidden = model.isHidden;

    this.tagAssetId = model.tagAssetId;
    this.tagName = model.tagName;
    this.sourceUrl = model.sourceUrl;
    this.serialized = model.serialized;

    this.examples = this.info.examples;

    this.isCollapsed = true;
    this.isDetailsCollapsed = true;
  }

  public getGitHubEditLink(): string {
    const url = this.sourceUrl;
    return `https://github.com/rubberduck-vba/Rubberduck/edit/next/${url}.cs`;
  }
  public getGitHubViewLink(): string {
    const url = this.sourceUrl;
    return `https://github.com/rubberduck-vba/Rubberduck/tree/next/${url}.cs`;
  }
}

