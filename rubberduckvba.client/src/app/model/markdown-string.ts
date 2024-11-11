import { ApiClientService } from "../services/api-client.service";

//export class MarkdownString extends String {

//    constructor(private service: ApiClientService, value: string) {
//      super(value);
//      this.rawText = value;
//      this.formattedText = value;
//    }

//    /**
//     * The unformatted markdown content (text/html).
//     */
//    public rawText: string;
//    /**
//     * The formatted markdown content (text/html).
//     */
//    public formattedText: string | undefined;

//    /**
//     * Asynchronously fetches the formatted markdown content.
//     */
//  public resolve(): void {
//      this.service.getMarkdown(this.rawText, true).forEach((result) => this.formattedText = result.markdownContent);
//  }
//}
