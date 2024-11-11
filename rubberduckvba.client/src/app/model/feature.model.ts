export interface ViewModel {
  id: number;
  dateInserted: string;
  dateUpdated: string;
  name: string;
  isNew: boolean;
  isHidden: boolean;

  isCollapsed: boolean;
  isDetailsCollapsed: boolean;
}

export interface SubFeatureViewModel extends ViewModel {
  featureId?: number;
  featureName?: string;
  featureTitle?: string;

  title: string;
  description: string;
}

export interface XmlDocViewModel extends SubFeatureViewModel {
  tagAssetId: number;
  tagName: string;
  sourceUrl: string;
  isDiscontinued: boolean;
}

export interface FeatureViewModel extends SubFeatureViewModel {
  shortDescription: string;

  hasImage: boolean;

  features: FeatureViewModel[];
}

export interface Example {
  description: string;
  sortOrder: number;
}

export interface BeforeAfterExample extends Example {
  modulesBefore: ExampleModule[];
  modulesAfter: ExampleModule[];
}

export interface ExampleModule {
  moduleName: string;
  moduleType: string;
  htmlContent: string;
  description: string;
}

export interface InspectionExample extends Example {
  hasResult: boolean;
  modules: ExampleModule[];

  isCollapsed: boolean | undefined;
}

export interface QuickFixExample extends BeforeAfterExample {

}

export interface AnnotationExample extends BeforeAfterExample {
  modules: ExampleModule[];
}

export type XmlDocExample = InspectionExample | QuickFixExample | AnnotationExample;

export interface AnnotationParameter {
  name: string;
  type: string;
  required: boolean;
  description: string;
}

export interface InspectionViewModel extends XmlDocViewModel {
  inspectionType: string;
  defaultSeverity: string;

  summary: string;
  reasoning: string;

  remarks?: string;
  hostApp?: string;

  references: string[];
  quickFixes: string[];

  examples: InspectionExample[];
}

export interface InspectionsFeatureViewModel extends SubFeatureViewModel {
  inspections: InspectionViewModel[];
}

export interface QuickFixesFeatureViewModel extends SubFeatureViewModel {
  quickFixes: QuickFixViewModel[];
}

export interface AnnotationsFeatureViewModel extends SubFeatureViewModel {
  annotations: AnnotationViewModel[];
}

export type XmlDocOrFeatureViewModel = FeatureViewModel | InspectionsFeatureViewModel | QuickFixesFeatureViewModel | AnnotationsFeatureViewModel;

export interface QuickFixViewModel extends XmlDocViewModel {
  summary: string;
  remarks?: string;
  canFixMultiple: boolean;
  canFixProcedure: boolean;
  canFixModule: boolean;
  canFixProject: boolean;
  canFixAll: boolean;

  inspections: string[];
  examples: QuickFixExample[];
}

export interface AnnotationViewModel extends XmlDocViewModel {
  summary: string;
  remarks?: string;

  parameters: AnnotationParameter[];
  examples: AnnotationExample[];
}

export type XmlDocItemViewModel = InspectionViewModel | QuickFixViewModel | AnnotationViewModel;

//export class FeatureItemViewModel {
//  id: number;
//  dateInserted: Date;
//  dateUpdated: Date | undefined;
//  featureId: number;
//  featureName: string;
//  featureTitle: string;
//  name: string;
//  title: string;
//  summary: string;
//  reasoning: string;
//  remarks: string;
//  isNew: boolean;
//  isDiscontinued: boolean;
//  isHidden: boolean;
//  tagAssetId: number;
//  sourceUrl: string;
//  serialized: string;
//  tagName: string;

//  isCollapsed: boolean;
//  isDetailsCollapsed: boolean;

//  info: any; // QuickFixInfo | AnnotationInfo | InspectionInfo;
//  examples: any[];

//  constructor(model: any) {
//    const info: any = JSON.parse(model.serialized);
//    info.Id = model.id;
//    info.FeatureName = model.featureName;
//    info.FeatureTitle = model.featureTitle;

//    if (model.featureName == 'Inspections') {
//      this.info = new InspectionInfoViewModel(model);
//    }
//    else if (model.featureName == 'QuickFixes') {
//      //this.info = new QuickFixInfoViewModel(JSON.parse(model.serialized));
//    }
//    else if (model.featureName == 'Annotations') {
//      //this.info = new AnnotationInfoViewModel(JSON.parse(model.serialized));
//    }

//    this.id = model.id;
//    this.dateInserted = model.dateTimeInserted;
//    this.dateUpdated = model.dateTimeUpdated;
//    this.featureId = model.featureId;
//    this.featureName = model.featureName;
//    this.featureTitle = model.featureTitle;
//    this.name = model.name;
//    this.title = model.title;
//    this.summary = model.summary;

//    this.reasoning = this.info.Reasoning;
//    this.remarks = this.info.Remarks;

//    this.isNew = model.isNew;
//    this.isDiscontinued = model.isDiscontinued;
//    this.isHidden = model.isHidden;

//    this.tagAssetId = model.tagAssetId;
//    this.tagName = model.tagName;
//    this.sourceUrl = model.sourceUrl;
//    this.serialized = model.serialized;

//    this.examples = this.info.examples;

//    this.isCollapsed = true;
//    this.isDetailsCollapsed = true;
//  }

//  public getGitHubEditLink(): string {
//    const url = this.sourceUrl;
//    return `https://github.com/rubberduck-vba/Rubberduck/edit/next/${url}.cs`;
//  }
//  public getGitHubViewLink(): string {
//    const url = this.sourceUrl;
//    return `https://github.com/rubberduck-vba/Rubberduck/tree/next/${url}.cs`;
//  }
//}
