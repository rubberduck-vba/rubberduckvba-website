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
  links: BlogLink[];
}

export interface BlogLink {
  name: string;
  url: string;
  author: string;
  published: string;
}

export class BlogLinkViewModelClass implements BlogLink {
  name: string;
  url: string;
  author: string;
  published: string;

  constructor(model: BlogLink) {
    this.name = model.name;
    this.url = model.url;
    this.author = model.author;
    this.published = model.published;
  }
}

export interface Example {
  description: string;
  sortOrder: number;

  isCollapsed: boolean | undefined;
}

export interface BeforeAfterExample extends Example {
  modulesBefore: ExampleModule[];
  modulesAfter: ExampleModule[];
}

export interface ExampleModule {
  moduleName: string;
  moduleTypeName: string;
  htmlContent: string;
  description: string;
}

export interface InspectionExample extends Example {
  hasResult: boolean;
  modules: ExampleModule[];
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

  getGitHubViewLink(): string;
  getGitHubEditLink(): string;
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

export type XmlDocOrFeatureViewModel = SubFeatureViewModel | InspectionsFeatureViewModel | QuickFixesFeatureViewModel | AnnotationsFeatureViewModel;

export interface QuickFixInspectionLinkViewModel {
  name: string;
  summary: string;
  inspectionType: string;
  defaultSeverity: string;
}

export class QuickFixInspectionLinkViewModelClass implements QuickFixInspectionLinkViewModel {
    name: string;
    summary: string;
    inspectionType: string;
    defaultSeverity: string;

  constructor(model: QuickFixInspectionLinkViewModel) {
    this.name = model.name;
    this.summary = model.summary;
    this.inspectionType = model.inspectionType;
    this.defaultSeverity = model.defaultSeverity;
  }

  public get getSeverityIconClass(): string {
    return `icon icon-severity-${this.defaultSeverity.toLowerCase()}`;
  }
}

export interface QuickFixViewModel extends XmlDocViewModel {
  summary: string;
  remarks?: string;
  canFixMultiple: boolean;
  canFixProcedure: boolean;
  canFixModule: boolean;
  canFixProject: boolean;
  canFixAll: boolean;

  inspections: QuickFixInspectionLinkViewModel[];
  examples: QuickFixExample[];

  getGitHubViewLink(): string;
  getGitHubEditLink(): string;
}

export interface AnnotationViewModel extends XmlDocViewModel {
  summary: string;
  remarks?: string;

  parameters: AnnotationParameter[];
  examples: AnnotationExample[];

  getGitHubViewLink(): string;
  getGitHubEditLink(): string;
}

export type XmlDocItemViewModel = InspectionViewModel | QuickFixViewModel | AnnotationViewModel;

export class ViewModelBase implements ViewModel {
  id: number;
  dateInserted: string;
  dateUpdated: string;
  name: string;
  isNew: boolean;
  isHidden: boolean;
  isCollapsed: boolean;
  isDetailsCollapsed: boolean;

  constructor(model: ViewModel) {
    this.id = model.id;
    this.dateInserted = model.dateInserted;
    this.dateUpdated = model.dateUpdated;
    this.name = model.name;
    this.isNew = model.isNew;
    this.isHidden = model.isHidden;

    this.isCollapsed = model.isCollapsed;
    this.isDetailsCollapsed = true;
  }

  protected depascalize(name: string): string {
    const lWords = ["The", "Is", "As", "Of", "In", "On", "Not"];
    const uWords = ["Udf", "Id"];
    const nsWords = ["By Ref", "By Val", "Def Type", "I If", "U D T", "UD T", "Is Missing", "Param Array", "Predeclared ID"];

    const words = name.split(/(?=[A-Z])/)

      .map(e => uWords.find(w => w == e) ? e.toUpperCase() : lWords.find(w => w == e) ? e.toLowerCase() : e);

    let depascalized = words.join(' ');

    nsWords.forEach(w => {
      depascalized = depascalized.replace(w, w.replace(' ', ''));
    });

    depascalized = depascalized.replace("is Missing", "IsMissing");
    let [first, ...rest] = depascalized;
    return first.toUpperCase() + rest.join('');
  }
}

export class FeatureViewModelClass extends ViewModelBase {
  featureId?: number;
  featureName?: string;
  featureTitle?: string;

  title: string;
  description: string;

  shortDescription: string;

  hasImage: boolean;

  features: FeatureViewModel[];
  links: BlogLink[];

  constructor(model: FeatureViewModel) {
    super(model);
    this.title = model.title;
    this.description = model.description;
    this.shortDescription = model.shortDescription;
    this.hasImage = model.hasImage;
    this.features = model.features.map(e => new FeatureViewModelClass(e));
    this.links = model.links?.map(e => new BlogLinkViewModelClass(e)) ?? [];

    this.isCollapsed = !model.hasImage;
  }
}

export class SubFeatureViewModelClass extends ViewModelBase implements SubFeatureViewModel {
  featureId?: number | undefined;
  featureName?: string | undefined;
  featureTitle?: string | undefined;
  title: string;
  description: string;

