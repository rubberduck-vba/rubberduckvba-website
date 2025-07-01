export interface ViewModel {
  id: number | undefined;
  dateInserted: string;
  dateUpdated: string;
  name: string;
  isNew: boolean;
  isHidden: boolean;

  isCollapsed: boolean;
  isDetailsCollapsed: boolean;
}

export interface MarkdownContent {
  content: string;
}

export interface AuditRecordViewModel {
  id: number,
  dateInserted: string,
  dateModified: string | null,
  author: string,
  approvedAt: string | null,
  approvedBy: string | null,
  rejectedAt: string | null,
  rejectedBy: string | null,
  isStale: boolean,
  isPending: boolean,
}

export interface FeatureEditViewModel extends AuditRecordViewModel {
  featureId: string,
  featureName: string,
  fieldName: string,
  valueBefore: string | null,
  valueAfter: string,
}

export enum FeatureOperation {
  Create = 1,
  Delete = 2,
}

export interface FeatureOperationViewModel extends AuditRecordViewModel {
  featureName: string;
  featureAction: FeatureOperation;
  parentId: number | null;
  name: string | null;
  title: string | null;
  shortDescription: string | null;
  description: string | null;
  isNew: boolean | null;
  isHidden: boolean | null;
  hasImage: boolean | null;
  links: BlogLink[];
}

export interface PendingAuditsViewModel {
  edits: FeatureEditViewModel[];
  other: FeatureOperationViewModel[];
}

export interface SubFeatureViewModel extends ViewModel {
  featureId?: number;
  featureName?: string;
  featureTitle?: string;

  title: string;
  description: string;
  shortDescription: string;

  links: BlogLink[];
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

  isCreatePending: boolean;
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
  quickFixes: QuickFixViewModel[];

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
  id: number | undefined;
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

  isCreatePending: boolean;

  constructor(model: FeatureViewModel) {
    super(model);
    this.title = model.title;
    this.description = model.description;
    this.shortDescription = model.shortDescription;
    this.hasImage = model.hasImage;
    this.features = model.features.map(e => new FeatureViewModelClass(e));
    this.links = model.links?.map(e => new BlogLinkViewModelClass(e)) ?? [];

    this.isCollapsed = !model.hasImage;
    this.isCreatePending = model.isCreatePending;
  }
}

export class SubFeatureViewModelClass extends ViewModelBase implements SubFeatureViewModel {
  featureId?: number | undefined;
  featureName?: string | undefined;
  featureTitle?: string | undefined;
  title: string;
  description: string;
  shortDescription: string;

  links: BlogLink[];

  constructor(model: SubFeatureViewModel) {
    super(model);
    this.title = model.title;
    this.description = model.description;
    this.shortDescription = model.shortDescription;
    this.isDetailsCollapsed = true;
    this.featureId = model.featureId;
    this.featureName = model.featureName;
    this.links = model.links;
  }
}

export class EditSubFeatureViewModelClass extends SubFeatureViewModelClass {
  constructor(model: SubFeatureViewModel) {
    super(model);
    this.isDetailsCollapsed = false;
    this.descriptionPreview = model.description;
  }

  public descriptionPreview: string;
}

export class InspectionViewModelClass extends SubFeatureViewModelClass implements InspectionViewModel {
  inspectionType: string;
  defaultSeverity: string;
  summary: string;
  reasoning: string;
  remarks?: string | undefined;
  hostApp?: string | undefined;
  references: string[];
  quickFixes: QuickFixViewModel[];
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

export interface UserViewModel {
  name: string;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isReviewer: boolean;
  isWriter: boolean;
}

export enum UserActivityType {
  SubmitEdit = 'SubmitEdit',
  ApproveEdit = 'ApproveEdit',
  RejectEdit = 'RejectEdit',
  SubmitCreate = 'SubmitCreate',
  ApproveCreate = 'ApproveCreate',
  RejectCreate = 'RejectCreate',
  SubmitDelete = 'SubmitDelete',
  ApproveDelete = 'ApproveDelete',
  RejectDelete = 'RejectDelete',
}

export interface UserActivityItem {
  id: number;
  activityTimestamp: string;
  author: string;
  activity: UserActivityType;
  description: string;
  status: UserActivityStatus;
  reviewedBy?: string;
}

export enum UserActivityStatus {
  pending = 'Pending',
  approved = 'Approved',
  rejected = 'Rejected'
}

export class UserActivityItemClass implements UserActivityItem {
  id: number;
  activityTimestamp: string;
  author: string;
  activity: UserActivityType;
  description: string;
  status: UserActivityStatus;
  reviewedBy?: string;

  constructor(item: UserActivityItem) {
    this.id = item.id;
    this.activityTimestamp = item.activityTimestamp;
    this.author = item.author;
    this.activity = item.activity;
    this.description = item.description;
    this.status = item.status;
    this.reviewedBy = item.reviewedBy;
  }

  public get linkUrl(): string {
    switch (this.activity) {
      case UserActivityType.SubmitEdit:
      case UserActivityType.ApproveEdit:
      case UserActivityType.RejectEdit:
        return `audits/edits/${this.id}`;
      default:
        return `audits/ops/${this.id}`;
    }
  }
}
