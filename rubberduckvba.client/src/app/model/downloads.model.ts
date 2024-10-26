export interface DownloadInfo {
  /**
   * The name / description of the available download.
   */
  name: string;
  filename: string;
  /**
   * An additional description string that serves as a tooltip.
   */
  title: string;
  /**
   * The type of download, to determine what icon to use for the available download.
   */
  kind: string | DownloadKindOther | DownloadKindTag;
  /**
   * The download url, to build the download link.
   */
  downloadUrl: string;
}

export type DownloadKindTag = "tag" | "pre";
export type DownloadKindOther = "pdf";