  constructor(model: SubFeatureViewModel) {
    super(model);
    this.title = model.title;
    this.description = model.description;
    this.isDetailsCollapsed = true;
    this.featureId = model.featureId;
    this.featureName = model.featureName;
  }
}

export class InspectionViewModelClass extends SubFeatureViewModelClass implements InspectionViewModel {
  inspectionType: string;
  defaultSeverity: string;
  summary: string;
  reasoning: string;
  remarks?: string | undefined;
  hostApp?: string | undefined;
  references: string[];
  quickFixes: string[];
  examples: InspectionExample[];
  tagAssetId: number;
  tagName: string;
  sourceUrl: string;
  isDiscontinued: boolean;

  public getGitHubViewLink(): string {
    return `https://github.com/rubberduck-vba/Rubberduck/blob/next/${this.sourceUrl}.cs`
  }

  public getGitHubEditLink(): string {
    return `https://github.com/rubberduck-vba/Rubberduck/edit/next/${this.sourceUrl}.cs`;
  }

  constructor(model: InspectionViewModel) {
    super(model);
    this.name = model.name;
    this.title = this.depascalize(model.name);

    this.inspectionType = model.inspectionType;
    this.defaultSeverity = model.defaultSeverity;
    this.summary = model.summary;
    this.reasoning = model.reasoning;
    this.remarks = model.remarks;
    this.hostApp = model.hostApp;
    this.references = model.references;

    this.quickFixes = model.quickFixes;
    this.examples = model.examples;

    this.tagAssetId = model.tagAssetId;
    this.tagName = model.tagName;
    this.sourceUrl = model.sourceUrl;
    this.isDiscontinued = model.isDiscontinued;
  }
}

export class QuickFixViewModelClass extends SubFeatureViewModelClass implements QuickFixViewModel {
  summary: string;
  remarks?: string | undefined;
  canFixMultiple: boolean;
  canFixProcedure: boolean;
  canFixModule: boolean;
  canFixProject: boolean;
  canFixAll: boolean;
  inspections: QuickFixInspectionLinkViewModelClass[];
  examples: QuickFixExample[];
  tagAssetId: number;
  tagName: string;
  sourceUrl: string;
  isDiscontinued: boolean;

  public getGitHubViewLink(): string {
    return `https://github.com/rubberduck-vba/Rubberduck/blob/next/${this.sourceUrl}.cs`
  }

  public getGitHubEditLink(): string {
    return `https://github.com/rubberduck-vba/Rubberduck/edit/next/${this.sourceUrl}.cs`;
  }
  constructor(model: QuickFixViewModel) {
    super(model);
    this.title = this.depascalize(model.name.replace('QuickFix',''));

    this.summary = model.summary;
    this.remarks = model.remarks;

    this.examples = model.examples;
    this.inspections = model.inspections.map(e => new QuickFixInspectionLinkViewModelClass(e));

    this.tagAssetId = model.tagAssetId;
    this.tagName = model.tagName;
    this.sourceUrl = model.sourceUrl;
    this.isDiscontinued = model.isDiscontinued;

    this.canFixMultiple = model.canFixMultiple;
    this.canFixAll = model.canFixAll;
    this.canFixProject = model.canFixProject;
    this.canFixModule = model.canFixModule;
    this.canFixProcedure = model.canFixProcedure;
  }
}

export class AnnotationViewModelClass extends SubFeatureViewModelClass implements AnnotationViewModel {
  summary: string;
  remarks?: string | undefined;
  examples: AnnotationExample[];
  tagAssetId: number;
  tagName: string;
  sourceUrl: string;
  isDiscontinued: boolean;

  parameters: AnnotationParameter[];

  public getGitHubViewLink(): string {
    return `https://github.com/rubberduck-vba/Rubberduck/blob/next/${this.sourceUrl}.cs`
  }

  public getGitHubEditLink(): string {
    return `https://github.com/rubberduck-vba/Rubberduck/edit/next/${this.sourceUrl}.cs`;
  }
  constructor(model: AnnotationViewModel) {
    super(model);
    this.title = this.depascalize(model.name.replace('Annotation', ''));

    this.summary = model.summary;
    this.remarks = model.remarks;

    this.examples = model.examples;

    this.tagAssetId = model.tagAssetId;
    this.tagName = model.tagName;
    this.sourceUrl = model.sourceUrl;
    this.isDiscontinued = model.isDiscontinued;

    this.parameters = model.parameters;
  }
}

export class InspectionsFeatureViewModelClass extends SubFeatureViewModelClass implements InspectionsFeatureViewModel {
  inspections: InspectionViewModel[];
  constructor(model: InspectionsFeatureViewModel) {
    super(model);
    this.inspections = model.inspections.map(e => new InspectionViewModelClass(e));
  }
}

export class QuickFixesFeatureViewModelClass extends SubFeatureViewModelClass implements QuickFixesFeatureViewModel {
  quickFixes: QuickFixViewModel[];

  constructor(model: QuickFixesFeatureViewModel) {
    super(model);
    this.quickFixes = model.quickFixes.map(e => new QuickFixViewModelClass(e));
  }
}

export class AnnotationsFeatureViewModelClass extends SubFeatureViewModelClass implements AnnotationsFeatureViewModel {
  annotations: AnnotationViewModel[];

  constructor(model: AnnotationsFeatureViewModel) {
    super(model);
    this.annotations = model.annotations.map(e => new AnnotationViewModelClass(e));
  }
}
