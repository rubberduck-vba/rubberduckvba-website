import { DownloadInfo } from "./downloads.model";

export interface LatestTags {
  main: Tag;
  next: Tag;
}

export interface Tag {
  name: string;
  installerDownloadUrl: string;
  installerDownloads: number;
  dateCreated: string;
  dateTimeUpdated: string;
  isPreRelease: boolean;
}

export class TagDownloadInfo implements DownloadInfo {
  name: string;
  title: string;
  kind: string;
  downloadUrl: string;

  filename: string;

  constructor(tag: Tag) {
    this.name = tag.name;
    this.title = tag.name;

    this.kind = tag.isPreRelease ? 'pre' : 'tag';
    this.downloadUrl = tag.installerDownloadUrl;

    const parts = tag.installerDownloadUrl.split('/');
    this.filename = parts[parts.length - 1];
  }
}
